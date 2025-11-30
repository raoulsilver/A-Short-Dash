using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;//

namespace DoodleStudio95 {

internal static class CompressionTools
{
	// bytes -> gzipped bytes
	internal static byte[] Compress(byte[] bSource) {
		if (bSource == null)
			return null;

		if (bSource.Length == 0)
			return new byte[0];

		using (var ms = new MemoryStream()) {
			using (var gzip = new GZipStream(ms, CompressionMode.Compress, true)) {
				gzip.Write(bSource, 0, bSource.Length);
				gzip.Close();
			}
			return ms.ToArray();
		}
	}

	// gzipped bytes -> bytes
	const int BUF_SIZE = 4096 * 2;
	static byte[] buffer = new byte[BUF_SIZE];
    internal static byte[] Decompress(byte[] bSource) {
		using (var instream = new MemoryStream(bSource)) {
			using (var gzip = new GZipStream(instream, CompressionMode.Decompress)) {
				using (var outstream = new MemoryStream()) {
					while (true) {
						int delta = gzip.Read(buffer, 0, buffer.Length);

						if (delta > 0)
							outstream.Write(buffer, 0, delta);

						if (delta < BUF_SIZE)
							break;
					}
					return outstream.ToArray();
				}
			}
		}
    }

	internal static void ByteArrayToColor32Array(byte[] bytes, ref Color32[] colors)
	{
		System.Array.Resize(ref colors, bytes.Length / 4);
		var j = 0;
		for (int i = 0; i < bytes.Length; i += 4) {
			colors[j++] = new Color32(bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]);
		}
	}

	internal static byte[] Color32ArrayToByteArray(Color32[] colors)
	{
		if (colors == null || colors.Length == 0)
			return null;

		int length = Marshal.SizeOf(typeof(Color32)) * colors.Length;
		var bytes = new byte[length];
		var handle = default(GCHandle);
		try {
			handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
			Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, length);
		} finally {
			if (handle != default(GCHandle))
			handle.Free();
		}

		return bytes;
	}
}

}