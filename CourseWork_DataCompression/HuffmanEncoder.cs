using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CourseWork_DataCompression
{
	class HuffmanTreeNode
	{
		// Character name, such as 'A' etc.
		public char? Name = null;
		// Character frequency
		public int Frequency = 0;
		// Huffman code for character, such as 101 
		public List<bool> BinaryCode = new List<bool>();

		// Node child. Left edge is 0, right edge is 1
		public HuffmanTreeNode Left;
		public HuffmanTreeNode Right;
	}

	class HuffmanTree
	{
		// List of free nodes after preprocessing. Root node after building
		public List<HuffmanTreeNode> HuffmanTreeList = new List<HuffmanTreeNode>();
		// Huffman codes for encoding. After building tree and codes
		public Dictionary<char, List<bool>> HuffmanCodes = new Dictionary<char, List<bool>>();
		// Frequency table saved to compressed file
		public List<HuffmanTreeNode> FrequencyTable;

		public void Serialize(Stream OutStream)
		{
			BinaryWriter Writer = new BinaryWriter(OutStream);
			Writer.Write(FrequencyTable.Count);
			foreach (var Entry in FrequencyTable)
			{
				Writer.Write(Entry.Name.Value);
				Writer.Write(Entry.Frequency);
			}
			Writer.Flush();
		}

		public void Deserialize(Stream InStream)
		{
			BinaryReader Reader = new BinaryReader(InStream);
			int Count = Reader.ReadInt32();
			for (int n = 0; n < Count; n++)
			{
				var Name = Reader.ReadChar();
				var Frequency = Reader.ReadInt32();
				HuffmanTreeList.Add(new HuffmanTreeNode { Name = Name, Frequency = Frequency });
			}
		}

		public void BuildTree(byte[] Data, long DataSize)
		{
			for (long i = 0; i < DataSize; i++)
			{
				char NodeName = (char)Data[i];

				var TreeNode = HuffmanTreeList.Find(Node => Node.Name == NodeName);
				if (TreeNode != null)
					TreeNode.Frequency++;
				else
					HuffmanTreeList.Add(new HuffmanTreeNode { Name = NodeName, Frequency = 1 });
			}

			FrequencyTable = new List<HuffmanTreeNode>(HuffmanTreeList);

			BuildTree();
		}

		public void BuildTree()
		{
			while (HuffmanTreeList.Count > 1)
			{
				HuffmanTreeList = HuffmanTreeList.OrderBy(Node => Node.Frequency).ToList();

				var Node1 = HuffmanTreeList.ElementAt(0);
				var Node2 = HuffmanTreeList.ElementAt(1);

				HuffmanTreeList.RemoveRange(0, 2);
				HuffmanTreeList.Insert(0, new HuffmanTreeNode
				{
					Name = null,
					Frequency = Node1.Frequency + Node2.Frequency,
					Left = Node1,
					Right = Node2
				});
			}
		}

		public void BuildCodes()
		{
			HuffmanCodes.Clear();
			HuffmanTreeNode Root = HuffmanTreeList.First();
			BuildBinaryCodesInternal(Root, Root.BinaryCode);
		}

		private void BuildBinaryCodesInternal(HuffmanTreeNode Node, List<bool> Code)
		{
			Node.BinaryCode = new List<bool>(Code);
			if (Node.Name != null)
				HuffmanCodes.Add(Node.Name.Value, Node.BinaryCode);

			if (Node.Left != null)
			{
				var LeftList = new List<bool>(Code);
				LeftList.Add(false);
				BuildBinaryCodesInternal(Node.Left, LeftList);
			}
			if (Node.Right != null)
			{
				var RightList = new List<bool>(Code);
				RightList.Add(true);
				BuildBinaryCodesInternal(Node.Right, RightList);
			}
		}

		public BitArray Encode(byte[] Data, long DataSize)
		{
			if (DataSize != 0 && Data != null)
			{
				List<bool> Encoded = new List<bool>();
				Encoded.AddRange(Data.SelectMany(character => HuffmanCodes[(char)character]));
				return new BitArray(Encoded.ToArray());
			}
			else
				return null;
		}

		public long Decode(BitArray EncodedData, byte[] DecodedData)
		{
			long WrittenData = 0;

			HuffmanTreeNode CurrentNode = HuffmanTreeList.First();

			foreach (bool EncodedByte in EncodedData)
			{
				CurrentNode = EncodedByte ? CurrentNode.Right ?? CurrentNode : CurrentNode.Left ?? CurrentNode;

				if (CurrentNode.Name != null)
				{
					DecodedData[WrittenData] = (byte)CurrentNode.Name.Value;
					WrittenData++;
					CurrentNode = HuffmanTreeList.First();
				}
			}

			return WrittenData - 1;
		}
	}

	class HuffmanEncoder : IEncoder
	{
		// Header tag for compressed files
		public byte[] Header = new byte[4] { (byte)'C', (byte)'W', (byte)'D', (byte)'C' };
		//public string Header = "CWDC" ;
		// Read header for determining run mode
		public byte[] ReadHeader = new byte[4];

		// Huffman tree used for encoding
		public HuffmanTree Tree = new HuffmanTree();
		// Data to be encoded
		public MemoryStream Data = new MemoryStream();

		public void CheckHeader(Stream InStream)
		{
			BinaryReader Reader = new BinaryReader(InStream);
			//ReadHeader = new string(Reader.ReadChars(4));
			ReadHeader = Reader.ReadBytes(4);
			InStream.Position = 0;
		}

		public void SerializeHeader(Stream OutStream)
		{
			BinaryWriter Writer = new BinaryWriter(OutStream);
			//Writer.Write(Header.ToCharArray());
			Writer.Write(Header);
			Tree.Serialize(OutStream);
		}

		public void DeserializeHeader(Stream InStream)
		{
			BinaryReader Reader = new BinaryReader(InStream);
			//ReadHeader = new string(Reader.ReadChars(4));
			ReadHeader = Reader.ReadBytes(4);
			Tree.Deserialize(InStream);
		}

		public void PreprocessCompression(Stream InputStream)
		{
			InputStream.CopyTo(Data);
			Data.Capacity = (int)Data.Length;

			Tree.BuildTree(Data.GetBuffer(), Data.Length);
			Tree.BuildCodes();
		}

		public void PreprocessDecompression(Stream InputStream)
		{
			InputStream.CopyTo(Data);
			Data.Capacity = (int)Data.Length;

			Tree.BuildTree();
		}

		public void Compress(Stream OutputStream, ref int OutDataSize)
		{
			BitArray BinaryData = Tree.Encode(Data.GetBuffer(), Data.Length);

			OutDataSize = (BinaryData.Length / 8) + 1;
			byte[] OutData = new byte[OutDataSize];
			BinaryData.CopyTo(OutData, 0);

			OutputStream.Write(OutData, 0, OutDataSize);	
		}

		public void Decompress(Stream OutputStream, ref int OutDataSize)
		{
			BitArray EncodedData = new BitArray(Data.GetBuffer());

			byte[] DecodedData = new byte[Data.Length * 8];
			long DecodedDataSize = Tree.Decode(EncodedData, DecodedData);

			OutputStream.Write(DecodedData, 0, (int)DecodedDataSize);
		}
	}
}
