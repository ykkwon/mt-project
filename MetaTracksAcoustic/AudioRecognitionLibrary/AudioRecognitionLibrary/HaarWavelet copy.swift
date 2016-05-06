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
        
            for(var index0 = 0; index0 < haar; index0++){
                temp[index0] = (array[2 * index0] + array[2 * index0 + 1]) / sqrt(2)
                temp[haar + index0] = (array[2 * index0] - array[2 * index0 + 1]) / sqrt(2)
            }
            for (var index = 0; index < 2 * haar; index++) {
                array[index] = temp[index]
            }
        }
        return temp
    }
    
    public static func Transform(var array: [[Float]]) -> [[Float]]{
        var inputArray:[[Float]] = array
        var retArray:[[Float]] = array
        
        var rowWidth: Int = 128
        var columnHeight: Int = 32
        
        for (var row = 0; row < rowWidth; row++){ // Transformation of each row
            retArray[row] = Transform(inputArray[row])
        }
        
        for (var col = 0; col < columnHeight; col++) // Transformation of each column
        {
            var column:[Float] = [Float](count: rowWidth, repeatedValue: 0) /*Length of each column is equal to number of rows*/
            
            for (var row = 0; row < rowWidth; row++){
                column[row] = retArray[row][col]
            }
            
            var tempArray = Transform(column); // 1d Transforms column
            
            for (var row = 0; row < rowWidth; row++){
               retArray[row][col] = tempArray[row]
            }
        }
     return retArray
    }
}