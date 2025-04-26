using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomArousalButton : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] MIDIfilepath midi;
    [SerializeField] CoinScriptable coinCounter;

    public void Pressed()
    {
        // loads the scene without music with the specified arousal value and random seed
        midi.arousal = slider.value;
        midi.seed = Random.Range(10000, 1000000);
        SceneManager.LoadSceneAsync(3);
        coinCounter.coins = 0;
    }
}
