using NAudio.Wave;

namespace dbApp.Fingerprint
{
    public class Video
    {
        public string FilePath { get; set; }
    
        public Video(string FilePath)
        {
            this.FilePath = FilePath; 
        }
    }
}
