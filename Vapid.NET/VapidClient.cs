using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vapid.NET.Internal;
using Vapid.NET.Internal.Models;
using Vapid.NET.Models;

namespace Vapid.NET
{
	[PublicAPI]
	public sealed class VapidClient : System.IDisposable
	{
		private static readonly MediaTypeHeaderValue contentType = new("application/octet-stream");

		private readonly HttpClient client;
		private readonly VapidOptions options;
		private readonly ILogger<VapidClient> logger;

		public VapidClient(HttpClient client, IOptions<VapidOptions> options, ILogger<VapidClient> logger)
		{
			this.client = client;
			this.logger = logger;
			this.options = options.Value;

			this.client.Timeout = System.TimeSpan.FromSeconds(10);
		}

		public async Task<bool> SendAsync(PushSubscription subscription, PushNotification notification, CancellationToken token = default)
		{
			const int defaultTtl = 2419200;

			Debug.Assert(this.options.IsValid);

			var jwtToken = Jwt.GetSignedToken(subscription.Endpoint, this.options);

			var declarativeNotification = new DeclarativePushNotification
			{
				// Ref: https://datatracker.ietf.org/doc/html/rfc8030
				WebPush = 8030,
				Notification = notification,
			};

			var payload = Encryption.Encrypt(subscription, declarativeNotification, this.options);

			var content = new ByteArrayContent(payload.Payload, 0, payload.Payload.Length);
			{
				content.Headers.ContentType = VapidClient.contentType;
				content.Headers.ContentLength = payload.Payload.Length;
				content.Headers.ContentEncoding.Add("aesgcm");
			}

			var ttl = notification.Ttl != default ? ((int)notification.Ttl.TotalSeconds) : defaultTtl;

			var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);
			{
				request.Headers.TryAddWithoutValidation("Authorization", $"WebPush {jwtToken}");

				request.Headers.TryAddWithoutValidation("TTL", ttl.ToString(NumberFormatInfo.InvariantInfo));
				request.Headers.TryAddWithoutValidation("Urgency", notification.Urgency.Str());

				if (!string.IsNullOrEmpty(notification.Topic))
				{
					request.Headers.TryAddWithoutValidation("Topic", notification.Topic);
				}

				request.Content = content;
				request.Headers.TryAddWithoutValidation("Encryption", VapidClient.GetEncryptionHeaderValue(payload.Salt));
				request.Headers.TryAddWithoutValidation("Crypto-Key", VapidClient.GetCryptoKey(payload.PublicKey, this.options.PublicKey));
			}

			var response = await this.client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

			using (payload)
			using (request)
			using (response)
			{
				if (!response.IsSuccessStatusCode)
				{
					var msg = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

					this.logger.LogError("Error sending push notification via Vapid: {Msg}", msg);
				}

				return response.IsSuccessStatusCode;
			}
		}

		private static string GetEncryptionHeaderValue(RentedArray<byte> salt)
		{
			const string prefix = "salt=";

			System.Span<char> saltBase64 = stackalloc char[Base64.GetMaxEncodedToUtf8Length(salt.Length)];

			var written = UrlSafeBase64.Encode(salt, saltBase64);

			saltBase64 = saltBase64.Slice(0, written);

			return string.Create(
				null,
				stackalloc char[prefix.Length + saltBase64.Length],
				$"{prefix}{saltBase64}"
			);
		}

		private static string GetCryptoKey(RentedArray<byte> payloadPublicKey, string serverPublicKey)
		{
			const string payloadPrefix = "dh=";
			const string serverPrefix = "p256ecdsa=";

			System.Span<char> payloadBase64 = stackalloc char[Base64.GetMaxEncodedToUtf8Length(payloadPublicKey.Length)];

			var written = UrlSafeBase64.Encode(payloadPublicKey, payloadBase64);

			payloadBase64 = payloadBase64.Slice(0, written);

			return string.Create(
				null,
				stackalloc char[payloadPrefix.Length + payloadBase64.Length + 1 + serverPrefix.Length + serverPublicKey.Length],
				$"{payloadPrefix}{payloadBase64};{serverPrefix}{serverPublicKey}"
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() =>
			this.client.Dispose();
	}
}
