using System.Text.Json.Serialization;
using Vapid.NET.Internal.Converters;

namespace Vapid.NET.Internal.Models
{
	internal readonly struct JwtData
	{
		[JsonPropertyName("aud")] public required string Audience { get; init; }

		[JsonPropertyName("exp")]
		[JsonConverter(typeof(JsonExpirationConverter))]
		public required System.DateTime Expiration { get; init; }

		[JsonPropertyName("sub")] public required string Subject { get; init; }
	}
}
