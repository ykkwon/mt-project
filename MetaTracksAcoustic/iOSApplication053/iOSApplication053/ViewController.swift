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
        super.viewDidLoad()

        
        var moviePickerButton = UIButton(type: UIButtonType.System) as UIButton
        moviePickerButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 150, 100, 50)
        moviePickerButton.setTitle("Pick movie", forState: UIControlState.Normal)
        moviePickerButton.addTarget(self, action: "moviePickerButtonAction:", forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(moviePickerButton)
        
        
        var getFingerprintsButton = UIButton(type: UIButtonType.System) as UIButton
        getFingerprintsButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 250, 100, 50)
        getFingerprintsButton.setTitle("Fingerprints", forState: UIControlState.Normal)
        getFingerprintsButton.addTarget(self, action: "getFingerprintsButtonAction:", forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(getFingerprintsButton)
        
        var recordButton = UIButton(type: UIButtonType.System) as UIButton
        recordButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 350, 100, 50)
        recordButton.setTitle("Record", forState: UIControlState.Normal)
        recordButton.addTarget(self, action: "recordButtonAction:", forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(recordButton)
        
        var stopButton = UIButton(type: UIButtonType.System) as UIButton
        stopButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 450, 100, 50)
        stopButton.setTitle("Stop", forState: UIControlState.Normal)
        stopButton.addTarget(self, action: "stopButtonAction:", forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(stopButton)
        
        
        print(BASSVERSION);
        super.viewDidLoad()
        BassProxy.Initialize()
        BassProxy.GetSamplesMono("test", sampleRate : 44100)
        
    }

    func moviePickerButtonAction(sender:UIButton!){
        print("Movie picker.")
    }
    
    func getFingerprintsButtonAction(sender:UIButton!){
        print("Get fingerprints.")
    }
    
    func recordButtonAction(sender:UIButton!){
        print("Record.")
    }
    
    func stopButtonAction(sender:UIButton!){
        print("Stop.")
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }


}

