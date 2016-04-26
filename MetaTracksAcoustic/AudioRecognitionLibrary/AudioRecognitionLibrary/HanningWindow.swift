//
// Created by metatracks on 24/04/16.
// Copyright (c) 2016 metatracks. All rights reserved.
//

import Foundation
import Surge

public class HanningWindow {
    public func WindowInPlace(var outerspace: [Float], length: Int) {
        // TODO: Might be buggy, but should work
        for (var i = 0, n = length; i < n; i++) {
            var iCast: Double = Double(i)
            outerspace[i] *= (Float)(0.5 * (1.0 - cos(2.0 * M_PI * iCast / (Double)(n - 1))))
        }
    }

    public func WindowInPlace(var outerspace: [Complex], length: Int) {
        var internalOuterspace = outerspace
        for (var i = 0, n = length; i < n; i++) {
            //outerspace[i] = mul(0.5, (1 - cos(2*M_PI * i / (n-1))))
        }

    }

    // TODO: Fix this
    public func GetWindow(length: Int) /*-> [Double]*/ {
        var array = [Double?](count: length, repeatedValue: nil)
        for(var i = 0; i < length; i++){
            //array[i] = 0.5 * (1-cos(2 * M_PI * i / (length - 1))
        }
        //return array
    }
}

