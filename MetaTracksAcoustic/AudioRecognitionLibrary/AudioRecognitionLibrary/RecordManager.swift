import UIKit
import AVFoundation

public class RecordManager {
    var movie:[HashedFingerprint] = []
    var storedFingerprints:[HashedFingerprint] = []
    var audioPlayer : AVAudioPlayer?
    var audioRecorder : AVAudioRecorder?
    let baseString : String = NSTemporaryDirectory()
    let session = AVAudioSession.sharedInstance()
    
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
        var manager:FingerprintManager = FingerprintManager()
        var iterator = 0
        while(true){
            do{
            var filePath = setRecorder(iterator)
            audioRecorder?.record()
            sleep(3)
            audioRecorder?.stop()
            let monoArray = try BassProxy.GetSamplesMono(filePath, sampleRate: 5512)
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray)
            //var test = manager.GetFingerHashes(preliminaryFingerprints)
            //var result = manager.CompareFingerprintListsHighest(movie, toCompare: storedFingerprints)
            }catch (_) {
                
            }
            }
 
        }
        
    }