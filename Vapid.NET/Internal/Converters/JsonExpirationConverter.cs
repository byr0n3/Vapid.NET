using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vapid.NET.Internal.Converters
{
	internal sealed class JsonExpirationConverter : JsonConverter<System.DateTime>
	{
		public override System.DateTime Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options) =>
			throw new System.NotSupportedException();

		public override void Write(Utf8JsonWriter writer, System.DateTime value, JsonSerializerOptions options)
		{
			var secondsSinceEpoc = (int)((value - System.DateTime.UnixEpoch).TotalSeconds);

			writer.WriteNumberValue(secondsSinceEpoc);
		}
	}
}
