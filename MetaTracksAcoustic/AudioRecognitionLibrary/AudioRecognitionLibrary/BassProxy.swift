//
//  _dummy.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 22/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//
import Foundation

public class BassProxy{
    public var defaultSampleRate = 44100
    public let alreadyDisposed = true
    
    public static func Initialize(){
        BASS_Init(-1, 44100, 0, nil, nil)
        print("BASS initialized.")
    }
    public static func GetSamplesMono(filename: String, sampleRate: Int) -> Array<Float>{
        var samples:[Float] = []
        var totalMilliseconds = Int.max
        var data: Array<Float>
        var flags = UInt32(BASS_STREAM_DECODE | BASS_SAMPLE_MONO | BASS_SAMPLE_FLOAT)
        var bassStream = BASS_StreamCreateFile(true, filename, 0, 0, flags)
        var mixerStream = BASS_Mixer_StreamCreate(44100, 1, flags)
        
        if BASS_Mixer_StreamAddChannel(mixerStream, bassStream, flags) {
            var bufferSizeInt = sampleRate * 20 * 4
            var bufferSize:UInt32 = UInt32(sampleRate * 20 * 4)
            var buffer = [Float]()
            var chunks = Array([Float]())
            var size:Float = Float(0)
            var sampleRateFloat:Float = Float(sampleRate)
            var totalMillisecondsFloat:Float = Float(totalMilliseconds)
            
            while ((size / sampleRateFloat * 1000) < totalMillisecondsFloat) {
                var bytesToRead = BASS_ChannelGetData(mixerStream, UnsafeMutablePointer<Float>(buffer), bufferSize)
                if bytesToRead == 0 {
                    print("Bytes to read: ")
                    print(bytesToRead)
                    break
                }
                else{
                    var chunkInt:Int = Int(bytesToRead)
                    var chunk = [Float]()
                    buffer = chunk
                    chunks.appendContentsOf(chunk)
                    var newBytes:Float = Float(bytesToRead)
                    size += newBytes / 4
                }
                var cursor = 0
                for var i = 0; i < chunks.count; i++ {
                    var chunk = chunks[i]
                    // cursor += chunk.Length
                }
                samples = chunks
            }
            BASS_StreamFree(mixerStream)
            BASS_StreamFree(bassStream)
            return samples
        }
        print("nil")
        return [0.0]
    }
    public static func Dispose(){
        // TODO: Trenge vi dinne?
    }
    
}