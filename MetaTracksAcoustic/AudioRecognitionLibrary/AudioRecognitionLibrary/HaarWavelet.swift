//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class HaarWavelet {
    
    public static func Transform(var array: [Float]) -> [Float]{
        var haar = array.count
        for(var i = 0; i < haar; i++) {
            array[i] /= Float(sqrt(Float(haar)))
        }
        var temp = [Float](count: haar, repeatedValue: 0)
        while (haar > 1) {
            haar /= 2
            
            for(var i = 0; i < haar; i++){
                temp[i] = (array[2 * i] + array[2 * i + 1]) / sqrt(2)
                temp[haar + i] = (array[2 * i] - array[2 * i + 1]) / sqrt(2)
            }
            for (var j = 0; j < 2 * haar; j++) {
                array[j] = temp[j]
            }
            
        }
        return temp
    }
    public static func Transform(var array: [[Float]]){
        var rowWidth: Int = 128
        var columnHeight: Int = 32
        for var row = 0; row < rowWidth; row++ {
            Transform(array[row])
        }
        for (var col = 0; col < columnHeight; col++) {
            var column:[Float] = [Float](count: rowWidth, repeatedValue: 0)
            for (var row = 0; row < rowWidth; row++) {
                column[row] = array[row][col]
            }
            var transformedArray = Transform(column)
            
            for (var row = 0; row < rowWidth; row++) {
                array[row][col] = column[row];
            }
        }
    }
}