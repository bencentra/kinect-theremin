using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using Microsoft.Kinect;

namespace kinect_theremin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Object for playing the sine wave
        private SineWavePlayer _player;

        // Kinect helper
        private KinectHelper _helper;

        // Player handedness
        private JointType _freqHand;
        private JointType _ampHand;

        // Intervals (for drawing guides/determining frequency);
        private double _freqInterval;
        //double _ampInterval;

        // Boolean to enable/disable frequency guides
        private bool _enableGuides = true;

        // MainWindow Constructor 
        public MainWindow()
        {
            InitializeComponent();
            // Instantiate the wave player
            _player = new SineWavePlayer();
            // Set up the UI
            freqLabel.Content = _player.Frequency;
            ampLabel.Content = _player.Amplitude;
            for (int i = 0; i < _player.frequencyDict.Count; i++)
            {
                keyBox.Items.Add(_player.frequencyDict.ElementAt(i).Key);
            }
            keyBox.SelectedIndex = 0;
            // Initialize the wave player
            ChangeKey();
            _player.Frequency = _player.MinFreq;
            // Set the starting handedness
            _freqHand = JointType.HandRight;
            _ampHand = JointType.HandLeft;
            // Draw the guides
            _freqInterval = guideCanvas.Height / 12;
            //_ampInterval = guideCanvas.Height / 4;
            DrawGuides();
            // Instantiate and initialize the KinectHelper
            _helper = new KinectHelper(true, false, true);
            _helper.ToggleSeatedMode(true);
            _helper.SkeletonDataChanged += new KinectHelper.SkeletonDataChangedEvent(SkeletonDataChange);
            skeletonImage.Source = _helper.skeletonBitmap;
            rgbImage.Source = _helper.colorBitmap;
        }

        // Event handler for the playButton's Click event
        // Used to start and stop playing the wave
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            // If the wave is not playing, start it
            if (!_player.IsPlaying())
                StartPlayer();
            // If the wave is playing, stop it
            else
                StopPlayer();
        }

        // Event handler for the keyBox's SelectionChanged event
        // Used to change the key of the wave
        private void keyBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Get the current playing status of the wave
            bool wasPlaying = false;
            if (_player.IsPlaying())
                wasPlaying = true;
            // Stop the player
            StopPlayer();
            // Change the key
            ChangeKey();
            // If the wave was playing, start it again
            Console.WriteLine(wasPlaying);
            if (wasPlaying)
                StartPlayer();
        }

        // Event handler for the leftHandCheckbox's Checked/Unchecked events
        // Used to toggle the user's handedness
        private void leftHandCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ToggleHandedness();
        }

        // Event handler for KinectHelper.SkeletonDataChanged event
        // Used to get the positions of the user's hands and control the theremin
        private void SkeletonDataChange(object o, SkeletonDataChangeEventArgs e)
        {
            // Get the primary skeleton (the first one being tracked)
            Skeleton skel = null;
            for (int i = 0; i < e.skeletons.Length; i++)
            {
                if (e.skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                {
                    skel = e.skeletons[i];
                    break;
                }
            }
            // If no skeletons found, no need to continue
            if (skel == null)
                return;

            // Get the left and right hand positions from the skeleton
            Point freqHandPos = _helper.SkeletonPointToScreen(skel.Joints[_freqHand].Position);
            Point ampHandPos = _helper.SkeletonPointToScreen(skel.Joints[_ampHand].Position);

            // Determine the frequency based on the position of the right hand
            double freqValue = 1 - freqHandPos.Y / skeletonImage.Height;
            float customFreq;
            // If guides are enabled, determine the exact chromatic note to play 
            if (_enableGuides)
                customFreq = GetChromaticNoteFrequency(freqValue);
            // If not, determine the frequency based on the exact position
            else 
                customFreq = LinearToLog(freqValue);  

            // Determine the amplitude based on the position of the left hand
            double ampValue = 1 - ampHandPos.Y / skeletonImage.Height;
            float customAmp = (float) ampValue;
            
            // Update the wave
            if (_player.IsPlaying())
            {
                _player.Frequency = customFreq;
                freqLabel.Content = customFreq;
                _player.Amplitude = customAmp;
                ampLabel.Content = customAmp;
            }
        }

        // Start the wave player
        private void StartPlayer()
        {
            _player.Start();
            playButton.Content = "Stop";
        }

        // Stop the wave player
        private void StopPlayer()
        {
            _player.Stop();
            playButton.Content = "Play";
        }

        // Change the key of the wave player
        private void ChangeKey()
        {
            _player.ChangeKeyTo((String)keyBox.SelectedValue);
            _player.Frequency = _player.MinFreq;
            freqLabel.Content = _player.MinFreq;
        }

        // Swap hand control for frequency and amplitude
        private void ToggleHandedness()
        {
            if (_freqHand == JointType.HandRight)
            {
                _freqHand = JointType.HandLeft;
                leftLabel.Content = "Frequency";
                _ampHand = JointType.HandRight;
                rightLabel.Content = "Amplitude";
            }
            else
            {
                _freqHand = JointType.HandRight;
                rightLabel.Content = "Frequency";
                _ampHand = JointType.HandLeft;
                leftLabel.Content = "Amplitude";
            }
            ClearGuides();
            DrawGuides();
        }

        // Draw frequency "guides" for differentiating each note
        private void DrawGuides()
        {
            int width = 25;
            int height = 5;
            double freqX;
            //double ampX;
            // Determine which side to draw each set of guides on
            if (_freqHand == JointType.HandRight)
            {
                freqX = guideCanvas.Width - width;
                //ampX = 0;            
            }
            else
            {
                freqX = 0;
                //ampX = guideCanvas.Width - width;
            }
            // Draw the frequency guides
            for (int i = 0; i < 11; i++)
            {
                Rectangle guide = new Rectangle();
                guide.Width = width;
                guide.Height = height;
                guide.Fill = new SolidColorBrush(Colors.Gray);
                //guide.Stroke = new SolidColorBrush(Colors.Black);
                guide.StrokeThickness = 1;
                guideCanvas.Children.Add(guide);
                Canvas.SetTop(guide, _freqInterval * (i + 1));
                Canvas.SetLeft(guide, freqX);
            }
            // Draw the amplitude guides
            /*
            for (int i = 0; i < 3; i++)
            {
                Rectangle guide = new Rectangle();
                guide.Width = width;
                guide.Height = height;
                guide.Fill = new SolidColorBrush(Colors.Gray);
                //guide.Stroke = new SolidColorBrush(Colors.Black);
                guide.StrokeThickness = 1;
                guideCanvas.Children.Add(guide);
                Canvas.SetTop(guide, _ampInterval * (i + 1));
                Canvas.SetLeft(guide, ampX);
            }
            */
        }

        // Clear frequency guides
        private void ClearGuides()
        {
            guideCanvas.Children.Clear();
        }

        // Convert a "linear" value (0 - 1) to a logarithmic frequency 
        private float LinearToLog(double value)
        {
            return (float) Math.Pow(10, value * (Math.Log10(_player.MaxFreq) - Math.Log10(_player.MinFreq)) + Math.Log10(_player.MinFreq));
        }

        // Convert a logarithmic frequency to a "linear" value (0 - 1)
        private double LogToLinear(float freq)
        {
            return (double) ((Math.Log10(freq) - Math.Log10(_player.MinFreq)) / (Math.Log10(_player.MaxFreq) - Math.Log10(_player.MinFreq)));
        }

        private float GetChromaticNoteFrequency(double value)
        {
            int chromaticNote = (int)Math.Ceiling(value * 12);
            double chromaticValue = (double) chromaticNote / 12;
            Console.WriteLine(chromaticNote+ " --- "+chromaticValue);
            float chromaticFreq = LinearToLog(chromaticValue);
            return chromaticFreq;
        }

        // Event handler for the useGuidesCheckbox's Checked and Unchecked events
        // Used to toggle _enableGuides, play discrete notes or just the current frequency.
        private void useGuidesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            // If guides are currently enabled, disable them and clear them from the canvas
            if (_enableGuides)
            {
                _enableGuides = false;
                ClearGuides();
            }
            // If guides are currently disabled, enable them and draw them on the canvas
            else
            {
                _enableGuides = true;
                DrawGuides();
            }
        }
    }
}
