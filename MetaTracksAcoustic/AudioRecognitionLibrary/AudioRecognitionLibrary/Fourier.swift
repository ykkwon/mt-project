//
//  Fourier.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 23/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Foundation

public class Fourier{
    var cMaxLength : Int = 4096
    var cMinLength : Int = 1
    var cMaxBits : Int = 12
    var cMinBits : Int = 0
    var reversedBits = [[Int]]()
    var reverseBits = [[Int]]()
    var lookupTabletLength : Int = -1
    var uRLookup = [[Double]]()
    var uILookup = [[Double]]()
    var uRLookupF = [[Float]]()
    var uILookupF = [[Float]]()
    var bufferFLocked : Bool = true
    var bufferCFLocked : Bool = true
    var bufferCLocked : Bool = true
    
    private static func Swap(inout a: Float, inout b: Float){
        var temp = a
        a = b
        b = temp
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
        // Todo: Fix
        return [0]
    }

    private static func ReorderArray(data: [Float]){
        // assert notNull
        var length = data.count / 2
        // assert isPowerOf2
        // assert lengths
        // assert lengths
        var reversedBits = GetReversedBits(Log2(length))
        for (var i = 0; i < length; i++){
            var swap = reversedBits[i]
            if(swap > i){
                //Swap(data[(i << 1)], data[swap << 1])
                // Swap
            }
        }
    }

    private static func ReorderArray(var data: [Complex]){
        // ASSERT
        var length = data.count
        // ASSERT
        // ASSERT
        // ASSERT

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

    private static func InitializeReverseBits(levels : Int){
        // TODO
    }

    private static func SyncLookupTableLength(length: Int){
        // TODO
        // Assert
        // Assert
        //if (length < lookupTabletLength){
         //   var level : Int = ceil(log(length, 2))
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
