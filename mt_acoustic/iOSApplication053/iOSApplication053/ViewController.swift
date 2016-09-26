import UIKit
import AudioRecognitionLibrary
import AudioToolbox
import AVFoundation


class ViewController: UIViewController{
    var audioQuality:AVAudioQuality = AVAudioQuality.Max
    
    var selectedMovie:String = ""
    var availableMovies:[String] = []
    var receivedHashes:[String] = []
    var receivedTimestamps:[String] = []
    var fingerprints:[HashedFingerprint] = []
    
    override func viewDidLoad() {
        super.viewDidLoad()
        RecordManager.initialize()
        RecordManager.indexMovies({(response) in self.setMovies(response)}) // Henter ned tilgjengelige filmer fra databasen
        sleep(5)
        RecordManager.getHashes({(response) in self.setHashes(response)}) // Henter ned hasher fra databasen
        sleep(5)
        RecordManager.getTimestamps({(response) in self.setTimestamps(response)}) // Henter ned timestamps fra databasen
        sleep(5)
        RecordManager.setStoredFingerprints(receivedHashes, inputTimestamps: receivedTimestamps) // Genererer fingeravtrykkene lokalt i RecordManager
        
        RecordManager.startSyncing()
    }
    
    internal func setMovies(resp: [String]) {
        availableMovies = resp
        selectedMovie = availableMovies[3]
        RecordManager.setMovie(selectedMovie)
        print(selectedMovie)
        
    }
    
    
    internal func setTimestamps(resp: [String]) {
        receivedTimestamps = resp
        print(receivedTimestamps)
    }
    
    
    internal func setHashes(resp: [String]) {
        receivedHashes = resp
        print(receivedHashes)
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
    }
}