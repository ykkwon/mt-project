//
//  Complex.swift
//  AudioRecognitionLibrary
//
//  Created by metatracks on 22/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Foundation

public struct Complex {
    var Re:Double = Double()
    var Im:Double = Double()
    
    init(Real: Double, Imaginary: Double){
        Re = Real
        Im = Imaginary
    }
    
    //init(Complex c){
    // Re = c.Re
    // Im = c.Im
    //}
    
    public static func FromRealImaginary(real: Double, imaginary: Double) -> Complex{
        var c = Complex(Real: 0, Imaginary: 0)
        c.Re = real
        c.Im = imaginary
        return c
    }
    
    public static func FromModulusArgument(modulus: Double, argument: Double) -> Complex{
        var c = Complex(Real: 0, Imaginary: 0)
        c.Re = (modulus * cos(argument))
        c.Im = (modulus * sin(argument))
        return c
    }
    
    public func Clone() -> Complex{
        let c = Complex(Real: self.Re, Imaginary: self.Im)
        return c
    }
    
    public func GetModulus() -> Double{
        let x = Double(Re)
        let y = Double(Im)
        return sqrt(x * x + y * y)
    }
    
    // TODO: Different implementations?
    public func GetHashCode() -> Int{
        return Re.hashValue ^ Im.hashValue
    }
    
    
    public func CompareTo(o: AnyObject) -> Int{
        if 0 == nil {
            return 1
        }
        if ((o as? Complex) != nil) {
            
            
            //return GetModulus() o.GetModulus()
        }
        if ((o as? Double) != nil) {
            //return GetModulus.CompareTo((double)o)
        }
        //if o as? ComplexF {
            //return GetModulus.CompareTo.ComplexF)o).GetModulus
        //}
        if ((o as? Float) != nil){
            //return getModulus.CompareTo((float)o)
        }
        return 0
    }
    
    //public static func Complex(a: Complex, f: Double) -> Complex{
   //
    //}
    
    public /* override */ func ToString() -> String{
        let timeString = String(format: "%02d:%02d", Re, Im)
        return timeString
    }
}