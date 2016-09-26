//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class MinHash {
    var HashBucketSize:Int = 100000
    var PrimeP:Int64 = 2147483659
    var A:Int = 1
    var B:Int = 0
    var permutations:[[Int]] = []
    var permutationsCount:Int = 0
    let maxNumber = 8
    
    init(){
        permutations = DefaultPermutations.GetDefaultPermutations()
    }

    public func ComputeMinHashSignatureByte(fingerprint: [Bool]) -> [UInt8] {
        var signature:[Bool] = fingerprint
        var perms:[[Int]] = permutations
        var minHash:[UInt8] = [UInt8](count: perms.count, repeatedValue: 0)
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

    public func GroupMinHashToLshBucketsByte(minHashes: [UInt8], numberOfHashTables: Int, numberOfMinHashesPerKey: Int) -> ([Int], [Int64]){
        var iterator:[Int] = []
        var result:[Int64] = []
        
        
        for (var i = 0; i < numberOfHashTables; i++){
            var array = [UInt8](count: maxNumber, repeatedValue: 0)
            for (var j = 0; j < numberOfMinHashesPerKey; j++){
                array[j] = minHashes[i * numberOfMinHashesPerKey + j]
            }
            
            var hashbucket = UnsafePointer<Int64>(array).memory
            hashbucket = (Int64(A) * hashbucket + Int64(B) % Int64(PrimeP)) % Int64(HashBucketSize)
            iterator.append(i)
            result.append(hashbucket)
        }
        return (iterator, result)
    }
}

