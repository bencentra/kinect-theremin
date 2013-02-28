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

To-Do's / Future Improvements
-----------------------------
* Control refinement and improvement
* Labels for each note when guides are enabled  