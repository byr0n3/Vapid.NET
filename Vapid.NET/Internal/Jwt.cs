using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Vapid.NET.Internal.Models;

namespace Vapid.NET.Internal
{
	internal static class Jwt
	{
		private static System.ReadOnlySpan<byte> Separator =>
			"."u8;

		public static string GetSignedToken(string endpoint, VapidOptions vapid)
		{
			Debug.Assert(vapid.IsValid);

			var info = new JwtInfo
			{
				Type = "JWT",
				Algorithm = "ES256",
			};

			// @todo Skip parsing to Uri
			var uri = new System.Uri(endpoint, System.UriKind.Absolute);
			var audience = string.Create(null, stackalloc char[uri.Scheme.Length + 3 + uri.Host.Length], $"{uri.Scheme}://{uri.Host}");

			var data = new JwtData
			{
				Audience = audience,
				Expiration = System.DateTime.UtcNow.AddHours(12),
				Subject = vapid.Subject,
			};

			return Jwt.GetSignedToken(vapid.PublicKey, vapid.PrivateKey, info, data);
		}

		private static string GetSignedToken(string publicKey, string privateKey, JwtInfo info, JwtData data)
		{
			var buffer = new RentedArray<byte>(256);
			var builder = new ByteBuilder(buffer);

			using (buffer)
			{
				// Append unsigned token
				{
					Jwt.AppendBase64(ref builder, info, WebPushJsonSerializerContext.Default.JwtInfo!);
					builder.Append(Jwt.Separator);
					Jwt.AppendBase64(ref builder, data, WebPushJsonSerializerContext.Default.JwtData!);
				}

				var unsignedToken = builder.Result;

				// Append token signature
				{
					builder.Append(Jwt.Separator);
					Jwt.AppendSignature(ref builder, unsignedToken, publicKey, privateKey);
				}

				return Encoding.UTF8.GetString(builder.Result);
			}
		}

		private static void AppendBase64<T>(ref ByteBuilder builder, T value, JsonTypeInfo<T> typeInfo)
		{
			var infoBytes = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);

			builder.AppendUrlSafeBase64(infoBytes);
		}

		private static void AppendSignature(ref ByteBuilder builder,
											scoped System.ReadOnlySpan<byte> unsignedToken,
											string publicKey,
											string privateKey)
		{
			System.Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];

			var hashed = SHA256.TryHashData(unsignedToken, hash, out _);

			Debug.Assert(hashed);

			using (var ecdsa = ECDsa.Create(Encryption.GetEncryptionParameters(publicKey, privateKey)))
			{
				System.Span<byte> signature = stackalloc byte[ecdsa.GetMaxSignatureSize(default)];

				var signed = ecdsa.TrySignHash(hash, signature, default, out var written);

				Debug.Assert(signed);

				signature.Slice(0, written);

				builder.AppendUrlSafeBase64(signature);
			}
		}
	}
}
