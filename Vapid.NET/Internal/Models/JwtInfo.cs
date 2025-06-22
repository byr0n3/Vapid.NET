using System.Text.Json.Serialization;

namespace Vapid.NET.Internal.Models
{
	internal readonly struct JwtInfo
	{
		[JsonPropertyName("typ")] public required string Type { get; init; }

		[JsonPropertyName("alg")] public required string Algorithm { get; init; }
	}
}
