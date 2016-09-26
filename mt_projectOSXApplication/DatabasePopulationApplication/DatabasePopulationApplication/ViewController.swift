//
//  ViewController.swift
//  DatabasePopulationApplication
//
//  Created by metatracks on 05/05/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import Cocoa
import Foundation
import AcousticFingerprintingLibrary

class ViewController: NSViewController {
    
    var fileURL:NSURL = NSURL()
    var manager:FingerprintManager = FingerprintManager()
    var inputTitle:String = String()
    var inputType:String = String()
    var inputId:Int = Int()
    
    @IBOutlet weak var openButton: NSButton!
    @IBOutlet weak var titleField: NSTextField!
    @IBOutlet weak var typeField: NSTextField!
    @IBOutlet weak var idField: NSTextField!
    @IBOutlet weak var sendButton: NSButton!
    @IBOutlet weak var fglabel: NSTextField!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        print(BASS_GetVersion())
    }

    override var representedObject: AnyObject? {
        didSet {
        }
    }

    @IBAction func openAction(sender: AnyObject) {
        let openPanel = NSOpenPanel()
        openPanel.title = "Choose a movie or music file."
        openPanel.beginWithCompletionHandler({(result:Int) in
            if(result == NSFileHandlingPanelOKButton)
            {
                self.fileURL = openPanel.URL!
                print(self.fileURL)
            }
        })
    }


    @IBAction func sendAction(sender: AnyObject) {
        var fullmessage:String = String()
        let fileName = "samples.csv"
        var filePath = "/Users/metatracks/Desktop/"
        let fullPath = filePath + fileName
        
        let fingerprints:Array<Fingerprint> = manager.CreateFingerprints(fileURL);
        let test = manager.GetFingerHashes(fingerprints);
        inputTitle = titleField.stringValue
        inputType = typeField.stringValue
        var prem = idField.stringValue
        inputId = Int(prem)!
    
        for var fingerprint in test{
          
            for (var i = 0; i < fingerprint.HashBins.count; i += 1)
            {
                var currentHash = fingerprint.HashBins[i];
                var naught = 0;
                var first = inputTitle;
                var second = fingerprint.Timestamp;
                var third = fingerprint.SequenceNumber;
                var fourth = currentHash;
                var fifth = inputType;
                var sixth = inputId;
                
                let message = "\(naught);\(first);\(second);\(third);\(fourth);\(fifth);\(sixth)\n"
                fullmessage.appendContentsOf(message)
            }
        
        
 
        do {
            try fullmessage.writeToFile(fullPath, atomically: true, encoding: NSUTF8StringEncoding)
            
        } catch {
            
            print("Failed to create file")
            print("\(error)")
        }
            
            
        }
    }
}



