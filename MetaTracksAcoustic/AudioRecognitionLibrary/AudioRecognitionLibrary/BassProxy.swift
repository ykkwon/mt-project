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
        let totalMilliseconds = Int.max
        var samples:[Float] = []
        let newPath = filename.relativePath!
        var _: Array<Float>
        let flags = UInt32(BASS_STREAM_DECODE | BASS_SAMPLE_MONO | BASS_SAMPLE_FLOAT)
        let bassStream:HSTREAM = BASS_StreamCreateFile(false, String(newPath), 0, 0, flags)
        if bassStream != 0 {
            print(BASS_ErrorGetCode())
        }
        let mixerStream = BASS_Mixer_StreamCreate(5512, 1, flags)
        
        if BASS_Mixer_StreamAddChannel(mixerStream, bassStream, flags) {
            let bufferSizeInt = sampleRate * 20 * 4
            let bufferSize:UInt32 = UInt32(sampleRate * 20 * 4)
            var buffer:[Float] = []
            for (var i = 0; i < bufferSizeInt; i += 1){
                buffer.append(0.0)
            }
            var chunks = Array([Float]())
            var size:Float = Float(0)
            let sampleRateFloat:Float = Float(sampleRate)
            let totalMillisecondsFloat:Float = Float(totalMilliseconds)
            
            while ((size / sampleRateFloat * 1000) < totalMillisecondsFloat) {
                let bytesToRead = BASS_ChannelGetData(mixerStream, UnsafeMutablePointer<Float>(buffer), bufferSize)
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
                    let newBytes:Float = Float(bytesToRead)
                    size += newBytes / 4
                }

                for var i = 0; i < Int(size); i += 1 {
                    let chunk = chunks[i]
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