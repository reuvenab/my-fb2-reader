using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myFBReader
{
    public partial class Form1 : Form
    {

        

        public static string ExecutableLocation = @"c:\temp\catalog";
        public Form1()
        {
            InitializeComponent();
        }


        private static StreamReader AuthorsIndexFile()
        {
            return new StreamReader(Path.Combine(ExecutableLocation, "authors_index.txt"));
        }

        private FileStream _titlesFile = new FileStream(Path.Combine(ExecutableLocation, "titles.txt"), FileMode.Open);
        private FileStream _authorsTitleFile =  new FileStream(Path.Combine(ExecutableLocation, "authors.txt"), FileMode.Open);
        
        private void Form1_Load(object sender, EventArgs e)
        {
            using (TextReader letterIndexReader = AuthorsIndexFile())
            {
                string line;
                while ((line = letterIndexReader.ReadLine()) != null)
                {
                    var letterOffset = line.Split(';');
                    if (letterOffset[0][0] < 'А' || 'я' < letterOffset[0][0] )
                        continue;
                    var tp = new TabPage {Text = letterOffset[0][0].ToString(), Tag = long.Parse(letterOffset[1]) };
                    
                    tabControl2.TabPages.Add(tp);
                }
                
            }
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
            
            //lb.MouseDoubleClick += tlb_MouseDoubleClick;

            t.Controls.Add(titlesLb);

        }


        private void tlb_DoubleClick(object sender, System.EventArgs e)
        {
            var tlb = (ListBox)sender;
            var id = tlb.SelectedValue;
            toolStripStatusLabel1.Text = $"Downloading {id}";

            var client = new WebClient();
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            var url = $"http://flibusta.is/b/{id}/fb2";
            client.DownloadFileAsync(new Uri(url), Path.Combine(ExecutableLocation, "Downloaded", $"{id}.fb2"), id);
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var id = (string) e.UserState;
            toolStripStatusLabel1.Text = $"Download complete: {id}";
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
                titles.Add(new Title(titleLine[1], titleLine[0]));
            }

            tlb.DisplayMember = "Name";
            tlb.ValueMember = "Id";
            tlb.DataSource = titles;
            tlb.DoubleClick += tlb_DoubleClick;
        }

    }
}
