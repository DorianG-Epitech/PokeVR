using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    CaptureManager captureManager;
    TextMeshProUGUI timerText;
    float timeRemaining = 0f;
    string minutes;
    string seconds;

    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (captureManager == null) {
            captureManager = GameObject.FindObjectOfType<CaptureManager>();
            if (captureManager == null) {
                return;
            }
        }
        timeRemaining = captureManager.GetTimeRemaining();
        minutes = Mathf.Floor(timeRemaining / 60).ToString("00");
        seconds = Mathf.RoundToInt(timeRemaining % 60).ToString("00");
        timerText.SetText("Time left:<br>"+ minutes + ":" + seconds);
    }
}
