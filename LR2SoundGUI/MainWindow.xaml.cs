using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

        private void MainWindow_Unload(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
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

        public static class Globals
        {
            public static bool IsFolderPack { get; set; }
            public static string PackPath { get; set; }
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
                    Globals.PackPath = ofd.FileName;
                    ParseContents(ofd.FileName);
                }
                else
                {
                    MessageBox.Show("You have selected an invalid directory. Please select a valid directory.", "Invalid directory selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            Globals.IsFolderPack = true;
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
                    Globals.PackPath = ofd.FileName;
                    UnpackZip(ofd.FileName);
                    ParseContents(Properties.Settings.Default.MusicPath + "\\temp");
                }
                else
                {
                    MessageBox.Show("You have selected an invalid or non-existent file. Please select a valid file.", "Invalid file selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            Globals.IsFolderPack = false;
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
            string backupName = "backup" + DateTime.Now.ToString("-dd-MM-yy-HH-mm-ss") + ".zip";
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
            string destFolder = Properties.Settings.Default.MusicPath;
            if (Globals.IsFolderPack)
            {
                if (Directory.Exists(Globals.PackPath))
                {
                    string[] files = Directory.GetFiles(Globals.PackPath);
                    foreach (string f in files)
                    {
                        string fileName = System.IO.Path.GetFileName(f);
                        string destFile = System.IO.Path.Combine(destFolder, fileName);
                        File.Copy(f, destFile, true);
                    }
                }
                else
                {
                    MessageBox.Show("Pack does not exist!", "Pack not found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (Directory.Exists(Properties.Settings.Default.MusicPath + "\\temp"))
                {
                    string[] files = Directory.GetFiles(Properties.Settings.Default.MusicPath + "\\temp");
                    foreach (string f in files)
                    {
                        string fileName = System.IO.Path.GetFileName(f);
                        string destFile = System.IO.Path.Combine(destFolder, fileName);
                        File.Copy(f, destFile, true);
                    }
                }
                else
                {
                    MessageBox.Show("Pack does not exist!", "Pack not found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderBackupBtn.IsChecked == true)
            {
                string backupPath = Properties.Settings.Default.MusicPath + "\\backup" + DateTime.Now.ToString("-dd-MM-yy-HH-mm-ss") + "\\";
                Directory.CreateDirectory(backupPath);
                if (Directory.Exists(backupPath))
                {
                    string[] files = Directory.GetFiles(Properties.Settings.Default.MusicPath);
                    foreach (string f in files)
                    {
                        string fileName = System.IO.Path.GetFileName(f);
                        string destFile = System.IO.Path.Combine(backupPath, fileName);
                        File.Copy(f, destFile, true);
                    }
                }
            }
            else if (ZipBackupBtn.IsChecked == true)
            {
                PackZip(Properties.Settings.Default.MusicPath);
            }
        }
    }
}