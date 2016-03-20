# HuffmanCoding
Implementation of Huffman coding algorithm with C#

Implemented as part of my course work on data compression algorithms. I decided to try a new language and chose C#, which I never touched before this project.
Here I have first working version. It is a command line program which is able to compress and decompress files. It is not optimized in any way, since I'm still learning C# and not aware of its best practices. I plan to continue work on this project as I have time to do so. This might and does have bugs and lacks usability. But it still could be starting point for those interested in the subject. The code isn't commented well, but it's pretty easy to understand.

My plans for the project are:
- Comment important code parts
- Increase usability
- Introduce modes for initial algorithm (static Huffman encoding): either read whole file once and process it (IO operations saving) or read file by blocks (memory savings)
- Add few other data compression algorithms such as adaptive Huffman, Shannon and other codings

# How to use
Program has few command line arguments
-i <file_name> - input file name.
-o <file_name> - output file name.
-m <compression|decompression> - mode of a program run. What you want to do with file, compress or decompress.

You also can drop file on the program and it will determine what to do with it
