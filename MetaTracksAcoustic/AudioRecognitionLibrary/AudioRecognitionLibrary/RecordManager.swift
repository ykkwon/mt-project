//
//  RecordManager.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 25/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Foundation
import AudioToolbox
import AVFoundation

public var Recorder:AVAudioRecorder = AVAudioRecorder()
public var Player:AVAudioPlayer = AVAudioPlayer()
public var AudioFilePath:NSURL = NSURL()
public var Observer:NSObject = NSObject()
// TODO: var FingerprintManager:FingerprintManager = FingerprintManager
public var ReceivedHashes:[String] = []
public var ReceivedTimestamps: [String] = []
// TODO: var StoredFingerprints:[HashedFingerprint] = []
// TODO: var movie:[HashedFingerprint] = []
// TODO: var hashedFingerprints:[[HashedFingerprint]] = [[]]
public var tempRecording: String = String()
public var secondaryIndex: Int = Int()
public var currentWaveFile:NSURL = NSURL()
public var tempDirURL:NSURL = NSURL()

public class RecordManager{
   
    public static func Record(){
        Recorder.record()
        sleep(3000)
        Recorder.stop()
        currentWaveFile = AudioFilePath
        print("CURRENT WAVE FILE:")
        print(currentWaveFile)
    }
    
    public static func PrepareAudioRecording(nameIterator: Int){
        do {
        AudioFilePath = CreateOutputURL(nameIterator)
        let session = AVAudioSession.sharedInstance()
        try session.setCategory(AVAudioSessionCategoryRecord)
        try session.overrideOutputAudioPort(AVAudioSessionPortOverride.Speaker)
        try session.setActive(true)
        
        var recordSettings = [String : AnyObject]()
        recordSettings[AVFormatIDKey] = Int(kAudioFormatLinearPCM)
        recordSettings[AVSampleRateKey] = 5512
        recordSettings[AVNumberOfChannelsKey] = 1
        
        Recorder = try AVAudioRecorder(URL: AudioFilePath, settings: recordSettings)
        Recorder.meteringEnabled = true
        Recorder.prepareToRecord()
     
    } catch (_) {
    }
        
    }
    
        public static func finishRecording(success success: Bool) {
        Recorder.stop()
    }
    

    public static func CreateOutputURL(nameIterator: Int) -> NSURL{
        var fileName = "split" + String(nameIterator) + ".wav"
        tempRecording = NSTemporaryDirectory() + fileName
        return NSURL(fileURLWithPath: tempRecording)
    }
    
    public static func OnDidPlayToEndTime(sender: AnyObject, e: NSNotification){
        // TODO: Player.Dispose()
        // TODO: Player = null
    }
    
    
    public static func InitializeComponents()  {
        var audioSession:AVAudioSession = AVAudioSession.sharedInstance()
    }
    
    
}