using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomArousalValue : MonoBehaviour
{
    [SerializeField] Slider slider;

    private void Update()
    {
        // updates the text display to the slider value
        float val = slider.value;
        val = Mathf.Round(val*100) / 100;
        GetComponent<TextMeshProUGUI>().text = val.ToString();
    }
}
