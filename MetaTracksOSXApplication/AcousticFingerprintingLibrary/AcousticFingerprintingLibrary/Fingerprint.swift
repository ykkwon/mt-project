//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class Fingerprint {
    var Signature:[Bool] = [false]
    var SequenceNumber:Int = Int()
    var Timestamp:Double = Double()
    
    init(signature: [Bool], sequenceNo: Int, timestamp: Double){
        Signature = signature
        SequenceNumber = sequenceNo
        Timestamp = timestamp
    }
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
