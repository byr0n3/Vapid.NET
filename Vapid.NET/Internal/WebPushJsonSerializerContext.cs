using System.Text.Json.Serialization;
using Vapid.NET.Internal.Models;
using Vapid.NET.Models;

namespace Vapid.NET.Internal
{
	[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata,
								 PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
								 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonSerializable(typeof(PushNotification))]
	[JsonSerializable(typeof(JwtInfo))]
	[JsonSerializable(typeof(JwtData))]
	[JsonSerializable(typeof(DeclarativePushNotification))]
	internal sealed partial class WebPushJsonSerializerContext : JsonSerializerContext;
}
