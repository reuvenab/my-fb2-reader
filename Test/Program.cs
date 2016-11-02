using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {


        private static bool Eof(BinaryReader reader)
        {
            return (reader.BaseStream.Position == reader.BaseStream.Length);
        }

        static void Main(string[] args)
        {
            var fileName = @"c:\temp\catalog\catalog.txt";
            var fileSize = new FileInfo(fileName).Length;

            var buffer = new byte[4096];

            var authors = new Dictionary<char, int>();

            using (var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open))
            {
                using (var accessor = mmf.CreateViewAccessor(0, fileSize))
                {
                    long titleStart = 0;
                    int j = 0;
                    for (long i = 0; i < fileSize; ++i, ++j)
                    {
                        buffer[j] = accessor.ReadByte(i);
                        if (buffer[j] != 0xA) continue;
                        var title = Encoding.UTF8.GetString(buffer, 0, j);
                        j = -1;
                        int value = 1;
                        if (authors.TryGetValue(title[0], out value))
                        {
                            ++value;
                        }
                        authors[title[0]] = value;

                    }
                }
            }


        }
    }
}
