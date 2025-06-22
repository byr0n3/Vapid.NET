using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Vapid.NET.Models
{
	[PublicAPI]
	public enum PushNotificationUrgency
	{
		Normal,
		VeryLow,
		Low,
		High,
	}

	internal static class PushNotificationUrgencyExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Str(this PushNotificationUrgency value) =>
			(value) switch
			{
				PushNotificationUrgency.Normal  => "normal",
				PushNotificationUrgency.VeryLow => "very-low",
				PushNotificationUrgency.Low     => "low",
				PushNotificationUrgency.High    => "high",
				_                               => throw new System.ArgumentOutOfRangeException(nameof(value), value, null),
			};
	}
}
