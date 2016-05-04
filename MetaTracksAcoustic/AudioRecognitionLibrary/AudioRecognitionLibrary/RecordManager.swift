import UIKit
import AVFoundation

public class RecordManager {
    var storedFingerprints:[HashedFingerprint] = []
    var audioPlayer : AVAudioPlayer?
    var audioRecorder : AVAudioRecorder?
    let baseString : String = NSTemporaryDirectory()
    let session = AVAudioSession.sharedInstance()
    var manager:FingerprintManager = FingerprintManager()
    
    public init(){
    
    }
    
    public func setRecorder(iterator: Int) -> NSURL {
        
        do {
            let pathComponents = [baseString, "split" + String(iterator) + ".wav"]
            let audioURL = NSURL.fileURLWithPathComponents(pathComponents)!
            try session.setCategory(AVAudioSessionCategoryPlayAndRecord)
            try session.overrideOutputAudioPort(AVAudioSessionPortOverride.Speaker)
            try session.setActive(true)
            var recordSettings = [String : AnyObject]()
            recordSettings[AVFormatIDKey] = Int(kAudioFormatLinearPCM)
            recordSettings[AVSampleRateKey] = 5512.0
            recordSettings[AVNumberOfChannelsKey] = 1
            self.audioRecorder = try AVAudioRecorder(URL: audioURL, settings: recordSettings)
            self.audioRecorder!.meteringEnabled = true
            self.audioRecorder!.prepareToRecord()
            print(audioURL)
            return audioURL
        } catch (_) {
            return NSURL(fileURLWithPath: "nil")
        }
    }
    public func record(){
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0)) {
        var iterator = 0
        while(true){
            do{
            var filePath = self.setRecorder(iterator)
            self.audioRecorder?.record()
            sleep(5)
            self.audioRecorder?.stop()
            let monoArray = try BassProxy.GetSamplesMono(filePath, sampleRate: 5512)
            var preliminaryFingerprints = self.manager.CreateFingerprints(monoArray)
            var test = self.manager.GetFingerHashes(preliminaryFingerprints)
            var result = self.manager.CompareFingerprintListsHighest(test, toCompare: self.storedFingerprints)
            iterator++
            }catch (_) {
            
            }
        }
    }
    }
    
    public func getFingerprints(receivedHashes: [String], receivedTimestamps: [String]){
        var t = manager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps: receivedTimestamps)
        storedFingerprints = t
        
    }
        
    }