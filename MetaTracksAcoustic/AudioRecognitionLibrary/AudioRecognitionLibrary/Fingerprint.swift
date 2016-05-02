//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class Fingerprint {
    var Signature:[Bool] = [true]
    var SequenceNumber:Int = 0
    var Timestamp:Double = 0
    init(signature: [Bool], sequenceNo: Int, timestamp: Double){}
}



public class HashedFingerprint {
    init(hashBins: [Int64], sequenceNumber: Int, sequenceAt: Double){
        HashBins = hashBins;
        SequenceNumber = sequenceNumber
        Timestamp = sequenceAt
    }
    
    init(hashBins: [Int64], timestamp: Double){
        HashBins = hashBins
        Timestamp = timestamp
        SequenceNumber = 0
    }
    
    public var HashBins:[Int64]
    public var SequenceNumber:Int
    public var Timestamp:Double
    
}
