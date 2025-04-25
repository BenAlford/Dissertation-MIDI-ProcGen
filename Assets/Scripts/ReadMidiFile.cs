using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

struct DataTimestamp
{
    public DataTimestamp(int _data, long _timestamp)
    {
        data = _data;
        timestamp = _timestamp;
    }
    public int data;
    public long timestamp;
}

public class ReadMidiFile : MonoBehaviour
{
    //public string filepath;
    byte[] data;
    int datapos = 0;

    List<DataTimestamp> tempos = new List<DataTimestamp>();
    List<DataTimestamp> modes = new List<DataTimestamp>();
    List<List<DataTimestamp>> dynamics = new List<List<DataTimestamp>>();

    List<int> notes = new List<int>();

    long songLength = 0;
    int division = 0;

    float songLengthSeconds = 0;


    int seed = 10000;
    long totalTimestamp = 0;
    [SerializeField] WFC_Overlap proc;
    [SerializeField] MIDIfilepath MIDIfile;

    private void Awake()
    {
        // reads all the data in the midi file
        data = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, MIDIfile.path)); ;

        // checks the first 4 bytes to see if they match the midi file starting bytes
        string checkismidi = ReadNextByte() + ReadNextByte() + ReadNextByte() + ReadNextByte();
        print(checkismidi);
        if (checkismidi != "4d546864")
        {
            print("not midi");
        }
        else
        {
            // retreives the basic info of this midi file
            int length = int.Parse(ReadBytes(4), System.Globalization.NumberStyles.HexNumber);
            print("length: " + length.ToString());

            int miditype = int.Parse(ReadBytes(2), System.Globalization.NumberStyles.HexNumber);
            print("Type " + miditype.ToString() + " midi file");

            int chunks = int.Parse(ReadBytes(2), System.Globalization.NumberStyles.HexNumber);
            print(chunks.ToString() + " track chunks");

            division = int.Parse(ReadBytes(2), System.Globalization.NumberStyles.HexNumber);
            print("division: " + division.ToString());


            // reads each chunk
            for (int i = 0; i < chunks; i++)
            {
                // resets totalTimestamp as every midi chunk starts at time 0
                totalTimestamp = 0;

                // dynamics is assumed to be 100 if not stated otherwise
                dynamics.Add(new List<DataTimestamp>());
                dynamics[dynamics.Count - 1].Add(new DataTimestamp(100, 0));

                // Reads Track Marker
                print (ReadBytes(4));
                int chunklength = int.Parse(ReadBytes(4), System.Globalization.NumberStyles.HexNumber);
                print("chunk length: " + chunklength.ToString());

                // Reads all events in this chunk
                bool chunkEnded = false;
                while (!chunkEnded)
                {
                    chunkEnded = ProcessTrackEvent();
                }
                print("chunk ended");

                // the longest track length is the length of the song
                if (totalTimestamp > songLength)
                {
                    songLength = totalTimestamp;
                }
            }
            MIDIfile.seed = seed;

            // calculates arousal value from this file
            AnalyseMIDIFile();
        }

    }

    bool ProcessTrackEvent()
    {
        // reads the time this event occurs measured in ticks since last event in this chunk
        int timestamp = ProcessVariableLengthValue();
        totalTimestamp += timestamp;

        // if this is the end of the chunk, return to read the next chunk / end
        if (EndOfChunk())
        {
            return true;
        }

        // checks which track event this is: meta, midi or sysex
        string trackEvent = ReadNextByte();
        if (trackEvent == "ff")
        {
            ProcessMetaEvent();
        }
        else if (trackEvent == "f0" || trackEvent == "f7")
        {
            print("sysex event");
        }
        else
        {
            ProcessMidiEvent(trackEvent, timestamp);
        }
        return false;
    }

    void ProcessMidiEvent(string trackEvent, int timestamp)
    {
        // gets the channel this midi event corresponds to
        int channel = Convert.ToInt32(trackEvent.Substring(1), 16) + 1;

        // the first character of the track corresponds to which event this is
        switch (trackEvent[0])
        {

            case '9':
                {
                    // 9 represents when a note starts to be played
                    int note = Convert.ToInt32(ReadNextByte(),16);
                    int velocity = Convert.ToInt32(ReadNextByte(), 16);
                    seed += note;
                    MIDIfile.seed = seed;
                    if (velocity != 0 && channel != 10)
                        notes.Add(note);
                    print("note on channel " + channel.ToString() + " note: " + note.ToString() + " vel: " + velocity.ToString());
                    break;
                }
            case '8':
                {
                    // 8 represents when a note stops being played
                    int note = Convert.ToInt32(ReadNextByte(), 16);
                    int velocity = Convert.ToInt32(ReadNextByte(), 16);
                    print("note off channel " + channel.ToString() + " note: " + note.ToString() + " vel: " + velocity.ToString());
                    break;
                }
            case 'a':
                {
                    // a represents the note velocity changes (the note gets softer / harder)
                    int note = Convert.ToInt32(ReadNextByte(), 16);
                    int velocity = Convert.ToInt32(ReadNextByte(), 16);
                    print("vel change channel " + channel.ToString() + " note: " + note.ToString() + " vel: " + velocity.ToString());
                    break;
                }
            case 'b':
                {
                    // b represents a controller change, these can do a lot of things, the only one we care
                    // about is channel 7 for dynamics change
                    int controllerNumber = Convert.ToInt32(ReadNextByte(), 16);
                    int value = Convert.ToInt32(ReadNextByte(), 16);
                    print("controller change");
                    if (controllerNumber == 7)
                    {
                        print("dynamics: " + value.ToString());
                        dynamics[dynamics.Count - 1].Add(new DataTimestamp(value, totalTimestamp));
                    }
                    break;
                }
            case 'c':
                {
                    // c represents a channel changing its program
                    int programNumber = Convert.ToInt32(ReadNextByte(), 16);
                    print("channel: " + channel.ToString() + " program changed to " + programNumber.ToString());
                    break;
                }
            case 'd':
                {
                    // d represents every velocity changes
                    int velocity = Convert.ToInt32(ReadNextByte(), 16);
                    print("vel change all channel " + channel.ToString() + " vel: " + velocity.ToString());
                    break;
                }
            case 'e':
                {
                    // e represents the pitch bending (going slightly up or down in pitch)
                    ReadBytes(2);
                    print("pitch bend");
                    break;
                }
            default:
                print("error unexpected midi event");
                print(trackEvent);
                ReadBytes(2);
                break;
        }
    }

    void ProcessMetaEvent()
    {
        // gets the type of meta event this is
        string metaEvent = ReadNextByte();
        int length = ProcessVariableLengthValue();

        switch (metaEvent)
        {
            // track name
            case "03":
                string name = ReadString(length);
                print(name);
                break;

            // things I don't care about
            case "02":
            case "04":
            case "05":
            case "06":
            case "07":
            case "01":
            case "08":
            case "09":
                ReadString(length);
                print("unimportant data");
                break;

            // tempo
            case "51":
                int tempo = 60000000 / Convert.ToInt32(ReadBytes(3), 16);
                print(tempo.ToString() + "bpm");
                tempos.Add(new DataTimestamp(tempo, totalTimestamp));
                break;

            // time signiture
            case "58":
                int numerator = Convert.ToInt32(ReadNextByte(), 16);
                int denominator = Convert.ToInt32(ReadNextByte(), 16);
                denominator *= denominator;
                print(numerator.ToString() + denominator.ToString() + " time signiture");

                //ignore the rest
                ReadBytes(2);
                break;
            
            // key signiture
            case "59":
                int keysig = Convert.ToInt32(ReadNextByte(), 16);
                bool major = ReadNextByte() == "1";
                print(keysig.ToString() + major.ToString());
                if (major)
                    modes.Add(new DataTimestamp(1, totalTimestamp));
                else
                    modes.Add(new DataTimestamp(-1, totalTimestamp));

                break;

            default:
                ReadBytes(length);
                print("unknown event");
                print(metaEvent);
                break;
        }
    }

    string ReadString(int length)
    {
        // reads a string
        string str = "";
        for (int i = 0; i < length; i++)
        {
            str += (char)Convert.ToInt32(ReadNextByte(), 16);
        }
        return str;
    }

    int ProcessVariableLengthValue()
    {
        // in midi files, variable length values ignore the MSB and instead use it to signal if the next byte is part of
        // this value as well (1 = not finished, 0 = last byte in this value)
        string total = "";
        char msb = '1';
        while (msb == '1')
        {
            string val = Convert.ToString(data[datapos++], 2).PadLeft(8, '0');
            msb = val[0];
            val = val.Substring(1);
            total += val;
        }
        return Convert.ToInt32(total,2);
    }

    string ReadNextByte()
    {
        return data[datapos++].ToString("x2");
    }

    string ReadBytes(int length)
    {
        string val = "";
        for (int i = 0; i < length; i++)
        {
            val += data[datapos++].ToString("x2");
        }
        return val;
    }

    bool EndOfChunk()
    {
        // checks if the next bytes signal the end of a chunk
        string val = data[datapos].ToString("x2") + data[datapos + 1].ToString("x2") + data[datapos + 2].ToString("x2");
        if (val == "ff2f00")
        {
            datapos += 3;
            return true;
        }
        return false;
    }

    void AnalyseMIDIFile()
    {

        // analyses tempo for total song length and average bpm
        long sectionStart = 0;
        float totalLengthTime = 0;
        float tempoCountTotal = 0;

        // calculates the average tempo, weighted by how long each bpm lasts for
        for (int i = 0; i < tempos.Count - 1; i++)
        {
            // calculates section length
            long sectionLength = tempos[i + 1].timestamp - sectionStart;
            int sectionTempo = tempos[i].data;

            // adds the section length to an ongoing total
            totalLengthTime += ((sectionLength / division) / (sectionTempo / 60f));

            // sets the next section to start where this one ends
            sectionStart += sectionLength;

            // stores the total of the section length * the section tempo for calculating weighted average
            tempoCountTotal += sectionLength * sectionTempo;
        }

        // repeats this for the last section
        long sectionLengthLast = songLength - sectionStart;
        int sectionTempoLast = tempos[tempos.Count - 1].data;

        totalLengthTime += ((sectionLengthLast / division) / (sectionTempoLast / 60f));

        tempoCountTotal += sectionLengthLast * sectionTempoLast;

        // calculatess the average tempo in bpm
        float averageTempo = tempoCountTotal / songLength;

        // uses S shaped curve to calculate the tempo arousal val
        float tempoArousalVal = 1 + 8 * (1 / (1 + Mathf.Exp(-0.06f * (averageTempo - 114f))));
        print(tempoArousalVal);

        // calculates notes per second
        songLengthSeconds = totalLengthTime;
        float notesPerSecond = notes.Count / songLengthSeconds;


        // analyses pitch to calculate the average pitch of the song
        float totalNote = 0;
        for (int i = 0; i < notes.Count; i++)
        {
            totalNote += notes[i];
        }

        float averagePitch = totalNote / notes.Count;

        // S shaped curve
        float pitchArousalVal = 1 + 8 * (1 / (1 + Mathf.Exp(-0.06f * (averagePitch - 63.5f))));

        print(pitchArousalVal);

        // repeats the same process for dynamics as with tempo, but calculates the average of each chunk
        // and then calculates the average of those averages
        List<float> averageChunkDynamics = new List<float>();

        for (int i = 0; i < dynamics.Count; i++)
        {
            int dynamicsSectionStart = 0;
            float dynamicsCountTotal = 0;
            for (int j = 0; j < dynamics[i].Count - 1; j++)
            {
                long dynamicsSectionLength = dynamics[i][j + 1].timestamp - dynamicsSectionStart;
                int dynamicSectionVolume = dynamics[i][j].data;
                print(dynamics[i][j].data);

                dynamicsCountTotal += dynamicsSectionLength * dynamicSectionVolume;
            }

            long dynamicsSectionLengthLast = songLength - dynamicsSectionStart;
            int dynamicsSectionVolumeLast = dynamics[i][dynamics[i].Count - 1].data;

            print(dynamicsSectionVolumeLast);

            dynamicsCountTotal += dynamicsSectionLengthLast * dynamicsSectionVolumeLast;

            float thisAverageDynamics = dynamicsCountTotal / songLength;
            averageChunkDynamics.Add(thisAverageDynamics);
        }

        float dynamicsChunkTotal = 0;
        for (int i = 0; i < averageChunkDynamics.Count; i++)
        {
            dynamicsChunkTotal += averageChunkDynamics[i];
        }

        float averageDynamics = dynamicsChunkTotal / averageChunkDynamics.Count;

        // S shaped curve
        float dynamicsArousalVal = 1 + 8 * (1 / (1 + Mathf.Exp(-0.06f * (averageDynamics - 63.5f))));

        print(dynamicsArousalVal);

        // calculates the weighted average for the piece's arousal val based on which factors are more important
        float finalArousalVal = ((tempoArousalVal * 2f) + (dynamicsArousalVal * 1.5f) + (pitchArousalVal)) / 4.5f;

        print(finalArousalVal);

        MIDIfile.arousal = finalArousalVal;
    }
}
