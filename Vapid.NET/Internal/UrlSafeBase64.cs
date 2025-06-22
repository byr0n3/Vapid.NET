using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vapid.NET.Internal
{
	internal static class UrlSafeBase64
	{
		public static byte[] Decode(scoped System.ReadOnlySpan<char> src)
		{
			System.Span<byte> dst = stackalloc byte[128];

			var written = UrlSafeBase64.Decode(src, dst);

			return dst.Slice(0, written).ToArray();
		}

		public static int Decode(scoped System.ReadOnlySpan<char> src, scoped System.Span<byte> dst)
		{
			var suffix = "="u8;

			var length = src.Length;

			while (length % 4 != 0)
			{
				length++;
			}

			System.Span<byte> temp = stackalloc byte[Encoding.UTF8.GetMaxByteCount(length)];

			var copied = Encoding.UTF8.TryGetBytes(src, temp, out var written);

			Debug.Assert(copied);

			UrlSafeBase64.Replace(temp, "-"u8, "+"u8);
			UrlSafeBase64.Replace(temp, "_"u8, "/"u8);

			for (var i = 0; i < (length - src.Length); i++)
			{
				copied = suffix.TryCopyTo(temp.Slice(src.Length + i));

				Debug.Assert(copied);

				written += suffix.Length;
			}

			temp = temp.Slice(0, written);

			var status = Base64.DecodeFromUtf8(temp, dst, out _, out written);

			Debug.Assert(status == OperationStatus.Done);

			return written;
		}

		public static int Encode(scoped System.ReadOnlySpan<byte> src, scoped System.Span<byte> dst)
		{
			var status = Base64.EncodeToUtf8(src, dst, out _, out var written);

			Debug.Assert(status == OperationStatus.Done);

			dst.Slice(0, written);

			UrlSafeBase64.Replace(dst, "+"u8, "-"u8);
			UrlSafeBase64.Replace(dst, "/"u8, "_"u8);

			dst = System.MemoryExtensions.TrimEnd(dst, "="u8);

			return dst.Length;
		}

		public static int Encode(scoped System.ReadOnlySpan<byte> src, scoped System.Span<char> dst)
		{
			System.Span<byte> temp = stackalloc byte[Base64.GetMaxEncodedToUtf8Length(src.Length)];

			var written = UrlSafeBase64.Encode(src, temp);

			temp = temp.Slice(0, written);

			var copied = Encoding.UTF8.TryGetChars(temp, dst, out written);

			Debug.Assert(copied);

			return written;
		}

		private static void Replace(scoped System.Span<byte> dst,
									scoped System.ReadOnlySpan<byte> find,
									scoped System.ReadOnlySpan<byte> replace)
		{
			Debug.Assert(find.Length == replace.Length);

			int idx;

			while ((idx = System.MemoryExtensions.IndexOf(dst, find)) != -1)
			{
				replace.TryCopyTo(dst.Slice(idx));
			}
		}
	}
}
