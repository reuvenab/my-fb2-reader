using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Normalize
{
    class Program
    {
        public static string ExecutableLocation = @"c:\temp\catalog";


        private static BinaryWriter TitlesFile()
        {
            return new BinaryWriter(
                File.Open(Path.Combine(ExecutableLocation, "titles.txt"), FileMode.Create)
                );
        }

        private static StreamWriter AuthorsIndexFile()
        {
            return new StreamWriter(Path.Combine(ExecutableLocation, "authors_index.txt"));
        }

        private static BinaryWriter AuthorsTitleFile()
        {
            return new BinaryWriter(
                File.Open(Path.Combine(ExecutableLocation, "authors.txt"), FileMode.Create)
                );
        }

        static void Main(string[] args)
        {
            var fileName = @"c:\temp\catalog\catalog.txt";
            if (args.Length != 0)
                fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File doesn't exist {0}", fileName);
                return ;
            }

            
            var start = DateTime.Now;

            int ind = 1;
            var authors = new Authors();
            
            using (var titles = TitlesFile())
            {
                var catalogReader =
                    new CatalogReaderAdapter(new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.UTF8));
                
                long prevPos = 0;
                
                foreach (var book in catalogReader)
                {
                    var titleBytes = Encoding.UTF8.GetBytes($"{book.Id};{book.Title}");

                    titles.Write((short)titleBytes.Length);
                    titles.Write(titleBytes);
                    titles.Write(Encoding.UTF8.GetBytes("\n"));
                    authors.AddBook(book.Author, prevPos);
                    prevPos = titles.BaseStream.Position;

                    
                }

            }

            var letterIndex = new SortedDictionary<char, long>();

            char letter = (char)0;
            using (var authorsBooks = AuthorsTitleFile())
            {
                foreach (var author in authors)
                {
                    if (author[0] != letter)
                    {
                        letter = author[0];
                        if (!letterIndex.ContainsKey(letter))
                        {
                            letterIndex.Add(letter, authorsBooks.BaseStream.Position);
                        }

                    }
                    var authorBytes = Encoding.UTF8.GetBytes(author);
                    authorsBooks.Write((short)authorBytes.Length);
                    authorsBooks.Write(authorBytes);
                    authorsBooks.Write(Encoding.UTF8.GetBytes("\n"));
                }
            }

            using (var authorsIndex = AuthorsIndexFile())
            {
                foreach (var letterPos in letterIndex)
                {
                    authorsIndex.Write($"{letterPos.Key};{letterPos.Value}\n");
                }
            }


            var end = DateTime.Now;


            Console.WriteLine("Time elapsed: {0}", end-start);
            Console.WriteLine("Books average: {0}, More than 4 books {1}", authors.AveragePerAuthor, authors.MoreThan4);

        }
    }
}
