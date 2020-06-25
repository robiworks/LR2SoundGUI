using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Ionic.Zip;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using System.Security.Permissions;
using Ionic.Zlib;

namespace LR2SoundGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MusicPath != null)
            {
                MusicDirectoryTextBox.Text = Properties.Settings.Default.MusicPath;
            }
        }

        private void ChooseDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
            };
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(ofd.FileName))
                {
                    Properties.Settings.Default.MusicPath = ofd.FileName;
                    Properties.Settings.Default.Save();
                    MusicDirectoryTextBox.Text = Properties.Settings.Default.MusicPath;
                }
                else
                {
                    MessageBox.Show("You have selected an invalid directory. Please select a valid directory.", "Invalid directory selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChooseFolderPackButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select music pack folder..."
            };
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(ofd.FileName))
                {
                    ParseContents(ofd.FileName);
                }
                else
                {
                    MessageBox.Show("You have selected an invalid directory. Please select a valid directory.", "Invalid directory selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChooseZIPPackButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "ZIP files|*.zip",
                Title = "Select music pack ZIP file..."
            };
            if (ofd.ShowDialog() == true)
            {
                if (File.Exists(ofd.FileName))
                {
                    UnpackZip(ofd.FileName);
                    ParseContents(Properties.Settings.Default.MusicPath + "\\temp");
                }
                else
                {
                    MessageBox.Show("You have selected an invalid or non-existent file. Please select a valid file.", "Invalid file selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UnpackZip(string filePath)
        {
            string[] songs = new string[] { "1pack.contents", "menu.1", "sandybay.1", "sandybay.2", "dino.1", "dino.2", "dino.3", "dino.4", "mars.1", "mars.2", "mars.3", "mars.4", "arctic.1", "arctic.2", "arctic.3", "arctic.4", "xalax.1", "xalax.2", "xalax.3", "xalax.4", "bonus.1" };
            ZipFile archive = ZipFile.Read(filePath);
            string tempDir = Properties.Settings.Default.MusicPath + "\\temp\\";
            Directory.CreateDirectory(tempDir);
            foreach (ZipEntry e in archive.Where(e => songs.Contains(e.FileName)))
            {
                e.Extract(tempDir, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        private void PackZip(string directory)
        {
            string[] songs = new string[] { "1pack.contents", "menu.1", "sandybay.1", "sandybay.2", "dino.1", "dino.2", "dino.3", "dino.4", "mars.1", "mars.2", "mars.3", "mars.4", "arctic.1", "arctic.2", "arctic.3", "arctic.4", "xalax.1", "xalax.2", "xalax.3", "xalax.4", "bonus.1" };
            string backupName = "backup.zip";
            using (ZipFile zip = new ZipFile(directory + "\\" + backupName))
            {
                foreach (string song in songs)
                {
                    if (File.Exists(directory + "\\" + song))
                    {
                        zip.AddItem(directory + "\\" + song, "");
                    }
                }
                if (FastestComprBtn.IsChecked == true)
                {
                    zip.CompressionLevel = CompressionLevel.BestSpeed;
                }
                else if (BestComprBtn.IsChecked == true)
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                }
                else
                {
                    zip.CompressionLevel = CompressionLevel.Default;
                }
                zip.Save();
            }
        }

        public class Song
        {
            public string Filename { get; set; }
            public string Title { get; set; }
        }

        private void ParseContents(string folderPath)
        {
            string[] songs = new string[] { "1pack.contents", "menu.1", "sandybay.1", "sandybay.2", "dino.1", "dino.2", "dino.3", "dino.4", "mars.1", "mars.2", "mars.3", "mars.4", "arctic.1", "arctic.2", "arctic.3", "arctic.4", "xalax.1", "xalax.2", "xalax.3", "xalax.4", "bonus.1" };
            string[] files = Directory.GetFiles(folderPath);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);
            }
            List<Song> items = new List<Song>();
            if (files.Contains("1pack.contents"))
            {
                string[] contents = File.ReadAllLines(folderPath + "\\1pack.contents");
                for (int i = 6; i < 34; i++)
                {
                    if (contents[i].StartsWith("="))
                    {
                        continue;
                    }
                    else
                    {
                        string file = contents[i].Substring(0, 12).Trim();
                        string song = contents[i].Substring(13).Trim();
                        if (songs.Contains(file))
                        {
                            items.Add(new Song() { Filename = file, Title = song });
                        }
                        else
                        {
                            items.Add(new Song() { Filename = file, Title = "won't be used" });
                        }
                    }
                }
                items.RemoveAll(s => !files.Contains(s.Filename));
            }
            else
            {
                MessageBox.Show("Selected music pack doesn't contain 1pack.contents file. Continuing without...", "No contents file found", MessageBoxButton.OK, MessageBoxImage.Information);
                foreach (string file in files)
                {
                    if (songs.Contains(file))
                    {
                        items.Add(new Song() { Filename = file, Title = "unknown" });
                    }
                    else
                    {
                        items.Add(new Song() { Filename = file, Title = "won't be used" });
                    }
                }
            }
            PackContentsListView.ItemsSource = items;
        }

        private void SwitchPackButton_Click(object sender, RoutedEventArgs e)
        {
            string[] songs = new string[] { "1pack.contents", "menu.1", "sandybay.1", "sandybay.2", "dino.1", "dino.2", "dino.3", "dino.4", "mars.1", "mars.2", "mars.3", "mars.4", "arctic.1", "arctic.2", "arctic.3", "arctic.4", "xalax.1", "xalax.2", "xalax.3", "xalax.4", "bonus.1" };

        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderBackupBtn.IsChecked == true)
            {

            }
            else if (ZipBackupBtn.IsChecked == true)
            {
                PackZip(Properties.Settings.Default.MusicPath);
            }
        }
    }
}
