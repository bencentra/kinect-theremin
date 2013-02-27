using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace kinect_theremin
{
    public class SineWavePlayer
    {
        // Map keys to root note frequencies
        public readonly Dictionary<String, float> frequencyDict = new Dictionary<String, float>()
        {
            {"C", 261.63f },
            {"C#", 277.18f },
            {"D", 293.66f },
            {"D#", 311.13f },
            {"E", 329.63f },
            {"F", 349.23f },
            {"F#", 369.99f },
            {"G", 392.00f },
            {"G#", 415.30f },
            {"A", 440.00f },
            {"A#", 466.16f },
            {"B", 493.88f }
        };

        // Variables for playing a sine wave
        private WaveOut _wave;
        private SineWaveGenerator _waveGenerator;

        // Wave properties
        private float _frequency;
        private float _amplitude;

        // Minimum/Maximum allowable frequencies;
        private float _minFreq;
        private float _maxFreq;

        // Getter and setter for _frequency
        public float Frequency
        {
            get { return _frequency; }
            set 
            { 
                value = ConstrainFrequency(value);
                _frequency = value;
                if (_waveGenerator != null)
                    _waveGenerator.Frequency = _frequency;
            }
        }

        // Getter and setter for _amplitude
        public float Amplitude
        {
            get { return _amplitude; }
            set 
            {
                value = ConstrainAmplitude(value);
                _amplitude = value;
                if (_waveGenerator != null)
                    _waveGenerator.Amplitude = _amplitude;
            }
        }

        // Getters for Minimum and Maximum frequency
        public float MinFreq { get { return _minFreq; } }
        public float MaxFreq { get { return _maxFreq; } }

        // Default Constructor
        public SineWavePlayer()
        {
            _minFreq = frequencyDict.ElementAt(0).Value;
            _maxFreq = CalcMaxFreq();
            _frequency = _minFreq;
            _amplitude = 0.25f;
        }

        // Stop or start the wave
        public void StartStopWave()
        {
            if (_wave == null)
                Start();
            else
                Stop();
        }

        // Start playing the wave
        public void Start()
        {
            // Only continue if the wave is not currently playing
            if (_wave == null)
            {
                // Create a new sineWaveGenerator if not already instantiated
                if (_waveGenerator == null)
                    _waveGenerator = new SineWaveGenerator();
                // Set the sineWaveGenerator properties (sample rate, number of channels, frequency, and amplitude)
                _waveGenerator.SetWaveFormat((int)(_maxFreq * 2) + (int)_minFreq, 1);
                _waveGenerator.Frequency = _frequency;
                _waveGenerator.Amplitude = _amplitude;
                // Instantiate the _wave output object
                _wave = new WaveOut();
                // Initialize and play the wave
                _wave.Init(_waveGenerator);
                _wave.Play();
            }
        }

        // Stop playing the wave
        public void Stop()
        {
            // Only continue if the wave is playing
            if (_wave != null)
            {
                // Stop playing
                _wave.Stop();
                // Nullify the _wave object
                _wave.Dispose();
                _wave = null;
            }
        }

        // Determine if the wave is currently playing
        public bool IsPlaying()
        {
            if (_wave == null)
                return false;
            else
                return true;
        }

        // Keep the frequency in the appropriate range
        private float ConstrainFrequency(float freq)
        {
            if (freq < _minFreq)
                freq = _minFreq;
            else if (freq > _maxFreq)
                freq = _maxFreq;
            return freq;
        }

        // Keep the amplitude in the appropriate range
        private float ConstrainAmplitude(float amp)
        {
            if (amp < 0)
                amp = 0;
            else if (amp > 1)
                amp = 1;
            return amp;
        }

        // Change the key of the wave
        public void ChangeKeyTo(String key)
        {
            // If the key isn't in the frequencyDict, bail
            key = key.ToUpper();
            if (!frequencyDict.ContainsKey(key))
                return;
            // Set the min and max frequency according to the new root frequency
            _minFreq = frequencyDict[key];
            _maxFreq = CalcMaxFreq();
        }

        // Calculate the _maxFreq (_minFreq must be set first!)
        private float CalcMaxFreq()
        {
            // Get a frequency two octaves higher than the root frequency (each octave is 2x the root)
            return _minFreq * 4;
        }
    }
}
