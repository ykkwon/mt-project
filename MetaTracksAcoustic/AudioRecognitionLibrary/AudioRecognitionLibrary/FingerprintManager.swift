//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public var WindowFunction:HanningWindow = HanningWindow()
public var Haar:HaarWavelet = HaarWavelet()
public var LogBins:Int = 32
public var Overlap:Int = 64
public var WindowSize:Int = 2048
public var MinFrequency:Int = 318
public var MaxFrequency:Int = 2000
public var TopWavelets:Int = 200
public var SampleRate:Int = 5512
public var LogBase:Double = M_E
public var FingerprintWidth = 128
public var Stride:Int = -(Overlap * FingerprintWidth) + 1024
private var lshTableSize:Int = 33
private var lshKey:Int = 3
private let Minrms:Float = 0.1
private let Maxrms:Float = 3.0
private var matchedFingerprints:[HashedFingerprint] = []
private var bestMatchedFingerprint:HashedFingerprint = HashedFingerprint(hashBins: [0], sequenceNumber: 0, sequenceAt: 0.0)
private var needToExpandSearch:Bool = false
public var searchFieldSize:Int = 15
public var LatestTimeStamp:Double = Double()
private var spacedLogFreq:[Int] = []
private var windowArray:[Double] = []
private var hasFingerprints = false

public class FingerprintManager {
    init(){
        lshTableSize = 33
        lshKey = 3
        WindowFunction = HanningWindow()
        Haar = HaarWavelet()
        LogBins = 32
        FingerprintWidth = 128
        Overlap = 64
        WindowSize = 2048
        MinFrequency = 318
        MaxFrequency = 2000
        TopWavelets = 200
        SampleRate = 5512
        LogBase = M_E
        Stride = -(Overlap * FingerprintWidth) + 1024
        if spacedLogFreq.isEmpty {
            spacedLogFreq = GetLogSpacedFrequencies(MinFrequency, maxFrequencies: MaxFrequency, fftSize: WindowSize)
            windowArray = WindowFunction.GetWindow(WindowSize)
        }
    }
    
    private func GetLogSpacedFrequencies(minFrequencies: Int, maxFrequencies: Int, fftSize: Int) -> [Int]{
        let logMin = log(Double(minFrequencies))
        let logMax = log(Double(maxFrequencies))
        let delta = (logMax - logMin) / Double(LogBins)
        var indexes:[Int] = [Int](count: LogBins+1, repeatedValue: 0)
        var accDelta:Double = 0
        
        for(var index0 = 0; index0 <= LogBins; index0 += 1){
            let freq = pow(LogBase, logMin+accDelta)
            accDelta += delta
            
            let chunk = freq/(Double(SampleRate/2))
            indexes[index0] = Int(round((Double(fftSize/2 + 1)*chunk)))
        }
        return indexes
    }
    
    public func CreateLogSpectrogram(samples: [Float]) -> [[Float]]{
        
        NormalizeInPlace(samples)
        let overlap = Overlap
        let windowSize = WindowSize
        var windowArray = WindowFunction.GetWindow(windowSize)
        let width = (samples.count - windowSize) / overlap
        
        var frames:[[Float]] = [[Float]](count:width, repeatedValue:[Float](count:0, repeatedValue:0.0))
        var fftSamples:[Float] = [Float](count:2*windowSize, repeatedValue:0.0)
        
        for(var widthIndex = 0; widthIndex < width; widthIndex += 1){
            for(var windowIndex = 0; windowIndex < windowSize; windowIndex += 1){
                fftSamples[2*windowIndex] = (Float(windowArray[windowIndex])*samples[widthIndex * overlap + windowIndex])
                fftSamples[2*windowIndex + 1] =  0
            }
            var res = Fourier.FFT(fftSamples, length: windowSize, direction: FourierDirection.Forward)
            frames[widthIndex] = ExtractLogBins(res)
        }
        
        return frames
    }
    
