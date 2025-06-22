using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Vapid.NET.Internal
{
	[StructLayout(LayoutKind.Sequential)]
	internal ref struct ByteBuilder
	{
		private readonly System.Span<byte> buffer;
		private int position;

		public readonly System.ReadOnlySpan<byte> Result =>
			this.buffer.Slice(0, this.position);

		public ByteBuilder(System.Span<byte> buffer)
		{
			this.buffer = buffer;
			this.position = 0;
		}

		[Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly void AssertAvailable(int length) =>
			Debug.Assert(this.position + length <= this.buffer.Length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly System.Span<byte> Take(int length = 0) =>
			this.buffer.Slice(this.position, length > 0 ? length : (this.buffer.Length - this.position));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Move(int length) =>
			this.position += length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(byte value)
		{
			this.AssertAvailable(1);

			this.buffer[this.position++] = value;
		}

		public void Append(scoped System.ReadOnlySpan<byte> value)
		{
			this.AssertAvailable(value.Length);

			if (value.TryCopyTo(this.Take(value.Length)))
			{
				this.Move(value.Length);
			}
		}

		public void Append<T>(T value) where T : INumber<T>
		{
			var length = Unsafe.SizeOf<T>();

			this.AssertAvailable(length);

			var dst = this.Take(length);

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(dst), value);

			if (System.BitConverter.IsLittleEndian)
			{
				System.MemoryExtensions.Reverse(dst);
			}

			this.Move(length);
		}

		public void AppendUtf8<T>(T value) where T : INumber<T>
		{
			var length = Unsafe.SizeOf<T>();

			this.AssertAvailable(length);

			if (value.TryFormat(this.Take(length), out var written, default, NumberFormatInfo.InvariantInfo))
			{
				this.Move(written);
			}
		}

		public void AppendUtf8(scoped System.ReadOnlySpan<char> value)
		{
			var length = Encoding.UTF8.GetMaxByteCount(value.Length);

			this.AssertAvailable(length);

			if (Encoding.UTF8.TryGetBytes(value, this.Take(length), out var written))
			{
				this.Move(written);
			}
		}

		public void AppendUrlSafeBase64(scoped System.ReadOnlySpan<byte> value)
		{
			var maxLength = Base64.GetMaxEncodedToUtf8Length(value.Length);

			this.AssertAvailable(maxLength);

			var written = UrlSafeBase64.Encode(value, this.Take(maxLength));

			this.Move(written);
		}
	}
}
