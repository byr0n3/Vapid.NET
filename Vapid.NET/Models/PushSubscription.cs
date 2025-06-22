using JetBrains.Annotations;

namespace Vapid.NET.Models
{
	[PublicAPI]
	public readonly struct PushSubscription
	{
		/// <summary>
		/// The endpoint to send the push notification to.
		/// </summary>
		public required string Endpoint { get; init; }

		/// <summary>
		/// The client's 'public key' for this subscription.
		/// </summary>
		public required string P256dh { get; init; }

		/// <summary>
		/// The client's 'private key' for this subscription.
		/// </summary>
		public required string Auth { get; init; }
	}
}
