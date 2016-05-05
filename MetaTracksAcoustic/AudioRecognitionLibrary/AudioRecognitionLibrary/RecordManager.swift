import AVFoundation

public class RecordManager {
    var storedFingerprints:[HashedFingerprint] = []
    var audioPlayer : AVAudioPlayer?
    var audioRecorder : AVAudioRecorder?
    let baseString : String = NSTemporaryDirectory()
    let session = AVAudioSession.sharedInstance()
    var manager:FingerprintManager = FingerprintManager()
    var currentFile = NSURL()
    public init(){
    
    }
    
    public func setRecorder(iterator: Int) -> NSURL {
        
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
            print(audioURL)
            currentFile = audioURL
            return audioURL
        } catch (_) {
            return NSURL(fileURLWithPath: "nil")
        }
    }
    public func record(){
        let queue = NSOperationQueue()
        do {
            try session.setCategory(AVAudioSessionCategoryRecord)
            try session.setActive(true)
        } catch (_) {

        }
        
        queue.addOperationWithBlock() {
            for(var i = 1; i <= 10000; i++){
                do{
                    var filePath = self.setRecorder(i)
                    self.audioRecorder?.record()
                    sleep(3)
                    self.audioRecorder?.stop()
                    let monoArray = try BassProxy.GetSamplesMono(filePath, sampleRate: 5512)
                    var preliminaryFingerprints = self.manager.CreateFingerprints(monoArray)
                    var test = self.manager.GetFingerHashes(preliminaryFingerprints)
                    var result = self.manager.CompareFingerprintListsHighest(test, toCompare: self.storedFingerprints)
                }catch (_) {
                    
                }
            }
            
        }
    }
    
    public func play(){
        var error:NSError?
        do{
        try audioPlayer = AVAudioPlayer(contentsOfURL: currentFile)
        try audioPlayer?.prepareToPlay()
        try audioPlayer?.play()
        }catch (_){
                
        }
    }
    
    public func getFingerprints(receivedHashes: [String], receivedTimestamps: [String]){
        var t = manager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps: receivedTimestamps)
        storedFingerprints = t
        
    }
        
    }