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

        private static StreamReader LetterIndexFile()
        {
            return new StreamReader(Path.Combine(ExecutableLocation, Properties.Settings.Default.LetterIndexFilename));
        }

        private readonly FileStream _titlesFile = new FileStream(Path.Combine(ExecutableLocation, Properties.Settings.Default.TItlesFilename), FileMode.Open);
        private readonly FileStream _authorsTitleFile =  new FileStream(Path.Combine(ExecutableLocation, Properties.Settings.Default.AuthorsFilename), FileMode.Open);


        class LetterData
        {
            public LetterData(long letterOffset)
            {
                LetterOffset = letterOffset;
            }

            public long LetterOffset;
            public List<Author> authors;
        }


        readonly Dictionary<char, LetterData> _lettersDict = new Dictionary<char, LetterData>(200);

        private void Form1_Load(object sender, EventArgs e)
        {
            using (TextReader letterIndexReader = LetterIndexFile())
            {
                string line;
                while ((line = letterIndexReader.ReadLine()) != null)
                {
                    var letterOffset = line.Split(';');
                    if (_filterFunction != null && _filterFunction(letterOffset[0][0]) )
                        continue;
                    _lettersDict.Add( letterOffset[0][0], new LetterData(long.Parse(letterOffset[1])) );
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

        private string ReadAuthorLine(ref byte[] buff)
        {
            _authorsTitleFile.Read(buff, 0, 2);
            var lineSize = BitConverter.ToUInt16(buff, 0);
            if (buff.Length < lineSize)
                buff = new byte[lineSize];
            //_authorsTitleFile.Seek(2, SeekOrigin.Current);
            _authorsTitleFile.Read(buff, 0, lineSize);
            var authorLine = Encoding.UTF8.GetString(buff, 0, lineSize);
            _authorsTitleFile.Seek(1, SeekOrigin.Current); // \n
            return authorLine;

        }

        private string ReadTitleLine(long offset, ref byte[] buff)
        {
            _titlesFile.Seek(offset, SeekOrigin.Begin); 
            _titlesFile.Read(buff, 0, 2);
            var lineSize = BitConverter.ToUInt16(buff, 0);
            if (buff.Length < lineSize)
                buff = new byte[lineSize];
            _titlesFile.Read(buff, 0, lineSize);
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

        class Title
        {
            public Title(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; }

            public string Id { get; }
        }

        List<Author> GetAuthors(char letter, long offset)
        {
            var authors = new List<Author>();
            var buff = new byte[1000];
            _authorsTitleFile.Seek(offset, SeekOrigin.Begin);
            while (true)
            {
                var authorLine = ReadAuthorLine(ref buff).Split(';');
                if (authorLine[0][0] != letter)
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

            if (letterData.authors == null)
            {
                letterData.authors = GetAuthors(firstUpper, letterData.LetterOffset);
            }

            AuthorsList.DisplayMember = "Name";
            AuthorsList.ValueMember = "Titles";
            var sortedAuthors = letterData.authors.Where(item => item.Name.StartsWith(t));
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
                var titleLine = ReadTitleLine(offset, ref buff).Split(';');
                //if (FilterFunction != null && FilterFunction(titleLine[1][0]))
                //    continue;
                titles.Add(new Title(titleLine[1], titleLine[0]));
            }

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

        private void readButton_Click(object sender, EventArgs e)
        {
            var tlb = TitlesList;

            var title = tlb.Text;
            var id = (string)tlb.SelectedValue;
            var url = $"http://flibusta.is/b/{id}/fb2";
            var fileName = Path.Combine(DownloadLocation, $"{id}.fb2");
            if (File.Exists(fileName))
            {
                OpenBook(fileName);
                return;
            }
            var ds = new DownloadState(title, id, url, fileName);

            toolStripStatusLabel1.Text = $"Downloading {id}";
            messages.Text = $"Скачивает книгу: {title} \n Ссылка: {url} \n";

            var client = new WebClient();
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(url), ds.Filename, ds);
        }

        private static void OpenBook(string fileName)
        {
            var reader = new ProcessStartInfo
            {
                Arguments = fileName,
                FileName = ExternalReaderName,
                WindowStyle = ProcessWindowStyle.Maximized
            };
            Process.Start(reader);
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var ds = (DownloadState)e.UserState;
            if (e.Error != null)
            {
                toolStripStatusLabel1.Text = $"Download failed: {ds.Id}";
                messages.Text = e.Error.Message;
                return;
            }

            toolStripStatusLabel1.Text = $"Download complete: {ds.Id}";
            messages.Text += $"Книга {ds.Title} доступна: {ds.Filename}\n\n";
            try
            {
                OpenBook(ds.Filename);
            }
            catch (Exception ex)
            {
                messages.Text += ex.Message;
            }
            
        }

     
    }
}
