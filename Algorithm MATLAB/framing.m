function [framedMatrix] = framing( avgSpect, fileSampleRate )
    matSize = fileSampleRate * 0.25;
    framedMatrix = vec2mat(avgSpect,matSize)'; % Del opp
    disp('Matrix has been split into frames');
end

