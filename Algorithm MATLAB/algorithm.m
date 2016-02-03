close all;

file = 'testFile.mp4';
fileInfo = audioinfo(file); % Lagre filinfo under
fileSampleRate = fileInfo.SampleRate; 
fileTotalSamples = fileInfo.TotalSamples;
fileDuration = fileInfo.Duration;
fileBitRate = fileInfo.BitRate;

display(fileInfo); 
y = audioread(file); % Les lydsporet til testfilen.

% Del opp kanalene og stereo -> mono
avgSpect = doMakeMono(y);

t = 0:seconds(1/fileSampleRate):seconds(fileDuration);
t = t(1:end-1);

% Plot lydsignalene
%plot(t, avgSpect);
xlabel('Time');
ylabel('Frequency');

% Todo: Del opp frames
framedMatrix = framing(avgSpect, fileSampleRate);
spectrogram();

% Plot spektrogram(er)
%plot(right);
%plot(left);
%spectrogram(right,fs,'yaxis'); % Plot den høyre kanalen med STFT(!)
%spectrogram(right,kaiser(256,5),220,512,fs,'yaxis')


