using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Normalize
{
    class CatalogReaderAdapter : IEnumerable<CatalogReaderAdapter.Book>
    {
        public class Book
        {
            protected const int LastNameInd = 0;
            protected const int FisrtNameInd = 1;
            protected const int MiddleNameInd = 2;
            private const int TitleNameInd = 3;
            protected const int SubtitleNameInd = 4;
            protected const int LanguageInd = 5;
            protected const int YearInd = 6;
            protected const int SeriesInd = 7;
            private const int IdInd = 8;
            private readonly string[] _fields = new string[IdInd+1];

            private string author_;

            public Book()
            {
            }

            public string Title => _fields[TitleNameInd].Trim();
            public string Id => _fields[IdInd].Trim();
            public string Author => author_ ??
                                    (author_ =
                                        $"{_fields[LastNameInd].Trim()} {_fields[FisrtNameInd].Trim()} {_fields[MiddleNameInd].Trim()}"
                                            .Trim());

            public string this[int fieldInd]
            {
                //get { return _fields[fieldInd]; }
                set
                {
                    if (fieldInd < _fields.Length)
                        _fields[fieldInd] = value;
                    else
                        _fields[IdInd] = value;
                }
            }
        }


        private readonly BinaryReader _reader;
        private string[] _fieldsNames;
        private int _fieldsCount = 0;

        private bool Eof()
        {
            return (_reader.BaseStream.Position == _reader.BaseStream.Length);
        }

        private Char ReadChar()
        {
            return _reader.ReadChar();
        }

        public CatalogReaderAdapter(BinaryReader reader)
        {
            _reader = reader;
            ReadFieldNames();
        }

        private void ReadFieldNames()
        {
            var curLine = "";
            while (!Eof() && _reader.PeekChar() != 0xA)
                curLine += ReadChar();

            _fieldsNames = curLine.Split(';');
            _fieldsCount = _fieldsNames.Length;
            ReadChar();
        }


        public IEnumerator<Book> GetEnumerator()
        {
            while (!Eof())
            {
                var b = ReadBook();
                if (b.Author == "")
                    continue;
                yield return b;
            }
        }

        private Book ReadBook()
        {
            var fieldInd = 0;
            var curBook = new Book();
            var curField = "";
            while (!Eof())
            {
                var curChar = ReadChar();
                if (curChar == ';' || curChar == 0xA)
                {
                    curBook[fieldInd] = curField;
                    if (curChar == 0xA)
                        break;
                    curField = "";
                    ++fieldInd;
                }
                else
                {
                    curField += curChar;
                }
            }
            return curBook;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}