//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation
import Surge

public var FingerprintWidth:Int = 128
public var lshTableSize:Int = 33
public var lshKey:Int = 3
public var spacedLogFreq:[Int] = []
public var windowArray:[Double] = []
public  var WindowFunction:HanningWindow = HanningWindow()
public var Stride:Int = -(Overlap * FingerprintWidth) + 1024
public var LogBins:Int = 32
public var Overlap:Int = 128
public var WindowSize:Int = 2048
public var MinFrequency:Int = 318
public var MaxFrequency:Int = 2000
public var TopWavelets:Int = 200
public var SampleRate:Int = 5512
public var LogBase:Double = M_E
public var matchedFingerprints:[HashedFingerprint] = []
public var needToExpandSearch:Bool = false
public var searchFieldSize:Int = 15
public var LatestTimeStamp:Double = 0

public class FingerprintManager {
    init(){}
    
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

    public static func CreateLogSpectrogram(samples: [Float]) -> [[Float]]{
        NormalizeInPlace(samples)
        var overlap = Overlap
        var windowSize = WindowSize
        var width = (samples.count - windowSize) / overlap

        var frames:[[Float]] = [[Float]]() //creates an empty matrix
        var framesRow = [Float]() //fill this row
        frames.append(framesRow) //add this row
        var fftSamples:[Float] = [Float]()
        var fftRow = [Float]()
        frames.append(fftRow*2)
        for(var widthIndex = 0; widthIndex < width; widthIndex++){
            for(var windowIndex = 0; windowIndex < windowSize; windowIndex++){
                var o = fftSamples[2*windowIndex]
                var ot = samples[widthIndex * overlap + windowIndex]
                o = Float(windowArray[windowIndex]) * ot
                fftSamples[2*windowIndex + 1] = 0
            }
            fft(fftSamples) // TODO: Possibly incomplete
            frames[widthIndex] = ExtractLogBins(fftSamples)
        }
        return frames
    }
    
    public static func CreateFingerprints(spectrogram: [[Float]]) -> [Fingerprint]{
        // TODO: Probably broken
        var fingerprintWidth = FingerprintWidth
        var overlap = Overlap
        var fingerprintHeight = LogBins
        var start = 0
        var sampleRate = SampleRate
        var sequenceNr = 0
        var fingerPrints:[Fingerprint] = []
        var length = spectrogram.count
        var frames = spectrogram
        HaarWavelet.Transform(frames)
        var image = ExtractTopWavelets(frames)
        var fp:Fingerprint = Fingerprint(sequenceNumber: sequenceNr++, signature: image, timeStamp: Double(start)*Double(overlap/sampleRate))
        fingerPrints.append(fp)
        start += fingerprintWidth + (Stride/overlap)
        return fingerPrints
        
    
    }
    
    public static func ExtractTopWavelets(frames: [[Float]]) -> Bool{
        // TODO: Broken afaik
        var topWavelets = TopWavelets
        var width = frames.count // 128
        var height = frames.count // TODO: Should be 32, is not. Gotta figure out multi-dimensional arrays.
        var result:[Bool] = []
        for(var i = 0; i < topWavelets; i++){
            var value = frames[i]
            //if(value > 0){
                
            }
            //else if(value < 0){
                
        
        return true
    }
    
    public static func funcEncodeFingerprint(concatenated: [Float], indexes: [Int], topWavelets: Int) -> [Bool]{
        var results:[Bool] = []
        for(var i = 0; i < topWavelets; i++){
            var index = indexes[i]
            var value:Float = concatenated[i]
            if value > 0 {
                results[index*2] = true // TODO: Possible indexoutofbounds
            }
            else if value < 0{
                results[index*2 + 1] = true // TODO: Possible indexoutofbounds
            }
            
        }
        return results
    }
    
    public static func GetFingerHashes(listdb: [Fingerprint]) -> [HashedFingerprint]{
        var minHash:MinHash = MinHash()
        var minhashdb:[BYTE] = [] // TODO: rewrite
        var lshbuckets:[CLong] = []
        
        var hashedFinger:[HashedFingerprint] = []
        for(var index = 0; index < listdb.count; index++){ // TODO fix this
            //var hashfinger = HashedFingerprint(hashBins: lshbuckets[index].HashBins, sequenceNumber: listdb[index].SequenceNumber, sequenceAt: listdb[index].Timestamp)
            //hashedFinger.append(hashfinger)
        }
        return hashedFinger
    }
    
    
    public static func CompareFingerprintListsHighest(fingerprints: [HashedFingerprint], toCompare: [HashedFingerprint]) -> Double {
        var fingerprintList = fingerprints
        var toCompareList = toCompare
        
        var commonCounter = 0
        var highestCommon = 0
        
        //if(bestMatchedFingerprint.Timestamp != 0.0){
        var foundAnyFingerprints:Bool = false
        var timestamps:[Double] = []
        for timestamp in fingerprints {
            timestamps.append(timestamp.Timestamp)
        }
        var lastTime = LatestTimeStamp
        var matchingIndexes:[Int] = []
        if needToExpandSearch {
            searchFieldSize *= 4
        }
        var seconds = searchFieldSize
        var plusTime:Double = 0
        var minusTime:Double = 0
        // TODO var plusTime = min(timestamps)
        // TODO var minusTime = min(0. . .)
        for(var i = 0; i < timestamps.count; i++){
            if timestamps[i] < plusTime && timestamps[i] >= minusTime {
                matchingIndexes.append(i)
            }
            var currentList:[HashedFingerprint] = []
            for(var i = 0; i < matchingIndexes.count; i++){
                currentList[i] = fingerprints[matchingIndexes[i]]
            }
            for list in currentList {
                // TODO
                }
            }
        return 0.0
        }
}