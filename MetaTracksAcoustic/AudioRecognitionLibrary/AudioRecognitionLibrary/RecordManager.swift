import UIKit
import AVFoundation

public class RecordManager {
    
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
        var iterator = 0
        while(true){
            var t = setRecorder(iterator)
            audioRecorder?.record()
            sleep(3)
            audioRecorder?.stop()
            iterator++
            var u = FingerprintManager.CreateSpectrogram(t, milliseconds: 0, startMilliseconds: 0)
            var v = FingerprintManager.ExtractLogBins(u)
            var w = FingerprintManager.NormalizeInPlace(v)
            print("Everything is good.")
            }
        }
        
    }