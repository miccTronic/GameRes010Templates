using System.IO.Compression;
using System.Text;

namespace Asura;

public class AsuraDecompress
{
	public static void Decompress(string input, string output)
	{
		using var fs = File.OpenRead(input);
		var br = new BinaryReader(fs);
		var sig = br.ReadBytes(8);
		if (Encoding.ASCII.GetString(sig) != "AsuraZlb") throw new InvalidDataException("Not an Azura Zlib archive.");
		br.ReadInt32();
		var compressedSize = br.ReadInt32();
		var uncompressedSize = br.ReadInt32();
		int inputLen = (int)(fs.Length - fs.Position);
		if (inputLen < compressedSize) throw new InvalidDataException("Stream too short for specified compressed size");
		if (inputLen < 0) throw new InvalidDataException("Invalid AsuraZlb lengths.");
		using var zs = new ZLibStream(fs, CompressionMode.Decompress);
		var outBuf = new byte[uncompressedSize];
		int totalRead = 0;
		while (totalRead < uncompressedSize) {
			int read = zs.Read(outBuf, totalRead, uncompressedSize - totalRead);
			if (read <= 0) break;
			totalRead += read;
		}
		if (totalRead > uncompressedSize) {
			byte[] trimmed = new byte[uncompressedSize];
			Buffer.BlockCopy(outBuf, 0, trimmed, 0, uncompressedSize);
			outBuf = trimmed;
		}
		if (totalRead < uncompressedSize) throw new InvalidDataException("Could not decompress all bytes.");
		File.WriteAllBytes(output, outBuf);
	}
}
