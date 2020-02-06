using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace myFBReader
{
    public partial class Form1 : Form
    {
        private static readonly string ExecutableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly string DownloadLocation = Path.Combine(ExecutableLocation, Properties.Settings.Default.DownloadFolder);
        private static readonly string ExternalReaderName = Path.Combine(ExecutableLocation, Properties.Settings.Default.ReaderName);
        public Form1()
        {
            InitializeComponent();

            toolStripStatusLabel1.Text = "";

            if (Properties.Settings.Default.RussianOnly)
                _filterFunction = RussianOnly;

        }

        private FileStream _titlesFile;
        private FileStream _authorsFile;

        class LetterData
        {
            public LetterData(char letter, long offset, FileStream authorsStream)
            {
                _offset = offset;
                _authorsStream = authorsStream;
                _letter = letter;
            }

            private readonly char _letter;
            private readonly long _offset;
            private readonly FileStream _authorsStream;

            private List<Author> _authors;
            public List<Author> Authors => _authors ?? (_authors = GetAuthors());

            
            private List<Author> GetAuthors()
            {
                var authors = new List<Author>();
                var buff = new byte[1000];
                _authorsStream.Seek(_offset, SeekOrigin.Begin);
                while (true)
                {
                    var authorLine = ReadAuthorLine(ref buff, _authorsStream).Split(';');
                    if (authorLine[0][0] != _letter)
                        break;
                    var titles = new uint[authorLine.Length - 1];
                    for (var i = 0; i < titles.Length; i++)
                    {
                        titles[i] = uint.Parse(authorLine[i + 1]);
                    }
                    authors.Add(new Author($"{authorLine[0]} ({titles.Length})", titles));
                }
                return authors;
            }
        }


        readonly Dictionary<char, LetterData> _lettersDict = new Dictionary<char, LetterData>(200);


        static StreamReader LetterIndexFile()
        {
            return new StreamReader(Path.Combine(ExecutableLocation, CatalogLib.Catalog.LetterIndexFileName));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CatalogLib.Catalog.Check(ExecutableLocation);

            _titlesFile = new FileStream(Path.Combine(ExecutableLocation, CatalogLib.Catalog.TitlesFileName), FileMode.Open);
            _authorsFile = new FileStream(Path.Combine(ExecutableLocation, CatalogLib.Catalog.AuthorsFileName), FileMode.Open);

            using (TextReader letterIndexReader = LetterIndexFile())
            {
                string line;
                while ((line = letterIndexReader.ReadLine()) != null)
                {
                    var letterOffset = line.Split(';');
                    if (_filterFunction != null && _filterFunction(letterOffset[0][0]) )
                        continue;
                    _lettersDict.Add( letterOffset[0][0], new LetterData(letterOffset[0][0], long.Parse(letterOffset[1]), _authorsFile) );
                }
            }
            if (!Directory.Exists(DownloadLocation))
            {
                Directory.CreateDirectory(DownloadLocation);
            }
        }

        delegate bool FilterFunctionDelegate(char letter);

        private readonly FilterFunctionDelegate _filterFunction;
        private static bool RussianOnly(char letter)
        {
            return letter < 'А' || 'я' < letter;
        }

        private static string ReadAuthorLine(ref byte[] buff, FileStream authorsStream)
        {
            authorsStream.Read(buff, 0, 2);
            var lineSize = BitConverter.ToUInt16(buff, 0);
            if (buff.Length < lineSize)
                buff = new byte[lineSize];
            //_authorsFile.Seek(2, SeekOrigin.Current);
            authorsStream.Read(buff, 0, lineSize);
            var authorLine = Encoding.UTF8.GetString(buff, 0, lineSize);
            authorsStream.Seek(1, SeekOrigin.Current); // \n
            return authorLine;
        }

        private static string ReadTitleLine(long offset, ref byte[] buff, FileStream titlesStream)
        {
            titlesStream.Seek(offset, SeekOrigin.Begin);
            titlesStream.Read(buff, 0, 2);
            var lineSize = BitConverter.ToUInt16(buff, 0);
            if (buff.Length < lineSize)
                buff = new byte[lineSize];
            titlesStream.Read(buff, 0, lineSize);
            var titleLine = Encoding.UTF8.GetString(buff, 0, lineSize);
            return titleLine;
        }


        class Author
        {
            public Author(string name, uint[] titles)
            {
                Name = name;
                Titles = titles;
            }

            public string Name { get; }

            public uint[] Titles { get; }
        }

        class Title : IComparable<Title>
        {
            public Title(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; }

            public string Id { get; }
            public int CompareTo(Title other)
            {
                return Name.CompareTo(other.Name);
            }
        }
    //private void tlb_DoubleClick(object sender, System.EventArgs e)
        //{
        //}

        private void AuthorLookup_TextChanged(object sender, EventArgs e)
        {
            var t = AuthorLookup.Text.Trim();
            if (t.Length == 0)
                return;
            var firstUpper = char.ToUpper(t[0]);

            if (!_lettersDict.ContainsKey(firstUpper))
                return;

            var letterData = _lettersDict[firstUpper];

            AuthorsList.DisplayMember = "Name";
            AuthorsList.ValueMember = "Titles";
            var sortedAuthors = letterData.Authors.Where(item => item.Name.StartsWith(t));
            AuthorsList.DataSource = sortedAuthors.ToList();
                
        }


        private void AuthorsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = AuthorsList;

            var titleOffsets = (uint[])lb.SelectedValue;

            var titles = new List<Title>();

            var buff = new byte[1000];

            foreach (var offset in titleOffsets)
            {
                var titleLine = ReadTitleLine(offset, ref buff, _titlesFile).Split(';');
                //if (FilterFunction != null && FilterFunction(titleLine[1][0]))
                //    continue;
                titles.Add(new Title(titleLine[1], titleLine[0]));
            }
            titles.Sort();

            TitlesList.Tag = titles;
            TitlesList.DisplayMember = "Name";
            TitlesList.ValueMember = "Id";
            TitlesList.DataSource = titles;

        }


        class DownloadState
        {
            public string Url;
            public readonly string Id;
            public readonly string Title;
            public readonly string Filename;

            public DownloadState(string title, string id, string url, string filename)
            {
                Title = title;
                Id = id;
                Url = url;
                Filename = filename;
            }
        }

        private const int max_messages_cnt = 10;
        private List<string> last_messages = new List<string>(max_messages_cnt);

        private void AddMessage(string Message)
        {
            if (last_messages.Count > max_messages_cnt)
                last_messages.RemoveAt(max_messages_cnt - 1);
            last_messages.Insert(0, Message);
            messages.Text = last_messages.Aggregate( (string Agg, string CurItem) => Agg + CurItem + Environment.NewLine);
        }

        private void readButton_Click(object sender, EventArgs e)
        {
            var tlb = TitlesList;

            var title = tlb.Text;
            var id = (string)tlb.SelectedValue;
            var url = $"http://flibusta.is/b/{id}/fb2";
            var fileName = Path.Combine(DownloadLocation, $"{id}.fb2");
            if (File.Exists(fileName))
            {
                try
                {
                    OpenBook(fileName);
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message);
                }
                return;
            }
            var ds = new DownloadState(title, id, url, fileName);

            toolStripStatusLabel1.Text = $"Скачивает книгу";
            AddMessage($"Скачивает книгу: {title} Ссылка: {url}");

            var client = new WebClient();
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(url), ds.Filename, ds);
        }

        private static void OpenBook(string fileName)
        {
            try
            {
                var reader = new ProcessStartInfo
                {
                    Arguments = fileName,
                    FileName = ExternalReaderName,
                    WindowStyle = ProcessWindowStyle.Maximized
                };
                Process.Start(reader);
            }
            catch (Exception)
            {
                throw new Exception($"Программа просмотра {ExternalReaderName} не найдена");
            }
           
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var ds = (DownloadState)e.UserState;
            if (e.Error != null)
            {
                toolStripStatusLabel1.Text = $"Не удалось скачать файл: {ds.Id}";
                AddMessage(e.Error.Message);
                return;
            }

            toolStripStatusLabel1.Text = $"Файл скачан: {ds.Id}";
            AddMessage($"Книга {ds.Title} доступна: {ds.Filename}");
            
            try
            {
                OpenBook(ds.Filename);
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
            
        }

        private void TitleLookup_TextChanged(object sender, EventArgs e)
        {
            var t = TitleLookup.Text.Trim();
            var titles = TitlesList.Tag as List<Title>;
            if (t.Length == 0  || titles == null)
            {
                if (titles != null)
                    TitlesList.DataSource = titles;
                return;
            }
            var filteredTitles = titles.Where(item => item.Name.StartsWith(t));
            TitlesList.DataSource = filteredTitles.ToList();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            AuthorLookup.Focus();
        }
    }
}
