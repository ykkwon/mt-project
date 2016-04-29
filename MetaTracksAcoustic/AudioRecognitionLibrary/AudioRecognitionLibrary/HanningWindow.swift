//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation

public class HanningWindow {
    public func GetWindow(length: Int) -> [Double] {
        var array = [Double](count: length, repeatedValue: 0.0)
        for(var i = 0; i < length; i++){
            array[i] = 0.5 * (1 - cos(2.0 * M_PI * Double(i) / (Double(length)) - Double(i)))
        }
        return array
    }
}

