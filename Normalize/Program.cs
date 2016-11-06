using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.IO.Compression;


namespace Test
{
    class Program
    {
        private static readonly string ExecutableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string CatalogZipFile = "catalog.zip";
        private static readonly string CatalogTxtFile = "catalog.txt";


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
            DownloadFile();

            var fileName = Path.Combine(ExecutableLocation, CatalogTxtFile);
            
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

                        WriteAuthorIndex(letterIndexWriter, book.Author[0], 0);

                        var authorTitles = $"{book.Author};{titlesWriter.BaseStream.Position}";
                        WriteBook(titlesWriter, book);

                        for (int i = 1; i < catalog.Books.Count; i++)
                        {
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
                        }
                        WriteAuthorIndex(letterIndexWriter, prevAuthor[0], authorTitlesWriter.BaseStream.Position);
                        Console.WriteLine($"Procesing elapsed: {DateTime.Now - startTime}");
                    }
                }
            }


        }

        private static void DownloadFile()
        {
            var url = $"http://flibusta.is/catalog/{CatalogZipFile}";
            var fileName = Path.Combine(ExecutableLocation, CatalogZipFile);

            var startTime = DateTime.Now;
            Console.WriteLine($"Download started at: {DateTime.Now}, {url}");
            var client = new WebClient();
            client.DownloadFile(new Uri(url), fileName);
            Console.WriteLine($"Download finished : {DateTime.Now - startTime}");

            startTime = DateTime.Now;
            Console.WriteLine($"Unzip started at: {DateTime.Now}, {fileName}");
            
            ZipFile.ExtractToDirectory(fileName, ExecutableLocation);
            Console.WriteLine($"Unzip finished: {DateTime.Now - startTime}");
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




