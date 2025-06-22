using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using JetBrains.Annotations;
using Vapid.NET.Internal.Models;
using Vapid.NET.Models;

namespace Vapid.NET.Internal
{
	internal static partial class Encryption
	{
		[MustDisposeResource]
		public static EncryptionResult Encrypt(PushSubscription subscription, DeclarativePushNotification notification, VapidOptions vapid)
		{
			Debug.Assert(vapid.IsValid);

			var salt = new RentedArray<byte>(16);

			System.Random.Shared.NextBytes(salt);

			var payload = JsonSerializer.SerializeToUtf8Bytes(notification,
															  WebPushJsonSerializerContext.Default.DeclarativePushNotification!);

			System.Span<byte> userPublicKey = stackalloc byte[65];
			System.Span<byte> userPrivateKey = stackalloc byte[65];

			var written = UrlSafeBase64.Decode(subscription.P256dh, userPublicKey);
			userPublicKey = userPublicKey.Slice(0, written);

			written = UrlSafeBase64.Decode(subscription.Auth, userPrivateKey);
			userPrivateKey = userPrivateKey.Slice(0, written);

			RentedArray<byte> serverPublicKey;
			byte[] key;

			using (var server = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
			using (var user = ECDiffieHellman.Create(Encryption.GetEncryptionParameters(userPublicKey)))
			{
				server.GenerateKey(ECCurve.NamedCurves.nistP256);

				serverPublicKey = Encryption.ExportRawPublicKey(server.PublicKey);
				key = server.DeriveRawSecretAgreement(user.PublicKey);
			}

			System.Span<byte> prk = stackalloc byte[32];
			System.Span<byte> cek = stackalloc byte[16];
			System.Span<byte> nonce = stackalloc byte[12];

			Encryption.HkdfPrk(prk, userPrivateKey, key);
			Encryption.HkdfCek(cek, salt, prk, userPublicKey, serverPublicKey);
			Encryption.HkdfNonce(nonce, salt, prk, userPublicKey, serverPublicKey);

			System.Span<byte> paddedPayload = stackalloc byte[2 + payload.Length];

			Encryption.PadInput(payload, paddedPayload);
			var encrypted = Encryption.EncryptPayload(paddedPayload, cek, nonce);

			return new EncryptionResult
			{
				Salt = salt,
				Payload = encrypted,
				PublicKey = serverPublicKey,
			};
		}

		[MustDisposeResource]
		private static RentedArray<byte> ExportRawPublicKey(ECDiffieHellmanPublicKey key)
		{
			var parameters = key.ExportParameters();

			var x = parameters.Q.X;
			var y = parameters.Q.Y;

			Debug.Assert(x is not null);
			Debug.Assert(y is not null);

			var buffer = new RentedArray<byte>(1 + x.Length + y.Length);

			buffer[0] = 0x04;
			System.MemoryExtensions.AsSpan(x).TryCopyTo(buffer.Slice(1));
			System.MemoryExtensions.AsSpan(y).TryCopyTo(buffer.Slice(1 + x.Length));

			return buffer;
		}

		[MustDisposeResource]
		private static RentedArray<byte> EncryptPayload(scoped System.ReadOnlySpan<byte> payload,
														scoped System.ReadOnlySpan<byte> cek,
														scoped System.ReadOnlySpan<byte> nonce)
		{
			System.Span<byte> cipher = stackalloc byte[payload.Length];
			System.Span<byte> tag = stackalloc byte[AesGcm.TagByteSizes.MaxSize];

			using (var aes = new AesGcm(cek, AesGcm.TagByteSizes.MaxSize))
			{
				aes.Encrypt(nonce, payload, cipher, tag);
			}

			var encryptedPayload = new RentedArray<byte>(cipher.Length + tag.Length);

			var copied = cipher.TryCopyTo(encryptedPayload);
			Debug.Assert(copied);

			copied = tag.TryCopyTo(encryptedPayload.Slice(cipher.Length));
			Debug.Assert(copied);

			return encryptedPayload;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void PadInput(scoped System.ReadOnlySpan<byte> src, scoped System.Span<byte> dst)
		{
			var copied = src.TryCopyTo(dst.Slice(2));
			Debug.Assert(copied);
		}
	}
}
