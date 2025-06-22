using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Vapid.NET.Internal
{
	internal static partial class Encryption
	{
		public static ECParameters GetEncryptionParameters(string publicKey, string privateKey)
		{
			var decodedPublicKey = UrlSafeBase64.Decode(publicKey);
			var decodedPrivateKey = UrlSafeBase64.Decode(privateKey);

			return Encryption.GetEncryptionParameters(decodedPublicKey, decodedPrivateKey);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ECParameters GetEncryptionParameters(scoped System.ReadOnlySpan<byte> decodedPublicKey, byte[] decodedPrivateKey) =>
			new()
			{
				Curve = ECCurve.NamedCurves.nistP256,
				D = decodedPrivateKey,
				Q = new ECPoint
				{
					X = decodedPublicKey.Slice(1, 32).ToArray(),
					Y = decodedPublicKey.Slice(33, 32).ToArray(),
				},
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ECParameters GetEncryptionParameters(scoped System.ReadOnlySpan<byte> decodedPublicKey) =>
			new()
			{
				Curve = ECCurve.NamedCurves.nistP256,
				Q = new ECPoint
				{
					X = decodedPublicKey.Slice(1, 32).ToArray(),
					Y = decodedPublicKey.Slice(33, 32).ToArray(),
				},
			};
	}
}
