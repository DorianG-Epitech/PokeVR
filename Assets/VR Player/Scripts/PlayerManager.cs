using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Multiplayer")
            return;
        if (!GameObject.FindObjectOfType<CaptureManager>())
            return;
        gameObject.GetComponent<PlayerBattle>().enabled = true;
        gameObject.GetComponent<PlayerBattle>().InitPokemonsList();
        // List<PokemonData> pokemons = GameObject.FindObjectOfType<CaptureManager>().GetPokemons();
        // for (int i = 0; i < pokemons.Count; i++)
        //     pokemons[i].data = null;
        // gameObject.GetComponent<PlayerBattle>().InitPlayerPokemons(pokemons);
        // gameObject.GetComponent<PlayerBattle>().HidePokemons();
    }
}
