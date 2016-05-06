//
//  Complex.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 22/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Foundation

public struct Complex {
    public var Re:Double = Double()
    public var Im:Double = Double()
    
    public func GetModulus() -> Double {
        var x:Double = Re
        var y:Double = Im
        return sqrt(x * x + y * y)
    }
    
    public func GetHashCode() -> Int {
        return (Re.hashValue ^ Im.hashValue)
    }
    
    public static func Multiplier(a: Complex, f: Double) -> Complex{
        var b = a
        b.Re = a.Re * f
        b.Im = a.Im * f
        return b
    }
    
    public func ToString() -> String{
        return String(format: "%.01f, %.01f", Re, Im)
    }
    
    
    
    
}