using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text FPSCounterText;
    
    private void Update()
    {
        FPSCounterText.text = (1 / Time.deltaTime).ToString("F0");
    }
}
