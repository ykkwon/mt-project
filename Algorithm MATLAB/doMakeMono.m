function [ avgSpect ] = doMakeMono( double )
    left = double(:,1); 
    right = double(:,2);
    disp('Channel is now mono');
    avgSpect = (left + right)/2;
end



