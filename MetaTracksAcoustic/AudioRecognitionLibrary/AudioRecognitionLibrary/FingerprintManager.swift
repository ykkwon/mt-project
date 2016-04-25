//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation
import Surge




public class FingerprintManager {
    init(){}
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
    public var FingerprintWidth:Int = 128
    //public var Stride:Int = (Overlap * FingerprintWidth) + 1024
    
    public static func CreateSpectrogram(filename: String, milliseconds: Int, startMilliseconds: Int) -> [Float]{
        var samples = BassProxy.GetSamplesMono(filename, sampleRate: 5512)
        var t = fft(samples)
        print(t)
        return t
    }
}
