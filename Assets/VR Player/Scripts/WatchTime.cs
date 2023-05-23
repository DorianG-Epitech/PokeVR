using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WatchTime : MonoBehaviour
{
    private TextMeshProUGUI _text;
    
    // Start is called before the first frame update
    private void Start() 
    {   
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetTime(float time)
    {
        if (_text)
            _text.text =  "Time left :" + '\n' + time.ToString("0.00");
    }
}
