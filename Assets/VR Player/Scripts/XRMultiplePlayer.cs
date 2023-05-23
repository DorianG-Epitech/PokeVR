using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class XRMultiplePlayer : NetworkBehaviour
{
    public MonoBehaviour[] ComponentsToDisable;
    public GameObject[] ObjectsToDisable;
    public GameObject[] ObjectsToRename;
    public GameObject[] PlayerModels;
    public GameObject PlayerCamera;

    private void Start()
    {
        print(gameObject.name);
        if (!isLocalPlayer)
        {
            foreach (var component in ComponentsToDisable)
            {
                if (component != null)
                    DestroyImmediate(component);
            }
            foreach (var obj in ObjectsToDisable)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }
            foreach (var obj in ObjectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            DestroyImmediate(PlayerCamera.GetComponent<AudioListener>());
            DestroyImmediate(PlayerCamera.GetComponent<Camera>());
        }

        if (isLocalPlayer)
        {
            foreach (var obj in PlayerModels)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
