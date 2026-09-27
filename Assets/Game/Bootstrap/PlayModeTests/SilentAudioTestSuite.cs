using NUnit.Framework;
using UnityEngine;

// Applies to every PlayMode test in this assembly, including runs selected by a test filter.
[SetUpFixture]
public sealed class SilentAudioTestSuite
{
    private float _previousVolume;

    [OneTimeSetUp]
    public void Silence()
    {
        _previousVolume = AudioListener.volume;
        AudioListener.volume = 0f;
    }

    [OneTimeTearDown]
    public void Restore()
    {
        AudioListener.volume = _previousVolume;
    }
}
