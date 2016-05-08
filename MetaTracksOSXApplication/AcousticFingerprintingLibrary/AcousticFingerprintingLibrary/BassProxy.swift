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
    }
    public static func GetSamplesMono(filename: NSURL!, sampleRate: Int) throws -> Array<Float>{
        Initialize()
        let totalMilliseconds:Int = 2147483647
        var samples:[Float] = []
        let newPath = filename.relativePath!
        let flags = UInt32(BASS_STREAM_DECODE | BASS_SAMPLE_MONO | BASS_SAMPLE_FLOAT)
        let bassStream:HSTREAM = BASS_StreamCreateFile(false, String(newPath), 0, 0, flags)
        if (bassStream == 0){
            print(BASS_ErrorGetCode())
        }
        let mixerStream = BASS_Mixer_StreamCreate(5512, 1, flags)
        if BASS_Mixer_StreamAddChannel(mixerStream, bassStream, flags) {
            let bufferSizeInt = sampleRate * 20 * 4
            var buffer:[Float] = [Float](count: bufferSizeInt, repeatedValue: 0)
            var chunks:[[Float]] = [[]]
            var size:Float = 0
            
            while ((Float(size) / Float(sampleRate) * 1000) < Float(totalMilliseconds)) {
                let bytesToRead = BASS_ChannelGetData(mixerStream, UnsafeMutablePointer<Float>(buffer), UInt32(bufferSizeInt))
                if bytesToRead == 0 {
                    break
                }
                var chunk:[Float] = []
                
                for(var i = 0; i <= Int(bytesToRead)/4; i++){
                    chunk.append(buffer[i])
                }
                chunks.append(chunk)
                size += (Float(bytesToRead) / 4)
            }
            
            
            chunks.removeFirst()
            for(var i = 0; i < chunks.count; i++){
                var chunk = chunks[i]
                for(var j = 0; j < chunk.count; j++){
                    samples.append(chunk[j])
                }
            }
            BASS_StreamFree(mixerStream)
            BASS_StreamFree(bassStream)
            
            return samples
        }
        print(BASS_ErrorGetCode())
        print("An error occured.")
        return [0.0]
    }
}