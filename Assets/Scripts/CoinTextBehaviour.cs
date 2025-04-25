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
        GetComponent<TextMeshProUGUI>().text = "Coins: " + coinCounter.coins.ToString() + "/3";
    }
}
