using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace kinect_theremin
{
    public class SineWaveGenerator : WaveProvider32
    {
        // Initial frequency and amplitude
        private const float DEFAULT_INITIAL_FREQUENCY = 440f;
        private const float DEFAULT_INITIAL_AMPLITUDE = 0.25f;

        // Variable to track the angular position in the waveform
        private float _phaseAngle = 0f;

        // Variable to store the last frquency played
        private float _lastFrequency = 0f;

        // Getters and setters for wave frequency and amplitude
        public float Frequency { get; set; }
        public float Amplitude { get; set; }

        // Default constructor
        public SineWaveGenerator()
        {
            Frequency = DEFAULT_INITIAL_FREQUENCY;
            Amplitude = DEFAULT_INITIAL_AMPLITUDE;
            _lastFrequency = Frequency;
        }

        // Parameterized constructor for a custom default frequency
        public SineWaveGenerator(float freq)
        {
            Frequency = freq;
            Amplitude = DEFAULT_INITIAL_AMPLITUDE;
            _lastFrequency = Frequency;
        }

        // Parameterized constructor for custom default frequency and amplitude
        public SineWaveGenerator(float freq, float amp)
        {
            Frequency = freq;
            Amplitude = amp;
            _lastFrequency = Frequency;
        }

        // Populate the wave buffer and read the data
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            // Get the wave's sample rate
            int sampleRate = WaveFormat.SampleRate;
            // Loop through every sample
            for (int i = 0; i < sampleCount; i++)
            {
                // If the current frequency != the last frequency, transition between them smoothly.
                float freq = Frequency;
                if (Frequency != _lastFrequency)
                {
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
    }
}
