//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class Fingerprint {
    var Signature:[Bool] = []
    var SequenceNumber:Int = 0
    var Timestamp:Double = 0
}

public class HashedFingerprint {
    init(hashBins: [CLong], sequenceNumber: Int, sequenceAt: Double){
        HashBins = hashBins;
        SequenceNumber = sequenceNumber
        Timestamp = sequenceAt
    }
    
    init(hashBins: [CLong], timestamp: Double){
        HashBins = hashBins
        Timestamp = timestamp
        SequenceNumber = 0
    }
    
    public var HashBins:[CLong]
    public var SequenceNumber:Int
    public var Timestamp:Double
    
}