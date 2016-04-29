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
var reversedBits = [[Int]]()
var reverseBits = [[Int]]()
var lookupTabletLength : Int = -1
var uRLookup = [[[Double]]]()
var uILookup = [[[Double]]]()
var uRLookupF = [[[Float]]]()
var uILookupF = [[[Float]]]()
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
        assert(numberOfBits >= cMinBits)
        assert(numberOfBits <= cMaxBits)
        if reversedBits[numberOfBits-1].isEmpty { // TODO: equivalent to == null?
            var maxBits:Int = Pow2(numberOfBits)
            var reversedBitsTwo:[Int] = [maxBits]
            for (var i = 0; i < maxBits; i++){
                var oldBits:Int = i
                var newBits:Int = 0
                
                for (var j = 0; j < numberOfBits; j++){
                    newBits = (newBits << 1) | (oldBits & 1)
                    oldBits = (oldBits >> 1)
                }
                reversedBitsTwo[i] = newBits
            }
            reversedBits[numberOfBits-1] = reversedBitsTwo
        }
        return reversedBits[numberOfBits-1]
    }

    /*
    private static func ReorderArray(data: [Float]){
        var data = [Float]()
        assert(!data.isEmpty)
        var length:Int = data.count / 2
        assert(IsPowerOf2(length))
        assert(length >= cMinLength)
        assert(length <= cMaxLength)
        var reversedBits = GetReversedBits(Log2(length))
        for (var i = 0; i < length; i++){
            var swap:Int = reversedBits[i]
            if(swap > i){
                Swap(&data[i], b: &data[swap])
                Swap(&data[i+1], b: &data[swap+1])
            }
        }
    }
    */
    private static func ReverseBits(bits: Int, n: Int) -> Int{
        var bitsIn = bits
        var bitsReversed:Int = 0
        for (var i = 0; i < n; i++){
            bitsReversed = (bitsReversed << 1) | (bits & 1)
            bitsIn = (bits >> 1)
        }
        return bitsReversed
    }
    
    private static func InitializeReverseBits(levels: Int){
        reverseBits = [[levels+1]]
        for(var j = 0; j < levels+1; j++){
            var count:Double = pow(2, Double(j))
            reverseBits[j] = [Int(count)]
            for (var i = 0; i < Int(count); i += 1){
                reverseBits[j][i] = ReverseBits(i, n: j) // TODO: Possible bug here
            }
        }
    }

    private static func SyncLookupTableLength(length: Int){
        if (length > lookupTabletLength){
            let level : Double = ceil(log(Double(length)))
            InitializeReverseBits(Int(level))
            InitializeComplexRotations(Int(level))
            lookupTabletLength = length
        }
    }
    
    private static func InitializeComplexRotations(levels: Int){
        // TODO
        
    }

    public static func FFT(data: [Float], length: Int, direction: FourierDirection){
        // TODO
    }
}
