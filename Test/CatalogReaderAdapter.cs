using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Test
{
    class CatalogReaderAdapter 
        //: IEnumerable<CatalogReaderAdapter.Book>
    {
        public class Book
        {
            private static readonly char[] IgnoreChars = { '"', '(', '«' };

            private const int LastNameInd = 0;
            private const int FisrtNameInd = 1;
            private const int MiddleNameInd = 2;
            private const int TitleNameInd = 3;
            //private const int SubtitleNameInd = 4;
            //private const int LanguageInd = 5;
            //private const int YearInd = 6;
            //private const int SeriesInd = 7;
            private readonly int _idInd = 8;
            private readonly string[] _fields; //= new string[IdInd+1];
            private readonly string _author;
            
            public Book(string bookLine, long pos)
            {
                _fields = bookLine.Split(';');
                _author = SetAuthor(_fields);

                if (_idInd < _fields.Length - 1)
                    _idInd = _fields.Length - 1;

            }

            private string SetAuthor(string[] fields)
            {
                //var author =
                //    $"{_fields[LastNameInd]} {_fields[FisrtNameInd]} {_fields[MiddleNameInd]}";

                //if (0 < author.Length)
                //    author = char.ToUpper(author[0]) + author.Substring(1);

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
            public string Author => _author;

            //public string this[int fieldInd]
            //{
            //    //get { return _fields[fieldInd]; }
            //    //set
            //    //{
            //    //    if (fieldInd < _fields.Length)
            //    //        _fields[fieldInd] = value;
            //    //    else
            //    //        _fields[IdInd] = value;
            //    //}
            //}
        }


        //private readonly BinaryReader _reader;
        //private string[] _fieldsNames;
        //private int _fieldsCount = 0;
        private readonly string _fileName;
        private readonly long _fileSize;
        private BinaryReader _reader;
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        public readonly List<Book> Books = new List<Book>(423190);

        public CatalogReaderAdapter(BinaryReader reader)
        {
            _reader = reader;
        }

        public CatalogReaderAdapter(string fileName)
        {
            _fileName = fileName;
            _fileSize = new FileInfo(fileName).Length;

            var startTime = DateTime.Now;

            _mmf = MemoryMappedFile.CreateFromFile(_fileName, FileMode.Open);
            _accessor = _mmf.CreateViewAccessor(0, _fileSize);

            var buffer = new byte[4096];
            var j = 0;
            long i = 0;
            while (i < _fileSize && _accessor.ReadByte(i) != 0xA) ++i;
            ++i;
            for (; i < _fileSize; ++i, ++j)
            {
                buffer[j] = _accessor.ReadByte(i);
                if (buffer[j] != 0xA) continue;
                var bookPos = i - j;
                var book = new Book(Encoding.UTF8.GetString(buffer, 0, j), bookPos);
                j = -1;

                //if (!HasAuthor(book.Author)) continue;

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

        //public IEnumerator<Book> GetEnumerator()
        //{
        //    using (var mmf = MemoryMappedFile.CreateFromFile(_fileName, FileMode.Open))
        //    {
        //        using (var accessor = mmf.CreateViewAccessor(0, _fileSize))
        //        {
        //            var buffer = new byte[4096];
        //            var j = 0;
        //            long i = 0;
        //            while (i < _fileSize && accessor.ReadByte(i) != 0xA) ++i;
        //            ++i;
        //            for (; i < _fileSize; ++i, ++j)
        //            {
        //                buffer[j] = accessor.ReadByte(i);
        //                if (buffer[j] != 0xA) continue;
        //                var bookPos = i - j;
        //                var book = new Book(Encoding.UTF8.GetString(buffer, 0, j), bookPos);
        //                j = -1;

        //                if (!HasAuthor(book.Author)) continue;

        //                yield return book;
        //            }
        //        }
        //    }

        //}

        private bool HasAuthor(string author)
        {
            if (author.Length == 0)
                return false;
            var c = author[0];
            return ('A' <= c && c <= 'Z') || ('А' <= c && c <= 'Я');
        }


        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }
}