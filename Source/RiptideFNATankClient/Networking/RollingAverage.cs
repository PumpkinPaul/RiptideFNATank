//-----------------------------------------------------------------------------
// RollingAverage.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Wombat.Engine;
using Wombat.Engine.DebugTools;

namespace RiptideFNATankClient.Networking;

/// <summary>
/// To compensate for network latency, we need to know exactly how late each
/// packet is. Trouble is, there is no guarantee that the clock will be set the
/// same on every machine! The sender can include packet data indicating what
/// time their clock showed when they sent the packet, but this is meaningless
/// unless our local clock is in sync with theirs. To compensate for any clock
/// skew, we maintain a rolling average of the send times from the last 100
/// incoming packets. If this average is, say, 50 milliseconds, but one specific
/// packet arrives with a time difference of 70 milliseconds, we can deduce this
/// particular packet was delivered 20 milliseconds later than usual.
/// </summary>
class RollingAverage
{
    // Array holding the N most recent sample values.
    readonly float[] _sampleValues;

    // Counter indicating how many of the sampleValues have been filled up.
    int _sampleCount;

    // Cached sum of all the valid sampleValues.
    float _valueSum;

    // Write position in the sampleValues array. When this reaches the end,
    // it wraps around, so we overwrite the oldest samples with newer data.
    int _currentPosition;

    /// <summary>
    /// Constructs a new rolling average object that will track
    /// the specified number of sample values.
    /// </summary>
    public RollingAverage() : this(120) { }

    /// <summary>
    /// Constructs a new rolling average object that will track
    /// the specified number of sample values.
    /// </summary>
    public RollingAverage(int sampleCount)
    {
        _sampleValues = new float[sampleCount];

#if DEBUG_TOOLS || PROFILE
        DebugSystem.RegisterDrawing(DebugDraw);
#endif
    }

    /// <summary>
    /// Adds a new value to the rolling average, automatically
    /// replacing the oldest existing entry.
    /// </summary>
    public void AddValue(float newValue)
    {
        // To avoid having to recompute the sum from scratch every time
        // we add a new sample value, we just subtract out the value that
        // we are replacing, then add in the new value.
        _valueSum -= _sampleValues[_currentPosition];
        _valueSum += newValue;

        // Store the new sample value.
        _sampleValues[_currentPosition] = newValue;

        // Increment the write position.
        _currentPosition++;

        // Track how many of the sampleValues elements are filled with valid data.
        if (_currentPosition > _sampleCount)
            _sampleCount = _currentPosition;

        // If we reached the end of the array, wrap back to the beginning.
        if (_currentPosition >= _sampleValues.Length)
        {
            _currentPosition = 0;

            // The trick we used at the top of this method to update the sum
            // without having to recompute it from scratch works pretty well to
            // keep the average efficient, but over time, floating point rounding
            // errors could accumulate enough to cause problems. To prevent that,
            // we recalculate from scratch each time the counter wraps.
            _valueSum = 0;

            foreach (float value in _sampleValues)
                _valueSum += value;
        }
    }

    /// <summary>
    /// Gets the current value of the rolling average.
    /// </summary>
    public float AverageValue => _sampleCount == 0 ? 0 : _valueSum / _sampleCount;

    public void DebugDraw()
    {
        var y = BaseGame.Instance.Window.ClientBounds.Height - 60;
        var x = 480;

        DebugSystem.Plotter.DrawLines("Latency", _sampleValues, new Vector2(x, y), DebugSystem.MediumPlotSize, 0, FlatTheme.Carrot);
    }
}