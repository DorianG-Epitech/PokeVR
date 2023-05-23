using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class CaptureManager : NetworkBehaviour
{
    [SyncVar]
    public float timePassed;
    [SerializeField]
    float captureTime;
    public readonly SyncDictionary<string, List<PokemonData>> _capturedPokemon = new SyncDictionary<string, List<PokemonData>>();

    public void AddPokemon(string ownerId, PokemonData pokemonData, Pokemon pokemon)
    {
        Debug.Log("AddPokemon: " + ownerId);
        if (!_capturedPokemon.ContainsKey(GameManager.instance.PlayerId))
        {
            _capturedPokemon.Add(GameManager.instance.PlayerId, new List<PokemonData>());
        }
        _capturedPokemon[GameManager.instance.PlayerId].Add(pokemonData);
        foreach (PokeballBelt belt in GameObject.FindObjectsOfType<PokeballBelt>())
        {
            if (belt.GetComponentInParent<NetworkIdentity>().netId.ToString() == ownerId)
            {
                belt.AddPokemonToBelt(pokemonData);
            }
        }
    }

    public List<PokemonData> GetPokemons()
    {
        if (!_capturedPokemon.ContainsKey(GameManager.instance.PlayerId))
        {
            return null;
        }
        return _capturedPokemon[GameManager.instance.PlayerId];
    }

    public void DisplayDict()
    {
        foreach (var player in _capturedPokemon)
        {
            Debug.Log(player.Key);
            foreach (var pokemon in player.Value)
            {
                Debug.Log(pokemon.data.name);
            }
        }
    }

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Multiplayer")
            return;
        timePassed += Time.deltaTime;

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            NetworkManager.singleton.ServerChangeScene("Multiplayer");
        }

        foreach (WatchTime watchTime in GameObject.FindObjectsOfType<WatchTime>())
        {
            watchTime.SetTime(GetTimeRemaining());
        }

        if (timePassed > captureTime)
        {
            NetworkManager.singleton.ServerChangeScene("Multiplayer");
        }
    }

    private bool CheckIfFinished()
    {
        foreach (var player in _capturedPokemon)
        {
            if (player.Value.Count < 6)
            {
                return false;
            }
        }
        return true;
    }

    public float GetTimeRemaining()
    {
        return (captureTime - timePassed);
    }
}
