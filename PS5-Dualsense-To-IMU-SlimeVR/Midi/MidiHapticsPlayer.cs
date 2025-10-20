using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Everything_To_IMU_SlimeVR.Tracking;

public class MidiHapticsPlayer : IDisposable
{
    private readonly List<IBodyTracker> trackers;
    private Playback playback;

    // === Haptic pitch settings ===
    private const int SupportedOctaveSize = 12;  // one octave of pitch range
    private const int BaseNote = 60;             // middle C (C4)
    private const float MinIntensity = 0.3f;     // motor barely running
    private const float MaxIntensity = 1.0f;     // full speed

    // Track active notes to handle early NoteOff
    private readonly Dictionary<(int channel, int noteNumber), IBodyTracker> activeNotes = new();
    private readonly Dictionary<IBodyTracker, DateTime> trackerEndTimes = new();

    public MidiHapticsPlayer(IEnumerable<IBodyTracker> trackerList)
    {
        trackers = new List<IBodyTracker>(trackerList);
    }

    public void Load(string midiPath)
    {
        var midiFile = MidiFile.Read(midiPath);

        // PlaybackSettings: real-time playback ensures events fire correctly
        var settings = new PlaybackSettings
        {
           //seRealTime = true
        };

        playback = midiFile.GetPlayback(settings);

        // Subscribe to note events
        playback.NotesPlaybackStarted += OnNoteOn;
        playback.NotesPlaybackFinished += OnNoteOff;

        Console.WriteLine($"Loaded {midiFile.GetNotes().Count()} notes from {midiPath}");
    }

    public void Play()
    {
        if (playback == null)
        {
            Console.WriteLine("Playback not initialized. Call Load() first.");
            return;
        }

        playback.Start();
        Console.WriteLine("Playback started...");
    }

    public void Stop()
    {
        playback?.Stop();
        activeNotes.Clear();
    }

    private void OnNoteOn(object sender, NotesEventArgs e)
    {
        foreach (var note in e.Notes)
        {
            try
            {
                int durationMs = (int)note.LengthAs<MetricTimeSpan>(playback.TempoMap).TotalMilliseconds;
                float intensity = MapNoteToIntensity(note.NoteNumber);

                // Find an available tracker
                var tracker = trackers.FirstOrDefault(t => !trackerEndTimes.ContainsKey(t));

                if (tracker == null)
                {
                    // Voice stealing: pick the tracker whose note ends soonest
                    tracker = trackerEndTimes.OrderBy(kv => kv.Value).First().Key;
                }

                // Assign note to tracker
                activeNotes[(note.Channel, note.NoteNumber)] = tracker;
                trackerEndTimes[tracker] = DateTime.Now.AddMilliseconds(durationMs);

                tracker.EngageHaptics(durationMs, intensity);
            } catch
            {

            }
        }
    }


    private void OnNoteOff(object sender, NotesEventArgs e)
    {
        //foreach (var note in e.Notes)
        //{
        //    var key = (note.Channel, note.NoteNumber);
        //    if (activeNotes.TryGetValue(key, out var tracker))
        //    {
        //        // Stop vibration
        //        tracker.EngageHaptics(0, 0f);
        //        activeNotes.Remove(key);
        //        trackerEndTimes.Remove(tracker);
        //    }
        //}
    }

    private static readonly float[] NoteIntensities = new float[19]
    {
    0.195f,//C
    0.2f, // C#/Db
    0.222f,// D
    0.235f, // D#/Eb 
    0.243f, // E
    0.2607f, // F
    0.2778f, // F#/Gb
    0.2949f, // G
    0.3148f, // G#/Ab
    0.339f, // A
    0.3647f, // A#/Bb 
    0.3989f, //B
    0.443f,  // C
    0.4915f, // C#/Db
    0.5285f,  // D
    0.5783f, // D#/Eb
    0.6496f,  // E
    0.7493f,  // F
    0.8846f, // F#/Gb
    };

    private float MapNoteToIntensity(int noteNumber)
    {
        int noteIndex = (noteNumber - BaseNote) % NoteIntensities.Length;
        if (noteIndex < 0) noteIndex += NoteIntensities.Length; // handle negative notes

        return NoteIntensities[noteIndex];

    }


    public void Dispose()
    {
        playback?.Dispose();
    }
}
