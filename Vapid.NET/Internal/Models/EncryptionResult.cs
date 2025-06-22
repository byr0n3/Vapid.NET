using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Vapid.NET.Internal.Models
{
	[MustDisposeResource]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct EncryptionResult : System.IDisposable
	{
		public required RentedArray<byte> Salt { get; init; }

		public required RentedArray<byte> Payload { get; init; }

		public required RentedArray<byte> PublicKey { get; init; }

		public void Dispose()
		{
			this.Salt.Dispose();
			this.Payload.Dispose();
			this.PublicKey.Dispose();
		}
	}
}
