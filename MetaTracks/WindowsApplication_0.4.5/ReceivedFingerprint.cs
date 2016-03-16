using Newtonsoft.Json;

namespace WindowsApplication_0._4._5
{
    class ReceivedFingerprint
    {
        public string Type { get; set; }
        public string Timestamp { get; set; }
        public string Fingerprint { get; set; }
        [JsonProperty("Title")]
        public string Title { get; set; }
    }
}
