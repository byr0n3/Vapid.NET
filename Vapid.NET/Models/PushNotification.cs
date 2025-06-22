using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Vapid.NET.Models
{
	[PublicAPI]
	public readonly struct PushNotification
	{
		public required string Title { get; init; }

		public required string Body { get; init; }

		public required string Navigate { get; init; }

		[JsonPropertyName("lang")] public string? Language { get; init; }

		[JsonPropertyName("dir")] public PushNotificationDirection Direction { get; init; }

		public bool Silent { get; init; }

		[JsonPropertyName("app_badge")] public int BadgeCount { get; init; }

		public string? Topic { get; init; }

		[JsonIgnore] public PushNotificationUrgency Urgency { get; init; }

		[JsonIgnore] public System.TimeSpan Ttl { get; init; }
	}
}
