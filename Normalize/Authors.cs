using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Normalize
{
    public class Authors : IEnumerable<string>
    {
        private char[] ignoreChars = {'"', '(', '«'};


        class FirstLetterCaseIgnoreComp : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, true);
            }
        }


        private readonly SortedDictionary<string, long[]> _authors = new SortedDictionary<string, long[]>();

        public void AddBook(string author, long pos)
        {
            if (author.IndexOfAny(ignoreChars, 0, 1) == 0)
            {
                author = author.Substring(1).Trim();
            }
            author = char.ToUpper(author[0]) + author.Substring(1);

            long[] books;
            if (_authors.TryGetValue(author, out books))
            {
                if (books.Length == books[0])
                {
                    var newSize = Convert.ToInt32(Math.Round(books[0]*1.5));
                    var newBooks = new long[newSize];

                    books.CopyTo(newBooks, 0);
                    books = newBooks;
                }

            }
            else
            {
                books = new long[6];
                books[0] = 1;
            }
            books[books[0]] = pos;
            books[0] += 1;
            _authors[author] = books;
        }

        private long _totalBooks = 0;
        long _moreThan4 = 0;
        public double AveragePerAuthor => _totalBooks / _authors.Keys.Count;
        public double MoreThan4 => _moreThan4;

        public IEnumerator<string> GetEnumerator()
        {
            
            foreach (var author in _authors)
            {
                var books = author.Value;
                string positions = books[1].ToString();
                
                var maxInd = books[0];
                _totalBooks += maxInd - 1;
                if (maxInd > 5)
                    _moreThan4 += 1;
                 
                for (int i = 2; i < maxInd; i++)
                {
                    positions += $";{books[i]}";
                }
                yield return $"{author.Key};{positions}";
            }
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}