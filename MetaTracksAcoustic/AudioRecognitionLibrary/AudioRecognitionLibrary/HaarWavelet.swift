//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class HaarWavelet {

    public static func Transform(var array: [Float]){
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
    public static func Transform(var array: [[Float]]){
        var rowWidth: Int = array.count
        var columnHeight: Int = array.count
        for var row = 0; row < rowWidth; row++ {
            Transform(array[row])
        }
        for (var col = 0; col < columnHeight; col++) {
            var column = [Double](count: rowWidth, repeatedValue: 0.0)
            for (var row = 0; row < rowWidth; row++) {
                column.append(Double(row)) //TODO: possible bug
            
                
                // TODO: Transform(column)
            
            for var row = 0; row < rowWidth; row++ {
                //TODO: array.append(column[[Double(row)]])
            }
        }
    }
}
}