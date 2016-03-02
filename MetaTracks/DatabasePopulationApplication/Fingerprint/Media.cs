using System.IO;

namespace dbApp.Fingerprint
{
    class Media
    {
        public string FilePath { get; set; }
        public string DirPath { get; set; }
        public string FileName { get; set; }

        public Media(string filePath)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(this.FilePath);
            this.DirPath = Path.GetDirectoryName(this.FilePath);
        }
    }
}