    public func NormalizeInPlace(samples: [Float]){
        var internalSamples:[Float] = samples
        let Minrms:Double = 0.1
        let Maxrms:Double = 3.0
        
        var squares:Double = 0
        let nsamples = internalSamples.count
        for(var i = 0; i < nsamples; i += 1){
            squares += Double(internalSamples[i] * internalSamples[i])
            
        }
        
        var rms = sqrt(squares/Double(nsamples))*10.0
        if rms < Minrms {
            rms = Minrms
        }
        if rms > Maxrms {
            rms = Maxrms
        }
        for(var i = 0; i < nsamples; i += 1){
            internalSamples[i] = internalSamples[i] / Float(rms)
            internalSamples[i] = min(internalSamples[i], 1)
            internalSamples[i] = max(internalSamples[i], -1)
            internalSamples = samples
        }
    }
    
    public func CreateFingerprints(samples: [Float]) -> [Fingerprint] {
        let spectrum = CreateLogSpectrogram(samples)
        return CreateFingerprints(spectrum)
    }
    
    public func CreateFingerprints(spectrogram: [[Float]]) -> [Fingerprint]{
        let fingerprintWidth = FingerprintWidth
        let overlap = Overlap
        let fingerprintHeight = LogBins
        var start = 0
        let sampleRate = SampleRate
        var sequenceNr = 0
        var fingerPrints:[Fingerprint] = []
        
        let length = spectrogram.count
        while (start + fingerprintWidth <= length) {
            var frames:[[Float]] = [[Float]](count:fingerprintWidth, repeatedValue:[Float](count:fingerprintHeight, repeatedValue:0.0))
            
            for(var index = 0; index < fingerprintWidth; index += 1){
                
                frames[index] = [Float](count: fingerprintHeight, repeatedValue: 0.0)
                frames[index] = (spectrogram[start+index])
            }
            
            
            var timestamp:Double = (Double(overlap)/Double(sampleRate))
            
            var temp = HaarWavelet.Transform(frames)
            let image = ExtractTopWavelets(temp)
            var fingerp:Fingerprint = Fingerprint(signature: image, sequenceNo: sequenceNr, timestamp: Double(start)*timestamp)
            fingerPrints.append(fingerp)
            start += fingerprintWidth + (Stride/overlap)
            sequenceNr++
        }
        return fingerPrints
    }
    
    
    public func ExtractLogBins(spectrum: [Float]) -> [Float]{
        let logBins:Int = LogBins
        var totalFreq:[Float] = [Float](count: logBins, repeatedValue: 0.0)
        
        for(var index = 0; index < logBins; index += 1){
            let low = spacedLogFreq[index]
            let high = spacedLogFreq[index+1]
            
            for(var index2 = low; index2 < high; index2 += 1){
                let re = spectrum[2*index2];
                let img = spectrum[(2*index2)+1];
                totalFreq[index] += Float(sqrt(re*re + img*img))
            }
            totalFreq[index] = (totalFreq[index] / Float((high-low)))
        }
        return totalFreq
    }
    
    public func ExtractTopWavelets(frames: [[Float]]) -> [Bool]{
        var width = 128
        var height = 32
        var concatenated:[Float] = []
        var concatenated2:[Float] = []
        
        for (var row = 0; row < width; row += 1)
        {
            for(var col = 0; col < height; col += 1){
                concatenated.append(frames[row][col])
                concatenated2.append(frames[row][col])
            }
        }
        
        concatenated.sortInPlace()
        var reverseConcatenated:[Float] = []
        for(var i = concatenated.count-1; i >= 0; i--){
            reverseConcatenated.append(concatenated[i])
        }
        var indexes = SortIndexes(concatenated, oldconcat: concatenated2)
        
        var result:[Bool] = [Bool](count: concatenated.count*2, repeatedValue: false)
        
        for (var i = 0; i < TopWavelets; i += 1)
        {
            var index = indexes[i]
            var value = reverseConcatenated[i]
            if (value > 0){
                result[index*2] = true;
            }
            else if (value < 0){
                result[index*2 + 1] = true;
            }
        }
        return result;
    }
    
    private func SortIndexes(concat: [Float], oldconcat: [Float]) -> [Int]{
        var testArray:[Int] = [Int](count: concat.count, repeatedValue: 0)
        for(var index = 0; index < concat.count; index++){
            var newf = concat[index]
            for(var index2 = 0; index2 < oldconcat.count; index2++){
                var oldf = oldconcat[index2]
                if(newf == oldf)
                {
                    testArray[index] = index2
                }
            }
        }
        return testArray
    }
    
