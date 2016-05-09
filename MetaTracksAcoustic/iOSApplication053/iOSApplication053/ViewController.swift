import UIKit
import AudioRecognitionLibrary
import AudioToolbox
import AVFoundation

class ViewController: UIViewController,UITableViewDelegate, UITableViewDataSource {

    var sampleRate:Int = Int()
    var channels:Int = Int()
    var audioQuality:AVAudioQuality = AVAudioQuality.Max
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
        
        RecordManager.indexMovies()
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
        super.viewDidLoad()
        RecordManager.initialize()
        
    }

    func moviePickerButtonAction(sender:UIButton!){
        self.view.addSubview(tableView)
        
    }
    
    func getFingerprintsButtonAction(sender:UIButton!){
        RecordManager.getFingerprintsFull()
    }
    
    func recordButtonAction(sender:UIButton!){
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0)) {
            RecordManager.startSyncing()
            dispatch_async(dispatch_get_main_queue()) {
                self.foregroundLabel.text = String(RecordManager.LatestTimestamp)
            }
        }
    }
    
    func stopButtonAction(sender:UIButton!){
       RecordManager.stopSyncing()
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
    }
    
    let textCellIdentifier = "cell"
    
    
    func numberOfSectionsInTableView(tableView: UITableView) -> Int {
        return 1
    }
    
    func tableView(tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return RecordManager.availableMovies.count
    }
    
    func tableView(tableView: UITableView, didSelectRowAtIndexPath indexPath: NSIndexPath) {
        RecordManager.selectedMovie = RecordManager.availableMovies[indexPath.row]
        tableView.hidden = true
        print("Selected movie: " + RecordManager.selectedMovie)
    }
    
    func tableView(tableView: UITableView, cellForRowAtIndexPath indexPath: NSIndexPath) -> UITableViewCell{
        let cell = tableView.dequeueReusableCellWithIdentifier(textCellIdentifier, forIndexPath: indexPath)
        let row = indexPath.row
        out = RecordManager.availableMovies[indexPath.row]
        cell.textLabel?.text = RecordManager.availableMovies[row]
        return cell
    }
}