using System.IO;

namespace AcousticFingerprintingLibrary
{
    public class Media
    {
        // Full file path
        public string FilePath { get; set; }
        // Full directory path minus the actual file
        public string DirPath { get; set; }
        // File name only
        public string FileName { get; set; }

        // Require full file path to work out the dir and file name
        public Media(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(this.FilePath);
            DirPath = Path.GetDirectoryName(this.FilePath);
        }
    }
}