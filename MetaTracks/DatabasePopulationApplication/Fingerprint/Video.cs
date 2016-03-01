using System;
using System.IO;
using System.Runtime.CompilerServices;
using NAudio.Wave;

namespace dbApp.Fingerprint
{
    public class Video
    {
        public string FilePath { get; set; }
        public string DirPath { get; set; }

        public Video(string FilePath)
        {
            this.FilePath = FilePath;
            this.DirPath = Path.GetDirectoryName(this.FilePath);
        }
    }
}
