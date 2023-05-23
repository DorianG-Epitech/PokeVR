using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PokeballManager : NetworkBehaviour
{
    public static PokeballManager Instance;
    public GameObject PokeballPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void SpawnPokeball(NetworkIdentity owner, Vector3 position)
    {
        SpawnPokeballServer(owner, position);
    }

    [Command]
    void SpawnPokeballServer(NetworkIdentity owner, Vector3 position)
    {
        Debug.Log("SpawnPokeballServer");
        GameObject pokeball = Instantiate(PokeballPrefab, position, Quaternion.identity);
        // pokeball.GetComponent<Pokeball>().ownerIdentity = owner;
        NetworkServer.Spawn(pokeball);
    }

    [Command]
    public void SpawnTestCube()
    {
        Debug.Log("SpawnTestCube");
        SpawnCube();
    }

    [ClientRpc]
    public void SpawnCube()
    {
        Debug.Log("SpawnTestCube 2");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(10, 10, 10);
        NetworkServer.Spawn(cube);
    }

}
