using System.IO;

namespace CourseWork_DataCompression
{
	public interface IEncoder
	{
		void CheckHeader(Stream InStream);
		void SerializeHeader(Stream OutStream);
		void DeserializeHeader(Stream InStream);

		void PreprocessCompression(Stream InputStream);
		void PreprocessDecompression(Stream InputStream);
		void Compress(Stream OutputStream, ref int OutDataSize);
		void Decompress(Stream OutputStream, ref int OutDataSize);
	}
}
