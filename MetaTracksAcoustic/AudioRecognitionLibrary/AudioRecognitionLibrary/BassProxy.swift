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
    public static func GetSamplesMono(filename: NSURL!, sampleRate: Int) throws -> Array<Float>{
        var totalMilliseconds = Int.max
        var samples:[Float] = []
        var newPath = filename.relativePath!
        var data: Array<Float>
        var flags = UInt32(BASS_STREAM_DECODE | BASS_SAMPLE_MONO | BASS_SAMPLE_FLOAT)
        var bassStream:HSTREAM = BASS_StreamCreateFile(false, String(newPath), 0, 0, flags)
        if bassStream != 0 {
            print(BASS_ErrorGetCode())
        }
        var mixerStream = BASS_Mixer_StreamCreate(5512, 1, flags)
        
        if BASS_Mixer_StreamAddChannel(mixerStream, bassStream, flags) {
            var bufferSizeInt = sampleRate * 20 * 4
            var bufferSize:UInt32 = UInt32(sampleRate * 20 * 4)
            var buffer:[Float] = []
            for (var i = 0; i < bufferSizeInt; i++){
                buffer.append(0.0)
            }
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
                    var chunk = [Float](count: Int(bytesToRead)/4, repeatedValue: 0.0)
                    chunk = buffer
                    chunks.appendContentsOf(chunk)
                    var newBytes:Float = Float(bytesToRead)
                    size += newBytes / 4
                }

                for var i = 0; i < Int(size); i++ {
                    var chunk = chunks[i]
                    samples.append(chunk)
                }
            }
            BASS_StreamFree(mixerStream)
            BASS_StreamFree(bassStream)
            return samples
        }
        print("An error occured.")
        return [0.0]
    }
}