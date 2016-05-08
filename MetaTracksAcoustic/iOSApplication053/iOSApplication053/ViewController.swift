//
//  ViewController.swift
//  iOSApplication053
//
//  Created by metatracks on 22/04/16.
//  Copyright © 2016 metatracks. All rights reserved.
//

import UIKit
import AudioRecognitionLibrary
import AudioToolbox
import AVFoundation

class ViewController: UIViewController,UITableViewDelegate, UITableViewDataSource {

    var sampleRate:Int = Int()
    var channels:Int = Int()
    var audioQuality:AVAudioQuality = AVAudioQuality.Max
    var selectedMovie:String = String()
    var availableMovies:[String] = []
    var mediaTypes:[String] = []
    var receivedHashes:[String] = []
    var receivedTimestamps:[String] = []
    var matchCounter:Double = Double()
    let manager : RecordManager = RecordManager()
    @IBOutlet weak var foregroundLabel: UILabel!
    var tableView:UITableView = UITableView()
    var out:String = String()
    
    override func viewDidLoad() {
        
        super.viewDidLoad()
        IndexMovies()
        tableView = UITableView(frame: UIScreen.mainScreen().bounds, style: UITableViewStyle.Plain)
        tableView.delegate      =   self
        tableView.dataSource    =   self
        tableView.registerClass(UITableViewCell.self, forCellReuseIdentifier: "cell")
        
        let moviePickerButton = UIButton(type: UIButtonType.System) as UIButton
        moviePickerButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 150, 100, 50)
        moviePickerButton.setTitle("Pick movie", forState: UIControlState.Normal)
        moviePickerButton.addTarget(self, action: #selector(ViewController.moviePickerButtonAction(_:)), forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(moviePickerButton)
        
        
        let getFingerprintsButton = UIButton(type: UIButtonType.System) as UIButton
        getFingerprintsButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 250, 100, 50)
        getFingerprintsButton.setTitle("Fingerprints", forState: UIControlState.Normal)
        getFingerprintsButton.addTarget(self, action: #selector(ViewController.getFingerprintsButtonAction(_:)), forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(getFingerprintsButton)
        
        let recordButton = UIButton(type: UIButtonType.System) as UIButton
        recordButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 350, 100, 50)
        recordButton.setTitle("Record", forState: UIControlState.Normal)
        recordButton.addTarget(self, action: #selector(ViewController.recordButtonAction(_:)), forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(recordButton)
        
        let stopButton = UIButton(type: UIButtonType.System) as UIButton
        stopButton.frame = CGRectMake(CGRectGetMidX(view.frame)-50, 450, 100, 50)
        stopButton.setTitle("Stop", forState: UIControlState.Normal)
        stopButton.addTarget(self, action: #selector(ViewController.stopButtonAction(_:)), forControlEvents: UIControlEvents.TouchUpInside)
        self.view.addSubview(stopButton)

        print(BASSVERSION);
        super.viewDidLoad()
        BassProxy.Initialize()
    }

    func moviePickerButtonAction(sender:UIButton!){
        self.view.addSubview(tableView)
        
    }
    
    func getFingerprintsButtonAction(sender:UIButton!){
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let orig = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='" + selectedMovie + "'"
        let spacesEscaped :String = orig.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url = NSURL(string: spacesEscaped)
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in
            
            dispatch_async(dispatch_get_main_queue()) {
                UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            }
            let hashtemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            self.receivedHashes = hashtemp.componentsSeparatedByString(";")
        }
        dataTask?.resume()
        let orig2 = "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='" + self.selectedMovie + "'"
        let spacesEscaped2 :String = orig2.stringByAddingPercentEncodingWithAllowedCharacters(NSCharacterSet.URLQueryAllowedCharacterSet())!
        
        let url2 = NSURL(string: spacesEscaped2)
        
        dataTask = defaultSession.dataTaskWithURL(url2!) {
            data, response, error in
            
            dispatch_async(dispatch_get_main_queue()) {
                UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            }
            let timestamptemp = String(data: data!, encoding: NSUTF8StringEncoding)!
            self.receivedTimestamps = timestamptemp.componentsSeparatedByString(";")
            print("Received hashes and timestamps for " + self.selectedMovie + ".")
            print("Hashes: " + String(self.receivedHashes.count))
            print("Timestamps: " + String(self.receivedTimestamps.count))
            
            self.manager.getFingerprints(self.receivedHashes, receivedTimestamps: self.receivedTimestamps)
        }
        dataTask?.resume()
        
    }
    
    func recordButtonAction(sender:UIButton!){
        manager.record()
        print("Done recording")
        
    }
    
    func stopButtonAction(sender:UIButton!){
       self.manager.play()
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    let textCellIdentifier = "cell"
    
    
    func numberOfSectionsInTableView(tableView: UITableView) -> Int {
        return 1
    }
    
    func tableView(tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return self.availableMovies.count
    }
    
    func tableView(tableView: UITableView, didSelectRowAtIndexPath indexPath: NSIndexPath) {
        selectedMovie = availableMovies[indexPath.row]
        tableView.hidden = true
        print("Selected movie: " + selectedMovie)
    }
    
    func tableView(tableView: UITableView, cellForRowAtIndexPath indexPath: NSIndexPath) -> UITableViewCell{
        let cell = tableView.dequeueReusableCellWithIdentifier(textCellIdentifier, forIndexPath: indexPath)
        let row = indexPath.row
        out = availableMovies[indexPath.row]
        cell.textLabel?.text = availableMovies[row]
        return cell
    }
    
    func IndexMovies(){
        let defaultSession = NSURLSession(configuration: NSURLSessionConfiguration.defaultSessionConfiguration())
        var dataTask: NSURLSessionDataTask?
        let url = NSURL(string: "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL")
        
        dataTask = defaultSession.dataTaskWithURL(url!) {
            data, response, error in
            
            dispatch_async(dispatch_get_main_queue()) {
                UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            }
            let temp = String(data: data!, encoding: NSUTF8StringEncoding)!
            self.availableMovies = temp.componentsSeparatedByString(",")
            print("Indexed movies.")
        }
        dataTask?.resume()
        
    }
}