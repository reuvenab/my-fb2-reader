using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace CatalogLib
{
    public class CatalogReaderAdapter 
    {
        public class Book
        {
            private static readonly char[] IgnoreChars = { '"', '(', '«' };

            private const int LastNameInd = 0;
            private const int FisrtNameInd = 1;
            private const int MiddleNameInd = 2;
            private const int TitleNameInd = 3;
            private readonly int _idInd = 8;
            private readonly string[] _fields;

            public Book(string bookLine, long pos)
            {
                _fields = bookLine.Split(';');
                Author = SetAuthor(_fields);

                if (_idInd < _fields.Length - 1)
                    _idInd = _fields.Length - 1;

            }

            private string SetAuthor(string[] fields)
            {
                var author =
                    $"{_fields[LastNameInd].Trim()} {_fields[FisrtNameInd].Trim()} {_fields[MiddleNameInd].Trim()}".Trim
                        ();
                if (author.Length == 0)
                    author = _fields[TitleNameInd].Trim();

                if (author.IndexOfAny(IgnoreChars, 0, 1) == 0)
                {
                    author = author.Substring(1);
                }

                if (0 < author.Length)
                    author = char.ToUpper(author[0]) + author.Substring(1);

                return author;
            }

            public string Title => _fields[TitleNameInd].Trim();
            public string Id => _fields[_idInd].Trim();
            public string Author { get; }
        }

        public readonly List<Book> Books = new List<Book>(423190);

        public CatalogReaderAdapter(string fileName)
        {
            var fileSize = new FileInfo(fileName).Length;

            var startTime = DateTime.Now;

            var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
            var accessor = mmf.CreateViewAccessor(0, fileSize);

            var buffer = new byte[4096];
            var j = 0;
            long i = 0;
            while (i < fileSize && accessor.ReadByte(i) != 0xA) ++i;
            ++i;
            for (; i < fileSize; ++i, ++j)
            {
                buffer[j] = accessor.ReadByte(i);
                if (buffer[j] != 0xA) continue;
                var bookPos = i - j;
                var book = new Book(Encoding.UTF8.GetString(buffer, 0, j), bookPos);
                j = -1;

                Books.Add(book);
            }
            Console.WriteLine($"Read elapsed: {DateTime.Now - startTime}");
            startTime = DateTime.Now;
            Books.Sort(new AuthorComparer());
            Console.WriteLine($"Sort elapsed: {DateTime.Now - startTime}");
        }


        class AuthorComparer : IComparer<Book>
        {
            public int Compare(Book x, Book y)
            {
                return string.CompareOrdinal(x.Author, y.Author);
            }
        }

    }
}