//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation
import Surge


public var lshTableSize:Int = 33
public var lshKey:Int = 3
public var spacedLogFreq:[Int] = []
public var windowArray:[Double] = []
public  var WindowFunction:HanningWindow = HanningWindow()
//var HaarWavelet:HaarWavelet
public var LogBins:Int = 32
public var Overlap:Int = 128
public var WindowSize:Int = 2048
public var MinFrequency:Int = 318
public var MaxFrequency:Int = 2000
public var TopWavelets:Int = 200
public var SampleRate:Int = 5512
public var LogBase:Double = M_E

public class FingerprintManager {
    init(){}
    public var FingerprintWidth:Int = 128
    //public var Stride:Int = (Overlap * FingerprintWidth) + 1024
    
    public static func CreateSpectrogram(filename: NSURL, milliseconds: Int, startMilliseconds: Int) -> [Float]{
        do{
        var samples = try BassProxy.GetSamplesMono(filename, sampleRate: 5512)
        var t = fft(samples)
        return t
        } catch (_) {
    return [0.0]
    }
}
    public static func ExtractLogBins(spectrum: [Float]) -> [Float]{
        for(var i = 0; i <= LogBins + 1; i++){
            spacedLogFreq.append(0)
        }
        var logBins:Int = LogBins
        var totalFreq:[Float] = []
        for(var i = 0; i < logBins; i++){
            totalFreq.append(0.0)
        }
        for(var j = 0; j <= logBins; j++){
            var low = spacedLogFreq[j]
            var high = spacedLogFreq[j + 1]
            
            for(var k = low; k < high; k++){
                var re:Float = spectrum[2 * k]
                var img:Float = spectrum[2 * k + 1]
                totalFreq[j] += sqrt(re*re + img*img)
            }
            totalFreq[j] = totalFreq[j]/Float((high-low))
        }
        return totalFreq
    }
    
    public static func NormalizeInPlace(samples: [Float]){
        var internalSamples:[Float] = samples
        var Minrms:Double = 0.1
        var Maxrms:Double = 3.0
        
        var squares:Double = 0
        var nsamples = internalSamples.count
        for(var i = 0; i < nsamples; i++){
            squares += Double(internalSamples[i] * internalSamples[i])
            
        }
        
        var rms = sqrt(squares/Double(nsamples))*10.0
        if rms < Minrms {
            rms = Minrms
        }
        if rms > Maxrms {
            rms = Maxrms
        }
        for(var i = 0; i < nsamples; i++){
            internalSamples[i] = internalSamples[i] / Float(rms)
            internalSamples[i] = min(internalSamples[i], 1)
            internalSamples[i] = max(internalSamples[i], -1)
            internalSamples = samples
        }
    }
}