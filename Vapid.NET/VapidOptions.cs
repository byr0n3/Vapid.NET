using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Vapid.NET
{
	[PublicAPI]
	public sealed class VapidOptions : IOptions<VapidOptions>
	{
		public string? Subject { get; set; }

		public string? PublicKey { get; set; }

		public string? PrivateKey { get; set; }

		public bool IsValid
		{
			[MemberNotNullWhen(true, nameof(this.Subject), nameof(this.PublicKey), nameof(this.PrivateKey))]
			get => (this.Subject is not null) && (this.PublicKey is not null) && (this.PrivateKey is not null);
		}

		VapidOptions IOptions<VapidOptions>.Value =>
			this;
	}
}
