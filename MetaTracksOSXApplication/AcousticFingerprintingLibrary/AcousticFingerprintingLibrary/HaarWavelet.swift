//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class HaarWavelet {
    
    private static var globalArrays:[[Float]] = [[]]
    
    public static func TransformImage(array: [[Float]]) -> [[Float]]{
        globalArrays = array
        TwoHaarWavelet()
        return globalArrays
    }

    
    public static func TwoHaarWavelet(){
        
        var rowWidth: Int = 128
        var columnHeight: Int = 32
        
        for (var row = 0; row < rowWidth; row++){ // Transformation of each row
            globalArrays[row] = OneHaarWavelet(globalArrays[row])
        }
        
        for (var col = 0; col < columnHeight; col++) // Transformation of each column
        {
            var column:[Float] = [Float](count: rowWidth, repeatedValue: 0) /*Length of each column is equal to number of rows*/
            for (var row = 0; row < rowWidth; row++){
                column[row] = globalArrays[row][col]
            }
            
            var colm = OneHaarWavelet(column); // 1d Transforms column
            
            for (var row = 0; row < rowWidth; row++){
               globalArrays[row][col] = colm[row]
            }
        }
    }
    
    public static func OneHaarWavelet(var arrayNN: [Float]) -> [Float]{
        var array = arrayNN
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
        return array
    }
}