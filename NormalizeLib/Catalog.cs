using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;

namespace CatalogLib
{
    public static class Catalog
    {
        public static string TitlesFileName { get; } = "titles.txt";
        public static string LetterIndexFileName { get; } = "letter_index.txt";
        public static string AuthorsFileName { get; } = "authors.txt";

        private static BinaryWriter TitlesFile(string executableLocation)
        {
            return new BinaryWriter(
                File.Open(Path.Combine(executableLocation, TitlesFileName), FileMode.Create)
                );
        }

        private static StreamWriter LetterIndexFile(string executableLocation)
        {
            return new StreamWriter(
                File.Open(Path.Combine(executableLocation, LetterIndexFileName), FileMode.Create)
                );
        }

        private static BinaryWriter AuthorTitlesFile(string executableLocation)
        {
            return new BinaryWriter(
                File.Open(Path.Combine(executableLocation, AuthorsFileName), FileMode.Create)
                );
        }

        private static void WriteBook(BinaryWriter writer, CatalogReaderAdapter.Book book)
        {
            var titleBytes = Encoding.UTF8.GetBytes($"{book.Id};{book.Title}");

            writer.Write((short) titleBytes.Length);
            writer.Write(titleBytes);
            writer.Write(Encoding.UTF8.GetBytes("\n"));
        }

        private static void WriteAuthorTitles(BinaryWriter writer, string authorTitles)
        {
            var titleBytes = Encoding.UTF8.GetBytes(authorTitles);

            writer.Write((short) titleBytes.Length);
            writer.Write(titleBytes);
            writer.Write(Encoding.UTF8.GetBytes("\n"));
        }

        private static void WriteAuthorIndex(TextWriter writer, char c, long position)
        {
            writer.Write($"{c};{position};{(uint) c}\n");
        }

        private static string DownloadCatalogFile(string executableLocation)
        {
            var catalogZipFile = "catalog.zip";

            var url = $"http://flibusta.is/catalog/{catalogZipFile}";
            var fileName = Path.Combine(executableLocation, catalogZipFile);

            var startTime = DateTime.Now;
            Debug.WriteLine($"Download started at: {DateTime.Now}, {url}");
            var client = new WebClient();
            client.DownloadFile(new Uri(url), fileName);
            Debug.WriteLine($"Download finished : {DateTime.Now - startTime}");

            startTime = DateTime.Now;
            Debug.WriteLine($"Unzip started at: {DateTime.Now}, {fileName}");
            ZipFile.ExtractToDirectory(fileName, executableLocation);
            Debug.WriteLine($"Unzip finished: {DateTime.Now - startTime}");
            return "catalog.txt";
        }

        public static void Check(string location)
        {
            var files = new[] { AuthorsFileName, TitlesFileName, LetterIndexFileName };
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    CreateCatalog(location);
                    break;
                }
            }
        }


        static void CreateCatalog(string executableLocation)
        {
            var fileName = DownloadCatalogFile(executableLocation);

            var catalog = new CatalogReaderAdapter(fileName);

            var startTime = DateTime.Now;

            using (TextWriter letterIndexWriter = LetterIndexFile(executableLocation))
            {
                using (var authorTitlesWriter = AuthorTitlesFile(executableLocation))
                {
                    using (var titlesWriter = TitlesFile(executableLocation))
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
                                WriteAuthorIndex(letterIndexWriter, book.Author[0],
                                    authorTitlesWriter.BaseStream.Position);
                            }

                            authorTitles = $"{book.Author};{curTitlePos}";
                            prevAuthor = book.Author;
                        }
                        WriteAuthorIndex(letterIndexWriter, prevAuthor[0], authorTitlesWriter.BaseStream.Position);
                        Debug.WriteLine($"Procesing elapsed: {DateTime.Now - startTime}");
                    }
                }
            }
        }

    }
}
