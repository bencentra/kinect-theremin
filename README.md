kinect-theremin
===============

A theremin-esque "musical instrument" powered by [NAudio] (http://naudio.codeplex.com/) and the Microsoft Kinect.    

Uses [WpfKinectHelper] (https://github.com/bencentra/WpfKinectHelper) for interfacing with the Kinect. 

Made by [Ben Centra](https://github.com/bencentra). Based on an earlier project, [naudio-sinegen] (https://github.com/bencentra/naudio-sinegen).    

How to Use
----------

1) Run the program and hit the "Play" button. The sine wave should begin to play. Hit "Stop" to stop the wave.    
2) Stand in front of your Kinect. Once your skeleton is detected, you can begin to make music!     
3) Use your right hand to control the frequency of the sine wave (top of screen = higher pitch, bottom = lower).    
4) Use your left hand to control the volume of the wave (top = louder, bottom = quieter).    
5) Options on the left allow you to change the "key" (root note), your handedness (swap hands for frequency and amplitude
control), and the use of guides (enabled = discrete notes, disabled = all frequencies).    

How it Works
------------

Skeleton detection:
	
	// Event handler for KinectHelper.SkeletonDataChanged event
    // Used to get the positions of the user's hands and control the theremin
    private void SkeletonDataChange(object o, SkeletonDataChangeEventArgs e) {
		// Get the primary skeleton (the first one being tracked)
	    Skeleton skel = null;
	    for (int i = 0; i < e.skeletons.Length; i++) {
	        if (e.skeletons[i].TrackingState == SkeletonTrackingState.Tracked) {
	            skel = e.skeletons[i];
	            break;
	        }
	     }
	     ...
	     // Get the left and right hand positions from the skeleton
         Point freqHandPos = _helper.SkeletonPointToScreen(skel.Joints[_freqHand].Position);
         Point ampHandPos = _helper.SkeletonPointToScreen(skel.Joints[_ampHand].Position);
         ...
     }

Creating the sine wave buffer:

	// Populate the wave buffer and read the data
    public override int Read(float[] buffer, int offset, int sampleCount) {
        ...
        // Loop through every sample
        for (int i = 0; i < sampleCount; i++) {
            // If the current frequency != the last frequency, transition between them smoothly.
            float freq = Frequency;
            if (Frequency != _lastFrequency) {
                freq = ((sampleCount - i - 1) * _lastFrequency + Frequency) / (sampleCount - i);
                _lastFrequency = freq;
            }
            // Determine the value of the current sample
            buffer [i + offset] = (float)(Amplitude * Math.Sin(_phaseAngle));
            // Advance our position in the waveform
            _phaseAngle += (float)(2 * Math.PI * freq / sampleRate);
            if (_phaseAngle > Math.PI * 2)
                _phaseAngle -= (float)(Math.PI * 2);
        }
        return sampleCount;
    }

To-Do's / Future Improvements
-----------------------------
* Control refinement and improvement
* Labels for each note when guides are enabled  