//enire midify source code, please note that midify is made up
//out of multiple scripts and executables
//compiling this script will not give you a working program
//its only for documentation
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Net;
using System.Diagnostics;
using NAudio.Wave;
namespace midify
{
    public partial class Form1 : Form
    {
        private IDictionary<string, string> CustomPlaylists = new Dictionary<string, string>();
        private IDictionary<string, string> PlayList0 = new Dictionary<string, string>();
        private static readonly MediaPlayer.MediaPlayer Player = new MediaPlayer.MediaPlayer();
        private List<string> DeletedIndices = new List<string>();
        private static List<bool> IsPlaylist = new List<bool>();
        private static List<string> Queue = new List<string>();
        private readonly WebClient Client = new WebClient();
        private List<string> Pictures = new List<string>();
        private int CustomPlaylistAmount;
        private string CurrentPlaylist;
        private bool Shuffle = false;
        private bool Looping = false;
        private string Colorscheme;
        public static List<string> LoadedSongs = new List<string>();
        public static Thread MusicThread;
        public static string CurrentSongPath;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Astolfo.wav"))
                {
                    var url = new Uri("https://drive.google.com/uc?export=download&id=1YcaicczB6nuxIWcg_wILmPx6SgLxBLlQ");
                    Client.DownloadFile(url, "Astolfo.wav");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\midify uninstaller.exe"))
                {
                    var url = new Uri("https://drive.google.com/uc?export=download&id=1ovQO_W8BslPYlpPiy_65Qn2oVIzA_8x-"); //add uninstaller download link
                    Client.DownloadFile(url, "midify uninstaller.zip");
                    ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\midify uninstaller.zip", Directory.GetCurrentDirectory());
                    File.Delete(Directory.GetCurrentDirectory() + "\\midify uninstaller.zip");
                    Directory.Delete(Directory.GetCurrentDirectory() + "\\midify uninstaller");
                }
                Onload();
            }
            catch (Exception)
            {
                Onload();
            }
        }
        private void Onload()
        {
            var Colorpath = Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt";
            checkedListBox1.CheckOnClick = true;
            checkedListBox2.CheckOnClick = true;
            checkedListBox3.CheckOnClick = true;
            checkedListBox4.CheckOnClick = true;
            checkedListBox5.CheckOnClick = true;
            this.DoubleBuffered = true;
            this.Text = "midify";
            richTextBox1.Text = "all songs:";
            richTextBox1.AppendText(Environment.NewLine);
            panel1.Visible = true;
            Setupcfg();
            LoadStartUI(false);
            LoadColorScheme(Colorscheme);
            LoadPictures();
        }
        private void Setupcfg()
        {
            var path2file = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt";
            var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
            var pathfilepath = Directory.GetCurrentDirectory() + @"\cfg\directory.txt";
            var DIpath = Directory.GetCurrentDirectory() + @"\cfg\deletedindices.txt";
            var Colorpath = Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt";
            var CPAP = Directory.GetCurrentDirectory() + @"\cfg\customplaylistamount.txt";
            var PlaylistDir = Directory.GetCurrentDirectory() + @"\cfg\playlists";
            var ConfigDir = Directory.GetCurrentDirectory() + @"\cfg";
            var Picturepath = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var StartupPath = Directory.GetCurrentDirectory() + @"\cfg\startup.txt";
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    string[] stdarray = { "false", "false" };
                    Pictures = stdarray.ToList();
                    Colorscheme = "light";
                    if (!Directory.Exists(PlaylistDir))
                    {
                        Directory.CreateDirectory(PlaylistDir);
                        File.Create(path2file).Close();
                    }
                    Directory.CreateDirectory(ConfigDir);
                    File.Create(DIpath).Close();
                    File.Create(pathtodic).Close();
                    File.WriteAllLines(Picturepath, stdarray);
                    File.WriteAllText(Colorpath, "light");
                    File.WriteAllText(CPAP, "0");
                    File.WriteAllText(pathfilepath, "enter directory");
                    File.WriteAllText(StartupPath, "no");
                    UpdateDctnry();
                }
                else
                {
                    var lastenteredpath = File.ReadAllText(pathfilepath);
                    CustomPlaylists = File.ReadAllLines(pathtodic).Select(line => line.Split(';')).ToDictionary(split => split[0], split => split[1]);
                    DeletedIndices = File.ReadAllLines(DIpath).ToList();
                    CustomPlaylistAmount = Convert.ToInt32(File.ReadAllText(CPAP));
                    Colorscheme = File.ReadAllText(Colorpath);
                    Pictures = File.ReadAllLines(Picturepath).ToList();
                    textBox1.Text = lastenteredpath;
                    if (!Directory.Exists(PlaylistDir))
                    {
                        Directory.CreateDirectory(PlaylistDir);
                        File.Create(path2file).Close();
                        UpdateDctnry();
                    }
                    else
                    {
                        if (!File.Exists(path2file))
                        {
                            File.Create(path2file).Close();
                            UpdateDctnry();
                        }
                        PlayList0 = File.ReadAllLines(path2file).Select(line => line.Split(';')).ToDictionary(split => split[0], split => split[1]);
                        for (int a = 0; a < PlayList0.Count; a++)
                        {
                            var content = PlayList0.ElementAt(a).Key;
                            richTextBox1.AppendText(Environment.NewLine);
                            richTextBox1.AppendText(content);
                        }
                    }
                }
            }
            catch (Exception)
            {
                var text = " go to " + Directory.GetCurrentDirectory() + " and delete the cfg directory or reset the cfg in the settings";
                MessageBox.Show("there was an error during program setup," + text, "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopCurrentSong(); 
                Invoke(new Action(() =>
                {
                    trackBar1.Value = 0;
                }));
            }
        }
        private void UpdateDctnry()
        {
            var path2file = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt";
            var pathfilepath = Directory.GetCurrentDirectory() + @"\cfg\directory.txt";
            var firsttime = false;
            try
            {
                if (!String.IsNullOrEmpty(textBox1.Text))
                {
                    var contentlist = new List<string>();
                    var SPList = GetSongPath(textBox1.Text);
                    var SNList = GetSongName(textBox1.Text);
                    if (File.Exists(SPList[0]))
                    {
                        LoadStartUI(false);
                        richTextBox1.Clear();
                        richTextBox1.AppendText("all songs:");
                        richTextBox1.AppendText(Environment.NewLine);
                        File.Delete(Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt");
                        PlayList0.Clear();
                        for (int a = 0; a < SNList.Count; a++)
                        {
                            PlayList0.Add(SNList[a], SPList[a]);
                            richTextBox1.AppendText(Environment.NewLine);
                            richTextBox1.AppendText(SNList[a]);
                            var content = SNList[a] + ";" + PlayList0[SNList[a]];
                            contentlist.Add(content);
                        }
                    }
                    else
                    {
                        throw new DirectoryNotFoundException();
                    }
                    File.WriteAllText(pathfilepath, textBox1.Text);
                    File.WriteAllLines(path2file, contentlist);
                }
                else
                {
                    firsttime = true;
                    throw new DirectoryNotFoundException();
                }
            }
            catch (DirectoryNotFoundException)
            {
                var stdtext = File.ReadAllText(pathfilepath);
                textBox1.Text = stdtext;
                if (firsttime != true)
                {
                    MessageBox.Show("the directory is invalid or there's a duplicate file", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception) { }
        }
        private void Resetcfg()
        {
            try
            {
                textBox1.Text = "";
                var path = Directory.GetCurrentDirectory() + @"\cfg";
                string[] Files = Directory.GetFiles(path);
                string[] subFiles = Directory.GetFiles(path + @"\playlists");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\backup");
                File.Move(path + @"\customplaylistnames.txt", Directory.GetCurrentDirectory() + @"\backup\customplaylistnames.txt");
                for (int i = 0; i < Files.Length; i++)
                {
                    File.Delete(Files[i].ToString());
                }
                for (int i = 1; i < subFiles.Length; i++)
                {
                    var name = subFiles[i].ToString().Split('\\');
                    File.Copy(subFiles[i].ToString(), Directory.GetCurrentDirectory() + @"\backup\" + name[name.Length - 1]);
                    File.Delete(subFiles[i].ToString());
                }
                File.Delete(subFiles[0].ToString());
                Directory.Delete(path + @"\playlists");
                Directory.Delete(path);
                Setupcfg();
                for (int i = 1; i < subFiles.Length; i++)
                {
                    var name = subFiles[i].ToString().Split('\\');
                    var filename = path + @"\playlists\playlist" + i.ToString() + ".txt";
                    File.Move(Directory.GetCurrentDirectory() + @"\backup\" + name[name.Length - 1], filename);
                }
                File.Delete(path + @"\customplaylistamount.txt");
                File.Delete(path + @"\customplaylistnames.txt");
                File.Move(Directory.GetCurrentDirectory() + @"\backup\customplaylistnames.txt", path + @"\customplaylistnames.txt");
                File.WriteAllText(path + @"\customplaylistamount.txt", (subFiles.Length - 1).ToString());
                Directory.Delete(Directory.GetCurrentDirectory() + @"\backup");
                var Playlistnames = File.ReadAllLines(path + @"\customplaylistnames.txt").ToList();
                var NewPlaylistnames = new List<string>();
                for (int i = 0; i < Playlistnames.Count; i++)
                {
                    var entry = Playlistnames[i].Split(' ');
                    var newentry = entry[0].ToString() + " " + (i + 1).ToString();
                    NewPlaylistnames.Add(newentry);
                }
                File.WriteAllLines(path + @"\customplaylistnames.txt", NewPlaylistnames);
                LoadColorScheme("light");
                LoadPictures();
                for (int i = 0; i < checkedListBox4.Items.Count; i++)
                {
                    checkedListBox4.SetItemChecked(i, false);
                }
                checkedListBox4.SetItemChecked(1, true);
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                richTextBox1.Text = "all songs:";
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                textBox3.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void StopCurrentSong()
        {
            if (MusicThread != null && MusicThread.IsAlive)
            {
                Queue.RemoveAt(0);
                IsPlaylist.RemoveAt(0);
                Player.Stop();
                MusicThread.Abort();
            }
        }
        private void AddListenToQueue(string song)
        {
            StopCurrentSong();
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            if (Queue.Count != 0)
            {
                SortLatestQueued(song);
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start(); 
            }
            else
            {
                AddtoQueue(song, false);
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start();
            }
        }
        private void SortLatestQueued(string song)
        {
            var templist = new List<string>();
            var templist2 = new List<bool>();
            for (int i = 0; i < Queue.Count; i++)
            {
                templist.Add(Queue[i]);
                templist2.Add(IsPlaylist[i]);
            }
            Queue.Clear();
            IsPlaylist.Clear();
            Queue.Add(song);
            IsPlaylist.Add(false);
            for (int i = 0; i < templist.Count; i++)
            {
                Queue.Add(templist[i]);
                IsPlaylist.Add(templist2[i]);
            }
            UpdateDisplay();
        }
        private void PlaySong(string song)
        {
            var foundsong = false;
            try
            {
                if (PlayList0.ContainsKey(song))
                {
                    AddListenToQueue(song);
                }
                else
                {
                    for (int a = 1; a <= CustomPlaylists.Count; a++)
                    {
                        var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + a + ".txt"; 
                        if (File.Exists(listpath))
                        {
                            var trylist = new Dictionary<string, string>();
                            var filecontent = File.ReadAllLines(listpath).ToList();
                            for (int i = 0; i < filecontent.Count; i++)
                            {
                                var contentarray = filecontent[i].Split(';');
                                try
                                {
                                    trylist.Add(contentarray[0], contentarray[1]);
                                }
                                catch (ArgumentException) { }
                            }
                            if (trylist.ContainsKey(song))
                            {
                                foundsong = true;
                                a = CustomPlaylistAmount + 1;
                                AddListenToQueue(song);
                            }
                        }
                    }
                    if (foundsong == false)
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception)
            {
                UpdateDctnry();
                MessageBox.Show("the song couldn't be found in any playlist", "no song found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PlayAudio(string song)
        {
            try
            {
                if (!PlayList0.ContainsKey(song))
                {
                    TrylistSystem(song);
                }
                else
                {
                    Invoke (new Action(() =>
                    {
                        var time = GetSongDuration(PlayList0[song]);
                        trackBar1.Maximum = Convert.ToInt32(time.TotalSeconds);
                    }));
                    var filepath = PlayList0[song];
                    CurrentSongPath = filepath;
                    Player.FileName = filepath;
                    Player.Play();
                    SongEnds();
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("file not found", "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TrylistSystem(string song)
        {
            var gotsong = false;
            try
            {
                for (int a = 1; a <= CustomPlaylists.Count; a++)
                {
                    var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + a + ".txt";
                    if (File.Exists(listpath))
                    {
                        var trylist = new Dictionary<string, string>();
                        var filecontent = File.ReadAllLines(listpath).ToList();
                        for (int i = 0; i < filecontent.Count; i++)
                        {
                            var contentarray = filecontent[i].Split(';');
                            try
                            {
                                trylist.Add(contentarray[0], contentarray[1]);
                            }
                            catch (ArgumentException) { }
                        }
                        if (trylist.ContainsKey(song))
                        {
                            var filepath = trylist[song];
                            Invoke(new Action(() =>
                            {
                                var time = GetSongDuration(trylist[song]);
                                trackBar1.Maximum = Convert.ToInt32(time.TotalSeconds);
                            }));
                            CurrentSongPath = filepath;
                            Player.FileName = filepath;
                            Player.Play();
                            SongEnds();
                        }
                    }
                }
                if (!gotsong)
                {
                    var text = "song not found, double check if it's in the right directory or in one of your playlists. current directory: ";
                    MessageBox.Show(text + textBox1.Text, "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception) { }
        }
        private void SongEnds()
        {
            var currentsecond = new int();
            var fullduration = new int();
            do
            {
                Thread.Sleep(100);
                currentsecond = Convert.ToInt32(Player.CurrentPosition);
                fullduration = Convert.ToInt32(GetSongDuration(CurrentSongPath).TotalSeconds - 0.5f);
                Invoke(new Action(() =>
                {
                    trackBar1.Value = currentsecond;
                }));
            } while (currentsecond != fullduration);
            Thread.Sleep(1060);
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            if (!Looping)
            {
                Player.Stop();
                if (Queue.Count != 1)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                    Invoke(new Action(UpdateDisplay));
                    PlayAudio(Queue[0]);
                    MusicThread.Abort();
                }
                if (Queue.Count != 0)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                }
                Invoke(new Action(UpdateDisplay));
                MusicThread.Abort();
            }
            else
            {
                Player.FileName = CurrentSongPath;
                Player.Play();
                SongEnds();
            }
        }
        private void NextSong()
        {
            try
            {
                var donesmth = false;
                Player.Stop();
                MusicThread.Abort();
                if (Queue.Count != 1 && donesmth == false)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                    MusicThread = new Thread(() => PlayAudio(Queue[0]));
                    MusicThread.Start();
                    donesmth = true;
                }
                if (Queue.Count != 0 && donesmth == false)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                    donesmth = true;
                }
            }
            catch (Exception) { }
        }
        private void PreviousSong()
        {
            try
            {
                if (!String.IsNullOrEmpty(CurrentPlaylist) && !IsPlaylist[0] == false)
                {
                    var songs = GetCustomPlaylistContent(CurrentPlaylist);
                    var songnames = new List<string>();
                    for (int i = 0; i < songs.Count; i++)
                    {
                        var songname = songs[i].Split(';');
                        songnames.Add(songname[0]);
                    }
                    var index = GetCurrentSongIndex();
                    if (index >= 1)
                    {
                        StopCurrentSong();
                        Invoke(new Action(() =>
                        {
                            trackBar1.Value = 0;
                        }));
                        SortLatestQueued(songnames[index - 1]);
                        MusicThread = new Thread(() => PlayAudio(Queue[0]));
                        MusicThread.Start();
                    }
                }
                UpdateDisplay();
            }
            catch (Exception) { }
        }
        private void AddtoQueue(string songname, bool isplaylist)
        {
            Queue.Add(songname);
            IsPlaylist.Add(isplaylist);
            var templist = new List<bool>();
            var tempqueue = new List<string>();
            if (IsPlaylist[IsPlaylist.Count - 1] == false && IsPlaylist.Contains(true))
            {
                int index = IsPlaylist.FindIndex(true.Equals);
                if (index == 0)
                {
                    for (int i = 1; i < IsPlaylist.Count; i++)
                    {
                        if (IsPlaylist[i].Equals(true))
                        {
                            index = i;
                            i = IsPlaylist.Count;
                        }
                    }
                }
                for (int i2 = index; i2 < IsPlaylist.Count - 1; i2++)
                {
                    templist.Add(IsPlaylist[i2]);
                    tempqueue.Add(Queue[i2]);
                }
                IsPlaylist.RemoveRange(index, IsPlaylist.Count - index - 1);
                Queue.RemoveRange(index, Queue.Count - index - 1);
                for (int i3 = 0; i3 < templist.Count; i3++)
                {
                    IsPlaylist.Add(templist[i3]);
                    Queue.Add(tempqueue[i3]);
                }
            }
        }
        private void PlayPLaylist(string playlistname)
        {
            Random random = new Random();
            var indices = new List<int>();
            for (int i = 0; i < IsPlaylist.Count; i++)
            {
                if (IsPlaylist[i].Equals(true))
                {
                    indices.Add(i);
                }
            }
            indices.Reverse();
            for(int i = 0; i < indices.Count; i++)
            {
                IsPlaylist.RemoveAt(indices[i]);
                Queue.RemoveAt(indices[i]);
            }
            var playlist = GetCustomPlaylistContent(playlistname);
            var count = playlist.Count;
            for (int i = 0; i < count; i++)
            {
                if (Shuffle == true)
                {
                    var randomIndex = random.Next(0, playlist.Count);
                    var songname = playlist[randomIndex].Split(';');
                    AddtoQueue(songname[0], true);
                    playlist.RemoveAt(randomIndex);
                }
                else
                {
                    var songname = playlist[i].Split(';');
                    AddtoQueue(songname[0], true);
                }
            }
            StopCurrentSong(); //this fixed everything
            UpdateDisplay();
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            MusicThread = new Thread(() => PlayAudio(Queue[0]));
            MusicThread.Start();
        }
        private void UpdateDisplay()
        {
            if (checkedListBox1.Visible == true)
            {
                checkedListBox1.Items.Clear();
                for (int i = 0; i < Queue.Count; i++)
                {
                    checkedListBox1.Items.Add(Queue[i]);
                }
            }
        }
        private TimeSpan GetSongDuration(string path)
        {
            try
            {
                using (var reader = new AudioFileReader(path))
                {
                    return reader.TotalTime;
                }
            }
            catch (Exception) 
            {
                return TimeSpan.FromSeconds(0);
            }
        }
        private List<string> GetSongPath(string dirpath)
        {
            var PathList = new List<string>();
            try
            {
                string[] paths = Directory.GetFiles(dirpath);
                for (int a = 0; a < paths.Length; a++)
                {
                    PathList.Add(paths[a]);
                }
                return PathList;
            }
            catch (Exception)
            {
                PathList.Add("");
                return PathList;
            }
        }
        private List<string> GetSongName(string dirpath)
        {
            var NameList = new List<string>();
            try
            {
                string[] files = Directory.GetFiles(dirpath);
                for (int a = 0; a < files.Length; a++)
                {
                    var fileExt = GetFileExt(files[a]);
                    string s1 = "\\";
                    string[] s2 = { "\\" };
                    switch (fileExt)
                    {
                        case "mp3":
                            s1 = s1 + ";" + ".mp3";
                            s2 = s1.Split(';');
                            break;
                        case "wav":
                            s1 = s1 + ";" + ".wav";
                            s2 = s1.Split(';');
                            break;
                        case "mp4":
                            s1 = s1 + ";" + ".mp4";
                            s2 = s1.Split(';');
                            break;
                        case "midi":
                            s1 = s1 + ";" + ".midi";
                            s2 = s1.Split(';');
                            break;
                        case "mid":
                            s1 = s1 + ";" + ".mid";
                            s2 = s1.Split(';');
                            break;
                        default:
                            goto skip;
                    }
                    var namesec = files[a].Split(s2, StringSplitOptions.None);
                    var songname = namesec[namesec.Length - 2];
                    NameList.Add(songname);
                skip:
                    Console.WriteLine();
                }
                return NameList;
            }
            catch (Exception)
            {
                NameList.Add("no songs here :(");
                return NameList;
            }
        }
        private static string GetFilename(string path)
        {
            var file = path.Split('\\');
            var extension = file[file.Length - 1].Split('.');
            var name = "";
            for (int j = 0; j < extension.Length - 1; j++)
            {
                if (extension.Length - 1 > 1 && j != extension.Length - 2)
                {
                    name = name + extension[j] + ".";
                }
                else
                {
                    name = name + extension[j];
                }
            }
            return name;
        }
        private static string GetFileExt(string filename)
        {
            var fileextension = filename.Split('.');
            return fileextension[fileextension.Length - 1];
        }
        private void CreatePlaylist(string name)
        {
            try
            {
                if (!String.IsNullOrEmpty(name))
                {
                    var playlist = new List<string>();
                    for (int a = 0; a < checkedListBox5.CheckedItems.Count; a++)
                    {
                        var songs = checkedListBox5.CheckedItems;
                        playlist.Add(songs[a].ToString() + ";" + PlayList0[songs[a].ToString()]);
                    }
                    CustomPlaylistAmount++;
                    var pathtoplaylist = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + CustomPlaylistAmount + ".txt"; 
                    var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
                    var CPAP = Directory.GetCurrentDirectory() + @"\cfg\customplaylistamount.txt";
                    CustomPlaylists.Add(name, "playlist " + CustomPlaylistAmount); 
                    var contentlist = new List<string>();
                    for (int i = 0; i < CustomPlaylists.Count; i++)
                    {
                        var content = CustomPlaylists.ElementAt(i).Key.ToString() + ";" + CustomPlaylists.ElementAt(i).Value.ToString();
                        contentlist.Add(content);
                    }
                    File.WriteAllText(CPAP, CustomPlaylistAmount.ToString());
                    File.WriteAllLines(pathtodic, contentlist);
                    File.WriteAllLines(pathtoplaylist, playlist);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                var caption = "missing input Exception";
                MessageBox.Show("the input was either null or empty", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AddSongToPlaylist(string playlist, string song, string path)
        {
            var programname = CustomPlaylists[playlist].ToString();
            var filename = ProgramToFilename(programname);
            var playlistcontent = GetCustomPlaylistContent(playlist);
            var filepath = Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename;
            playlistcontent.Add(song + ";" + path);
            File.WriteAllLines(filepath, playlistcontent);
        }
        private string ProgramToFilename(string programname)
        {
            var c = '.';
            string[] s = { "playlist " };
            var digitstep = programname.Split(c);
            var digit = digitstep[0].Split(s, StringSplitOptions.None);
            var filename = "playlist" + digit[1] + ".txt";
            return filename;
        }
        private string FileToProgramname(string filename) //not needed as of rn 30.11.24
        {
            string[] s = { "playlist", "." };
            var programname = filename.Split(s, StringSplitOptions.None);
            return programname[0];
        }
        private string GetCustomPlaylistPath(string currentplaylist)
        {
            try
            {
                if (CustomPlaylists.ContainsKey(currentplaylist)) 
                {
                    var filename = ProgramToFilename(CustomPlaylists[currentplaylist]);
                    var path = Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename;
                    return path;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                var caption = "missing argument Exception";
                MessageBox.Show("the playlist doesnt exist", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private List<string> GetCustomPlaylistContent(string playlistname)
        {
            try
            {
                var cplpath = GetCustomPlaylistPath(playlistname);
                var customplaylist = File.ReadAllLines(cplpath).ToList();
                return customplaylist;
            }
            catch (Exception)
            {
                var caption = "missing argument Exception";
                MessageBox.Show("the playlist doesnt exist", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void DisplayPlaylists()
        {
            checkedListBox2.Items.Clear();
            for (int i = 0; i < CustomPlaylists.Count; i++)
            {
                checkedListBox2.Items.Add(CustomPlaylists.ElementAt(i).Key);
            }
            checkedListBox2.Visible = true;
        }
        private void DisplaySongs(string playlistname) //not needed rn
        {
            var playlist = new List<string>();
            playlist = GetCustomPlaylistContent(playlistname);
            for (int a = 0; a < playlist.Count; a++)
            {
                var name = playlist[a].Split(';');
                checkedListBox3.Items.Add(name[0]);
            }
            checkedListBox3.Visible = true;
        }
        private void ShuffleCurrentPlaylist()
        {
            if (IsPlaylist.Contains(true))
            {
                Random random = new Random();
                var templist = new List<string>();
                var templist2 = new List<bool>();
                var count = Queue.Count;
                int index = IsPlaylist.FindIndex(true.Equals);
                //if index is first element in queue get the second true element
                if (index == 0)
                {
                    for (int i = 1; i < IsPlaylist.Count; i++)
                    {
                        if (IsPlaylist[i].Equals(true))
                        {
                            index = i;
                            i = IsPlaylist.Count;
                        }
                    }
                }
                for (int i = index; i < count; i++)
                {
                    templist.Add(Queue[i]);
                    templist2.Add(IsPlaylist[i]);
                }
                Queue.RemoveRange(index, Queue.Count - index);
                IsPlaylist.RemoveRange(index, IsPlaylist.Count - index);
                count = templist.Count;
                for (int i = 0; i < count; i++)
                {
                    var randomIndex = random.Next(0, templist.Count);
                    Queue.Add(templist[randomIndex]);
                    IsPlaylist.Add(templist2[randomIndex]);
                    templist.RemoveAt(randomIndex);
                    templist2.RemoveAt(randomIndex);
                }
                UpdateDisplay();
            }
        }
        private void UnShuffleCurrentPlaylist()
        {
            var programname = CustomPlaylists[CurrentPlaylist];
            var filename = ProgramToFilename(programname);
            var oldlist = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename).ToList();
            var inPLindex = GetCurrentSongIndex() + 1;
            var inQueueindex = IsPlaylist.IndexOf(true);
            if (inQueueindex == 0)
            {
                for (int i = 1; i < IsPlaylist.Count; i++)
                {
                    if (IsPlaylist[i].Equals(true))
                    {
                        inQueueindex = i;
                        i = IsPlaylist.Count;
                    }
                }
            }
            oldlist.RemoveRange(0, inPLindex);
            Queue.RemoveRange(inQueueindex, Queue.Count - inQueueindex);
            IsPlaylist.RemoveRange(inQueueindex, IsPlaylist.Count - inQueueindex);
            for (int i = 0; i < oldlist.Count; i++)
            {
                var songname = oldlist[i].Split(';');
                AddtoQueue(songname[0], true);
            }
            UpdateDisplay();
        }
        private int GetCurrentSongIndex()
        {
            var songs = GetCustomPlaylistContent(CurrentPlaylist);
            var songnames = new List<string>();
            for (int i = 0; i < songs.Count; i++)
            {
                var songname = songs[i].Split(';');
                songnames.Add(songname[0]);
            }
            var name = GetFilename(CurrentSongPath);
            var index = songnames.IndexOf(name);
            return index;
        }
        private void LoadStartUI(bool queuebug)
        {
            if (!queuebug == true)
            {
                checkedListBox1.Visible = false;
            }
            checkedListBox2.Visible = false;
            checkedListBox3.Visible = false;
            checkedListBox5.Visible = false;
            checkedListBox1.Items.Clear();
            checkedListBox2.Items.Clear();
            checkedListBox3.Items.Clear();
            checkedListBox5.Items.Clear();
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = true;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button14.Visible = true;
            button15.Visible = false;
            button16.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button19.Visible = true;
            button20.Visible = true;
            button21.Visible = true;
            button22.Visible = false;
            button23.Visible = false;
            button24.Visible = false;
            button25.Visible = false;
            button26.Visible = true;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            button36.Visible = true;
            textBox1.Visible = true;
            textBox3.Visible = true;
            textBox4.Visible = false;
            textBox5.Visible = false;
            richTextBox1.Visible = true;
            checkBox1.Visible = true;
            checkBox4.Visible = true;
            trackBar1.Visible = true;
            volumeSlider1.Visible = true;
        }
        private void DisableAllUIinPanel1()
        {
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button14.Visible = false;
            button15.Visible = false;
            button16.Visible = false;
            button17.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
            button22.Visible = false;
            button23.Visible = false;
            button24.Visible = false;
            button25.Visible = false;
            button26.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button29.Visible = false;
            button30.Visible = false;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            button36.Visible = true;
            checkedListBox1.Visible = false;
            checkedListBox2.Visible = false;
            checkedListBox3.Visible = false;
            checkedListBox5.Visible = false;
            textBox1.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            richTextBox1.Visible = false;
            checkBox1.Visible = false;
            checkBox4.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox5.Visible = false;
            trackBar1.Visible = false;
        }
        private void LoadUIinPanel2()
        {
            panel1.Visible = false;
            panel2.Visible = true;
            button27.Visible = true;
            button28.Visible = true;
            button29.Visible = true;
            button30.Visible = true;
            textBox2.Visible = true;
            textBox6.Visible = true;
            textBox7.Visible = true;
            richTextBox2.Visible = true;
            richTextBox2.Clear();
            checkedListBox4.Visible = true;
            checkedListBox4.Items.Clear();
            checkBox2.Visible = true;
            checkBox3.Visible = true;
            comboBox1.Visible = true;
            comboBox1.SelectedItem = File.ReadAllText(Directory.GetCurrentDirectory() + "\\cfg\\startup.txt");
        }
        private void UnloadUIinPanel2()
        {
            panel1.Visible = true;
            panel2.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button29.Visible = false;
            textBox2.Visible = false;
            textBox6.Visible = false;
            textBox7.Visible = false;
            richTextBox2.Visible = false;
            richTextBox2.Clear();
            checkedListBox4.Visible = false;
            checkedListBox4.Items.Clear();
            checkBox2.Visible = false;
            checkBox3.Visible = false;
            comboBox1.Visible = false;
        }

        private void LoadSettingsMenu()
        {
            LoadUIinPanel2();
            var picturebools = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\cfg\pictures.txt").ToList();
            string[] colorschemes = { "dark", "light", "pink", "purple", "blue", "green" };
            var currentcolor = File.ReadAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt");
            for (int i = 0; i < colorschemes.Length; i++)
            {
                checkedListBox4.Items.Add(colorschemes[i]);
                if (colorschemes[i] == currentcolor)
                {
                    checkedListBox4.SetItemChecked(i, true);
                }
            }
            if (picturebools[0] == "true")
            {
                checkBox3.Checked = true;
            }
            if (picturebools[1] == "true")
            {
                checkBox2.Checked = true;
            }
            ShowSizeOnDisk();
        }
        private void ShowSizeOnDisk()
        {
            var currentdirectory = File.ReadAllText(Directory.GetCurrentDirectory() + @"\cfg\directory.txt");
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var dirsize = CalculateDirSize(dir, false);
            var dirsizemb = dirsize / 1048576; //coverts bytes to megabytes
            richTextBox2.AppendText("midify: " + dirsizemb.ToString("F3") + " MB");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("temporary files: M/D");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            if (!Directory.Exists(currentdirectory))
            {
                richTextBox2.AppendText("main song dir: M/D");
            }
            else
            {
                DirectoryInfo dir2 = new DirectoryInfo(currentdirectory);
                var dir2size = CalculateDirSize(dir2, false);
                var dir2sizemb = dir2size / 1048576;
                richTextBox2.AppendText("main song dir: " + dir2sizemb.ToString("F3") + " MB");
            }
        }
        private double CalculateDirSize(DirectoryInfo dir, bool tempfilesize)
        {
            DirectoryInfo[] subFolders = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            double dirsize = new double();
            for (int i = 0; i < files.Length; i++)
            {
                if (GetFileExt(files[i].ToString()) != "wav" && tempfilesize == false|| GetFilename(files[i].ToString()) == "Astolfo" && tempfilesize == false || GetFileExt(files[i].ToString()) == "wav" && GetFilename(files[i].ToString()) != "Astolfo" && tempfilesize == true)
                {
                    dirsize = dirsize + files[i].Length;
                }
            }
            for (int i = 0; i < subFolders.Length; i++)
            {
                dirsize = dirsize + CalculateDirSize(subFolders[i], tempfilesize);
            }
            return dirsize;
        }
        private void LoadColorScheme(string color)
        {
            switch (color) //for some reason this throws a nullref when tried with Form1.ActiveForm. but works with this. i do not know why but it works
            {
                case "dark":
                    ChangeColor(this, System.Drawing.Color.DimGray);
                    this.BackColor = System.Drawing.Color.Black;
                    panel1.BackColor = System.Drawing.Color.Black;
                    panel2.BackColor = System.Drawing.Color.Black;
                    richTextBox1.BackColor = System.Drawing.Color.Black;
                    richTextBox1.ForeColor = System.Drawing.Color.White;
                    textBox2.ForeColor = System.Drawing.Color.White;
                    textBox5.ForeColor = System.Drawing.Color.White;
                    textBox6.ForeColor = System.Drawing.Color.White;
                    button26.BackgroundImage = midify.Properties.Resources.graysettings;
                    break;
                case "light":
                    ChangeColor(this, System.Drawing.SystemColors.Control);
                    this.BackColor = System.Drawing.SystemColors.Window;
                    panel1.BackColor = System.Drawing.SystemColors.Window;
                    panel2.BackColor = System.Drawing.SystemColors.Window;
                    richTextBox1.BackColor = System.Drawing.SystemColors.Window;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.graysettings;
                    break;
                case "pink":
                    ChangeColor(this, System.Drawing.Color.LightPink);
                    this.BackColor = System.Drawing.Color.HotPink;
                    panel1.BackColor = System.Drawing.Color.HotPink;
                    panel2.BackColor = System.Drawing.Color.HotPink;
                    richTextBox1.BackColor = System.Drawing.Color.HotPink;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.lightpinksettings;
                    break;
                case "purple":
                    ChangeColor(this, System.Drawing.Color.MediumSlateBlue);
                    this.BackColor = System.Drawing.Color.DarkSlateBlue;
                    panel1.BackColor = System.Drawing.Color.DarkSlateBlue;
                    panel2.BackColor = System.Drawing.Color.DarkSlateBlue;
                    richTextBox1.BackColor = System.Drawing.Color.DarkSlateBlue;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.purplesettings;
                    break;
                case "blue":
                    ChangeColor(this, System.Drawing.Color.LightSkyBlue);
                    this.BackColor = System.Drawing.Color.DarkBlue;
                    panel1.BackColor = System.Drawing.Color.DarkBlue;
                    panel2.BackColor = System.Drawing.Color.DarkBlue;
                    richTextBox1.BackColor = System.Drawing.Color.DarkBlue;
                    richTextBox1.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox2.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox5.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox6.ForeColor = System.Drawing.Color.LightSkyBlue;
                    button26.BackgroundImage = midify.Properties.Resources.lightbluesettings;
                    break;
                case "green":
                    ChangeColor(this, System.Drawing.Color.LightGreen);
                    this.BackColor = System.Drawing.Color.DarkGreen;
                    panel1.BackColor = System.Drawing.Color.DarkGreen;
                    panel2.BackColor = System.Drawing.Color.DarkGreen;
                    richTextBox1.BackColor = System.Drawing.Color.DarkGreen;
                    richTextBox1.ForeColor = System.Drawing.Color.LightGreen;
                    textBox2.ForeColor = System.Drawing.Color.LightGreen;
                    textBox5.ForeColor = System.Drawing.Color.LightGreen;
                    textBox6.ForeColor = System.Drawing.Color.LightGreen;
                    button26.BackgroundImage = midify.Properties.Resources.lightgreensettings;
                    break;
            }
            trackBar1.BackColor = panel1.BackColor;
            textBox2.BackColor = panel1.BackColor;
            textBox5.BackColor = panel1.BackColor;
            textBox6.BackColor = panel1.BackColor;
            button26.BackColor = System.Drawing.Color.Transparent;
            button27.BackColor = System.Drawing.Color.Red;
            button29.BackColor = System.Drawing.Color.Red;
            pictureBox1.BackColor = System.Drawing.Color.Transparent;
            pictureBox2.BackColor = System.Drawing.Color.Transparent;
            pictureBox3.BackColor = System.Drawing.Color.Transparent;
            pictureBox4.BackColor = System.Drawing.Color.Transparent;
            pictureBox5.BackColor = System.Drawing.Color.Transparent;
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt", color);
        }
        private void ChangeColor(Control parent, Color newcolor)
        {
            foreach (Control control in parent.Controls)
            {
                control.BackColor = newcolor;
                control.Invalidate(); 
                control.Update();
                if (control.HasChildren == true)
                {
                    ChangeColor(control, newcolor);
                }
            }
        }
        private void AstolfoVoice()
        {
            SoundPlayer astolfo = new SoundPlayer();
            astolfo.SoundLocation = Directory.GetCurrentDirectory() + @"\Astolfo.wav";
            astolfo.Load();
            astolfo.Play();
        }
        private void LoadPictures()
        {
            try
            {
                if (Pictures[0] == "true") //0 is Astolfo, 1 is Miku
                {
                    ShowAstolfo();
                }
                else
                {
                    pictureBox1.Visible = false;
                    pictureBox2.Visible = false;
                    pictureBox3.Visible = false;
                }
                if (Pictures[1] == "true")
                {
                    ShowMiku();
                }
                else
                {
                    pictureBox4.Visible = false;
                    pictureBox5.Visible = false;
                }
            }
            catch (Exception) { }
        }
        private void ShowAstolfo()
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox1.Image = midify.Properties.Resources.Astolfo1;
            pictureBox2.Image = midify.Properties.Resources.Astolfo1;
            pictureBox3.Image = midify.Properties.Resources.Astolfo2;
        }
        private void ShowMiku()
        {
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            pictureBox4.Image = midify.Properties.Resources.Miku1;
            pictureBox5.Image = midify.Properties.Resources.MikuBooth;
        }
        private void Uninstall()
        {
            var uninstallerexe = Directory.GetCurrentDirectory() + "\\midify uninstaller.exe";
            var desktopexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "midify uninstaller.exe");
            File.Move(uninstallerexe, desktopexe);
            Process Uninstaller = new Process();
            Uninstaller.StartInfo.FileName = desktopexe;
            Uninstaller.StartInfo.CreateNoWindow = false;
            Uninstaller.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Uninstaller.StartInfo.UseShellExecute = false;
            Uninstaller.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Uninstaller.Start();
            Application.Exit();
        }
        public static void AddApplicationToStartup(string exePath)
        {
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "midify.lnk");
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath; shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Save();
        }
        private void LoadQueueUI()
        {
            pictureBox2.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button31.Visible = true;
            button32.Visible = true;
            button33.Visible = true;
            checkedListBox1.Visible = true;
        }
        private void RemoveFromQueue(int index)
        {
            if (MusicThread != null && index == 0)
            {
                NextSong();
            }
            else
            {
                Queue.RemoveAt(index);
                IsPlaylist.RemoveAt(index);
            }
        }
        private string SelectPlaylist()
        {
            if (checkedListBox2.CheckedItems.Count == 1)
            {
                var playlist = checkedListBox2.CheckedItems[0].ToString();
                return playlist;
            }
            else
            {
                MessageBox.Show("select one playlist", "Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                for (int a = 0; a < checkedListBox3.Items.Count; a++)
                {
                    checkedListBox3.SetItemChecked(a, false);
                }
                return "";
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            UpdateDctnry();
            if (Pictures.ElementAt(0) == "true")
            {
                pictureBox3.Visible = true;
            }
        }
        private void button16_Click(object sender, EventArgs e)
        {
            for (int i1 = 0; i1 < checkedListBox2.CheckedItems.Count; i1++)
            {
                var songs = GetCustomPlaylistContent(checkedListBox2.CheckedItems[i1].ToString());
                for (int i2 = 0; i2 < songs.Count; i2++)
                {
                    var songname = songs[i2].Split(';');
                    AddtoQueue(songname[0], false); 
                }
            }
            for (int a = 0; a < checkedListBox2.Items.Count; a++)
            {
                checkedListBox2.SetItemChecked(a, false);
            }
            UpdateDisplay();
        }
        private void button15_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox3.CheckedItems.Count; i++)
            {
                AddtoQueue(checkedListBox3.CheckedItems[i].ToString(), false);
            }
            for (int a = 0; a < checkedListBox3.Items.Count; a++)
            {
                checkedListBox3.SetItemChecked(a, false);
            }
            UpdateDisplay();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = true;
            button7.Visible = true;
            button10.Visible = true;
            button13.Visible = true;
            button16.Visible = true;
            button17.Visible = false;
            button18.Visible = false;
            checkedListBox1.Visible = false;
            DisplayPlaylists();
        }
        private void button10_Click(object sender, EventArgs e)
        {
            for (int a = 0; a < checkedListBox2.CheckedItems.Count; a++)
            {
                var currentplaylist = checkedListBox2.CheckedItems;
                var cplpath = GetCustomPlaylistPath(currentplaylist[a].ToString()); 
                if (cplpath != null)
                {
                    string[] s1 = { "\\", ".txt" };
                    var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
                    var contentlist = new List<string>();
                    var namesec = cplpath.Split(s1, StringSplitOptions.None);
                    var filename = namesec[namesec.Length - 2];
                    string[] s2 = { "playlist" };
                    var index = filename.Split(s2, StringSplitOptions.None);
                    DeletedIndices.Add(index[1]);
                    CustomPlaylists.Remove(currentplaylist[a].ToString());
                    File.WriteAllLines(Directory.GetCurrentDirectory() + @"\cfg\deletedindices.txt", DeletedIndices);
                    for (int i = 0; i < CustomPlaylists.Count; i++)
                    {
                        var content = CustomPlaylists.ElementAt(i).Key.ToString() + ";" + CustomPlaylists.ElementAt(i).Value.ToString();
                        contentlist.Add(content);
                    }
                    File.WriteAllLines(pathtodic, contentlist);
                    File.Delete(cplpath);
                }
                else
                {
                    MessageBox.Show("something went wrong");
                }
            }
            DisplayPlaylists();
        }
        private void button12_Click(object sender, EventArgs e)
        {
            var indices = new List<int>();
            var currentplaylist = checkedListBox2.CheckedItems;
            var playlistcontent = GetCustomPlaylistContent(currentplaylist[0].ToString());
            var playlistpath = GetCustomPlaylistPath(currentplaylist[0].ToString());
            var index = checkedListBox3.CheckedIndices;
            for (int a = 0; a < index.Count; a++)
            {
                indices.Add(index[a]);
            }
            indices.Sort();
            for (int a = checkedListBox3.CheckedItems.Count; a > 0; a--)
            {
                playlistcontent.RemoveAt(indices[a - 1]);
            }
            File.WriteAllLines(playlistpath, playlistcontent);
            checkedListBox3.Items.Clear();
            for (int a = 0; a < playlistcontent.Count; a++)
            {
                var name = playlistcontent[a].Split(';');
                checkedListBox3.Items.Add(name[0]);
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (checkedListBox2.CheckedItems.Count == 1)
            {
                var Playlistname = checkedListBox2.CheckedItems[0].ToString();
                CurrentPlaylist = Playlistname;
                PlayPLaylist(Playlistname);
            }
            else
            {
                MessageBox.Show("select one playlist to listen to (you can queue the rest)", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for (int a = 0; a < checkedListBox2.Items.Count; a++)
            {
                checkedListBox2.SetItemChecked(a, false);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var playlist = new List<string>();
                if (checkedListBox2.CheckedItems.Count == 1 && playlist != null)
                {
                    playlist = GetCustomPlaylistContent(checkedListBox2.CheckedItems[0].ToString());
                    CurrentPlaylist = checkedListBox2.CheckedItems[0].ToString();
                    button5.Visible = false;
                    button7.Visible = false;
                    button8.Visible = true;
                    button10.Visible = false;
                    button11.Visible = true;
                    button12.Visible = true;
                    button13.Visible = false;
                    button15.Visible = true;
                    button16.Visible = false;
                    button23.Visible = true;
                    button17.Visible = false;
                    button18.Visible = false;
                    checkedListBox3.Items.Clear();
                    checkedListBox2.Visible = false;
                    checkedListBox3.Visible = true;
                    for (int a = 0; a < playlist.Count; a++)
                    {
                        var name = playlist[a].Split(';');
                        checkedListBox3.Items.Add(name[0]);
                    }
                }
                else
                {
                    MessageBox.Show("select one playlist", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    for (int a = 0; a < checkedListBox2.Items.Count; a++)
                    {
                        checkedListBox2.SetItemChecked(a, false);
                    }
                }
                
            }
            catch (Exception)
            {
                MessageBox.Show("couldn't find playlist", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                for (int a = 0; a < checkedListBox2.Items.Count; a++)
                {
                    checkedListBox2.SetItemChecked(a, false);
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (checkedListBox3.CheckedItems.Count == 1)
            {
                PlaySong(checkedListBox3.CheckedItems[0].ToString());
            }
            else
            {
                MessageBox.Show("select one song to listen to (you can queue the rest)", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for (int a = 0; a < checkedListBox3.Items.Count; a++)
            {
                checkedListBox3.SetItemChecked(a, false);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            checkedListBox2.Items.Clear();
            checkedListBox2.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button5.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button10.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button16.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button23.Visible = false;
            checkBox4.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var song = textBox3.Text;
            textBox3.Text = "";
            PlaySong(song);
            UpdateDisplay();
        }
        private void button14_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            UpdateDisplay();
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            checkedListBox5.Items.Clear();
            button4.Visible = true;
            for (int a = 0; a < PlayList0.Count; a++)
            {
                checkedListBox5.Items.Add(PlayList0.ElementAt(a).Key);
            }
            checkedListBox5.Visible = true;
            button2.Visible = false;
            button3.Visible = false;
            button6.Visible = true;
            button17.Visible = false;
            button18.Visible = false;
        }
        private void button22_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            CreatePlaylist(textBox4.Text);
            textBox4.Clear();
            for (int a = 0; a < checkedListBox5.Items.Count; a++)
            {
                checkedListBox5.SetItemChecked(a, false);
            }
            checkedListBox5.Visible = false;
            checkedListBox5.Items.Clear();
            button6.Visible = false;
            button3.Visible = true;
            button4.Visible = false;
            button2.Visible = true;
            button17.Visible = true;
            button18.Visible = true;
            textBox4.Visible = false;
            textBox5.Visible = false;
            button22.Visible = false;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            checkedListBox2.ClearSelected();
            checkedListBox1.Visible = false;
            button4.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = true;
            textBox4.Visible = true;
            textBox5.Visible = true;
            button22.Visible = true;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            checkedListBox1.Items.Clear();
            checkedListBox5.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = false;
            button6.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button22.Visible = false;
        }
        private void button11_Click(object sender, EventArgs e)
        {
            button5.Visible = true;
            button7.Visible = true;
            button8.Visible = false;
            button10.Visible = true;
            button11.Visible = false;
            button13.Visible = true;
            button15.Visible = false;
            button16.Visible = true;
            button17.Visible = true;
            button18.Visible = true;
            button23.Visible = false;
            checkedListBox2.Visible = true;
            checkedListBox3.Visible = false;
            checkedListBox3.Items.Clear();
            DisplayPlaylists();
        }
        private void button17_Click(object sender, EventArgs e)
        {
            LoadStartUI(true);
            checkedListBox1.Items.Clear();
            if (checkedListBox1.Visible == false)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    checkedListBox1.Items.Add(Queue[i]);
                }
                LoadQueueUI();
            }
            else
            {
                if (Pictures[0] == "true")
                {
                    pictureBox2.Visible = true;
                }
                checkedListBox1.Visible = false;
                checkedListBox2.Visible = false;
                button2.Visible = true;
                button3.Visible = true;
                button31.Visible = false;
                button32.Visible = false;
                button33.Visible = false;
                button34.Visible = false;
                button35.Visible = false;
            }
        }
        private void button18_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            checkedListBox1.Items.Clear();
            Queue.Clear();
            IsPlaylist.Clear();
            UpdateDisplay();
        }
        private void button20_Click(object sender, EventArgs e)
        {
            NextSong();
            UpdateDisplay();
        }
        private void button19_Click(object sender, EventArgs e)
        {
            PreviousSong();
        }
        private void button21_Click(object sender, EventArgs e)
        {
            MusicThread = new Thread(() => Nothing()); //this is so stupid...
            if (MusicThread == null && Queue.Count > 0 || MusicThread.IsAlive != true && Queue.Count > 0)
            {
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start();
            }
        }
        private void Nothing(){}
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                Shuffle = true;
                if (IsPlaylist.Contains(true))
                {
                    ShuffleCurrentPlaylist();
                }
            }
            else
            {
                Shuffle = false;
                if (IsPlaylist.Contains(true))
                {
                    UnShuffleCurrentPlaylist();
                }
            }
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                Looping = true;
            }
            else
            {
                Looping = false;
            }
        }
        private void button23_Click(object sender, EventArgs e)
        {
            checkedListBox3.Visible = false;
            checkedListBox5.Items.Clear();
            for (int a = 0; a < PlayList0.Count; a++)
            {
                checkedListBox5.Items.Add(PlayList0.ElementAt(a).Key);
            }
            checkedListBox5.Visible = true;
            button8.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button15.Visible = false;
            button16.Visible = false;
            button23.Visible = false;
            button24.Visible = true;
            button25.Visible = true;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            for (int i = 0; i < checkedListBox5.CheckedItems.Count; i++)
            {
                AddSongToPlaylist(CurrentPlaylist, checkedListBox5.CheckedItems[i].ToString(), PlayList0[checkedListBox5.CheckedItems[i].ToString().ToString()]);
            }
            LoadStartUI(false);
        }
        private void button25_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            LoadStartUI(false);
        }
        private void button26_Click(object sender, EventArgs e)
        {
            DisableAllUIinPanel1();
            LoadSettingsMenu();
        }
        private void button27_Click(object sender, EventArgs e)
        {
            Resetcfg();
        }
        private void button28_Click(object sender, EventArgs e)
        {
            if (checkedListBox4.CheckedItems.Count != 1)
            {
                MessageBox.Show("select one color", "Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var color = checkedListBox4.CheckedItems[0].ToString();
                File.WriteAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt", color);
                LoadColorScheme(color);
            }
        }
        private void button29_Click(object sender, EventArgs e)
        {
            Uninstall();
        }
        private void button30_Click(object sender, EventArgs e)
        {
            UnloadUIinPanel2();
            LoadStartUI(false);
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox3.Visible = false;
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            StopCurrentSong(); 
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            Thread AstolfoThread = new Thread(() => AstolfoVoice());
            AstolfoThread.Start();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            StopCurrentSong(); 
            Invoke(new Action(() =>
            {
                trackBar1.Value = 0;
            }));
            Thread AstolfoThread = new Thread(() => AstolfoVoice());
            AstolfoThread.Start();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var content = File.ReadAllLines(path).ToList();
            var Miku = content[1].ToString();
            if (checkBox3.Checked != true)
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = false;
                pictureBox3.Visible = false;
                content.Clear();
                content.Add("false");
                content.Add(Miku);
            }
            else
            {
                ShowAstolfo();
                content.Clear();
                content.Add("true");
                content.Add(Miku);
            }
            File.WriteAllLines(path, content);
            Pictures = File.ReadAllLines(path).ToList();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var content = File.ReadAllLines(path).ToList();
            content.RemoveAt(1);
            if (checkBox2.Checked != true)
            {
                pictureBox4.Visible = false;
                pictureBox5.Visible = false;
                content.Add("false");
            }
            else
            {
                ShowMiku();
                content.Add("true");
            }
            File.WriteAllLines(path, content);
            Pictures = File.ReadAllLines(path).ToList();
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text.Contains("search for song.."))
            {
                var userChar = textBox3.Text.Split('.');
                textBox3.Clear();
                if (userChar[userChar.Length - 1] != " ")
                {
                    textBox3.AppendText(userChar[userChar.Length - 1]);
                }
            }
        }
        private void checkedListBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox4.Items.Count; i++)
            {
                checkedListBox4.SetItemChecked(i, false);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.TextChanged += ComboBox1_TextChanged;
            var item = comboBox1.SelectedItem;
            if (item.ToString() == "yes")
            {
                AddApplicationToStartup(Directory.GetCurrentDirectory() + "\\midify.exe");
            }
            else
            {
                var shortcutpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "midify.lnk");
                if (File.Exists(shortcutpath))
                {
                    File.Delete(shortcutpath);
                }
            }
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\cfg\\startup.txt", item.ToString());
        }
        private void ComboBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox1.ResetText();
        }
        private void button31_Click(object sender, EventArgs e)
        {
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = true;
            button35.Visible = true;
            checkedListBox2.Visible = true;
            DisplayPlaylists();
        }
        private void button32_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                AddtoQueue(checkedListBox1.CheckedItems[i].ToString(), false);
            }
            UpdateDisplay();
        }
        private void button33_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                RemoveFromQueue(checkedListBox1.CheckedIndices[i] - i);
            }
            UpdateDisplay();
        }
        private void button34_Click(object sender, EventArgs e)
        {
            button2.Visible = true;
            button3.Visible = true;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            checkedListBox1.Visible = false;
            checkedListBox2.Visible = false;
            var playlist = SelectPlaylist();
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                var path = "";
                if (PlayList0.ContainsKey(checkedListBox1.CheckedItems[i].ToString()))
                {
                    path = PlayList0[checkedListBox1.CheckedItems[i].ToString()];
                }
                else
                {
                    for (int j = 1; j <= CustomPlaylists.Count; j++)
                    {
                        var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + j + ".txt";
                        if (File.Exists(listpath))
                        {
                            var trylist = new Dictionary<string, string>();
                            var filecontent = File.ReadAllLines(listpath).ToList();
                            for (int k = 0; k < filecontent.Count; k++)
                            {
                                var contentarray = filecontent[k].Split(';');
                                try
                                {
                                    trylist.Add(contentarray[0], contentarray[1]);
                                }
                                catch (ArgumentException) { }
                            }
                            if (trylist.ContainsKey(checkedListBox1.CheckedItems[i].ToString()))
                            {
                                path = trylist[checkedListBox1.CheckedItems[i].ToString()];
                            }
                        }
                    }
                }
                AddSongToPlaylist(playlist, checkedListBox1.CheckedItems[i].ToString(), path);
            }
        }
        private void button35_Click(object sender, EventArgs e)
        {
            LoadStartUI(false);
        }
        private void button36_Click(object sender, EventArgs e)
        {
            if (Player.PlayState == MediaPlayer.MPPlayStateConstants.mpPlaying)
            {
                if (MusicThread != null)
                {
                    Player.Pause();
                }
            }
            else if (Player.PlayState == MediaPlayer.MPPlayStateConstants.mpPaused)
            {
                Player.Play();
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Player.CurrentPosition = trackBar1.Value;
        }
        private void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            Player.Volume = Convert.ToInt32(-5000 * volumeSlider1.Volume);
        }
    }
}
namespace midify
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button30 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.button28 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.checkedListBox4 = new System.Windows.Forms.CheckedListBox();
            this.button27 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.checkedListBox3 = new System.Windows.Forms.CheckedListBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button18 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.volumeSlider1 = new NAudio.Gui.VolumeSlider();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.button36 = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.button33 = new System.Windows.Forms.Button();
            this.button32 = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button35 = new System.Windows.Forms.Button();
            this.checkedListBox5 = new System.Windows.Forms.CheckedListBox();
            this.button34 = new System.Windows.Forms.Button();
            this.button31 = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Window;
            this.panel2.Controls.Add(this.textBox2);
            this.panel2.Controls.Add(this.comboBox1);
            this.panel2.Controls.Add(this.pictureBox4);
            this.panel2.Controls.Add(this.checkBox3);
            this.panel2.Controls.Add(this.checkBox2);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.button30);
            this.panel2.Controls.Add(this.button29);
            this.panel2.Controls.Add(this.richTextBox2);
            this.panel2.Controls.Add(this.textBox7);
            this.panel2.Controls.Add(this.button28);
            this.panel2.Controls.Add(this.textBox6);
            this.panel2.Controls.Add(this.checkedListBox4);
            this.panel2.Controls.Add(this.button27);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Window;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.ForeColor = System.Drawing.Color.Black;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            resources.GetString("comboBox1.Items"),
            resources.GetString("comboBox1.Items1")});
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox4, "pictureBox4");
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.TabStop = false;
            // 
            // checkBox3
            // 
            resources.ApplyResources(this.checkBox3, "checkBox3");
            this.checkBox3.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.UseVisualStyleBackColor = false;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox2
            // 
            resources.ApplyResources(this.checkBox2, "checkBox2");
            this.checkBox2.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.UseVisualStyleBackColor = false;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // button30
            // 
            this.button30.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button30, "button30");
            this.button30.ForeColor = System.Drawing.Color.Black;
            this.button30.Name = "button30";
            this.button30.UseVisualStyleBackColor = false;
            this.button30.Click += new System.EventHandler(this.button30_Click);
            // 
            // button29
            // 
            this.button29.BackColor = System.Drawing.Color.Red;
            this.button29.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.button29, "button29");
            this.button29.ForeColor = System.Drawing.Color.White;
            this.button29.Name = "button29";
            this.button29.UseVisualStyleBackColor = false;
            this.button29.Click += new System.EventHandler(this.button29_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox2.DetectUrls = false;
            resources.ApplyResources(this.richTextBox2, "richTextBox2");
            this.richTextBox2.ForeColor = System.Drawing.Color.Black;
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.SystemColors.Window;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox7, "textBox7");
            this.textBox7.ForeColor = System.Drawing.Color.Black;
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            // 
            // button28
            // 
            this.button28.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button28, "button28");
            this.button28.ForeColor = System.Drawing.Color.Black;
            this.button28.Name = "button28";
            this.button28.UseVisualStyleBackColor = false;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Window;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox6, "textBox6");
            this.textBox6.ForeColor = System.Drawing.Color.Black;
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            // 
            // checkedListBox4
            // 
            this.checkedListBox4.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.checkedListBox4, "checkedListBox4");
            this.checkedListBox4.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox4.FormattingEnabled = true;
            this.checkedListBox4.Name = "checkedListBox4";
            this.checkedListBox4.SelectedIndexChanged += new System.EventHandler(this.checkedListBox4_SelectedIndexChanged);
            // 
            // button27
            // 
            this.button27.BackColor = System.Drawing.Color.Red;
            this.button27.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.button27, "button27");
            this.button27.ForeColor = System.Drawing.Color.White;
            this.button27.Name = "button27";
            this.button27.UseVisualStyleBackColor = false;
            this.button27.Click += new System.EventHandler(this.button27_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.DetectUrls = false;
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.ForeColor = System.Drawing.Color.Black;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.textBox3, "textBox3");
            this.textBox3.ForeColor = System.Drawing.Color.Black;
            this.textBox3.Name = "textBox3";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button2, "button2");
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button3, "button3");
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button4, "button4");
            this.button4.ForeColor = System.Drawing.Color.Black;
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button5, "button5");
            this.button5.ForeColor = System.Drawing.Color.Black;
            this.button5.Name = "button5";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button6, "button6");
            this.button6.ForeColor = System.Drawing.Color.Black;
            this.button6.Name = "button6";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button7, "button7");
            this.button7.ForeColor = System.Drawing.Color.Black;
            this.button7.Name = "button7";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button8, "button8");
            this.button8.ForeColor = System.Drawing.Color.Black;
            this.button8.Name = "button8";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // checkedListBox3
            // 
            this.checkedListBox3.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox3.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox3.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox3, "checkedListBox3");
            this.checkedListBox3.Name = "checkedListBox3";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox1.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox1.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox1, "checkedListBox1");
            this.checkedListBox1.Name = "checkedListBox1";
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.SystemColors.Control;
            this.button9.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button9, "button9");
            this.button9.Name = "button9";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button10, "button10");
            this.button10.ForeColor = System.Drawing.Color.Black;
            this.button10.Name = "button10";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button11, "button11");
            this.button11.ForeColor = System.Drawing.Color.Black;
            this.button11.Name = "button11";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button12, "button12");
            this.button12.ForeColor = System.Drawing.Color.Black;
            this.button12.Name = "button12";
            this.button12.UseVisualStyleBackColor = false;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // checkedListBox2
            // 
            this.checkedListBox2.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox2.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox2.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox2, "checkedListBox2");
            this.checkedListBox2.Name = "checkedListBox2";
            // 
            // button13
            // 
            this.button13.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button13, "button13");
            this.button13.ForeColor = System.Drawing.Color.Black;
            this.button13.Name = "button13";
            this.button13.UseVisualStyleBackColor = false;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.BackColor = System.Drawing.SystemColors.Control;
            this.button14.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button14, "button14");
            this.button14.Name = "button14";
            this.button14.UseVisualStyleBackColor = false;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button15
            // 
            this.button15.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button15, "button15");
            this.button15.ForeColor = System.Drawing.Color.Black;
            this.button15.Name = "button15";
            this.button15.UseVisualStyleBackColor = false;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // button16
            // 
            this.button16.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button16, "button16");
            this.button16.ForeColor = System.Drawing.Color.Black;
            this.button16.Name = "button16";
            this.button16.UseVisualStyleBackColor = false;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // button17
            // 
            this.button17.BackColor = System.Drawing.SystemColors.Control;
            this.button17.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button17, "button17");
            this.button17.Name = "button17";
            this.button17.UseVisualStyleBackColor = false;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.ForeColor = System.Drawing.Color.Black;
            this.textBox1.Name = "textBox1";
            // 
            // button18
            // 
            this.button18.BackColor = System.Drawing.SystemColors.Control;
            this.button18.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button18, "button18");
            this.button18.Name = "button18";
            this.button18.UseVisualStyleBackColor = false;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // button20
            // 
            this.button20.BackColor = System.Drawing.SystemColors.Control;
            this.button20.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button20, "button20");
            this.button20.Name = "button20";
            this.button20.UseVisualStyleBackColor = false;
            this.button20.Click += new System.EventHandler(this.button20_Click);
            // 
            // button19
            // 
            this.button19.BackColor = System.Drawing.SystemColors.Control;
            this.button19.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button19, "button19");
            this.button19.Name = "button19";
            this.button19.UseVisualStyleBackColor = false;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // button21
            // 
            this.button21.BackColor = System.Drawing.SystemColors.Control;
            this.button21.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button21, "button21");
            this.button21.Name = "button21";
            this.button21.UseVisualStyleBackColor = false;
            this.button21.Click += new System.EventHandler(this.button21_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox1.ForeColor = System.Drawing.Color.Black;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.textBox4, "textBox4");
            this.textBox4.ForeColor = System.Drawing.Color.Black;
            this.textBox4.Name = "textBox4";
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Window;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox5, "textBox5");
            this.textBox5.ForeColor = System.Drawing.Color.Black;
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            // 
            // button22
            // 
            this.button22.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button22, "button22");
            this.button22.ForeColor = System.Drawing.Color.Black;
            this.button22.Name = "button22";
            this.button22.UseVisualStyleBackColor = false;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // button23
            // 
            this.button23.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button23, "button23");
            this.button23.ForeColor = System.Drawing.Color.Black;
            this.button23.Name = "button23";
            this.button23.UseVisualStyleBackColor = false;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // button24
            // 
            this.button24.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button24, "button24");
            this.button24.ForeColor = System.Drawing.Color.Black;
            this.button24.Name = "button24";
            this.button24.UseVisualStyleBackColor = false;
            this.button24.Click += new System.EventHandler(this.button24_Click);
            // 
            // button25
            // 
            this.button25.BackColor = System.Drawing.SystemColors.Control;
            this.button25.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.button25, "button25");
            this.button25.ForeColor = System.Drawing.Color.Black;
            this.button25.Name = "button25";
            this.button25.UseVisualStyleBackColor = false;
            this.button25.Click += new System.EventHandler(this.button25_Click);
            // 
            // button26
            // 
            this.button26.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.button26, "button26");
            this.button26.FlatAppearance.BorderColor = System.Drawing.Color.DarkSlateBlue;
            this.button26.FlatAppearance.BorderSize = 0;
            this.button26.ForeColor = System.Drawing.Color.Black;
            this.button26.Name = "button26";
            this.button26.UseVisualStyleBackColor = false;
            this.button26.Click += new System.EventHandler(this.button26_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.volumeSlider1);
            this.panel1.Controls.Add(this.textBox3);
            this.panel1.Controls.Add(this.button12);
            this.panel1.Controls.Add(this.button10);
            this.panel1.Controls.Add(this.pictureBox5);
            this.panel1.Controls.Add(this.button17);
            this.panel1.Controls.Add(this.button18);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.checkBox4);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.button20);
            this.panel1.Controls.Add(this.button36);
            this.panel1.Controls.Add(this.button19);
            this.panel1.Controls.Add(this.trackBar1);
            this.panel1.Controls.Add(this.button33);
            this.panel1.Controls.Add(this.button32);
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.button26);
            this.panel1.Controls.Add(this.button23);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.button14);
            this.panel1.Controls.Add(this.button9);
            this.panel1.Controls.Add(this.button6);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Controls.Add(this.button25);
            this.panel1.Controls.Add(this.textBox5);
            this.panel1.Controls.Add(this.textBox4);
            this.panel1.Controls.Add(this.button16);
            this.panel1.Controls.Add(this.button15);
            this.panel1.Controls.Add(this.button13);
            this.panel1.Controls.Add(this.button11);
            this.panel1.Controls.Add(this.button8);
            this.panel1.Controls.Add(this.button7);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.checkedListBox1);
            this.panel1.Controls.Add(this.checkedListBox3);
            this.panel1.Controls.Add(this.button35);
            this.panel1.Controls.Add(this.checkedListBox2);
            this.panel1.Controls.Add(this.checkedListBox5);
            this.panel1.Controls.Add(this.button34);
            this.panel1.Controls.Add(this.button31);
            this.panel1.Controls.Add(this.button24);
            this.panel1.Controls.Add(this.button22);
            this.panel1.Controls.Add(this.button21);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // volumeSlider1
            // 
            this.volumeSlider1.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.volumeSlider1, "volumeSlider1");
            this.volumeSlider1.ForeColor = System.Drawing.SystemColors.Window;
            this.volumeSlider1.Name = "volumeSlider1";
            this.volumeSlider1.Volume = 0.065F;
            this.volumeSlider1.VolumeChanged += new System.EventHandler(this.volumeSlider1_VolumeChanged);
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox5, "pictureBox5");
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.TabStop = false;
            // 
            // checkBox4
            // 
            resources.ApplyResources(this.checkBox4, "checkBox4");
            this.checkBox4.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox4.ForeColor = System.Drawing.Color.Black;
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.UseVisualStyleBackColor = false;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // button36
            // 
            this.button36.BackColor = System.Drawing.SystemColors.Control;
            this.button36.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button36, "button36");
            this.button36.Name = "button36";
            this.button36.UseVisualStyleBackColor = false;
            this.button36.Click += new System.EventHandler(this.button36_Click);
            // 
            // trackBar1
            // 
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.LargeChange = 1;
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.TickFrequency = 100;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // button33
            // 
            this.button33.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button33, "button33");
            this.button33.ForeColor = System.Drawing.Color.Black;
            this.button33.Name = "button33";
            this.button33.UseVisualStyleBackColor = false;
            this.button33.Click += new System.EventHandler(this.button33_Click);
            // 
            // button32
            // 
            this.button32.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button32, "button32");
            this.button32.ForeColor = System.Drawing.Color.Black;
            this.button32.Name = "button32";
            this.button32.UseVisualStyleBackColor = false;
            this.button32.Click += new System.EventHandler(this.button32_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // button35
            // 
            this.button35.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button35, "button35");
            this.button35.ForeColor = System.Drawing.Color.Black;
            this.button35.Name = "button35";
            this.button35.UseVisualStyleBackColor = false;
            this.button35.Click += new System.EventHandler(this.button35_Click);
            // 
            // checkedListBox5
            // 
            this.checkedListBox5.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox5.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox5.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox5, "checkedListBox5");
            this.checkedListBox5.Name = "checkedListBox5";
            // 
            // button34
            // 
            this.button34.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button34, "button34");
            this.button34.ForeColor = System.Drawing.Color.Black;
            this.button34.Name = "button34";
            this.button34.UseVisualStyleBackColor = false;
            this.button34.Click += new System.EventHandler(this.button34_Click);
            // 
            // button31
            // 
            this.button31.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button31, "button31");
            this.button31.ForeColor = System.Drawing.Color.Black;
            this.button31.Name = "button31";
            this.button31.UseVisualStyleBackColor = false;
            this.button31.Click += new System.EventHandler(this.button31_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Button button28;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.CheckedListBox checkedListBox4;
        private System.Windows.Forms.Button button27;
        private System.Windows.Forms.Button button29;
        private System.Windows.Forms.Button button30;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckedListBox checkedListBox3;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.CheckedListBox checkedListBox2;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckedListBox checkedListBox5;
        private System.Windows.Forms.Button button33;
        private System.Windows.Forms.Button button32;
        private System.Windows.Forms.Button button31;
        private System.Windows.Forms.Button button35;
        private System.Windows.Forms.Button button34;
        private System.Windows.Forms.Button button36;
        public System.Windows.Forms.TrackBar trackBar1;
        private NAudio.Gui.VolumeSlider volumeSlider1;
    }
}
using System;
using System.Windows.Forms;
using System.IO;
namespace midify
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += new EventHandler(ApplicationExits);
            Application.Run(new Form1());
        }
        private static void ApplicationExits(object sender, EventArgs e)
        {
            Form1.StopCurrentSong();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Net;
using System.Diagnostics;
using NAudio.Wave;
namespace midify
{
    public partial class Form1 : Form
    {
        private IDictionary<string, string> CustomPlaylists = new Dictionary<string, string>();
        private IDictionary<string, string> PlayList0 = new Dictionary<string, string>();
        private static readonly SoundPlayer Player = new SoundPlayer();
        private List<string> DeletedIndices = new List<string>();
        private static List<bool> IsPlaylist = new List<bool>();
        private static List<string> Queue = new List<string>();
        private readonly WebClient Client = new WebClient();
        private List<string> Pictures = new List<string>();
        private int CustomPlaylistAmount;
        private string CurrentPlaylist;
        private bool Shuffle = false;
        private bool Looping = false;
        private string Colorscheme;
        private delegate void LoadingHandler();
        private event LoadingHandler LoadingFinished;
        private event LoadingHandler LoadedSong;
        public static List<string> LoadedSongs = new List<string>();
        public static Thread MusicThread;
        public static string WavFilePath;
        public static Thread Loading;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                LoadedSong += new LoadingHandler(SongLoaded);
                LoadingFinished += new LoadingHandler(DirectoryLoaded);
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Astolfo.wav"))
                {
                    var url = new Uri("https://drive.google.com/uc?export=download&id=1YcaicczB6nuxIWcg_wILmPx6SgLxBLlQ");
                    Client.DownloadFile(url, "Astolfo.wav");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\midify uninstaller.exe"))
                {
                    var url = new Uri("https://drive.google.com/uc?export=download&id=1ovQO_W8BslPYlpPiy_65Qn2oVIzA_8x-"); //add uninstaller download link
                    Client.DownloadFile(url, "midify uninstaller.zip");
                    ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\midify uninstaller.zip", Directory.GetCurrentDirectory());
                    File.Delete(Directory.GetCurrentDirectory() + "\\midify uninstaller.zip");
                    Directory.Delete(Directory.GetCurrentDirectory() + "\\midify uninstaller");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\midify restarter.exe"))
                {
                    var url = new Uri("https://drive.google.com/uc?export=download&id=1xR1xDFhrQZVseM9tIjIS9vQm8jhNbYGF"); //add restarter download link
                    Client.DownloadFile(url, "midify restarter.zip");
                    ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\midify restarter.zip", Directory.GetCurrentDirectory());
                    File.Delete(Directory.GetCurrentDirectory() + "\\midify restarter.zip");
                    Directory.Delete(Directory.GetCurrentDirectory() + "\\midify restarter");
                }
                Onload();
            }
            catch (Exception)
            {
                Onload();
            }
        }
        private void Onload()
        {
            var Colorpath = Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt";
            checkedListBox1.CheckOnClick = true;
            checkedListBox2.CheckOnClick = true;
            checkedListBox3.CheckOnClick = true;
            checkedListBox4.CheckOnClick = true;
            checkedListBox5.CheckOnClick = true;
            this.DoubleBuffered = true;
            this.Text = "midify";
            richTextBox1.Text = "all songs:";
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(Environment.NewLine);
            panel1.Visible = true;
            Setupcfg();
            Loading = new Thread(() => LoadDir(textBox1.Text));
            Loading.Start();
            LoadStartUI(false);
            LoadColorScheme(Colorscheme);
            LoadPictures();
        }
        private void Setupcfg()
        {
            var path2file = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt";
            var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
            var pathfilepath = Directory.GetCurrentDirectory() + @"\cfg\directory.txt";
            var DIpath = Directory.GetCurrentDirectory() + @"\cfg\deletedindices.txt";
            var Colorpath = Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt";
            var CPAP = Directory.GetCurrentDirectory() + @"\cfg\customplaylistamount.txt";
            var PlaylistDir = Directory.GetCurrentDirectory() + @"\cfg\playlists";
            var ConfigDir = Directory.GetCurrentDirectory() + @"\cfg";
            var Picturepath = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var StartupPath = Directory.GetCurrentDirectory() + @"\cfg\startup.txt";
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    string[] stdarray = { "false", "false" };
                    Pictures = stdarray.ToList();
                    Colorscheme = "light";
                    if (!Directory.Exists(PlaylistDir))
                    {
                        Directory.CreateDirectory(PlaylistDir);
                        File.Create(path2file).Close();
                    }
                    Directory.CreateDirectory(ConfigDir);
                    File.Create(DIpath).Close();
                    File.Create(pathtodic).Close();
                    File.WriteAllLines(Picturepath, stdarray);
                    File.WriteAllText(Colorpath, "light");
                    File.WriteAllText(CPAP, "0");
                    File.WriteAllText(pathfilepath, "enter directory");
                    File.WriteAllText(StartupPath, "no");
                    UpdateDctnry();
                }
                else
                {
                    var lastenteredpath = File.ReadAllText(pathfilepath);
                    CustomPlaylists = File.ReadAllLines(pathtodic).Select(line => line.Split(';')).ToDictionary(split => split[0], split => split[1]);
                    DeletedIndices = File.ReadAllLines(DIpath).ToList();
                    CustomPlaylistAmount = Convert.ToInt32(File.ReadAllText(CPAP));
                    Colorscheme = File.ReadAllText(Colorpath);
                    Pictures = File.ReadAllLines(Picturepath).ToList();
                    textBox1.Text = lastenteredpath;
                    if (!Directory.Exists(PlaylistDir))
                    {
                        Directory.CreateDirectory(PlaylistDir);
                        File.Create(path2file).Close();
                        UpdateDctnry();
                    }
                    else
                    {
                        if (!File.Exists(path2file))
                        {
                            File.Create(path2file).Close();
                            UpdateDctnry();
                        }
                        PlayList0 = File.ReadAllLines(path2file).Select(line => line.Split(';')).ToDictionary(split => split[0], split => split[1]);
                        for (int a = 0; a < PlayList0.Count; a++)
                        {
                            var content = PlayList0.ElementAt(a).Key;
                            richTextBox1.AppendText(content);
                            richTextBox1.AppendText(Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception)
            {
                var text1 = " go to " + Directory.GetCurrentDirectory() + " and delete the cfg directory or reset the cfg in the settings,";
                var text2 = " alternatively the program can try and fix the error by restarting, should the program restart now?";
                var answer = MessageBox.Show("there was an error during program setup," + text1 + text2, "System.IO Exception", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (answer == DialogResult.Yes)
                {
                    Restart();
                }
                else
                {
                    StopCurrentSong();
                }
            }
        }
        private void UpdateDctnry()
        {
            var path2file = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt";
            var pathfilepath = Directory.GetCurrentDirectory() + @"\cfg\directory.txt";
            var firsttime = false;
            try
            {
                if (Loading != null)
                {
                    Loading.Abort();
                }
            }
            catch (ThreadAbortException) { }
            try
            {
                if (!String.IsNullOrEmpty(textBox1.Text))
                {
                    var contentlist = new List<string>();
                    var SPList = GetSongPath(textBox1.Text);
                    var SNList = GetSongName(textBox1.Text);
                    if (File.Exists(SPList[0]))
                    {
                        LoadStartUI(false);
                        richTextBox1.Clear();
                        richTextBox1.AppendText("all songs:");
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText(Environment.NewLine);
                        File.Delete(Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist0.txt");
                        PlayList0.Clear();
                        for (int a = 0; a < SNList.Count; a++)
                        {
                            PlayList0.Add(SNList[a], SPList[a]);
                            richTextBox1.AppendText(SNList[a]);
                            richTextBox1.AppendText(Environment.NewLine);
                            var content = SNList[a] + ";" + PlayList0[SNList[a]];
                            contentlist.Add(content);
                        }
                    }
                    else
                    {
                        throw new DirectoryNotFoundException();
                    }
                    File.WriteAllText(pathfilepath, textBox1.Text);
                    File.WriteAllLines(path2file, contentlist);
                    if (LoadedSongs.Count != 0)
                    {
                        DeleteLoadedSongs();
                    }
                    progressBar1.Value = 0;
                    Loading = new Thread(() => LoadDir(textBox1.Text));
                    Loading.Start();
                }
                else
                {
                    firsttime = true;
                    throw new DirectoryNotFoundException();
                }
            }
            catch (DirectoryNotFoundException)
            {
                var stdtext = File.ReadAllText(pathfilepath);
                textBox1.Text = stdtext;
                if (firsttime != true)
                {
                    MessageBox.Show("the directory is invalid or there's a duplicate file", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception) { }
        }
        private void Resetcfg()
        {
            try
            {
                textBox1.Text = "";
                var path = Directory.GetCurrentDirectory() + @"\cfg";
                string[] Files = Directory.GetFiles(path);
                string[] subFiles = Directory.GetFiles(path + @"\playlists");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\backup");
                File.Move(path + @"\customplaylistnames.txt", Directory.GetCurrentDirectory() + @"\backup\customplaylistnames.txt");
                for (int i = 0; i < Files.Length; i++)
                {
                    File.Delete(Files[i].ToString());
                }
                for (int i = 1; i < subFiles.Length; i++)
                {
                    var name = subFiles[i].ToString().Split('\\');
                    File.Copy(subFiles[i].ToString(), Directory.GetCurrentDirectory() + @"\backup\" + name[name.Length - 1]);
                    File.Delete(subFiles[i].ToString());
                }
                File.Delete(subFiles[0].ToString());
                Directory.Delete(path + @"\playlists");
                Directory.Delete(path);
                Setupcfg();
                for (int i = 1; i < subFiles.Length; i++)
                {
                    var name = subFiles[i].ToString().Split('\\');
                    var filename = path + @"\playlists\playlist" + i.ToString() + ".txt";
                    File.Move(Directory.GetCurrentDirectory() + @"\backup\" + name[name.Length - 1], filename);
                }
                File.Delete(path + @"\customplaylistamount.txt");
                File.Delete(path + @"\customplaylistnames.txt");
                File.Move(Directory.GetCurrentDirectory() + @"\backup\customplaylistnames.txt", path + @"\customplaylistnames.txt");
                File.WriteAllText(path + @"\customplaylistamount.txt", (subFiles.Length - 1).ToString());
                Directory.Delete(Directory.GetCurrentDirectory() + @"\backup");
                var Playlistnames = File.ReadAllLines(path + @"\customplaylistnames.txt").ToList();
                var NewPlaylistnames = new List<string>();
                for (int i = 0; i < Playlistnames.Count; i++)
                {
                    var entry = Playlistnames[i].Split(' ');
                    var newentry = entry[0].ToString() + " " + (i + 1).ToString();
                    NewPlaylistnames.Add(newentry);
                }
                File.WriteAllLines(path + @"\customplaylistnames.txt", NewPlaylistnames);
                LoadColorScheme("light");
                LoadPictures();
                for (int i = 0; i < checkedListBox4.Items.Count; i++)
                {
                    checkedListBox4.SetItemChecked(i, false);
                }
                checkedListBox4.SetItemChecked(1, true);
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                richTextBox1.Text = "all songs:";
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                textBox3.Text = "";
                DeleteLoadedSongs();
                DeleteLeftOvers(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void StopCurrentSong()
        {
            if (MusicThread != null && MusicThread.IsAlive)
            {
                Queue.RemoveAt(0);
                IsPlaylist.RemoveAt(0);
                File.Delete(WavFilePath);
                Player.Stop();
                MusicThread.Abort();
            }
        }
        private void AddListenToQueue(string song)
        {
            StopCurrentSong();
            if (Queue.Count != 0)
            {
                SortLatestQueued(song);
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start(); 
            }
            else
            {
                AddtoQueue(song, false);
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start();
            }
        }
        private void SortLatestQueued(string song)
        {
            var templist = new List<string>();
            var templist2 = new List<bool>();
            for (int i = 0; i < Queue.Count; i++)
            {
                templist.Add(Queue[i]);
                templist2.Add(IsPlaylist[i]);
            }
            Queue.Clear();
            IsPlaylist.Clear();
            Queue.Add(song);
            IsPlaylist.Add(false);
            for (int i = 0; i < templist.Count; i++)
            {
                Queue.Add(templist[i]);
                IsPlaylist.Add(templist2[i]);
            }
            UpdateDisplay();
        }
        private void PlaySong(string song)
        {
            var foundsong = false;
            try
            {
                if (PlayList0.ContainsKey(song))
                {
                    AddListenToQueue(song);
                }
                else
                {
                    for (int a = 1; a <= CustomPlaylists.Count; a++)
                    {
                        var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + a + ".txt"; 
                        if (File.Exists(listpath))
                        {
                            var trylist = new Dictionary<string, string>();
                            var filecontent = File.ReadAllLines(listpath).ToList();
                            for (int i = 0; i < filecontent.Count; i++)
                            {
                                var contentarray = filecontent[i].Split(';');
                                try
                                {
                                    trylist.Add(contentarray[0], contentarray[1]);
                                }
                                catch (ArgumentException) { }
                            }
                            if (trylist.ContainsKey(song))
                            {
                                foundsong = true;
                                a = CustomPlaylistAmount + 1;
                                AddListenToQueue(song);
                            }
                        }
                    }
                    if (foundsong == false)
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception)
            {
                UpdateDctnry();
                MessageBox.Show("the song couldn't be found in any playlist", "no song found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PlayAudio(string song)
        {
            try
            {
                if (!PlayList0.ContainsKey(song))
                {
                    TrylistSystem(song);
                }
                else
                {
                    var names1 = PlayList0[song].Split('\\');
                    var names2 = names1[names1.Length - 1].Split('.');
                    string[] name = { names2[names2.Length - 2] };
                    if (names2[1] == "mp3")
                    {
                        var filename = GetFilename(PlayList0[song]);
                        var extension = GetFileExt(filename);
                        var newFilepath = Directory.GetCurrentDirectory() + @"\" + filename.ToString() + ".wav";
                        if (!File.Exists(newFilepath))
                        {
                            newFilepath = WavConverter(PlayList0[song]); 
                        }
                        WavFilePath = newFilepath;
                        Player.SoundLocation = newFilepath;
                        Player.Play();
                        SongEnds(newFilepath);
                    }
                    else
                    {
                        var filename = GetFilename(PlayList0[song]);
                        var extension = GetFileExt(filename);
                        var newFilepath = Directory.GetCurrentDirectory() + @"\" + filename.ToString() + ".wav";
                        try
                        {
                            File.Copy(PlayList0[song], newFilepath);
                        }
                        catch (IOException) {}
                        WavFilePath = newFilepath;
                        Player.SoundLocation = newFilepath;
                        Player.Play();
                        SongEnds(newFilepath);
                        SongEnds(newFilepath);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("file not found", "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TrylistSystem(string song)
        {
            var gotsong = false;
            try
            {
                for (int a = 1; a <= CustomPlaylists.Count; a++)
                {
                    var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + a + ".txt";
                    if (File.Exists(listpath))
                    {
                        var trylist = new Dictionary<string, string>();
                        var filecontent = File.ReadAllLines(listpath).ToList();
                        for (int i = 0; i < filecontent.Count; i++)
                        {
                            var contentarray = filecontent[i].Split(';');
                            try
                            {
                                trylist.Add(contentarray[0], contentarray[1]);
                            }
                            catch (ArgumentException) { }
                        }
                        if (trylist.ContainsKey(song))
                        {
                            var names1 = trylist[song].Split('\\');
                            var names2 = names1[names1.Length - 1].Split('.');
                            string[] name = { names2[names2.Length - 2] };
                            if (names2[1] == "mp3")
                            {
                                var filename = GetFilename(trylist[song]);
                                var extension = GetFileExt(filename);
                                var newFilepath = Directory.GetCurrentDirectory() + @"\" + filename.ToString() + ".wav";
                                if (!File.Exists(newFilepath))
                                {
                                    newFilepath = WavConverter(trylist[song]);
                                }
                                WavFilePath = newFilepath;
                                Player.SoundLocation = newFilepath;
                                Player.Play();
                                SongEnds(newFilepath);
                                SongEnds(newFilepath);
                            }
                            else
                            {
                                var filename = GetFilename(trylist[song]);
                                var extension = GetFileExt(filename);
                                var newFilepath = Directory.GetCurrentDirectory() + @"\" + filename.ToString() + ".wav";
                                try
                                {
                                    File.Copy(trylist[song], newFilepath);
                                }
                                catch (IOException) { }
                                WavFilePath = newFilepath;
                                Player.SoundLocation = newFilepath;
                                Player.Play();
                                SongEnds(newFilepath);
                                SongEnds(newFilepath);
                            }
                        }
                    }
                }
                if (!gotsong)
                {
                    var text = "song not found, double check if it's in the right directory or in one of your playlists. current directory: ";
                    MessageBox.Show(text + textBox1.Text, "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception) { }
        }
        public string WavConverter(string mp3FilePath)
        {
            try
            {
                var wavFilePath = Path.ChangeExtension(mp3FilePath, ".wav");
                using (var reader = new Mp3FileReader(mp3FilePath))
                using (var writer = new WaveFileWriter(wavFilePath, reader.WaveFormat))
                {
                    reader.CopyTo(writer);
                }
                WavFilePath = wavFilePath;
                return wavFilePath;
            }
            catch (Exception) 
            {
                return "";
            }
        }
        private void SongEnds(string path)
        {
            try
            {
                var time = GetSongDuration(path);
                var buffer = TimeSpan.FromMilliseconds(50);
                Thread.Sleep(time + buffer);
                if (!Looping)
                {
                    File.Delete(path);
                    Player.Stop();
                    if (Queue.Count != 1)
                    {
                        Queue.RemoveAt(0);
                        IsPlaylist.RemoveAt(0);
                        Invoke(new Action(UpdateDisplay));
                        PlayAudio(Queue[0]);
                        MusicThread.Abort();
                    }
                    if (Queue.Count != 0)
                    {
                        Queue.RemoveAt(0);
                        IsPlaylist.RemoveAt(0);
                    }
                    Invoke(new Action(UpdateDisplay));
                    MusicThread.Abort();
                }
                else
                {
                    Player.SoundLocation = WavFilePath;
                    Player.Play();
                    SongEnds(WavFilePath);
                }
            }
            catch (Exception) { }
        }
        private void NextSong(string path)
        {
            try
            {
                var donesmth = false;
                File.Delete(path);
                Player.Stop();
                MusicThread.Abort();
                if (Queue.Count != 1 && donesmth == false)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                    MusicThread = new Thread(() => PlayAudio(Queue[0]));
                    MusicThread.Start();
                    donesmth = true;
                }
                if (Queue.Count != 0 && donesmth == false)
                {
                    Queue.RemoveAt(0);
                    IsPlaylist.RemoveAt(0);
                    donesmth = true;
                }
            }
            catch (Exception) { }
        }
        private void PreviousSong()
        {
            if (!String.IsNullOrEmpty(CurrentPlaylist))
            {
                var songs = GetCustomPlaylistContent(CurrentPlaylist);
                var songnames = new List<string>();
                for (int i = 0; i < songs.Count; i++)
                {
                    var songname = songs[i].Split(';');
                    songnames.Add(songname[0]);
                }
                var index = GetCurrentSongIndex();
                if (index >= 1)
                {
                    StopCurrentSong();
                    SortLatestQueued(songnames[index - 1]);
                    MusicThread = new Thread(() => PlayAudio(Queue[0]));
                    MusicThread.Start();
                }
            }
            UpdateDisplay();
        }
        private void AddtoQueue(string songname, bool isplaylist)
        {
            Queue.Add(songname);
            IsPlaylist.Add(isplaylist);
            var templist = new List<bool>();
            var tempqueue = new List<string>();
            if (IsPlaylist[IsPlaylist.Count - 1] == false && IsPlaylist.Contains(true))
            {
                int index = IsPlaylist.FindIndex(true.Equals);
                if (index == 0)
                {
                    for (int i = 1; i < IsPlaylist.Count; i++)
                    {
                        if (IsPlaylist[i].Equals(true))
                        {
                            index = i;
                            i = IsPlaylist.Count;
                        }
                    }
                }
                for (int i2 = index; i2 < IsPlaylist.Count - 1; i2++)
                {
                    templist.Add(IsPlaylist[i2]);
                    tempqueue.Add(Queue[i2]);
                }
                IsPlaylist.RemoveRange(index, IsPlaylist.Count - index - 1);
                Queue.RemoveRange(index, Queue.Count - index - 1);
                for (int i3 = 0; i3 < templist.Count; i3++)
                {
                    IsPlaylist.Add(templist[i3]);
                    Queue.Add(tempqueue[i3]);
                }
            }
        }
        private void PlayPLaylist(string playlistname)
        {
            Random random = new Random();
            var indices = new List<int>();
            for (int i = 0; i < IsPlaylist.Count; i++)
            {
                if (IsPlaylist[i].Equals(true))
                {
                    indices.Add(i);
                }
            }
            indices.Reverse();
            for(int i = 0; i < indices.Count; i++)
            {
                IsPlaylist.RemoveAt(indices[i]);
                Queue.RemoveAt(indices[i]);
            }
            var playlist = GetCustomPlaylistContent(playlistname);
            var count = playlist.Count;
            for (int i = 0; i < count; i++)
            {
                if (Shuffle == true)
                {
                    var randomIndex = random.Next(0, playlist.Count);
                    var songname = playlist[randomIndex].Split(';');
                    AddtoQueue(songname[0], true);
                    playlist.RemoveAt(randomIndex);
                }
                else
                {
                    var songname = playlist[i].Split(';');
                    AddtoQueue(songname[0], true);
                }
            }
            StopCurrentSong(); //this fixed everything
            UpdateDisplay();
            MusicThread = new Thread(() => PlayAudio(Queue[0]));
            MusicThread.Start();
        }
        private void SongLoaded()
        {
            Invoke(new Action(() =>
            {
                progressBar1.PerformStep();
            }));
        }
        private void DirectoryLoaded()
        {
            while (progressBar1.Value != 100)
            {
                Thread.Sleep(500);
            }
            Thread.Sleep(2000);
            Invoke(new Action(() =>
            {
                progressBar1.Visible = false;
                button9.Visible = true;
            }));
            Loading.Abort();
        }
        private void LoadDir(string directory)
        {
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory).ToList();
                var mp3files = new List<string>();
                for (int i = 0; i < files.Count; i++)
                {
                    var extension = GetFileExt(files[i]);
                    if (extension == "mp3")
                    {
                        mp3files.Add(files[i]);
                    }
                }
                Invoke(new Action(() =>
                {
                    button9.Visible = false; 
                    var step = 100 / mp3files.Count;
                    progressBar1.Step = step + 1;
                    progressBar1.Visible = true;
                }));
                for (int i = 0; i < mp3files.Count; i++)
                {
                    var filename = GetFilename(mp3files[i]);
                    var filepath = WavConverter(mp3files[i]);
                    try
                    {
                        File.Move(filepath, Directory.GetCurrentDirectory() + "\\" + filename + ".wav");
                        File.Delete(filepath);
                        LoadedSongs.Add(Directory.GetCurrentDirectory() + "\\" + filename + ".wav");
                        LoadedSong();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            File.Delete(filepath);
                        }
                        catch (Exception)
                        {
                            StopCurrentSong();
                            MessageBox.Show("you shouldnt play music while the directories are being loaded, midify will restart to try and fix the error", "midify needs to restart", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Restart();
                        }
                    }
                }
                LoadingFinished();
            }
        }
        public static void DeleteLoadedSongs()
        {
            Loading.Abort();
            for (int i = 0; i < LoadedSongs.Count; i++)
            {
                File.Delete(LoadedSongs[i]);
            }
            LoadedSongs.Clear();
        }
        public static void DeleteLeftOvers(bool configreset)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());
            for (int i = 0; i <  files.Length; i++)
            {
                var filename = GetFilename(files[i]);
                var extension = GetFileExt(files[i]);
                if (extension == "wav" && filename != "Astolfo" || extension == "wav" && configreset == true)
                {
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + filename + ".wav");
                }
            }
        }
        private void UpdateDisplay()
        {
            if (checkedListBox1.Visible == true)
            {
                checkedListBox1.Items.Clear();
                for (int i = 0; i < Queue.Count; i++)
                {
                    checkedListBox1.Items.Add(Queue[i]);
                }
            }
        }
        private TimeSpan GetSongDuration(string path)
        {
            try
            {
                using (var reader = new AudioFileReader(path))
                {
                    return reader.TotalTime;
                }
            }
            catch(Exception)
            {
                return TimeSpan.FromSeconds(0);
            }
        }
        private List<string> GetSongPath(string dirpath)
        {
            var PathList = new List<string>();
            try
            {
                string[] paths = Directory.GetFiles(dirpath);
                for (int a = 0; a < paths.Length; a++)
                {
                    PathList.Add(paths[a]);
                }
                return PathList;
            }
            catch (Exception)
            {
                PathList.Add("");
                return PathList;
            }
        }
        private List<string> GetSongName(string dirpath)
        {
            var NameList = new List<string>();
            try
            {
                string[] files = Directory.GetFiles(dirpath);
                for (int i = 0; i < files.Length; i++)
                {
                    var fileExt = GetFileExt(files[i]);
                    string s1 = "\\";
                    string[] s2 = { "\\" };
                    switch (fileExt)
                    {
                        case "mp3":
                            s1 = s1 + ";" + ".mp3";
                            s2 = s1.Split(';');
                            break;
                        case "wav":
                            s1 = s1 + ";" + ".wav";
                            s2 = s1.Split(';');
                            break;
                        default:
                            goto skip;
                    }
                    var namesec = files[i].Split(s2, StringSplitOptions.None);
                    var filename = namesec[namesec.Length - 2];
                    NameList.Add(filename);
                skip:
                    Console.WriteLine();
                }
                return NameList;
            }
            catch (Exception)
            {
                NameList.Add("no songs here :(");
                return NameList;
            }
        }
        private static string GetFilename(string path)
        {
            var file = path.Split('\\');
            var extension = file[file.Length - 1].Split('.');
            var name = "";
            for (int j = 0; j < extension.Length - 1; j++)
            {
                if (extension.Length - 1 > 1 && j != extension.Length - 2)
                {
                    name = name + extension[j] + ".";
                }
                else
                {
                    name = name + extension[j];
                }
            }
            return name;
        }
        private static string GetFileExt(string filename)
        {
            var fileextension = filename.Split('.');
            return fileextension[fileextension.Length - 1];
        }
        private void CreatePlaylist(string name)
        {
            try
            {
                if (!String.IsNullOrEmpty(name))
                {
                    var playlist = new List<string>();
                    for (int a = 0; a < checkedListBox5.CheckedItems.Count; a++)
                    {
                        var songs = checkedListBox5.CheckedItems;
                        playlist.Add(songs[a].ToString() + ";" + PlayList0[songs[a].ToString()]);
                    }
                    CustomPlaylistAmount++;
                    var pathtoplaylist = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + CustomPlaylistAmount + ".txt"; 
                    var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
                    var CPAP = Directory.GetCurrentDirectory() + @"\cfg\customplaylistamount.txt";
                    CustomPlaylists.Add(name, "playlist " + CustomPlaylistAmount); 
                    var contentlist = new List<string>();
                    for (int i = 0; i < CustomPlaylists.Count; i++)
                    {
                        var content = CustomPlaylists.ElementAt(i).Key.ToString() + ";" + CustomPlaylists.ElementAt(i).Value.ToString();
                        contentlist.Add(content);
                    }
                    File.WriteAllText(CPAP, CustomPlaylistAmount.ToString());
                    File.WriteAllLines(pathtodic, contentlist);
                    File.WriteAllLines(pathtoplaylist, playlist);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                var caption = "missing input Exception";
                MessageBox.Show("the input was either null or empty", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AddSongToPlaylist(string playlist, string song, string path)
        {
            var programname = CustomPlaylists[playlist].ToString();
            var filename = ProgramToFilename(programname);
            var playlistcontent = GetCustomPlaylistContent(playlist);
            var filepath = Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename;
            playlistcontent.Add(song + ";" + path);
            File.WriteAllLines(filepath, playlistcontent);
        }
        private string ProgramToFilename(string programname)
        {
            var c = '.';
            string[] s = { "playlist " };
            var digitstep = programname.Split(c);
            var digit = digitstep[0].Split(s, StringSplitOptions.None);
            var filename = "playlist" + digit[1] + ".txt";
            return filename;
        }
        private string FileToProgramname(string filename) //not needed as of rn 30.11.24
        {
            string[] s = { "playlist", "." };
            var programname = filename.Split(s, StringSplitOptions.None);
            return programname[0];
        }
        private string GetCustomPlaylistPath(string currentplaylist)
        {
            try
            {
                if (CustomPlaylists.ContainsKey(currentplaylist)) 
                {
                    var filename = ProgramToFilename(CustomPlaylists[currentplaylist]);
                    var path = Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename;
                    return path;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                var caption = "missing argument Exception";
                MessageBox.Show("the playlist doesnt exist", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private List<string> GetCustomPlaylistContent(string playlistname)
        {
            try
            {
                var cplpath = GetCustomPlaylistPath(playlistname);
                var customplaylist = File.ReadAllLines(cplpath).ToList();
                return customplaylist;
            }
            catch (Exception)
            {
                var caption = "missing argument Exception";
                MessageBox.Show("the playlist doesnt exist", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void DisplayPlaylists()
        {
            checkedListBox2.Items.Clear();
            for (int i = 0; i < CustomPlaylists.Count; i++)
            {
                checkedListBox2.Items.Add(CustomPlaylists.ElementAt(i).Key);
            }
            checkedListBox2.Visible = true;
        }
        private void DisplaySongs(string playlistname) //not needed rn
        {
            var playlist = new List<string>();
            playlist = GetCustomPlaylistContent(playlistname);
            for (int a = 0; a < playlist.Count; a++)
            {
                var name = playlist[a].Split(';');
                checkedListBox3.Items.Add(name[0]);
            }
            checkedListBox3.Visible = true;
        }
        private void ShuffleCurrentPlaylist()
        {
            if (IsPlaylist.Contains(true))
            {
                Random random = new Random();
                var templist = new List<string>();
                var templist2 = new List<bool>();
                var count = Queue.Count;
                int index = IsPlaylist.FindIndex(true.Equals);
                //if index is first element in queue get the second true element
                if (index == 0)
                {
                    for (int i = 1; i < IsPlaylist.Count; i++)
                    {
                        if (IsPlaylist[i].Equals(true))
                        {
                            index = i;
                            i = IsPlaylist.Count;
                        }
                    }
                }
                for (int i = index; i < count; i++)
                {
                    templist.Add(Queue[i]);
                    templist2.Add(IsPlaylist[i]);
                }
                Queue.RemoveRange(index, Queue.Count - index);
                IsPlaylist.RemoveRange(index, IsPlaylist.Count - index);
                count = templist.Count;
                for (int i = 0; i < count; i++)
                {
                    var randomIndex = random.Next(0, templist.Count);
                    Queue.Add(templist[randomIndex]);
                    IsPlaylist.Add(templist2[randomIndex]);
                    templist.RemoveAt(randomIndex);
                    templist2.RemoveAt(randomIndex);
                }
                UpdateDisplay();
            }
        }
        private void UnShuffleCurrentPlaylist()
        {
            var programname = CustomPlaylists[CurrentPlaylist];
            var filename = ProgramToFilename(programname);
            var oldlist = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\cfg\playlists\" + filename).ToList();
            var inPLindex = GetCurrentSongIndex() + 1;
            var inQueueindex = IsPlaylist.IndexOf(true);
            if (inQueueindex == 0)
            {
                for (int i = 1; i < IsPlaylist.Count; i++)
                {
                    if (IsPlaylist[i].Equals(true))
                    {
                        inQueueindex = i;
                        i = IsPlaylist.Count;
                    }
                }
            }
            oldlist.RemoveRange(0, inPLindex);
            Queue.RemoveRange(inQueueindex, Queue.Count - inQueueindex);
            IsPlaylist.RemoveRange(inQueueindex, IsPlaylist.Count - inQueueindex);
            for (int i = 0; i < oldlist.Count; i++)
            {
                var songname = oldlist[i].Split(';');
                AddtoQueue(songname[0], true);
            }
            UpdateDisplay();
        }
        private int GetCurrentSongIndex()
        {
            var songs = GetCustomPlaylistContent(CurrentPlaylist);
            var songnames = new List<string>();
            for (int i = 0; i < songs.Count; i++)
            {
                var songname = songs[i].Split(';');
                songnames.Add(songname[0]);
            }
            var name = GetFilename(WavFilePath);
            var index = songnames.IndexOf(name);
            return index;
        }
        private void LoadStartUI(bool queuebug)
        {
            if (!queuebug == true)
            {
                checkedListBox1.Visible = false;
            }
            checkedListBox2.Visible = false;
            checkedListBox3.Visible = false;
            checkedListBox5.Visible = false;
            checkedListBox1.Items.Clear();
            checkedListBox2.Items.Clear();
            checkedListBox3.Items.Clear();
            checkedListBox5.Items.Clear();
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = true;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button14.Visible = true;
            button15.Visible = false;
            button16.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button19.Visible = true;
            button20.Visible = true;
            button21.Visible = true;
            button22.Visible = false;
            button23.Visible = false;
            button24.Visible = false;
            button25.Visible = false;
            button26.Visible = true;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            textBox1.Visible = true;
            textBox3.Visible = true;
            textBox4.Visible = false;
            textBox5.Visible = false;
            richTextBox1.Visible = true;
            checkBox1.Visible = true;
            checkBox4.Visible = true;
            progressBar1.Visible = false;
            if (Loading != null && Loading.IsAlive)
            {
                progressBar1.Visible = true;
                button9.Visible = false;
            }
        }
        private void DisableAllUIinPanel1()
        {
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button14.Visible = false;
            button15.Visible = false;
            button16.Visible = false;
            button17.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
            button22.Visible = false;
            button23.Visible = false;
            button24.Visible = false;
            button25.Visible = false;
            button26.Visible = false;
            checkedListBox1.Visible = false;
            checkedListBox2.Visible = false;
            checkedListBox3.Visible = false;
            textBox1.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            richTextBox1.Visible = false;
            checkBox1.Visible = false;
            checkBox4.Visible = false;
        }
        private void LoadUIinPanel2()
        {
            panel1.Visible = false;
            panel2.Visible = true;
            button27.Visible = true;
            button28.Visible = true;
            button29.Visible = true;
            button30.Visible = true;
            textBox2.Visible = true;
            textBox6.Visible = true;
            textBox7.Visible = true;
            richTextBox2.Visible = true;
            richTextBox2.Clear();
            checkedListBox4.Visible = true;
            checkedListBox4.Items.Clear();
            checkBox2.Visible = true;
            checkBox3.Visible = true;
            comboBox1.Visible = true;
            comboBox1.SelectedItem = File.ReadAllText(Directory.GetCurrentDirectory() + "\\cfg\\startup.txt");
        }
        private void UnloadUIinPanel2()
        {
            panel1.Visible = true;
            panel2.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button29.Visible = false;
            textBox2.Visible = false;
            textBox6.Visible = false;
            textBox7.Visible = false;
            richTextBox2.Visible = false;
            richTextBox2.Clear();
            checkedListBox4.Visible = false;
            checkedListBox4.Items.Clear();
            checkBox2.Visible = false;
            checkBox3.Visible = false;
            comboBox1.Visible = false;
        }

        private void LoadSettingsMenu()
        {
            LoadUIinPanel2();
            var picturebools = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\cfg\pictures.txt").ToList();
            string[] colorschemes = { "dark", "light", "pink", "purple", "blue", "green" };
            var currentcolor = File.ReadAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt");
            for (int i = 0; i < colorschemes.Length; i++)
            {
                checkedListBox4.Items.Add(colorschemes[i]);
                if (colorschemes[i] == currentcolor)
                {
                    checkedListBox4.SetItemChecked(i, true);
                }
            }
            if (picturebools[0] == "true")
            {
                checkBox3.Checked = true;
            }
            if (picturebools[1] == "true")
            {
                checkBox2.Checked = true;
            }
            ShowSizeOnDisk();
        }
        private void ShowSizeOnDisk()
        {
            var currentdirectory = File.ReadAllText(Directory.GetCurrentDirectory() + @"\cfg\directory.txt");
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var dirsize = CalculateDirSize(dir, false);
            var dirsizemb = dirsize / 1048576; //coverts bytes to megabytes
            richTextBox2.AppendText("midify: " + dirsizemb.ToString("F3") + " MB");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            var tempfilessize = CalculateDirSize(dir, true);
            var tempfilessizemb = tempfilessize / 1048576;
            richTextBox2.AppendText("temporary files: " + tempfilessizemb.ToString("F3") + " MB");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            if (!Directory.Exists(currentdirectory))
            {
                richTextBox2.AppendText("main song dir: M/D");
            }
            else
            {
                DirectoryInfo dir2 = new DirectoryInfo(currentdirectory);
                var dir2size = CalculateDirSize(dir2, false);
                var dir2sizemb = dir2size / 1048576;
                richTextBox2.AppendText("main song dir: " + dir2sizemb.ToString("F3") + " MB");
            }
        }
        private double CalculateDirSize(DirectoryInfo dir, bool tempfilesize)
        {
            DirectoryInfo[] subFolders = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            double dirsize = new double();
            for (int i = 0; i < files.Length; i++)
            {
                if (GetFileExt(files[i].ToString()) != "wav" && tempfilesize == false|| GetFilename(files[i].ToString()) == "Astolfo" && tempfilesize == false || GetFileExt(files[i].ToString()) == "wav" && GetFilename(files[i].ToString()) != "Astolfo" && tempfilesize == true)
                {
                    dirsize = dirsize + files[i].Length;
                }
            }
            for (int i = 0; i < subFolders.Length; i++)
            {
                dirsize = dirsize + CalculateDirSize(subFolders[i], tempfilesize);
            }
            return dirsize;
        }
        private void LoadColorScheme(string color)
        {
            switch (color) //for some reason this throws a nullref when tried with Form1.ActiveForm. but works with this. i do not know why but it works
            {
                case "dark":
                    ChangeColor(this, System.Drawing.Color.DimGray);
                    this.BackColor = System.Drawing.Color.Black;
                    panel1.BackColor = System.Drawing.Color.Black;
                    panel2.BackColor = System.Drawing.Color.Black;
                    richTextBox1.BackColor = System.Drawing.Color.Black;
                    richTextBox1.ForeColor = System.Drawing.Color.White;
                    textBox2.ForeColor = System.Drawing.Color.White;
                    textBox5.ForeColor = System.Drawing.Color.White;
                    textBox6.ForeColor = System.Drawing.Color.White;
                    button26.BackgroundImage = midify.Properties.Resources.graysettings;
                    break;
                case "light":
                    ChangeColor(this, System.Drawing.SystemColors.Control);
                    this.BackColor = System.Drawing.SystemColors.Window;
                    panel1.BackColor = System.Drawing.SystemColors.Window;
                    panel2.BackColor = System.Drawing.SystemColors.Window;
                    richTextBox1.BackColor = System.Drawing.SystemColors.Window;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.graysettings;
                    break;
                case "pink":
                    ChangeColor(this, System.Drawing.Color.LightPink);
                    this.BackColor = System.Drawing.Color.HotPink;
                    panel1.BackColor = System.Drawing.Color.HotPink;
                    panel2.BackColor = System.Drawing.Color.HotPink;
                    richTextBox1.BackColor = System.Drawing.Color.HotPink;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.lightpinksettings;
                    break;
                case "purple":
                    ChangeColor(this, System.Drawing.Color.MediumSlateBlue);
                    this.BackColor = System.Drawing.Color.DarkSlateBlue;
                    panel1.BackColor = System.Drawing.Color.DarkSlateBlue;
                    panel2.BackColor = System.Drawing.Color.DarkSlateBlue;
                    richTextBox1.BackColor = System.Drawing.Color.DarkSlateBlue;
                    richTextBox1.ForeColor = System.Drawing.Color.Black;
                    textBox2.ForeColor = System.Drawing.Color.Black;
                    textBox5.ForeColor = System.Drawing.Color.Black;
                    textBox6.ForeColor = System.Drawing.Color.Black;
                    button26.BackgroundImage = midify.Properties.Resources.purplesettings;
                    break;
                case "blue":
                    ChangeColor(this, System.Drawing.Color.LightSkyBlue);
                    this.BackColor = System.Drawing.Color.DarkBlue;
                    panel1.BackColor = System.Drawing.Color.DarkBlue;
                    panel2.BackColor = System.Drawing.Color.DarkBlue;
                    richTextBox1.BackColor = System.Drawing.Color.DarkBlue;
                    richTextBox1.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox2.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox5.ForeColor = System.Drawing.Color.LightSkyBlue;
                    textBox6.ForeColor = System.Drawing.Color.LightSkyBlue;
                    button26.BackgroundImage = midify.Properties.Resources.lightbluesettings;
                    break;
                case "green":
                    ChangeColor(this, System.Drawing.Color.LightGreen);
                    this.BackColor = System.Drawing.Color.DarkGreen;
                    panel1.BackColor = System.Drawing.Color.DarkGreen;
                    panel2.BackColor = System.Drawing.Color.DarkGreen;
                    richTextBox1.BackColor = System.Drawing.Color.DarkGreen;
                    richTextBox1.ForeColor = System.Drawing.Color.LightGreen;
                    textBox2.ForeColor = System.Drawing.Color.LightGreen;
                    textBox5.ForeColor = System.Drawing.Color.LightGreen;
                    textBox6.ForeColor = System.Drawing.Color.LightGreen;
                    button26.BackgroundImage = midify.Properties.Resources.lightgreensettings;
                    break;
            }
            textBox2.BackColor = panel1.BackColor;
            textBox5.BackColor = panel1.BackColor;
            textBox6.BackColor = panel1.BackColor;
            button26.BackColor = System.Drawing.Color.Transparent;
            button27.BackColor = System.Drawing.Color.Red;
            button29.BackColor = System.Drawing.Color.Red;
            pictureBox1.BackColor = System.Drawing.Color.Transparent;
            pictureBox2.BackColor = System.Drawing.Color.Transparent;
            pictureBox3.BackColor = System.Drawing.Color.Transparent;
            pictureBox4.BackColor = System.Drawing.Color.Transparent;
            pictureBox5.BackColor = System.Drawing.Color.Transparent;
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt", color);
        }
        private void ChangeColor(Control parent, Color newcolor)
        {
            foreach (Control control in parent.Controls)
            {
                control.BackColor = newcolor;
                control.Invalidate(); 
                control.Update();
                if (control.HasChildren == true)
                {
                    ChangeColor(control, newcolor);
                }
            }
        }
        private void AstolfoVoice()
        {
            SoundPlayer astolfo = new SoundPlayer();
            astolfo.SoundLocation = Directory.GetCurrentDirectory() + @"\Astolfo.wav";
            astolfo.Load();
            astolfo.Play();
        }
        private void LoadPictures()
        {
            try
            {
                if (Pictures[0] == "true") //0 is Astolfo, 1 is Miku
                {
                    ShowAstolfo();
                }
                else
                {
                    pictureBox1.Visible = false;
                    pictureBox2.Visible = false;
                    pictureBox3.Visible = false;
                }
                if (Pictures[1] == "true")
                {
                    ShowMiku();
                }
                else
                {
                    pictureBox4.Visible = false;
                    pictureBox5.Visible = false;
                }
            }
            catch (Exception) { }
        }
        private void ShowAstolfo()
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox1.Image = midify.Properties.Resources.Astolfo1;
            pictureBox2.Image = midify.Properties.Resources.Astolfo1;
            pictureBox3.Image = midify.Properties.Resources.Astolfo2;
        }
        private void ShowMiku()
        {
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            pictureBox4.Image = midify.Properties.Resources.Miku1;
            pictureBox5.Image = midify.Properties.Resources.MikuBooth;
        }
        private void Uninstall()
        {
            var uninstallerexe = Directory.GetCurrentDirectory() + "\\midify uninstaller.exe";
            var desktopexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "midify uninstaller.exe");
            File.Move(uninstallerexe, desktopexe);
            Process Uninstaller = new Process();
            Uninstaller.StartInfo.FileName = desktopexe;
            Uninstaller.StartInfo.CreateNoWindow = false;
            Uninstaller.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Uninstaller.StartInfo.UseShellExecute = false;
            Uninstaller.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DeleteLoadedSongs();
            DeleteLeftOvers(true);
            Uninstaller.Start();
            Application.Exit();
        }
        public static void AddApplicationToStartup(string exePath)
        {
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "midify.lnk");
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath; shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Save();
        }
        private void LoadQueueUI()
        {
            pictureBox2.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button31.Visible = true;
            button32.Visible = true;
            button33.Visible = true;
            checkedListBox1.Visible = true;
        }
        private void RemoveFromQueue(int index)
        {
            if (MusicThread != null && index == 0)
            {
                NextSong(WavFilePath);
            }
            else
            {
                Queue.RemoveAt(index);
                IsPlaylist.RemoveAt(index);
            }
        }
        private string SelectPlaylist()
        {
            if (checkedListBox2.CheckedItems.Count == 1)
            {
                var playlist = checkedListBox2.CheckedItems[0].ToString();
                return playlist;
            }
            else
            {
                MessageBox.Show("select one playlist", "Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                for (int a = 0; a < checkedListBox3.Items.Count; a++)
                {
                    checkedListBox3.SetItemChecked(a, false);
                }
                return "";
            }
        }
        private void Restart()
        {
            Process Restarter = new Process();
            Restarter.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\midify restarter.exe";
            Restarter.StartInfo.UseShellExecute = true;
            Restarter.StartInfo.CreateNoWindow = false;
            Restarter.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Restarter.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            Restarter.Start();
            Application.Exit();
        }
        private void button9_Click(object sender, EventArgs e)
        {
            UpdateDctnry();
            if (Pictures.ElementAt(0) == "true")
            {
                pictureBox3.Visible = true;
            }
        }
        private void button16_Click(object sender, EventArgs e)
        {
            for (int i1 = 0; i1 < checkedListBox2.CheckedItems.Count; i1++)
            {
                var songs = GetCustomPlaylistContent(checkedListBox2.CheckedItems[i1].ToString());
                for (int i2 = 0; i2 < songs.Count; i2++)
                {
                    var songname = songs[i2].Split(';');
                    AddtoQueue(songname[0], false); 
                }
            }
            for (int a = 0; a < checkedListBox2.Items.Count; a++)
            {
                checkedListBox2.SetItemChecked(a, false);
            }
            UpdateDisplay();
        }
        private void button15_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox3.CheckedItems.Count; i++)
            {
                AddtoQueue(checkedListBox3.CheckedItems[i].ToString(), false);
            }
            for (int a = 0; a < checkedListBox3.Items.Count; a++)
            {
                checkedListBox3.SetItemChecked(a, false);
            }
            UpdateDisplay();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = true;
            button7.Visible = true;
            button10.Visible = true;
            button13.Visible = true;
            button16.Visible = true;
            button17.Visible = false;
            button18.Visible = false;
            checkedListBox1.Visible = false;
            DisplayPlaylists();
        }
        private void button10_Click(object sender, EventArgs e)
        {
            for (int a = 0; a < checkedListBox2.CheckedItems.Count; a++)
            {
                var currentplaylist = checkedListBox2.CheckedItems;
                var cplpath = GetCustomPlaylistPath(currentplaylist[a].ToString()); 
                if (cplpath != null)
                {
                    string[] s1 = { "\\", ".txt" };
                    var pathtodic = Directory.GetCurrentDirectory() + @"\cfg\customplaylistnames.txt";
                    var contentlist = new List<string>();
                    var namesec = cplpath.Split(s1, StringSplitOptions.None);
                    var filename = namesec[namesec.Length - 2];
                    string[] s2 = { "playlist" };
                    var index = filename.Split(s2, StringSplitOptions.None);
                    DeletedIndices.Add(index[1]);
                    CustomPlaylists.Remove(currentplaylist[a].ToString());
                    File.WriteAllLines(Directory.GetCurrentDirectory() + @"\cfg\deletedindices.txt", DeletedIndices);
                    for (int i = 0; i < CustomPlaylists.Count; i++)
                    {
                        var content = CustomPlaylists.ElementAt(i).Key.ToString() + ";" + CustomPlaylists.ElementAt(i).Value.ToString();
                        contentlist.Add(content);
                    }
                    File.WriteAllLines(pathtodic, contentlist);
                    File.Delete(cplpath);
                }
                else
                {
                    MessageBox.Show("something went wrong");
                }
            }
            DisplayPlaylists();
        }
        private void button12_Click(object sender, EventArgs e)
        {
            var indices = new List<int>();
            var currentplaylist = checkedListBox2.CheckedItems;
            var playlistcontent = GetCustomPlaylistContent(currentplaylist[0].ToString()); //broke due to index out of range
            var playlistpath = GetCustomPlaylistPath(currentplaylist[0].ToString());
            var index = checkedListBox3.CheckedIndices;
            for (int a = 0; a < index.Count; a++)
            {
                indices.Add(index[a]);
            }
            indices.Sort();
            for (int a = checkedListBox3.CheckedItems.Count; a > 0; a--)
            {
                playlistcontent.RemoveAt(indices[a - 1]);
            }
            File.WriteAllLines(playlistpath, playlistcontent);
            checkedListBox3.Items.Clear();
            for (int a = 0; a < playlistcontent.Count; a++)
            {
                var name = playlistcontent[a].Split(';');
                checkedListBox3.Items.Add(name[0]);
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (checkedListBox2.CheckedItems.Count == 1)
            {
                var Playlistname = checkedListBox2.CheckedItems[0].ToString();
                CurrentPlaylist = Playlistname;
                PlayPLaylist(Playlistname);
            }
            else
            {
                MessageBox.Show("select one playlist to listen to (you can queue the rest)", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for (int a = 0; a < checkedListBox2.Items.Count; a++)
            {
                checkedListBox2.SetItemChecked(a, false);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var playlist = new List<string>();
                if (checkedListBox2.CheckedItems.Count == 1 && playlist != null)
                {
                    playlist = GetCustomPlaylistContent(checkedListBox2.CheckedItems[0].ToString());
                    CurrentPlaylist = checkedListBox2.CheckedItems[0].ToString();
                    button5.Visible = false;
                    button7.Visible = false;
                    button8.Visible = true;
                    button10.Visible = false;
                    button11.Visible = true;
                    button12.Visible = true;
                    button13.Visible = false;
                    button15.Visible = true;
                    button16.Visible = false;
                    button23.Visible = true;
                    button17.Visible = false;
                    button18.Visible = false;
                    checkedListBox3.Items.Clear();
                    checkedListBox2.Visible = false;
                    checkedListBox3.Visible = true;
                    for (int a = 0; a < playlist.Count; a++)
                    {
                        var name = playlist[a].Split(';');
                        checkedListBox3.Items.Add(name[0]);
                    }
                }
                else
                {
                    MessageBox.Show("select one playlist", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    for (int a = 0; a < checkedListBox2.Items.Count; a++)
                    {
                        checkedListBox2.SetItemChecked(a, false);
                    }
                }
                
            }
            catch (Exception)
            {
                MessageBox.Show("couldn't find playlist", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                for (int a = 0; a < checkedListBox2.Items.Count; a++)
                {
                    checkedListBox2.SetItemChecked(a, false);
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (checkedListBox3.CheckedItems.Count == 1)
            {
                PlaySong(checkedListBox3.CheckedItems[0].ToString());
            }
            else
            {
                MessageBox.Show("select one song to listen to (you can queue the rest)", "System.Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            for (int a = 0; a < checkedListBox3.Items.Count; a++)
            {
                checkedListBox3.SetItemChecked(a, false);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            checkedListBox2.Items.Clear();
            checkedListBox2.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button5.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button10.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button16.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button23.Visible = false;
            checkBox4.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var song = textBox3.Text;
            textBox3.Text = "";
            PlaySong(song);
            UpdateDisplay();
        }
        private void button14_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            UpdateDisplay();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            checkedListBox5.Items.Clear();
            button4.Visible = true;
            for (int a = 0; a < PlayList0.Count; a++)
            {
                checkedListBox5.Items.Add(PlayList0.ElementAt(a).Key);
            }
            checkedListBox5.Visible = true;
            button2.Visible = false;
            button3.Visible = false;
            button6.Visible = true;
            button17.Visible = false;
            button18.Visible = false;
        }
        private void button22_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            CreatePlaylist(textBox4.Text);
            textBox4.Clear();
            for (int a = 0; a < checkedListBox5.Items.Count; a++)
            {
                checkedListBox5.SetItemChecked(a, false);
            }
            checkedListBox5.Visible = false;
            checkedListBox5.Items.Clear();
            button6.Visible = false;
            button3.Visible = true;
            button4.Visible = false;
            button2.Visible = true;
            button17.Visible = true;
            button18.Visible = true;
            textBox4.Visible = false;
            textBox5.Visible = false;
            button22.Visible = false;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            checkedListBox2.ClearSelected();
            checkedListBox1.Visible = false;
            button4.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = true;
            textBox4.Visible = true;
            textBox5.Visible = true;
            button22.Visible = true;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            checkedListBox1.Items.Clear();
            checkedListBox5.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = false;
            button6.Visible = false;
            button17.Visible = true;
            button18.Visible = true;
            button22.Visible = false;
        }
        private void button11_Click(object sender, EventArgs e)
        {
            button5.Visible = true;
            button7.Visible = true;
            button8.Visible = false;
            button10.Visible = true;
            button11.Visible = false;
            button13.Visible = true;
            button15.Visible = false;
            button16.Visible = true;
            button17.Visible = true;
            button18.Visible = true;
            button23.Visible = false;
            checkedListBox2.Visible = true;
            checkedListBox3.Visible = false;
            checkedListBox3.Items.Clear();
            DisplayPlaylists();
        }
        private void button17_Click(object sender, EventArgs e)
        {
            LoadStartUI(true);
            checkedListBox1.Items.Clear();
            if (checkedListBox1.Visible == false)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    checkedListBox1.Items.Add(Queue[i]);
                }
                LoadQueueUI();
            }
            else
            {
                if (Pictures[0] == "true")
                {
                    pictureBox2.Visible = true;
                }
                checkedListBox1.Visible = false;
                checkedListBox2.Visible = false;
                button2.Visible = true;
                button3.Visible = true;
                button31.Visible = false;
                button32.Visible = false;
                button33.Visible = false;
                button34.Visible = false;
                button35.Visible = false;
            }
        }
        private void button18_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            checkedListBox1.Items.Clear();
            Queue.Clear();
            IsPlaylist.Clear();
            UpdateDisplay();
        }
        private void button20_Click(object sender, EventArgs e)
        {
            NextSong(WavFilePath);
            UpdateDisplay();
        }
        private void button19_Click(object sender, EventArgs e)
        {
            PreviousSong();
        }
        private void button21_Click(object sender, EventArgs e)
        {
            MusicThread = new Thread(() => Nothing()); //this is so stupid...
            if (MusicThread == null && Queue.Count > 0 || MusicThread.IsAlive != true && Queue.Count > 0)
            {
                MusicThread = new Thread(() => PlayAudio(Queue[0]));
                MusicThread.Start();
            }
        }
        private void Nothing(){}
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                Shuffle = true;
                if (IsPlaylist.Contains(true))
                {
                    ShuffleCurrentPlaylist();
                }
            }
            else
            {
                Shuffle = false;
                if (IsPlaylist.Contains(true))
                {
                    UnShuffleCurrentPlaylist();
                }
            }
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                Looping = true;
            }
            else
            {
                Looping = false;
            }
        }
        private void button23_Click(object sender, EventArgs e)
        {
            checkedListBox3.Visible = false;
            checkedListBox5.Items.Clear();
            for (int a = 0; a < PlayList0.Count; a++)
            {
                checkedListBox5.Items.Add(PlayList0.ElementAt(a).Key);
            }
            checkedListBox5.Visible = true;
            button8.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            button13.Visible = false;
            button15.Visible = false;
            button16.Visible = false;
            button23.Visible = false;
            button24.Visible = true;
            button25.Visible = true;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            for (int i = 0; i < checkedListBox5.CheckedItems.Count; i++)
            {
                AddSongToPlaylist(CurrentPlaylist, checkedListBox5.CheckedItems[i].ToString(), PlayList0[checkedListBox5.CheckedItems[i].ToString().ToString()]);
            }
            LoadStartUI(false);
        }
        private void button25_Click(object sender, EventArgs e)
        {
            if (Pictures[0] == "true")
            {
                pictureBox2.Visible = true;
            }
            LoadStartUI(false);
        }
        private void button26_Click(object sender, EventArgs e)
        {
            DisableAllUIinPanel1();
            LoadSettingsMenu();
        }
        private void button27_Click(object sender, EventArgs e)
        {
            Resetcfg();
        }
        private void button28_Click(object sender, EventArgs e)
        {
            if (checkedListBox4.CheckedItems.Count != 1)
            {
                MessageBox.Show("select one color", "Argument Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var color = checkedListBox4.CheckedItems[0].ToString();
                File.WriteAllText(Directory.GetCurrentDirectory() + @"\cfg\colorscheme.txt", color);
                LoadColorScheme(color);
            }
        }
        private void button29_Click(object sender, EventArgs e)
        {
            Uninstall();
        }
        private void button30_Click(object sender, EventArgs e)
        {
            UnloadUIinPanel2();
            LoadStartUI(false);
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox3.Visible = false;
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            Thread AstolfoThread = new Thread(() => AstolfoVoice());
            AstolfoThread.Start();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            StopCurrentSong();
            Thread AstolfoThread = new Thread(() => AstolfoVoice());
            AstolfoThread.Start();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var content = File.ReadAllLines(path).ToList();
            var Miku = content[1].ToString();
            if (checkBox3.Checked != true)
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = false;
                pictureBox3.Visible = false;
                content.Clear();
                content.Add("false");
                content.Add(Miku);
            }
            else
            {
                ShowAstolfo();
                content.Clear();
                content.Add("true");
                content.Add(Miku);
            }
            File.WriteAllLines(path, content);
            Pictures = File.ReadAllLines(path).ToList();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + @"\cfg\pictures.txt";
            var content = File.ReadAllLines(path).ToList();
            content.RemoveAt(1);
            if (checkBox2.Checked != true)
            {
                pictureBox4.Visible = false;
                pictureBox5.Visible = false;
                content.Add("false");
            }
            else
            {
                ShowMiku();
                content.Add("true");
            }
            File.WriteAllLines(path, content);
            Pictures = File.ReadAllLines(path).ToList();
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text.Contains("search for song.."))
            {
                var userChar = textBox3.Text.Split('.');
                textBox3.Clear();
                if (userChar[userChar.Length - 1] != " ")
                {
                    textBox3.AppendText(userChar[userChar.Length - 1]);
                }
            }
        }
        private void checkedListBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox4.Items.Count; i++)
            {
                checkedListBox4.SetItemChecked(i, false);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.TextChanged += ComboBox1_TextChanged;
            var item = comboBox1.SelectedItem;
            if (item.ToString() == "yes")
            {
                AddApplicationToStartup(Directory.GetCurrentDirectory() + "\\midify.exe");
            }
            else
            {
                var shortcutpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "midify.lnk");
                if (File.Exists(shortcutpath))
                {
                    File.Delete(shortcutpath);
                }
            }
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\cfg\\startup.txt", item.ToString());
        }
        private void ComboBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox1.ResetText();
        }
        private void button31_Click(object sender, EventArgs e)
        {
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = true;
            button35.Visible = true;
            checkedListBox2.Visible = true;
            DisplayPlaylists();
        }
        private void button32_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                AddtoQueue(checkedListBox1.CheckedItems[i].ToString(), false);
            }
            UpdateDisplay();
        }
        private void button33_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                RemoveFromQueue(checkedListBox1.CheckedIndices[i] - i);
            }
            UpdateDisplay();
        }
        private void button34_Click(object sender, EventArgs e)
        {
            button2.Visible = true;
            button3.Visible = true;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            checkedListBox1.Visible = false;
            checkedListBox2.Visible = false;
            var playlist = SelectPlaylist();
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                var path = "";
                if (PlayList0.ContainsKey(checkedListBox1.CheckedItems[i].ToString()))
                {
                    path = PlayList0[checkedListBox1.CheckedItems[i].ToString()];
                }
                else
                {
                    for (int j = 1; j <= CustomPlaylists.Count; j++)
                    {
                        var listpath = Directory.GetCurrentDirectory() + @"\cfg\playlists\playlist" + j + ".txt";
                        if (File.Exists(listpath))
                        {
                            var trylist = new Dictionary<string, string>();
                            var filecontent = File.ReadAllLines(listpath).ToList();
                            for (int k = 0; k < filecontent.Count; k++)
                            {
                                var contentarray = filecontent[k].Split(';');
                                try
                                {
                                    trylist.Add(contentarray[0], contentarray[1]);
                                }
                                catch (ArgumentException) { }
                            }
                            if (trylist.ContainsKey(checkedListBox1.CheckedItems[i].ToString()))
                            {
                                path = trylist[checkedListBox1.CheckedItems[i].ToString()];
                            }
                        }
                    }
                }
                AddSongToPlaylist(playlist, checkedListBox1.CheckedItems[i].ToString(), path);
            }
        }
        private void button35_Click(object sender, EventArgs e)
        {
            LoadStartUI(false);
        }
    }
}
namespace midify
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button30 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.button28 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.checkedListBox4 = new System.Windows.Forms.CheckedListBox();
            this.button27 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.checkedListBox3 = new System.Windows.Forms.CheckedListBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button18 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button33 = new System.Windows.Forms.Button();
            this.button32 = new System.Windows.Forms.Button();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.button35 = new System.Windows.Forms.Button();
            this.checkedListBox5 = new System.Windows.Forms.CheckedListBox();
            this.button34 = new System.Windows.Forms.Button();
            this.button31 = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Window;
            this.panel2.Controls.Add(this.textBox2);
            this.panel2.Controls.Add(this.comboBox1);
            this.panel2.Controls.Add(this.pictureBox4);
            this.panel2.Controls.Add(this.checkBox3);
            this.panel2.Controls.Add(this.checkBox2);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.button30);
            this.panel2.Controls.Add(this.button29);
            this.panel2.Controls.Add(this.richTextBox2);
            this.panel2.Controls.Add(this.textBox7);
            this.panel2.Controls.Add(this.button28);
            this.panel2.Controls.Add(this.textBox6);
            this.panel2.Controls.Add(this.checkedListBox4);
            this.panel2.Controls.Add(this.button27);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Window;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.ForeColor = System.Drawing.Color.Black;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            resources.GetString("comboBox1.Items"),
            resources.GetString("comboBox1.Items1")});
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox4, "pictureBox4");
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.TabStop = false;
            // 
            // checkBox3
            // 
            resources.ApplyResources(this.checkBox3, "checkBox3");
            this.checkBox3.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.UseVisualStyleBackColor = false;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox2
            // 
            resources.ApplyResources(this.checkBox2, "checkBox2");
            this.checkBox2.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.UseVisualStyleBackColor = false;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // button30
            // 
            this.button30.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button30, "button30");
            this.button30.ForeColor = System.Drawing.Color.Black;
            this.button30.Name = "button30";
            this.button30.UseVisualStyleBackColor = false;
            this.button30.Click += new System.EventHandler(this.button30_Click);
            // 
            // button29
            // 
            this.button29.BackColor = System.Drawing.Color.Red;
            this.button29.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.button29, "button29");
            this.button29.ForeColor = System.Drawing.Color.White;
            this.button29.Name = "button29";
            this.button29.UseVisualStyleBackColor = false;
            this.button29.Click += new System.EventHandler(this.button29_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox2.DetectUrls = false;
            resources.ApplyResources(this.richTextBox2, "richTextBox2");
            this.richTextBox2.ForeColor = System.Drawing.Color.Black;
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.SystemColors.Window;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox7, "textBox7");
            this.textBox7.ForeColor = System.Drawing.Color.Black;
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            // 
            // button28
            // 
            this.button28.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button28, "button28");
            this.button28.ForeColor = System.Drawing.Color.Black;
            this.button28.Name = "button28";
            this.button28.UseVisualStyleBackColor = false;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Window;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox6, "textBox6");
            this.textBox6.ForeColor = System.Drawing.Color.Black;
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            // 
            // checkedListBox4
            // 
            this.checkedListBox4.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.checkedListBox4, "checkedListBox4");
            this.checkedListBox4.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox4.FormattingEnabled = true;
            this.checkedListBox4.Name = "checkedListBox4";
            this.checkedListBox4.SelectedIndexChanged += new System.EventHandler(this.checkedListBox4_SelectedIndexChanged);
            // 
            // button27
            // 
            this.button27.BackColor = System.Drawing.Color.Red;
            this.button27.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.button27, "button27");
            this.button27.ForeColor = System.Drawing.Color.White;
            this.button27.Name = "button27";
            this.button27.UseVisualStyleBackColor = false;
            this.button27.Click += new System.EventHandler(this.button27_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.DetectUrls = false;
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.ForeColor = System.Drawing.Color.Black;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.textBox3, "textBox3");
            this.textBox3.ForeColor = System.Drawing.Color.Black;
            this.textBox3.Name = "textBox3";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button2, "button2");
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button3, "button3");
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button4, "button4");
            this.button4.ForeColor = System.Drawing.Color.Black;
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button5, "button5");
            this.button5.ForeColor = System.Drawing.Color.Black;
            this.button5.Name = "button5";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button6, "button6");
            this.button6.ForeColor = System.Drawing.Color.Black;
            this.button6.Name = "button6";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button7, "button7");
            this.button7.ForeColor = System.Drawing.Color.Black;
            this.button7.Name = "button7";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button8, "button8");
            this.button8.ForeColor = System.Drawing.Color.Black;
            this.button8.Name = "button8";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // checkedListBox3
            // 
            this.checkedListBox3.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox3.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox3.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox3, "checkedListBox3");
            this.checkedListBox3.Name = "checkedListBox3";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox1.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox1.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox1, "checkedListBox1");
            this.checkedListBox1.Name = "checkedListBox1";
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.SystemColors.Control;
            this.button9.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button9, "button9");
            this.button9.Name = "button9";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button10, "button10");
            this.button10.ForeColor = System.Drawing.Color.Black;
            this.button10.Name = "button10";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button11, "button11");
            this.button11.ForeColor = System.Drawing.Color.Black;
            this.button11.Name = "button11";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button12, "button12");
            this.button12.ForeColor = System.Drawing.Color.Black;
            this.button12.Name = "button12";
            this.button12.UseVisualStyleBackColor = false;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // checkedListBox2
            // 
            this.checkedListBox2.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox2.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox2.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox2, "checkedListBox2");
            this.checkedListBox2.Name = "checkedListBox2";
            // 
            // button13
            // 
            this.button13.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button13, "button13");
            this.button13.ForeColor = System.Drawing.Color.Black;
            this.button13.Name = "button13";
            this.button13.UseVisualStyleBackColor = false;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.BackColor = System.Drawing.SystemColors.Control;
            this.button14.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button14, "button14");
            this.button14.Name = "button14";
            this.button14.UseVisualStyleBackColor = false;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button15
            // 
            this.button15.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button15, "button15");
            this.button15.ForeColor = System.Drawing.Color.Black;
            this.button15.Name = "button15";
            this.button15.UseVisualStyleBackColor = false;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // button16
            // 
            this.button16.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button16, "button16");
            this.button16.ForeColor = System.Drawing.Color.Black;
            this.button16.Name = "button16";
            this.button16.UseVisualStyleBackColor = false;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // button17
            // 
            this.button17.BackColor = System.Drawing.SystemColors.Control;
            this.button17.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button17, "button17");
            this.button17.Name = "button17";
            this.button17.UseVisualStyleBackColor = false;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.ForeColor = System.Drawing.Color.Black;
            this.textBox1.Name = "textBox1";
            // 
            // button18
            // 
            this.button18.BackColor = System.Drawing.SystemColors.Control;
            this.button18.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button18, "button18");
            this.button18.Name = "button18";
            this.button18.UseVisualStyleBackColor = false;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // button20
            // 
            this.button20.BackColor = System.Drawing.SystemColors.Control;
            this.button20.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button20, "button20");
            this.button20.Name = "button20";
            this.button20.UseVisualStyleBackColor = false;
            this.button20.Click += new System.EventHandler(this.button20_Click);
            // 
            // button19
            // 
            this.button19.BackColor = System.Drawing.SystemColors.Control;
            this.button19.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button19, "button19");
            this.button19.Name = "button19";
            this.button19.UseVisualStyleBackColor = false;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // button21
            // 
            this.button21.BackColor = System.Drawing.SystemColors.Control;
            this.button21.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button21, "button21");
            this.button21.Name = "button21";
            this.button21.UseVisualStyleBackColor = false;
            this.button21.Click += new System.EventHandler(this.button21_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox1.ForeColor = System.Drawing.Color.Black;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.textBox4, "textBox4");
            this.textBox4.ForeColor = System.Drawing.Color.Black;
            this.textBox4.Name = "textBox4";
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Window;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox5, "textBox5");
            this.textBox5.ForeColor = System.Drawing.Color.Black;
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            // 
            // button22
            // 
            this.button22.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button22, "button22");
            this.button22.ForeColor = System.Drawing.Color.Black;
            this.button22.Name = "button22";
            this.button22.UseVisualStyleBackColor = false;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // button23
            // 
            this.button23.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button23, "button23");
            this.button23.ForeColor = System.Drawing.Color.Black;
            this.button23.Name = "button23";
            this.button23.UseVisualStyleBackColor = false;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // button24
            // 
            this.button24.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button24, "button24");
            this.button24.ForeColor = System.Drawing.Color.Black;
            this.button24.Name = "button24";
            this.button24.UseVisualStyleBackColor = false;
            this.button24.Click += new System.EventHandler(this.button24_Click);
            // 
            // button25
            // 
            this.button25.BackColor = System.Drawing.SystemColors.Control;
            this.button25.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.button25, "button25");
            this.button25.ForeColor = System.Drawing.Color.Black;
            this.button25.Name = "button25";
            this.button25.UseVisualStyleBackColor = false;
            this.button25.Click += new System.EventHandler(this.button25_Click);
            // 
            // button26
            // 
            this.button26.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.button26, "button26");
            this.button26.FlatAppearance.BorderColor = System.Drawing.Color.DarkSlateBlue;
            this.button26.FlatAppearance.BorderSize = 0;
            this.button26.ForeColor = System.Drawing.Color.Black;
            this.button26.Name = "button26";
            this.button26.UseVisualStyleBackColor = false;
            this.button26.Click += new System.EventHandler(this.button26_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.button9);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.button33);
            this.panel1.Controls.Add(this.button32);
            this.panel1.Controls.Add(this.checkBox4);
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.button26);
            this.panel1.Controls.Add(this.button23);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.button21);
            this.panel1.Controls.Add(this.button19);
            this.panel1.Controls.Add(this.button20);
            this.panel1.Controls.Add(this.button18);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.button17);
            this.panel1.Controls.Add(this.button14);
            this.panel1.Controls.Add(this.button6);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.textBox3);
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Controls.Add(this.pictureBox5);
            this.panel1.Controls.Add(this.button25);
            this.panel1.Controls.Add(this.textBox5);
            this.panel1.Controls.Add(this.textBox4);
            this.panel1.Controls.Add(this.button16);
            this.panel1.Controls.Add(this.button15);
            this.panel1.Controls.Add(this.button13);
            this.panel1.Controls.Add(this.button12);
            this.panel1.Controls.Add(this.button11);
            this.panel1.Controls.Add(this.button10);
            this.panel1.Controls.Add(this.button8);
            this.panel1.Controls.Add(this.button7);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.checkedListBox1);
            this.panel1.Controls.Add(this.checkedListBox3);
            this.panel1.Controls.Add(this.button35);
            this.panel1.Controls.Add(this.checkedListBox2);
            this.panel1.Controls.Add(this.checkedListBox5);
            this.panel1.Controls.Add(this.button34);
            this.panel1.Controls.Add(this.button31);
            this.panel1.Controls.Add(this.button24);
            this.panel1.Controls.Add(this.button22);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // button33
            // 
            this.button33.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button33, "button33");
            this.button33.ForeColor = System.Drawing.Color.Black;
            this.button33.Name = "button33";
            this.button33.UseVisualStyleBackColor = false;
            this.button33.Click += new System.EventHandler(this.button33_Click);
            // 
            // button32
            // 
            this.button32.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button32, "button32");
            this.button32.ForeColor = System.Drawing.Color.Black;
            this.button32.Name = "button32";
            this.button32.UseVisualStyleBackColor = false;
            this.button32.Click += new System.EventHandler(this.button32_Click);
            // 
            // checkBox4
            // 
            resources.ApplyResources(this.checkBox4, "checkBox4");
            this.checkBox4.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox4.ForeColor = System.Drawing.Color.Black;
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.UseVisualStyleBackColor = false;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pictureBox5, "pictureBox5");
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.TabStop = false;
            // 
            // button35
            // 
            this.button35.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button35, "button35");
            this.button35.ForeColor = System.Drawing.Color.Black;
            this.button35.Name = "button35";
            this.button35.UseVisualStyleBackColor = false;
            this.button35.Click += new System.EventHandler(this.button35_Click);
            // 
            // checkedListBox5
            // 
            this.checkedListBox5.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox5.ForeColor = System.Drawing.Color.Black;
            this.checkedListBox5.FormattingEnabled = true;
            resources.ApplyResources(this.checkedListBox5, "checkedListBox5");
            this.checkedListBox5.Name = "checkedListBox5";
            // 
            // button34
            // 
            this.button34.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button34, "button34");
            this.button34.ForeColor = System.Drawing.Color.Black;
            this.button34.Name = "button34";
            this.button34.UseVisualStyleBackColor = false;
            this.button34.Click += new System.EventHandler(this.button34_Click);
            // 
            // button31
            // 
            this.button31.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.button31, "button31");
            this.button31.ForeColor = System.Drawing.Color.Black;
            this.button31.Name = "button31";
            this.button31.UseVisualStyleBackColor = false;
            this.button31.Click += new System.EventHandler(this.button31_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Button button28;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.CheckedListBox checkedListBox4;
        private System.Windows.Forms.Button button27;
        private System.Windows.Forms.Button button29;
        private System.Windows.Forms.Button button30;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckedListBox checkedListBox3;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.CheckedListBox checkedListBox2;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckedListBox checkedListBox5;
        private System.Windows.Forms.Button button33;
        private System.Windows.Forms.Button button32;
        private System.Windows.Forms.Button button31;
        private System.Windows.Forms.Button button35;
        private System.Windows.Forms.Button button34;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}
