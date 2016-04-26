//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class MinHash {
    var HashBucketSize:Int = 100000
    var PrimeP:CLong = 2147483659
    var A:Int = 1
    var B:Int = 0
    var permutations:[[Int]] = []
    var permutationsCount:Int = 0
    
    init(){
        // TODO permutations = DefaultPermutations.GetDefaultPermutations
        permutationsCount = permutations.count
    }

    public func ComputeMinHashSignatureByte(fingerprint: [Bool]) -> [UInt8] {
        // Should work
        var signature:[Bool] = fingerprint
        var perms:[[Int]] = []
        var minHash:[UInt8] = []
        for (var i = 0; i < perms.count; i++){
            minHash[i] = 255
            for (var j = 0; j < perms[i].count; j++){
                if(signature[perms[i][j]]){
                    minHash[i] = UInt8(j)
                    break
                }
            }

        }
        return minHash
    }

    public func GroupMinHashToLshBucketsByte(minHashes: [UInt8], numberOfHashTables: Int, numberOfMinHashesPerKey: Int) -> NSDictionary{
        // Should work
        var result:NSDictionary = NSDictionary()
        let maxNumber = 8
        for (var i = 0; i < numberOfHashTables; i++){
            var array = [UInt8?](count: maxNumber, repeatedValue: nil)
            for (var j = 0; j < numberOfMinHashesPerKey; j++){
                array[j] = minHashes[i * numberOfMinHashesPerKey * j]
            }
            var hashbucket = UnsafePointer<UInt64>(array).memory
            hashbucket = (UInt64(A) * hashbucket + UInt64(B) % UInt64(PrimeP)) % UInt64(HashBucketSize)
            result.setValue(i, forKey:String(hashbucket))
        }
        return result
    }
}

