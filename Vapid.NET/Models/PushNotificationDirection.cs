using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Vapid.NET.Models
{
	[PublicAPI]
	[JsonConverter(typeof(JsonPushNotificationDirectionConverter))]
	public enum PushNotificationDirection
	{
		Auto,
		LeftToRight,
		RightToLeft,
	}

	internal sealed class JsonPushNotificationDirectionConverter : JsonConverter<PushNotificationDirection>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override PushNotificationDirection Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options) =>
			throw new System.NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Write(Utf8JsonWriter writer, PushNotificationDirection value, JsonSerializerOptions options)
		{
			var str = (value) switch
			{
				PushNotificationDirection.Auto        => "auto",
				PushNotificationDirection.LeftToRight => "ltr",
				PushNotificationDirection.RightToLeft => "rtl",
				_                                     => throw new System.ArgumentOutOfRangeException(nameof(value), value, null),
			};

			writer.WriteStringValue(str);
		}
	}
}