using System;
using System.Windows.Forms;
using System.IO;
namespace midify
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += new EventHandler(ApplicationExits);
            Application.Run(new Form1());
        }
        private static void ApplicationExits(object sender, EventArgs e)
        {
            Form1.StopCurrentSong();
            Form1.DeleteLoadedSongs();
            Form1.DeleteLeftOvers(false);
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
namespace midify_restarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(3000);
            Process Midify = new Process();
            Midify.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\midify.exe";
            Midify.StartInfo.UseShellExecute = false;
            Midify.StartInfo.CreateNoWindow = false;
            Midify.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Midify.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            Midify.Start();
        }
    }
}
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
namespace midify_uninstaller
{
    public partial class Form1 : Form
    {
        private readonly List<string> Version = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "midify uninstaller";
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox2.Visible = true;
            button2.Visible = true;
            button2.Enabled = false;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "\"where will i rest?\"";
            textBox1.Visible = true;
            textBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox2.Visible = false;
            button1.Visible = true;
            button2.Visible = false;
            button3.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            try
            {
                var tempdirfile = new List<string>();
                var tempdirfile1 = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Temp\\midify\\midify legacy.txt";
                var tempdirfile2 = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Temp\\midify\\midify v2.txt";
                if (checkBox1.Checked && !checkBox2.Checked)
                {
                    tempdirfile.Add(tempdirfile1);
                    Version.Add("midify legacy");
                }
                else if (!checkBox1.Checked && checkBox2.Checked)
                {
                    tempdirfile.Add(tempdirfile2);
                    Version.Add("midify v2");
                }
                else if (checkBox1.Checked && checkBox2.Checked)
                {
                    tempdirfile.Add(tempdirfile1);
                    tempdirfile.Add(tempdirfile2);
                    Version.Add("midify legacy"); 
                    Version.Add("midify v2");
                }
                if (checkBox1.Checked && !File.Exists(tempdirfile1) || checkBox2.Checked && !File.Exists(tempdirfile2))
                {
                    throw new Exception();
                }
                for (int i = 0; i < tempdirfile.Count; i++)
                {
                    SetUpDeletion(tempdirfile[i], Version[i], false);
                }
                MessageBox.Show("midify was successfully uninstalled, you can now delete the uninstaller located on your desktop, stay safe!", "finshed deleting", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            catch (Exception)
            {
                MessageBox.Show("there should have been a save file, but there isn't", "Missing save file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Visible = false;
                textBox3.Visible = true;
                textBox4.Visible = true;
                button4.Visible = true;
            }
        }
        private void SetUpDeletion(string tempdirfile, string version, bool recovery)
        {
            try
            {
                var path = "";
                if (!recovery)
                {
                    path = File.ReadAllText(tempdirfile);
                }
                else
                {
                    path = textBox4.Text;
                }
                var tempdir = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Temp\\midify";
                var shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), version + ".lnk");
                var Fileamount = Directory.GetFiles(tempdir);
                DirectoryInfo maindir = new DirectoryInfo(path);
                DeleteFiles(maindir);
                if (Directory.Exists(path + "\\cfg\\playlists"))
                {
                    Directory.Delete(path + "\\cfg\\playlists");
                }
                if (Directory.Exists(path + "\\cfg"))
                {
                    Directory.Delete(path + "\\cfg");
                }
                if (Directory.Exists(path + "\\en"))
                {
                    Directory.Delete(path + "\\en");
                }
                if (File.Exists(shortcut))
                {
                    File.Delete(shortcut);
                }
                if (!recovery)
                {
                    File.Delete(tempdirfile);
                }
                if (Directory.Exists(tempdir) && Fileamount.Length == 0)
                {
                    Directory.Delete(tempdir);
                }
                Directory.Delete(path);
            }
            catch (IOException)
            {
                MessageBox.Show("something went wrong while trying to delete the main directories. Make sure the program is closed and try again", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }       
        private void DeleteFiles(DirectoryInfo directory)
        {
            try
            {
                var files = directory.GetFiles();
                var subdir = directory.GetDirectories();
                for (int i = 0; i < files.Length; i++)
                {
                    files[i].Delete();
                }
                for (int i = 0; i < subdir.Length; i++)
                {
                    DeleteFiles(subdir[i]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("something went wrong while trying to uninstall midify. Make sure the program is closed and try again", "System.IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void NextButtonControl()
        {
            if (checkBox1.Checked || checkBox2.Checked)
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            NextButtonControl();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            NextButtonControl();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Visible = false;
            textBox2.Visible = true;
            textBox3.Visible = false;
            textBox4.Visible =  false;
            button1.Visible = false;
            button2.Visible = true;
            button2.Enabled = false;
            button3.Visible = false;
            button4.Visible = false;
            checkBox1.Visible = true;
            checkBox1.Checked = false;
            checkBox2.Visible = true;
            checkBox2.Checked = false;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox4.Text))
            {
                for (int i = 0; i < Version.Count; i++)
                {
                    SetUpDeletion("", Version[i], true);
                    MessageBox.Show("midify was successfully uninstalled, you can now delete the uninstaller located on your desktop, stay safe!", "finshed deleting", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }
        }
    }
}
namespace midify_uninstaller
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Arial", 11F);
            this.textBox1.ForeColor = System.Drawing.Color.Black;
            this.textBox1.Location = new System.Drawing.Point(165, 37);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ShortcutsEnabled = false;
            this.textBox1.Size = new System.Drawing.Size(138, 17);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.Visible = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button1.Location = new System.Drawing.Point(165, 111);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 43);
            this.button1.TabIndex = 90;
            this.button1.Text = "uninstall midify";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Arial", 8.25F);
            this.checkBox1.Location = new System.Drawing.Point(46, 52);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(89, 18);
            this.checkBox1.TabIndex = 92;
            this.checkBox1.Text = "midify legacy";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Font = new System.Drawing.Font("Arial", 8.25F);
            this.checkBox2.Location = new System.Drawing.Point(369, 52);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(69, 18);
            this.checkBox2.TabIndex = 93;
            this.checkBox2.Text = "midify v2";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Visible = false;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Arial", 9F);
            this.textBox2.Location = new System.Drawing.Point(0, 18);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(498, 14);
            this.textBox2.TabIndex = 94;
            this.textBox2.Text = "select one or both of the versions you want to uninstall";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox2.Visible = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button3.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Arial", 10F);
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button3.Location = new System.Drawing.Point(12, 158);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(66, 26);
            this.button3.TabIndex = 95;
            this.button3.Text = "back";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Font = new System.Drawing.Font("Arial", 9F);
            this.textBox3.Location = new System.Drawing.Point(12, 52);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(178, 14);
            this.textBox3.TabIndex = 96;
            this.textBox3.Text = "input directory of the executable:";
            this.textBox3.Visible = false;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(190, 50);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(300, 20);
            this.textBox4.TabIndex = 97;
            this.textBox4.Visible = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button2.Enabled = false;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Arial", 10F);
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button2.Location = new System.Drawing.Point(179, 116);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(109, 33);
            this.button2.TabIndex = 91;
            this.button2.Text = "continue";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button4.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Font = new System.Drawing.Font("Arial", 10F);
            this.button4.ForeColor = System.Drawing.Color.Black;
            this.button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button4.Location = new System.Drawing.Point(179, 116);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(109, 33);
            this.button4.TabIndex = 98;
            this.button4.Text = "continue";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 196);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace midify_uninstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using IWshRuntimeLibrary;
namespace midify_installer
{
    public partial class Form1 : Form
    {
        private readonly WebClient Client = new WebClient();
        private readonly Uri MidifyOldlink = new Uri("https://drive.google.com/uc?export=download&id=1RuNLgtgQB-TUGsNUkrNjBrbFGKAkcdgQ");
        private readonly Uri Midify2link = new Uri("https://drive.google.com/uc?export=download&id=1uuAQIphY39Bge0GHJkShegIBm0pECKjq");
        private readonly List<string> ExePaths = new List<string>();
        private Thread DownloadThread;
        public Form1()
        {
            this.Text = "midify installer";
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            SetrichTextboxesUp();
        }
        private void NextButtonControl()
        {
            if (checkBox3.Checked || checkBox4.Checked)
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }
        private void SetrichTextboxesUp()
        {
            richTextBox1.AppendText("midify legacy version");
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText("the original version of midify");
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText("- supports wav and mp3 files");
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText("- has very little loading times when listening to music");
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText("- uses slightly less resources than midify v2");
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText("- i like it more");
            richTextBox2.AppendText("midify v2");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("the latest version of midify");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("- supports mp3, mp4, wav, midi and mid files");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("- longer loading times when listening to music");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("- has in app volume regulation");
            richTextBox2.AppendText(Environment.NewLine);
            richTextBox2.AppendText("- allows for pausing/resuming music as well as skipping to points within songs"); 
        }
        private void button1_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = false;
            DownloadThread = new Thread(() => DownloadMidify(MidifyOldlink));
            DownloadThread.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
            richTextBox2.Visible = false;
            checkBox3.Visible = false;
            checkBox4.Visible = false;
            button2.Visible = false;
            DownloadLogic();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            checkBox4.Checked = false;
            DownloadThread = new Thread(() => DownloadMidify(Midify2link));
            DownloadThread.Start();
        }
        private void DownloadLogic()
        {
            if (checkBox3.Checked && !checkBox4.Checked || checkBox3.Checked && checkBox4.Checked)
            {
                var stdpathML = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs";
                textBox1.Text = stdpathML;
                panel1.Visible = true;
            }
            else if (checkBox4.Checked && !checkBox3.Checked)
            {
                var stdpathM2 = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs";
                textBox3.Text = stdpathM2;
                panel2.Visible = true;
            }
        }
        private void DownloadMidify(Uri downloadlink)
        {
            Invoke(new Action(() =>
            {
                panel1.Visible = false;
                panel2.Visible = false;
                textBox2.Text = "thanks for downloading midify!";
                textBox2.AppendText(Environment.NewLine);
                textBox2.AppendText("(do not close this window)");
                textBox2.Visible = true;
            }));
            Client.DownloadFile(downloadlink, "midify.zip");
            var zipfile = Directory.GetCurrentDirectory() + "\\midify.zip";
            var tempdir = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Temp\\midify";
            var destination = "";
            var desktopShortcut = false;
            var exePath = "";
            var version = "";
            var dir = "";
            if (downloadlink == MidifyOldlink)
            {
                destination = textBox1.Text;
                dir = "\\midifylegacy";
                exePath = destination + dir + "\\midify.exe";
                version = "midify legacy";
                desktopShortcut = checkBox1.Checked;
            }
            else if (downloadlink == Midify2link)
            {
                destination = textBox3.Text;
                dir = "\\midify2";
                exePath = destination + dir + "\\midify.exe";
                version = "midify v2";
                desktopShortcut = checkBox5.Checked;
            }
            ZipFile.ExtractToDirectory(zipfile, destination + dir);
            System.IO.File.Delete(zipfile);
            CreateShortcut(desktopShortcut, exePath, version);
            if (!Directory.Exists(tempdir))
            {
                Directory.CreateDirectory(tempdir);
            }
            System.IO.File.WriteAllText(tempdir + "\\" + version + ".txt", destination + dir);
            ExePaths.Add(exePath);
            DownloadComplete();
        }
        private void CreateShortcut(bool createshortcut, string exePath, string midifyVersion)
        {
            if (createshortcut == true)
            {
                string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), midifyVersion + ".lnk");
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath; 
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Save();
            }
        }
        private void DownloadComplete() 
        {
            if (checkBox4.Checked)
            {
                Invoke(new Action(() =>
                {
                    checkBox4.Checked = false;
                    var stdpathM2 = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs";
                    textBox3.Text = stdpathM2;
                    panel2.Visible = true;
                    textBox2.Visible = false;
                }));
            }
            else
            {
                MessageBox.Show("installation complete you can now close this window", "finished installing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (int i = 0; i < ExePaths.Count; i++)
                {
                    LaunchMidify(ExePaths[i]);
                }
                Application.Exit();
            }
        }
        private void LaunchMidify(string executable)
        {
            try
            {
                var dir = executable.Split('\\');
                if (checkBox2.Checked == true && dir[dir.Length - 2] == Version.midifylegacy.ToString() || checkBox6.Checked && dir[dir.Length - 2] == Version.midify2.ToString())
                {
                    Process Midify = new Process();
                    Midify.StartInfo.FileName = executable;
                    Midify.StartInfo.CreateNoWindow = false;
                    Midify.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    Midify.StartInfo.UseShellExecute = false;
                    Midify.StartInfo.WorkingDirectory = Path.GetDirectoryName(executable);
                    Midify.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            NextButtonControl();
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            NextButtonControl();
        }
    }
    enum Version
    {
        midifylegacy,
        midify2
    }
}
namespace midify_installer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Location = new System.Drawing.Point(1, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 197);
            this.panel1.TabIndex = 7;
            this.panel1.Visible = false;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 9F);
            this.button1.Location = new System.Drawing.Point(183, 145);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(141, 36);
            this.button1.TabIndex = 9;
            this.button1.Text = "install midify legacy";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Arial", 9F);
            this.textBox1.Location = new System.Drawing.Point(51, 13);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(398, 21);
            this.textBox1.TabIndex = 7;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Arial", 9F);
            this.checkBox1.Location = new System.Drawing.Point(51, 59);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(154, 19);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "create desktop shortcut";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Font = new System.Drawing.Font("Arial", 9F);
            this.checkBox2.Location = new System.Drawing.Point(51, 103);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(153, 19);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "launch after installation";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Font = new System.Drawing.Font("Arial", 8.25F);
            this.checkBox3.Location = new System.Drawing.Point(36, 224);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(89, 18);
            this.checkBox3.TabIndex = 1;
            this.checkBox3.Text = "midify legacy";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Font = new System.Drawing.Font("Arial", 8.25F);
            this.checkBox4.Location = new System.Drawing.Point(391, 224);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(69, 18);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "midify v2";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Arial", 9F);
            this.textBox2.Location = new System.Drawing.Point(147, 61);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(207, 49);
            this.textBox2.TabIndex = 7;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox2.Visible = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Arial", 9F);
            this.richTextBox1.Location = new System.Drawing.Point(1, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(204, 216);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            this.richTextBox2.Font = new System.Drawing.Font("Arial", 9F);
            this.richTextBox2.Location = new System.Drawing.Point(298, 2);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox2.Size = new System.Drawing.Size(197, 216);
            this.richTextBox2.TabIndex = 8;
            this.richTextBox2.Text = "";
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Font = new System.Drawing.Font("Arial", 9F);
            this.button2.Location = new System.Drawing.Point(209, 224);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "next";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.textBox3);
            this.panel2.Controls.Add(this.checkBox5);
            this.panel2.Controls.Add(this.checkBox6);
            this.panel2.Location = new System.Drawing.Point(1, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(500, 197);
            this.panel2.TabIndex = 13;
            this.panel2.Visible = false;
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Arial", 9F);
            this.button3.Location = new System.Drawing.Point(183, 145);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(141, 36);
            this.button3.TabIndex = 9;
            this.button3.Text = "install midify v2";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Arial", 9F);
            this.textBox3.Location = new System.Drawing.Point(51, 13);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(398, 21);
            this.textBox3.TabIndex = 7;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Font = new System.Drawing.Font("Arial", 9F);
            this.checkBox5.Location = new System.Drawing.Point(51, 59);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(154, 19);
            this.checkBox5.TabIndex = 8;
            this.checkBox5.Text = "create desktop shortcut";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Font = new System.Drawing.Font("Arial", 9F);
            this.checkBox6.Location = new System.Drawing.Point(51, 103);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(153, 19);
            this.checkBox6.TabIndex = 10;
            this.checkBox6.Text = "launch after installation";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 254);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace midify_installer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}