using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Vapid.NET.Internal
{
	internal static partial class Encryption
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Hkdf(scoped System.Span<byte> dst,
								 scoped System.ReadOnlySpan<byte> salt,
								 scoped System.ReadOnlySpan<byte> prk,
								 scoped System.ReadOnlySpan<byte> info) =>
			HKDF.DeriveKey(HashAlgorithmName.SHA256, prk, dst, salt, info);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void HkdfPrk(scoped System.Span<byte> dst,
									scoped System.ReadOnlySpan<byte> userPrivateKey,
									scoped System.ReadOnlySpan<byte> key) =>
			Encryption.Hkdf(dst, userPrivateKey, key, "Content-Encoding: auth\0"u8);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void HkdfCek(scoped System.Span<byte> dst,
									scoped System.ReadOnlySpan<byte> salt,
									scoped System.ReadOnlySpan<byte> prk,
									scoped System.ReadOnlySpan<byte> userPublicKey,
									scoped System.ReadOnlySpan<byte> serverPublicKey)
		{
			System.Span<byte> info = stackalloc byte[Encryption.GetInfoChunkSize("aesgcm"u8, userPublicKey, serverPublicKey)];

			Encryption.FillInfoChunk(info, "aesgcm"u8, userPublicKey, serverPublicKey);

			Encryption.Hkdf(dst, salt, prk, info);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void HkdfNonce(scoped System.Span<byte> dst,
									  scoped System.ReadOnlySpan<byte> salt,
									  scoped System.ReadOnlySpan<byte> prk,
									  scoped System.ReadOnlySpan<byte> userPublicKey,
									  scoped System.ReadOnlySpan<byte> serverPublicKey)
		{
			System.Span<byte> info = stackalloc byte[Encryption.GetInfoChunkSize("nonce"u8, userPublicKey, serverPublicKey)];

			Encryption.FillInfoChunk(info, "nonce"u8, userPublicKey, serverPublicKey);

			Encryption.Hkdf(dst, salt, prk, info);
		}

		private static System.ReadOnlySpan<byte> InfoChunkEncoding =>
			"Content-Encoding: "u8;

		private static System.ReadOnlySpan<byte> InfoChunkEncryption =>
			"\0P-256\0"u8;

		private static void FillInfoChunk(scoped System.Span<byte> dst,
										  scoped System.ReadOnlySpan<byte> type,
										  scoped System.ReadOnlySpan<byte> userPublicKey,
										  scoped System.ReadOnlySpan<byte> serverPublicKey)
		{
			var builder = new ByteBuilder(dst);
			{
				builder.Append(Encryption.InfoChunkEncoding);
				builder.Append(type);
				builder.Append(Encryption.InfoChunkEncryption);
				builder.Append((ushort)userPublicKey.Length);
				builder.Append(userPublicKey);
				builder.Append((ushort)serverPublicKey.Length);
				builder.Append(serverPublicKey);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetInfoChunkSize(scoped System.ReadOnlySpan<byte> type,
											scoped System.ReadOnlySpan<byte> userPublicKey,
											scoped System.ReadOnlySpan<byte> serverPublicKey) =>
			Encryption.InfoChunkEncoding.Length +
			type.Length +
			Encryption.InfoChunkEncryption.Length +
			sizeof(ushort) +
			userPublicKey.Length +
			sizeof(ushort) +
			serverPublicKey.Length;
	}
}
