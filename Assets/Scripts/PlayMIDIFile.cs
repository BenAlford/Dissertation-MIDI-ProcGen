using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayMIDIFile : MonoBehaviour
{
    MidiFile midiFile = new MidiFile();

    private const string OutputDeviceName = "Microsoft GS Wavetable Synth";

    private OutputDevice _outputDevice;
    private Playback _playback;

    [SerializeField] MIDIfilepath _midifilepath;

    bool started = false;

    public bool play = true; 

    private void Start()
    {

    }

    private void Update()
    {

    }

    public void StartMusic()
    {
        if (!started && play)
        {
            midiFile = MidiFile.Read(Path.Combine(Application.streamingAssetsPath, _midifilepath.path));

            _outputDevice = OutputDevice.GetByName(OutputDeviceName);

            _playback = midiFile.GetPlayback(_outputDevice);
            _playback.Loop = true;
            _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
            _playback.NotesPlaybackFinished += OnNotesPlaybackFinished;

            _playback.Start();
            started = true;
        }
    }

    private void OnApplicationQuit()
    {
        Stop();
    }

    public void Stop()
    {
        Debug.Log("Releasing playback and device...");

        if (_playback != null)
        {
            _playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            _playback.NotesPlaybackFinished -= OnNotesPlaybackFinished;
            _playback.Dispose();
        }

        if (_outputDevice != null)
            _outputDevice.Dispose();
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        LogNotes("Notes finished:", e);
    }

    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        LogNotes("Notes started:", e);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        //var message = new StringBuilder()
        //    .AppendLine(title)
        //    .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
        //    .ToString();
        //Debug.Log(message.Trim());
    }
}
