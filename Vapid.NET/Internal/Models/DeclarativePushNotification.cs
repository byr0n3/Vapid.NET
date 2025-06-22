using System.Runtime.InteropServices;
using Vapid.NET.Models;

namespace Vapid.NET.Internal.Models
{
	[StructLayout(LayoutKind.Sequential)]
	internal readonly struct DeclarativePushNotification
	{
		public required int WebPush { get; init; }

		public required PushNotification Notification { get; init; }
	}
}
