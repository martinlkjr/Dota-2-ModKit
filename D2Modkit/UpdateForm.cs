﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace D2ModKit
{
    public partial class UpdateForm : Form
    {
        public UpdateForm(string url, string newVers)
        {
            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            InitializeComponent();
            Debug.WriteLine("Downloading new vers.");

            // delete D2ModKit.zip if exists.
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip"));
            }
            label1.Text = "Updating D2ModKit to v" + newVers + " ...";
            // start downloading.
            wc.DownloadFileAsync(new Uri(url),
                Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip"));

        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // delete the nested D2ModKit_temp folder if it exists.
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp")))
            {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"), true);
            }

            // extract it now
            string zipPath = Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip");
            ZipFile.ExtractToDirectory(zipPath, Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"));

            // get the new D2ModKit.exe.
            string tempDir = Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp");
            string path = Path.Combine(tempDir, "D2ModKit.exe");

            // delete D2ModKit_new.exe if it exists.
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe"));
            }

            // move the new d2modkit.exe to the main folder, and rename it.
            File.Move(path, Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe"));

            // add other files and folders if they're not already in the main folder.
            string[] files = Directory.GetFiles(tempDir);
            for (int i = 0; i < files.Length; i++)
            {
                string name = files[i].Substring(files[i].LastIndexOf('\\')+1);
                // it will raise an exception if file is already there.
                try
                {
                    File.Move(files[i], Path.Combine(Environment.CurrentDirectory, name));
                }
                catch (Exception) {}
            }
            string[] dirs = Directory.GetDirectories(tempDir);
            for (int i = 0; i < dirs.Length; i++)
            {
                string name = dirs[i].Substring(dirs[i].LastIndexOf('\\')+1);
                // it will raise an exception if dir is already there.
                try
                {
                    Directory.Move(dirs[i], Path.Combine(Environment.CurrentDirectory, name));
                }
                catch (Exception) {}
            }

            //delete the D2ModKit_temp folder.
            try {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"), true);
                // delete .zip
                File.Delete(zipPath);
            }
            catch (Exception) {  }

            // now run our other process to quit this application, and replace the .exe's.
            string batPath = Path.Combine(Environment.CurrentDirectory, "updater.bat");

            // let's always have a fresh batch file.
            if (File.Exists(batPath))
            {
                File.Delete(batPath);
            }

            // Create a file to write to.
            string orig = Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe");
            string dest = Path.Combine(Environment.CurrentDirectory, "D2ModKit.exe");
            using (StreamWriter sw = File.CreateText(batPath)) 
            {
                sw.WriteLine("taskkill /f /im \"D2ModKit.exe\"");
                sw.WriteLine("SLEEP 1");
                sw.WriteLine("DEL /Q " + dest);
                sw.WriteLine("MOVE /Y \"d2modkit_new.exe\" \"D2ModKit.exe\"");
                sw.WriteLine("start d2modkit.exe");
                sw.WriteLine("DEL /Q updater.bat");
            }
            Process.Start(batPath);
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = progressBar1.Value + (e.ProgressPercentage-progressBar1.Value);
        }
    }
}
