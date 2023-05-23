using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownBorder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var position = other.transform.root.transform.position;
        position.y += 30;
        other.transform.root.transform.position = position;
    }
}
