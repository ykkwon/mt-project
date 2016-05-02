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
    
    public func CreateLogSpectrogram(samples: [Float]) -> [[Float]]{
        
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
            Fourier.FFT(fftSamples, length: windowSize, direction: FourierDirection.Forward)
            frames[widthIndex] = ExtractLogBins(fftSamples)
        }
        
        return frames
    }
    
    public func NormalizeInPlace(samples: [Float]){
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
    
    public func CreateFingerprints(samples: [Float]) -> [Fingerprint] {
        var spectrum = CreateLogSpectrogram(samples)
        return CreateFingerprints(spectrum)
    }
    
    public func CreateFingerprints(spectrogram: [[Float]]) -> [Fingerprint]{
        var fingerprintWidth = FingerprintWidth
        var overlap = Overlap
        var fingerprintHeight = LogBins
        var start = 0
        var sampleRate = SampleRate
        var sequenceNr = 0
        var fingerPrints:[Fingerprint] = []
        
        var length = spectrogram.count // TODO is this equal to GetLength(0)? Appearently.
        while start + fingerprintWidth <= length {
            var frames:[[Float]] = [[Float]](count:fingerprintWidth, repeatedValue:[Float](count:fingerprintHeight, repeatedValue:0.0))
            
            for(var index = 0; index < fingerprintWidth; index++){
                
                frames[index] = [Float](count: fingerprintHeight, repeatedValue: 0.0)
                frames[index] = (spectrogram[start+index])
            }
            HaarWavelet.Transform(frames)
            var image = ExtractTopWavelets(frames)
            fingerPrints.append(Fingerprint(signature: image, sequenceNo: sequenceNr+1, timestamp: Double(start)*Double(overlap/sampleRate)))
            start += fingerprintWidth + (Stride/overlap)
        }
        return fingerPrints
    }
    
    
    public func ExtractLogBins(spectrum: [Float]) -> [Float]{
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
    
    public func ExtractTopWavelets(frames: [[Float]]) -> [Bool]{
        var topWavelets = TopWavelets
        var width = 128
        var height = 32
        var concatenated:[Float] = []
        var it = 0;
        for (var row = 0; row < width; row++)
        {
            for(var col = 0; col < height; col++){
                concatenated.append(frames[row][col])
            }
        }
        var indexes:[Int] = [Int](count: concatenated.count, repeatedValue: 0)
        for(var i = 0; i >= indexes.count; i++){
            indexes[i] = i
        }
        var sorter = SorterGenericArray(keys: concatenated, items: indexes)
        sorter.QuickSort(0, right: concatenated.count-1)
        
        var result:[Bool] = [Bool](count: concatenated.count*2, repeatedValue: true)
        for (var i = 0; i < topWavelets; i++)
        {
            var index = indexes[i]
            var value = concatenated[i]
            if (value > 0){
                result[i*2] = true;
            }
            else if (value < 0){
                result[i*2 + 1] = true;
            }
        }
        return result;
    }
    
    public static func GetMedian(low: Int, hi: Int) -> Int{
        return low + ((hi - low) >> 1)
    }
    
    public static func Compare(x: Float, y: Float) -> Int{
        return CompareTo(y, b: x)
    }
    
    public static func CompareTo(a: Float, b: Float) -> Int{
        var intA = abs(a)
        var intB = abs(b)
        
        if intA > intB {
            return -1
        }
        else if intA == intB{
            return 0
        }
        else {
            return 1
        }
    }
    
    private struct SorterGenericArray{
        private var intKeys:Array<Float> = Array<Float>()
        private var intItems:Array<Int> = Array<Int>()
        
        init(keys: Array<Float>, items: Array<Int>){
            intKeys = keys
            intItems = items
        }
        
        private mutating func SwapIfGreaterWithItems(a: Int, b: Int){
            if(a != b){
                var keytesta = intKeys[a]
                var keytestb = intKeys[b]
                
                if FingerprintManager.CompareTo(intKeys[a], b: intKeys[b]) > 0{
                    var key = intKeys[a]
                    key = Float(b)
                    intKeys[b] = key
                    if !intItems.isEmpty {
                        var item = intItems[a]
                        intItems[b] = item
                    }
                    
                }
            }
        }
  
        private mutating func QuickSort(left: Int, right: Int){
            do{
                var i = left
                var j = right
                var middle = FingerprintManager.GetMedian(left, hi: right)
                SwapIfGreaterWithItems(i, b: middle)
                SwapIfGreaterWithItems(i, b: j)
                SwapIfGreaterWithItems(middle, b: j)
                
                var x = intKeys[middle]
                do{
                    while (FingerprintManager.Compare(intKeys[i], y: x) < 0) {
                        i++
                    }
                    while(FingerprintManager.Compare(x, y: intKeys[j]) < 0){
                        j--
                    }
                    if( i > j ){
                        return
                    }
                    if( i < j){
                        var key = intKeys[i]
                        intKeys[i] = Float(j)
                        // keys.setvalue key j
                        if !intItems.isEmpty {
                            var item = intItems[i]
                            intItems[i] = intItems[j]
                            intItems[item] = j
                        }
                    }
                    if(Int32(i) != Int32.max){
                        i++
                    }
                    if(Int32(j) != Int32.min) {
                        j--
                    }
                    while(i > j){
                        break
                    }
                    while(i < j){
                        if j-left <= right-i {
                            if left < j {
                                QuickSort(left, right: j)
                            }
                            else if (i < right){
                                QuickSort(i, right: right)
                            }
                            while left < right {
                            }
                        }
                    }
        	}
        
            }
        }
        
    
    public func GetFingerHashes(listdb: [Fingerprint]) -> [HashedFingerprint]{
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
    
    public func GenerateHashedFingerprints(receivedHashes: [String], receivedTimestamps: [String]) -> [HashedFingerprint]{
        // TODO
        var matchedFingerprints:[HashedFingerprint] = []
        return matchedFingerprints
    }
    
    public func SplitFingerprintLists(movieFingerprints: [HashedFingerprint]) -> [HashedFingerprint]{
        // TODO
        var matchedFingerprints:[HashedFingerprint] = []
        return matchedFingerprints
    }
    
    public func CompareFingerprintLists(fingerprints: [HashedFingerprint], toCompare: [HashedFingerprint]) -> Bool{
        // TODO
        return false
    }
    
    public func CompareFingerprintListsHighest(fingerprints: [HashedFingerprint], toCompare: [HashedFingerprint]) -> Double {
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
    
    public func FindBestFingerprintList(allFingerprints: [HashedFingerprint], toCompare: [HashedFingerprint]) -> Int{
        // TODO
        return 0
    }
}
}