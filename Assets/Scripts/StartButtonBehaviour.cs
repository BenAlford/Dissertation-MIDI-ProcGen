using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonBehaviour : MonoBehaviour
{
    [SerializeField] MIDIfilepath midi;
    [SerializeField] CoinScriptable coinCounter;

    public string filepath;

    public void Pressed()
    {
        // loads the scene and tells it to use the midi file specified by filepath
        midi.path = filepath;
        SceneManager.LoadSceneAsync(1);
        coinCounter.coins = 0;
    }
}
