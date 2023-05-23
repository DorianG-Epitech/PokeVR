using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;
using Mirror;

public class PokeballSpawner : NetworkBehaviour
{
    public NetworkIdentity ownerIdentity;
    public GameObject pokeballPrefab;
    public Collider GrabPoint;

    [Command]
    private void SpawnPokeball(Vector3 position, uint ownerId, GameObject owner)
    {
        GameObject pokeball = Instantiate(pokeballPrefab, position, transform.rotation);
        pokeball.GetComponent<Pokeball>().ownerId = ownerIdentity.netId.ToString();
        Debug.LogError(gameObject.name + ": " + pokeball.GetComponent<Pokeball>().ownerId);
        NetworkServer.Spawn(pokeball, owner);
    }

    private void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && OVRInput.GetDown(OVRInput.RawButton.A))
        {
            if (isLocalPlayer)
            {
                SpawnPokeball(GrabPoint.transform.position, ownerIdentity.netId, gameObject);
            }
        }
    }
}
