//
//  ViewController.swift
//  iOSApplication053
//
//  Created by metatracks on 22/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import UIKit
import AudioRecognitionLibrary

class ViewController: UIViewController {

    override func viewDidLoad() {
        print(BASSVERSION);
        super.viewDidLoad()
        BassProxy.Initialize()
        BassProxy.GetSamplesMono("test", sampleRate : 44100)
        
        
        
        
        // Do any additional setup after loading the view, typically from a nib.
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }


}

