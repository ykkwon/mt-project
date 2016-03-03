function [x, pos] = myfunc(a, pos) 

[d,sr] = audioread(a);

f = figure('visible','off');
    specgram(d(:,1),1024,sr);
    saveas(f, sprintf('File_%d.jpg',pos));
x = 'Done';