using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinTextBehaviour : MonoBehaviour
{
    [SerializeField] CoinScriptable coinCounter;

    // Update is called once per frame
    void Update()
    {
        // displays the coin number
        GetComponent<TextMeshProUGUI>().text = "Coins: " + coinCounter.coins.ToString() + "/3";
    }
}
