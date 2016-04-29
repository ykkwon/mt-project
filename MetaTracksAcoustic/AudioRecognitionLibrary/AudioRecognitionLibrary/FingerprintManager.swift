//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation
import Surge

public var FingerprintWidth:Int = 128
public var lshTableSize:Int = 33
public var lshKey:Int = 3
public var WindowFunction:HanningWindow = HanningWindow()
public var Stride:Int = -(Overlap * FingerprintWidth) + 1024
public var LogBins:Int = 32
public var Overlap:Int = 64
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
    
    public static func GetLogSpacedFrequencies(minFrequencies: Int, maxFrequencies: Int, fftSize: Int) -> [Int]{
        var logMin = log(Double(minFrequencies))
        var logMax = log(Double(maxFrequencies))
        var delta = (logMax - logMin) / Double(LogBins)
        var indexes:[Int] = [Int](count: LogBins+1, repeatedValue: 0)
        var accDelta:Double = 0
        
        for(var index0 = 0; index0 <= LogBins; index0++){
            var freq = pow(LogBase, logMin+accDelta)
            accDelta += delta
            
            var chunk = freq/(Double(SampleRate/2))
            indexes[index0] = Int(round((Double(fftSize/2 + 1)*chunk)))
        }
        return indexes
    }
    
    public static func ExtractLogBins(spectrum: [Float]) -> [Float]{
        var spacedLogFreq = GetLogSpacedFrequencies(MinFrequency, maxFrequencies: MaxFrequency, fftSize: WindowSize)
        var logBins:Int = LogBins
        var totalFreq:[Float] = [Float](count: logBins, repeatedValue: 0.0)
        
        for(var index = 0; index < logBins; index++){
            var low = spacedLogFreq[index]
            var high = spacedLogFreq[index+1]
            
            for(var index2 = low; index2 < high; index2++){
                var re = spectrum[2*index2];
                var img = spectrum[(2*index2)+1];
                totalFreq[index] += Float(sqrt(re*re + img*img))
            }
            totalFreq[index] = (totalFreq[index] / Float((high-low)))
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
    public static func CreateFingerprints(monoArray: [Float]) -> [Fingerprint] {
        var spectrum = CreateLogSpectrogram(monoArray)
        return CreateFingerprints(spectrum)
    }
    
    public static func CreateLogSpectrogram(samples: [Float]) -> [[Float]]{
        
        NormalizeInPlace(samples)
        var overlap = Overlap
        var windowSize = WindowSize
        var windowArray = WindowFunction.GetWindow(windowSize)
        var width = (samples.count - windowSize) / overlap

        var frames:[[Float]] = [[Float]](count:width, repeatedValue:[Float](count:0, repeatedValue:0.0))
        var fftSamples:[Float] = [Float](count:2*windowSize, repeatedValue:0.0)
        
        for(var widthIndex = 0; widthIndex < width; widthIndex++){
            
            for(var windowIndex = 0; windowIndex < windowSize; windowIndex++){
                fftSamples[2*windowIndex] = (Float(windowArray[windowIndex])*samples[widthIndex * overlap + windowIndex])
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
        while start + fingerprintWidth <= length {
            var frames:[[Float]] = [[Float]](count:fingerprintHeight, repeatedValue:[Float](count:0, repeatedValue:0.0))
            for(var index = 0; index < fingerprintWidth; index++){
                frames[index] = [Float](count: fingerprintWidth, repeatedValue: 0.0)
                frames.append(spectrogram[start+index])
            }
                HaarWavelet.Transform(frames)
                var image = ExtractTopWavelets(frames)
                fingerPrints.append(Fingerprint(sequenceNumber: sequenceNr++, signature: image, timeStamp: Double(start)*Double(overlap/sampleRate)))
              start += fingerprintWidth + (Stride/overlap)
                
            }
      
    
        return fingerPrints
    }
    
    public static func ExtractTopWavelets(frames: [[Float]]) -> [Bool]{
        var topWavelets = TopWavelets
        var width = 128
        var height = 32 // TODO: Should be 32, is not. Gotta go figure out multi-dimensional arrays.
        var concatenated:[Float] = [Float](count: width*height, repeatedValue: 0.0)
        
        
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                concatenated.append(frames[i][j])
            }
            
        
        }

        var result:[Bool] = [Bool](count: concatenated.count*2, repeatedValue: true)
            for (var i = 0; i < topWavelets; i++)
            {
                var value = concatenated[i];
                if (value > 0){ /*positive wavelet*/
                result[i*2] = true;
                }
                else if (value < 0){ /*negative wavelet*/
                result[i*2 + 1] = true;
                }
            }
            return result;
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
        var listDb = listdb
        var minHash:MinHash = MinHash()
        var minhashdb:[BYTE] = []
        
        for fingerprint in listDb {
          //  var fing:Fingerprint = Fingerprint()
        }

        var lshbuckets:[u_long] = []
        
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
        var plusTime = min(Int(timestamps.last!), Int(lastTime) + seconds)
        var minusTime = max(0, Int(lastTime) - seconds)
        for(var i = 0; i < timestamps.count; i++){
            if timestamps[i] < Double(plusTime) && timestamps[i] >= Double(minusTime) {
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