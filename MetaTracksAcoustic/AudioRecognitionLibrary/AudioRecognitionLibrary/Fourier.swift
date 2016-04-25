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
    
    private static func Log2(x: Int) -> Int{
            if (x <= 65536)
            {
                if (x <= 256)
                {
                    if (x <= 16)
                    {
                        if (x <= 4)
                        {
                            if (x <= 2)
                            {
                                if (x <= 1)
                                {
                                    return 0
                                }
                                return 1
                            }
                            return 2
                        }
                        if (x <= 8){
                        return 3
                        }
                        return 4
                    }
                    if (x <= 64)
                    {
                        if (x <= 32){
                        return 5
                        }
                        return 6
                    }
                    if (x <= 128){
                    return 7
                    }
                    return 8
                }
                if (x <= 4096)
                {
                    if (x <= 1024)
                    {
                        if (x <= 512){
                        return 9
                        }
                        return 10
                    }
                    if (x <= 2048){
                    return 11
                    }
                    return 12
                }
                if (x <= 16384)
                {
                    if (x <= 8192){
                    return 13
                    }
                    return 14
                }
                if (x <= 32768){
                return 15
                }
                return 16
            }
            if (x <= 16777216)
            {
                if (x <= 1048576)
                {
                    if (x <= 262144)
                    {
                        if (x <= 131072){
                        return 17
                        }
                        return 18
                    }
                    if (x <= 524288){
                    return 19
                    }
                    return 20
                }
                if (x <= 4194304)
                {
                    if (x <= 2097152){
                    return 21
                    }
                    return 22
                }
                if (x <= 8388608){
                return 23
                }
                return 24
            }
            if (x <= 268435456)
            {
                if (x <= 67108864)
                {
                    if (x <= 33554432){
                    return 25
                    }
                    return 26
                }
                if (x <= 134217728){
                return 27
                }
                return 28
            }
            if (x <= 1073741824)
            {
                if (x <= 536870912){
                return 29
                }
                return 30
            }
            return 31
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

    private static func ReorderArray(var data: [Complex]){
        var length = data.count
        var reversedBits = GetReversedBits(Log2(length))
        for (var i = 0; i < length; i++){
            var swap = reversedBits[i]
            if(swap > i){
                var temp: Complex = data[i]
                data[i] = data[swap]
                data[swap] = temp
            }
        }
    }

    private static func ReorderArray(var data: [ComplexF]){
        // ASSERT
        var length = data.count
        // ASSERT
        // ASSERT
        // ASSERT

        var reversedBits = GetReversedBits(Log2(length))
        for (var i = 0; i < length; i++){
            var swap = reversedBits[i]
            if(swap > i){
                var temp: ComplexF = data[i]
                data[i] = data[swap]
                data[swap] = temp
            }
        }
    }

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

    public static func FFT_Quick(data: [Float], length: Int, direction: FourierDirection){
        // TODO
    }

    public static func FFT(data: [ComplexF], length: Int, direction: FourierDirection){
        // TODO
    }

    public static func FFT_Quick(data: [ComplexF], length: Int, direction: FourierDirection){
        // TODO 
    }
}
