using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IButton : MonoBehaviour
{
    public virtual void OnClick()
    {
        SoundManager.PlaySound(SoundManager.Sound.ButtonClick);
    }
}
