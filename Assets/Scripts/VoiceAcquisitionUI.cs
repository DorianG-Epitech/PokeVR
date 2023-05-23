using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class VoiceAcquisitionUI : MonoBehaviour
{
    public Sprite mic;
    public Sprite micMuted;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = micMuted;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UnMute()
    {
        if (image && image.sprite)
            image.sprite = mic;
    }

    public void Mute()
    {
        if (image && image.sprite)
            image.sprite = micMuted;
    }
}
