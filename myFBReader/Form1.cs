using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                FilterFunction = RussianOnly;
        }


        private static StreamReader LetterIndexFile()
        {
            return new StreamReader(Path.Combine(ExecutableLocation, Properties.Settings.Default.LetterIndexFilename));
        }

        private readonly FileStream _titlesFile = new FileStream(Path.Combine(ExecutableLocation, Properties.Settings.Default.TItlesFilename), FileMode.Open);
        private readonly FileStream _authorsTitleFile =  new FileStream(Path.Combine(ExecutableLocation, Properties.Settings.Default.AuthorsFilename), FileMode.Open);
        
        private void Form1_Load(object sender, EventArgs e)
        {
            using (TextReader letterIndexReader = LetterIndexFile())
            {
                tabControl2.Font = new Font(tabControl2.Font.FontFamily, Properties.Settings.Default.FontSize);
                string line;
                while ((line = letterIndexReader.ReadLine()) != null)
                {
                    var letterOffset = line.Split(';');
                    if (FilterFunction != null && FilterFunction(letterOffset[0][0]) )
                        continue;
                    var tp = new TabPage {Text = letterOffset[0][0].ToString(), Tag = long.Parse(letterOffset[1]) };
                    //tp.Font = new Font(tp.Font.FontFamily, Properties.Settings.Default.FontSize+10);
                    tabControl2.TabPages.Add(tp);
                    tp.AutoSize = true;
                }
            }
            tabControl2.SelectedIndexChanged += tabControl2_SelectedIndexChanged;
            if (!Directory.Exists(DownloadLocation))
            {
                Directory.CreateDirectory(DownloadLocation);
            }
            if (0 < tabControl2.TabPages.Count)
            {
                tabControl2.SelectedIndex = 0;
            }
        }

        delegate bool FilterFunctionDelegate(char letter);

        private readonly FilterFunctionDelegate FilterFunction;
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


        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {

            var tc = (TabControl)sender;

            var t = tc.SelectedTab;

            if (t.Controls.Count != 0)
                return;

            var letter = t.Text[0];
            var buff = new byte[1000];

            var offset = (long)t.Tag;

            var authors = new List<Author>();

            _authorsTitleFile.Seek(offset, SeekOrigin.Begin);
            while (true)
            {
                var authorLine = ReadAuthorLine(ref buff).Split(';');
                if (authorLine[0][0] != letter)
                    break;
                var titles = new uint[authorLine.Length - 1];
                for (int i = 0; i < titles.Length; i++)
                {
                    titles[i]= uint.Parse(authorLine[i+1]);
                }
                authors.Add(new Author($"{authorLine[0]} ({titles.Length})", titles));
            
            }
            
            var lb = new ListBox
            {
                DisplayMember = "Name",
                ValueMember = "Titles",
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                Width = t.Width / 4,
                Height = t.Height,
            };
            lb.Font = new Font(lb.Font.FontFamily, Properties.Settings.Default.FontSize);

            lb.DataSource = authors;

            lb.SelectedIndexChanged += lb_SelectedIndexChanged;

            t.Controls.Add(lb);

            var titlesLb = new ListBox
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                    Left = lb.Width,
                    Width = t.Width - lb.Width,
                    Height = t.Height,
            };
            titlesLb.Font = new Font(titlesLb.Font.FontFamily, Properties.Settings.Default.FontSize);
            
            //lb.MouseDoubleClick += tlb_MouseDoubleClick;

            t.Controls.Add(titlesLb);

        }


        private void tlb_DoubleClick(object sender, System.EventArgs e)
        {
        }

        private void lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = (ListBox)sender;
            var tlb = (ListBox) lb.Parent.Controls[1];

            var titleOffsets = (uint[])lb.SelectedValue;

            var titles = new List<Title>();

            var buff = new byte[1000];

            foreach (var offset in titleOffsets)
            {
                var titleLine = ReadTitleLine(offset, ref buff).Split(';');
                if (FilterFunction != null && FilterFunction(titleLine[1][0]))
                    continue;
                titles.Add(new Title(titleLine[1], titleLine[0]));
            }

            tlb.DisplayMember = "Name";
            tlb.ValueMember = "Id";
            tlb.DataSource = titles;
            tlb.DoubleClick += tlb_DoubleClick;
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
            var tlb = (ListBox)tabControl2.SelectedTab.Controls[1];
            
            var title = (string)tlb.Text;
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
            ProcessStartInfo reader = new ProcessStartInfo();
            reader.Arguments = fileName;
            reader.FileName = ExternalReaderName;
            reader.WindowStyle = ProcessWindowStyle.Maximized;
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
            OpenBook(ds.Filename);
        }
    }
}
