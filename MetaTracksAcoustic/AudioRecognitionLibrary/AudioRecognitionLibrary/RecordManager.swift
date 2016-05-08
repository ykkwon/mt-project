import AVFoundation

public class RecordManager {
    public static var storedFingerprints:[HashedFingerprint] = []
    private static var audioPlayer : AVAudioPlayer?
    private static var audioRecorder : AVAudioRecorder?
    private static let baseString : String = NSTemporaryDirectory()
    private static let session = AVAudioSession.sharedInstance()
    private static var manager:FingerprintManager = FingerprintManager()
    private static var currentFile = NSURL()
    
    public static var selectedMovie:String = String()
    public static var availableMovies:[String] = []
    private static var receivedHashes:[String] = []
    private static var receivedTimestamps:[String] = []
    private static var ableToRecord:Bool = true
    
    public init(){
        
    }
    public static func indexMovies(){
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let url = NSURL(string: "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL")
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in
            
            let temp = String(data: data!, encoding: NSUTF8StringEncoding)!
            availableMovies = temp.componentsSeparatedByString(",")
            print("Indexed movies.")
        }
        dataTask?.resume()
        
    }
    
    public static func getFingerprintsFull(){
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let orig = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='" + selectedMovie + "'"
        let spacesEscaped :String = orig.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url = NSURL(string: spacesEscaped)
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in
            
            let hashtemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            self.receivedHashes = hashtemp.componentsSeparatedByString(";")
        }
        dataTask?.resume()
        let orig2 = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='" + self.selectedMovie + "'"
        let spacesEscaped2 :String = orig2.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url2 = NSURL(string: spacesEscaped2)
        
        dataTask = defaultSession.dataTaskWithURL(url2!) {
            data, response, error in
            
            let timestamptemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            self.receivedTimestamps = timestamptemp.componentsSeparatedByString(";")
            print("Received hashes and timestamps for " + self.selectedMovie + ".")
            print("Hashes: " + String(self.receivedHashes.count))
            print("Timestamps: " + String(self.receivedTimestamps.count))
            getFingerprints(receivedHashes, receivedTimestamps: receivedTimestamps)
            
        }
        dataTask?.resume()
    }
    
    public static func setRecorder(iterator: Int) -> NSURL {
        do {
            let pathComponents = [baseString, "split" + String(iterator) + ".wav"]
            let audioURL = NSURL.fileURLWithPathComponents(pathComponents)!
            var recordSettings = [String : AnyObject]()
            recordSettings[AVFormatIDKey] = Int(kAudioFormatLinearPCM)
            recordSettings[AVSampleRateKey] = 5512.0
            recordSettings[AVNumberOfChannelsKey] = 1
            recordSettings[AVEncoderAudioQualityKey] = AVAudioQuality.Max.rawValue
            self.audioRecorder = try AVAudioRecorder(URL: audioURL, settings: recordSettings)
            self.audioRecorder!.prepareToRecord()
            currentFile = audioURL
            return audioURL
        } catch (_) {
            return NSURL(fileURLWithPath: "nil")
        }
    }
    public static func initialize(){
        BassProxy.Initialize()
    }
    public static func startSyncing(){
        ableToRecord = true
        if(ableToRecord == true){
        do {
            try session.setCategory(AVAudioSessionCategoryRecord)
            try session.setActive(true)
        } catch (_) {
            
        }
        for(var i = 1; i <= Int.max; i++){
            if(ableToRecord == true){
            do{
                var filePath = self.setRecorder(i)
                self.audioRecorder?.recordForDuration(5)
                sleep(4)
                self.audioRecorder?.stop()
                let monoArray = try BassProxy.GetSamplesMono(filePath, sampleRate: 5512)
                var preliminaryFingerprints = self.manager.CreateFingerprints(monoArray)
                var test = self.manager.GetFingerHashes(preliminaryFingerprints)
                var result = self.manager.CompareFingerprintListsHighest(test, toCompare: self.storedFingerprints)
            }catch (_) {
                
                }
                
            }
            else{
                break
            }
            }
        }
        else{
            ableToRecord = true
        }
        
    }
    
    public static func stopSyncing(){
        ableToRecord = false
    }

    
    private static func getFingerprints(receivedHashes: [String], receivedTimestamps: [String]){
        var t = manager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps: receivedTimestamps)
        storedFingerprints = t
        
    }
    
}