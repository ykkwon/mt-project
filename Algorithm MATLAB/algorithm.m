close all;

file = 'testFile.mp4';
fileInfo = audioinfo(file);
fileSampleRate = fileInfo.SampleRate; 
fileTotalSamples = fileInfo.TotalSamples;
fileDuration = fileInfo.Duration;
fileBitRate = fileInfo.BitRate;
display(fileInfo);
t = 0:seconds(1/fileSampleRate):seconds(fileDuration);
t = t(1:end-1);

% Read the audio signals from the input file
y = audioread(file); 

% Separate the channels and do stereo -> mono
avgSpect = doMakeMono(y);

% Split the frames
framedMatrix = framing(avgSpect, fileSampleRate);

% Todo: STFT each individual frame in framedMatrix
newArray = fft(framedMatrix(:));

% Todo: Hash each individual frame

%%%%%%%%% Plotting of signals and spectrograms for debugging %%%%%%%%%
plot(newArray);

% Plot lydsignalene
% plot(t, avgSpect);
% xlabel('Time');
% ylabel('Frequency');

% Plot spektrogram
% plot(right);
% plot(left);
% spectrogram(right,fs,'yaxis'); % Plot den h�yre kanalen med STFT(!)
% spectrogram(right,kaiser(256,5),220,512,fs,'yaxis')