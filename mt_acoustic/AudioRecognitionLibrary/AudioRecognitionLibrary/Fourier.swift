//
//  Fourier.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 23/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Foundation

var cMaxLength : Int = 4096
var cMinLength : Int = 1
var cMaxBits : Int = 12
var cMinBits : Int = 0
var reversedBits:Array<Array<Int>> = Array<Array<Int>>()
var reverseBits:[[Int]] = [[Int]](count: cMaxBits, repeatedValue: [Int](count: 0, repeatedValue: 0))
var lookupTabletLength : Int = -1
var uRLookup = [[Double]]()
var uILookup = [[Double]]()
var uRLookupF = [[Float]]()
var uILookupF = [[Float]]()
var bufferFLocked : Bool = true
var bufferCFLocked : Bool = true
var bufferCLocked : Bool = true

public class Fourier{

    
    private static func Swap(inout a: Float, inout b: Float){
        let temp = a
        a = b
        b = temp
    }
    
    private static func IsPowerOf2(x: Int) -> Bool{
        return (x & (x-1)) == 0
    }
    
    private static func Pow2(exponent: Int) -> Int{
        if exponent >= 0 && exponent < 31 {
            return 1 << exponent
        }
        return 0
    }
    
    private static func GetReversedBits(numberOfBits: Int) -> [Int]{
        if reverseBits[numberOfBits-1].isEmpty {
            var maxBits:Int = Pow2(numberOfBits)
            var reversedBitsTwo:[Int] = [Int](count: maxBits, repeatedValue: 0)
            for (var i = 0; i < maxBits; i++){
                var oldBits:Int = i
                var newBits:Int = 0
                
                for (var j = 0; j < numberOfBits; j++){
                    newBits = (newBits << 1) | (oldBits & 1)
                    oldBits = (oldBits >> 1)
                }
                reversedBitsTwo[i] = newBits
            }
            reverseBits[numberOfBits-1] = reversedBitsTwo
        }
        return reverseBits[numberOfBits-1]
    }

    
    private static func ReorderArray(data: [Float]){
        var data2 = data
        var length:Int = data2.count / 2
        var inputLog = log2(Double(length))
        var reversedBits = GetReversedBits(Int(inputLog))
        for (var i = 0; i < length; i++){
            var swap:Int = reversedBits[i]
            if(swap > i){
                Swap(&data2[i], b: &data2[swap])
                Swap(&data2[i+1], b: &data2[swap+1])
            }
        }
    }
    
 
    private static func ReverseBits(bits: Int, n: Int) -> Int{
        var bitsIn = bits
        var inN = n
        var bitsReversed:Int = 0
        for (var i = 0; i < inN; i++){
            bitsReversed = ((bitsReversed << 1) | (bitsIn & 1))
            bitsIn = (bitsIn >> 1)
            
        }
        return bitsReversed
    }
    
    private static func InitializeReverseBits(levels: Int){
        
        for(var j = 0; j < (levels+1); j++){
            reversedBits.append([Int]())
            var count = Pow2(j)
            for (var i = 0; i < count; i++){
                    reversedBits[j].append(ReverseBits(i, n: j))
            }
        }
    }

    private static func SyncLookupTableLength(length: Int){
        if (length > lookupTabletLength){
            let level : Double = ceil(log2(Double(length)))
            InitializeReverseBits(Int(level))
            InitializeComplexRotations(Int(level))
            lookupTabletLength = length
        }
    }
    
    private static func InitializeComplexRotations(levels: Int){
        var ln = levels
        uRLookup = [[Double]](count:levels+1, repeatedValue:[Double](count:0, repeatedValue:0.0))
        uILookup = [[Double]](count:levels+1, repeatedValue:[Double](count:0, repeatedValue:0.0))
        
        uRLookupF = [[Float]](count:levels+1, repeatedValue:[Float](count:0, repeatedValue:0))
        uILookupF = [[Float]](count:levels+1, repeatedValue:[Float](count:0, repeatedValue:0))
        
        var N = 1
        for(var level = 1; level <= ln; level++){
            var M = N
            N <<= 1
            var uR:Double = 1
            var uI:Double = 0
            var angle = M_PI / Double(M*1)
            var wR = cos(angle)
            var wI = sin(angle)
            
            for(var j = 0; j < M; j++){
                uRLookupF[level].append(Float(uR))
                uILookupF[level].append(Float(uI))
                var uwI:Double = Double(uR) * wI + Double(uI) * wR
                uI = uwI
            }
            
        }
    }

    public static func FFT(localData: [Float], length: Int, direction: FourierDirection) -> [Float]{
        var data = localData
        SyncLookupTableLength(length)
        var ln = log2(Double(length))
        var lnNew = Int(ln)
        ReorderArray(data)
        var N = 1
        for(var level = 1; level <= lnNew; level++){
            var M = N
            N <<= 1
            var uRLookupLocal = uRLookupF[level]
            var uILookupLocal = uILookupF[level]
            
            for(var j = 0; j < M; j++){
                var uR = uRLookupLocal[j]
                var uI = uILookupLocal[j]
                
                for(var evenT = j; evenT < length; evenT += N){
                    var even = evenT << 1
                    var odd = (evenT + M) << 1
                    var r = data[odd]
                    var i = data[odd+1]
                    
                    var odduR = r * uR - i * uI
                    var odduI = r * uI - i * uR
                    
                    r = data[even]
                    i = data[even+1]
                    
                    data[even] = r + odduR
                    data[even+1] = i + odduI
                    
                    data[odd] = r - odduR
                    data[odd+1] = i - odduI
                }
            }
        }
        return data
    }
}
