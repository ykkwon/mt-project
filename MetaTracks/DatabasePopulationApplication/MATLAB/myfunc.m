function [x] = myfunc(a) 

[d,sr] = audioread(a);
sg = subplot(313);
specgram(d(:,1),1024,sr);
saveas(sg,'filename.jpg');
x = 'filename.jpg';