    public func GetFingerHashes(listdb: [Fingerprint]) -> [HashedFingerprint]{
        var listDb = listdb
        var minHash:MinHash = MinHash()
        var minhashdb:[[UInt8]] = [[]]
        
        for(var i = 0; i < listDb.count; i++){
            var fing = listDb[i]
            var fp = minHash.ComputeMinHashSignatureByte(fing.Signature)
            minhashdb.append(fp)
        }
        
        var lshBuckets:[[Int64]] = [[]]
        minhashdb.removeFirst()
        for(var i = 0; i < minhashdb.count; i++){
            var fing = minhashdb[i]
            var fp = minHash.GroupMinHashToLshBucketsByte(fing, numberOfHashTables: lshTableSize, numberOfMinHashesPerKey: lshKey)
            var t = fp.1
            lshBuckets.append(t)
        }
        
        lshBuckets.removeFirst()
        var hashedFinger:[HashedFingerprint] = []
        for(var index = 0; index < listdb.count; index++){
            var hashfinger:HashedFingerprint = HashedFingerprint(hashBins: lshBuckets[index], sequenceNumber: listDb[index].SequenceNumber, sequenceAt: listDb[index].Timestamp)
            hashedFinger.append(hashfinger)
        }
        return hashedFinger
    }
    
    public func GenerateHashedFingerprints(receivedHashes: [String], receivedTimestamps: [String]) -> [HashedFingerprint]{
        
        var hashBins:[Int64] = []
        var timestamps:[Double] = []
        
        for(var index = 0; index < receivedHashes.count - 1; index++ ){
            var currentHash = Int64(receivedHashes[index])
            var a = receivedTimestamps[index].stringByReplacingOccurrencesOfString(",", withString: ".")
            var currentTimestamp = Double(a)
            hashBins.append(currentHash!)
            timestamps.append(currentTimestamp!)
        }
        
        var hashBinsList:[[Int64]] = [[]]
        var timestampsList:[Double] = []
        for(var j = 0; j < timestamps.count - 1; j++){
            if((j % lshTableSize == 0) && (hashBins.count > j + lshTableSize)){
                var bins:[Int64] = [Int64](count: lshTableSize, repeatedValue: 0)
                for(var i = 0; i < lshTableSize; i++){
                    bins[i] = hashBins[i + j]
                }
                hashBinsList.append(bins)
                timestampsList.append(timestamps[j])
            }
            
        }
        var list:[HashedFingerprint] = []
        for(var i = 0; i < timestampsList.count; i++){
            var t = hashBinsList[i]
            var hash:HashedFingerprint = HashedFingerprint(hashBins: t, timestamp: timestampsList[i])
            list.append(hash)
        }
        list.removeFirst()
        return list
    }
    
    public func CompareFingerprintListsHighest(fingerprints: [HashedFingerprint], toCompare: [HashedFingerprint]) -> Int {
        var offsetCounter = Int()
        var fingerprintList = toCompare
        var toCompareList = fingerprints
        var commonCounter = 0
        var highestCommon = 0
        
        for(var i = 0; i < fingerprintList.count; i++){
            var list = fingerprintList[i]
            
            for(var j = 0; j < toCompareList.count; j++){
                var fingerprint2 = toCompareList[j]
                var set2 = fingerprint2.HashBins
                var count = 0
                
                for(var l = 0; l < list.HashBins.count; l++){
                    var hash = list.HashBins[l]
                    var searchIndex = binarySearch(set2, searchItem: hash);
                    
                    if (searchIndex != -1) {
                        count++
                        }
                    }
                    if (count >= 3) {
                    LatestTimeStamp = list.Timestamp
                    break
                    }
                }
        }
        
        print("LatestTimeStamp: \(LatestTimeStamp)")
        return matchedFingerprints.count
    }
    
    
    
    
    public func binarySearch<T:Comparable>(inputArr:Array<T>, searchItem: T)->Int{
        var lowerIndex = 0;
        var upperIndex = inputArr.count - 1
        
        while (true) {
            var currentIndex = (lowerIndex + upperIndex)/2
            if(inputArr[currentIndex] == searchItem) {
                return currentIndex
            } else if (lowerIndex > upperIndex) {
                return -1
            } else {
                if (inputArr[currentIndex] > searchItem) {
                    upperIndex = currentIndex - 1
                } else {
                    lowerIndex = currentIndex + 1
                }
            }
        }
    }
}