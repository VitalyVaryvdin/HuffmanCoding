using System;
using System.Linq;
using System.IO;

// Program run mode. Decides what it should do with input file. Set withing command line, if not, program will read file and guess the mode
enum RunMode
{
	Default,
    Compression,
    Decompression
}

namespace CourseWork_DataCompression
{
    class Program
    {
        static string InputFileName = "";
        static string OutputFileName = "";
        static RunMode Mode = RunMode.Default;

		static string FormatBytes(long Bytes)
		{
			const int Scale = 1024;
			string[] Orders = new string[] { "GiB", "MiB", "KiB", "Bytes" };
			long Max = (long)Math.Pow(Scale, Orders.Length - 1);

			foreach (string Order in Orders)
			{
				if (Bytes > Max)
					return string.Format("{0:##.##} {1}", decimal.Divide(Bytes, Max), Order);

				Max /= Scale;
			}
			return "0 bytes";
		}

		static void ParseCommandLine(string[] Args)
        {
			if(Args.Length > 1)
			{
				for (int i = 0; i < Args.Length; i++)
				{
					if (Args[i] == "-i")
					{
						InputFileName = Args[i + 1];
					}
					else if (Args[i] == "-o")
					{
						OutputFileName = Args[i + 1];
					}
					else if (Args[i] == "-m")
					{
						if (Args[i + 1] == "compression")
							Mode = RunMode.Compression;
						else if (Args[i + 1] == "decompression")
							Mode = RunMode.Decompression;
					}
				}
			}
			else if(Args.Length == 1) // Only one argument - file was dropped onto executable
			{
				InputFileName = Args.First();
			}

			// Input file name specified but output isn't. Either file was dropped onto executable or argument is missing from command line arguments
			if (InputFileName != "" && OutputFileName == "")
			{
				int IndexCompressed = InputFileName.IndexOf(".cwdc");

				OutputFileName = (IndexCompressed < 0) ? InputFileName + ".cwdc" : InputFileName.Remove(IndexCompressed, ".cwdc".Length);
			}
		}

        static void Main(string[] args)
        {
            ParseCommandLine(args);

			FileStream InputFileStream = new FileStream(InputFileName, FileMode.Open, FileAccess.Read);
			FileStream OutputFileStream = new FileStream(OutputFileName, FileMode.Create, FileAccess.Write);

			HuffmanEncoder Encoder = new HuffmanEncoder();

			int OutputSize = 0;

			if(Mode == RunMode.Compression)
			{
				Encoder.PreprocessCompression(InputFileStream);
				Encoder.SerializeHeader(OutputFileStream);
				Encoder.Compress(OutputFileStream, ref OutputSize);
			}
			else if(Mode == RunMode.Decompression)
			{
				Encoder.DeserializeHeader(InputFileStream);
				Encoder.PreprocessDecompression(InputFileStream);
				Encoder.Decompress(OutputFileStream, ref OutputSize);
			}
			else if(Mode == RunMode.Default)
			{
				Encoder.CheckHeader(InputFileStream);

				if(Encoder.ReadHeader.SequenceEqual(Encoder.Header))
				{
					Encoder.DeserializeHeader(InputFileStream);
					Encoder.PreprocessDecompression(InputFileStream);
					Encoder.Decompress(OutputFileStream, ref OutputSize);
				}
				else
				{
					Encoder.PreprocessCompression(InputFileStream);
					Encoder.SerializeHeader(OutputFileStream);
					Encoder.Compress(OutputFileStream, ref OutputSize);
				}
			}
		}
    }
}
