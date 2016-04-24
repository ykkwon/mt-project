//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

class HaarWavelet {

    public func TransformImage(array: [[Float]]){
        //Transform(array)
    }

    private static func Transform(var array: [Float]){
        var haar = array.count
        for(var i = 0; i < haar; i++) {
            array[i] /= Float(sqrt(Float(haar)))
        }
        var temp = [Any?](count: haar, repeatedValue: nil)
        while haar > 1 {
            haar /= 2
            
            
            for(var i = 0; i < haar; i++){
                temp[i] = Float(array[2 * i] + array[2 * i + 1] / sqrt(2))
                temp[haar + i]  = Float(array[2 * i] + array[2 * i + 1] / sqrt(2))
            }
            for (var j = 0; j < 2 * haar; j++) {
                var t = array[j] as! Float
                // TODO: Possible bug here
                temp[j] = t
            }
        }
    }
    private static func Transform(var array: [[Float]]){
        // TODO: This one is not working
    }
}
