using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;


namespace Test
{
    class Program
    {
        public static string ExecutableLocation = @"c:\temp\catalog";

        private static bool Eof(BinaryReader reader)
        {
            return (reader.BaseStream.Position == reader.BaseStream.Length);
        }

        private static BinaryWriter TitlesFile()
        {
            return new BinaryWriter(
                File.Open(Path.Combine(ExecutableLocation, "titles.txt"), FileMode.Create)
                );
        }


        private static StreamWriter LetterIndexFile()
        {
            return new StreamWriter(
                File.Open(Path.Combine(ExecutableLocation, "letter_index.txt"), FileMode.Create)
                );
        }

        private static BinaryWriter AuthorTitlesFile()
        {
            return new BinaryWriter(
                File.Open(Path.Combine(ExecutableLocation, "authors.txt"), FileMode.Create)
                );
            
        }


        static void WriteBook(BinaryWriter writer, CatalogReaderAdapter.Book book)
        {
            var titleBytes = Encoding.UTF8.GetBytes($"{book.Id};{book.Title}");

            writer.Write((short) titleBytes.Length);
            writer.Write(titleBytes);
            writer.Write(Encoding.UTF8.GetBytes("\n"));
        }

        static void Main(string[] args)
        {
            var fileName = @"c:\temp\catalog\catalog.txt";
            var fileSize = new FileInfo(fileName).Length;

            var buffer = new byte[4096];

            var authors = new Dictionary<char, int>();
            
            var catalog = new CatalogReaderAdapter(fileName);
            
            var startTime = DateTime.Now;

            using (TextWriter letterIndexWriter = LetterIndexFile())
            {
                using (var authorTitlesWriter = AuthorTitlesFile())
                {
                    using (var titlesWriter = TitlesFile())
                    {
                        
                        var book = catalog.Books[0];
                        var prevAuthor = book.Author;

                        long curAuthorPos = 0;
                        WriteAuthorIndex(letterIndexWriter, book.Author[0], 0);

                        var authorTitles = $"{book.Author};{titlesWriter.BaseStream.Position}";
                        WriteBook(titlesWriter, book);

                        var niter = 10000;
                        for (int i = 1; i < catalog.Books.Count; i++)
                        {
                            //if (niter-- == 0)
                            //    break;
                            book = catalog.Books[i];
                            var curTitlePos = titlesWriter.BaseStream.Position;
                            WriteBook(titlesWriter, book);

                            if (prevAuthor == book.Author)
                            {
                                authorTitles += $";{curTitlePos}";
                                continue;
                            }

                            WriteAuthorTitles(authorTitlesWriter, authorTitles);

                            if (prevAuthor[0] != book.Author[0])
                            {
                                WriteAuthorIndex(letterIndexWriter, book.Author[0], authorTitlesWriter.BaseStream.Position);
                            }

                            authorTitles = $"{book.Author};{curTitlePos}";
                            prevAuthor = book.Author;
                            //authorStartPos = titlesWriter.BaseStream.Position;
                        }
                        WriteAuthorIndex(letterIndexWriter, prevAuthor[0], authorTitlesWriter.BaseStream.Position);
                        Console.WriteLine($"Procesing elapsed: {DateTime.Now - startTime}");
                    }
                }
            }


        }

        private static void WriteAuthorTitles(BinaryWriter writer, string authorTitles)
        {
            var titleBytes = Encoding.UTF8.GetBytes(authorTitles);

            writer.Write((short)titleBytes.Length);
            writer.Write(titleBytes);
            writer.Write(Encoding.UTF8.GetBytes("\n"));
        }

        private static void WriteAuthorIndex(TextWriter writer, char c, long position)
        {
            writer.Write($"{c};{position};{(uint)c}\n");
        }
    }
}




//var catalogIter = catalog.GetEnumerator();
//catalogIter.MoveNext();
//                        var book = catalogIter.Current;
//var prevAuthor = book.Author;
//var authorStartPos = book.Position;
//var authorTitles = $"{book.Author};{book.Id}";

//                        WriteAuthorIndex(letterIndexWriter, book.Author[0], authorStartPos);
//                        WriteBook(titlesWriter, book);

//var niter = 10000;
//                        while (catalogIter.MoveNext())
//                        {
//                            //if (niter-- == 0)
//                            //    break;
//                            book = catalogIter.Current;
//                            WriteBook(titlesWriter, book);

//                            if (prevAuthor != book.Author)
//                            {
//                                if (prevAuthor[0] != book.Author[0])
//                                {
//                                    if ((int) book.Author[0] - (int) prevAuthor[0] != 1)
//                                        continue; 
//                                    WriteAuthorIndex(letterIndexWriter, prevAuthor[0], authorStartPos);
//                                }
//                                WriteAuthorTitles(authorTitlesWriter, authorTitles);

//authorTitles = $"{book.Author};{book.Id}";
//                                prevAuthor = book.Author;
//                                authorStartPos = book.Position;
//                            }
//                            else
//                            {
//                                authorTitles += $";{book.Id}";
//                            }
//                        }
