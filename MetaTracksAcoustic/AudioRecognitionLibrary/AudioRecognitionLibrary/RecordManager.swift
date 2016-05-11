import AVFoundation

@objc public class RecordManager:NSObject {
    public static var storedFingerprints:[HashedFingerprint] = []
    private static var audioPlayer : AVAudioPlayer?
    private static var audioRecorder : AVAudioRecorder?
    private static let baseString : String = NSTemporaryDirectory()
    private static let session = AVAudioSession.sharedInstance()
    private static var manager:FingerprintManager = FingerprintManager()
    private static var currentFile = NSURL()
    public static var LatestTimestamp:Double = 0
    public static var selectedMovie:String = String()
    public static var availableMovies:[String] = []
    private static var receivedHashes:[String] = []
    private static var receivedTimestamps:[String] = []
    private static var ableToRecord:Bool = true
    
    public override init(){
        
    }
    
    public static func setStoredFingerprints(inputHashes: [String], inputTimestamps: [String]){
        self.storedFingerprints = getFingerprints(inputHashes, timestamps: inputTimestamps)
        
    }
    
    public static func setMovie(inputMovie: String){
        self.selectedMovie = inputMovie
    }
    
    public static func indexMovies(successHandler: (response: [String]) -> Void) {
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let url = NSURL(string: "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL")
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in

            
            if error != nil {
                return
            }
            let temp = String(data: data!, encoding: NSUTF8StringEncoding)!
            let responseString = temp.componentsSeparatedByString(",")
            successHandler(response: responseString)
        }
        dataTask!.resume();
    }
    
    
    public static func getHashes(successHandler: (response: [String]) -> Void) {
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let orig = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='" + selectedMovie + "'"
        let spacesEscaped :String = orig.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url = NSURL(string: spacesEscaped)
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in
            
            let hashtemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            let responseString = hashtemp.componentsSeparatedByString(";")
            successHandler(response: responseString)
        }
         dataTask?.resume()
    }
    
    
    public static func getTimestamps(successHandler: (response: [String]) -> Void) {
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        dataTask?.resume()
        let orig2 = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='" + self.selectedMovie + "'"
        let spacesEscaped2 :String = orig2.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url2 = NSURL(string: spacesEscaped2)
        
        dataTask = defaultSession.dataTaskWithURL(url2!) {
            data, response, error in
            
            let timestamptemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            let responseString = timestamptemp.componentsSeparatedByString(";")
            successHandler(response: responseString)
        }
        dataTask?.resume()
    }

    private static func getFingerprints(hashes: [String], timestamps: [String]) -> [HashedFingerprint]{
        var t = getFingerprints(hashes, receivedTimestamps: timestamps)
        return t
    }
    
    private static func setRecorder(iterator: Int) -> NSURL {
        do {
            let pathComponents = [baseString, "split" + String(iterator) + ".wav"]
            let audioURL = NSURL.fileURLWithPathComponents(pathComponents)!
            print(audioURL)
            var recordSettings = [String : AnyObject]()
            recordSettings[AVFormatIDKey] = Int(kAudioFormatLinearPCM)
            recordSettings[AVSampleRateKey] = 5512
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
    public override static func initialize(){
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
                LatestTimeStamp = result
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

    
    private static func getFingerprints(receivedHashes: [String], receivedTimestamps: [String]) -> [HashedFingerprint]{
        var t = manager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps: receivedTimestamps)
        return t
        
    }
    
}