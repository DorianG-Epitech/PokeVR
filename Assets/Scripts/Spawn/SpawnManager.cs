using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    public List<Zone> zones;
    public List<PokemonDataSO> existingPokemons;
    public Pokemon pokemonPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        SpawnPokemons();
    }

    void SpawnPokemons()
    {
        if (!isServer) return;

        foreach (Zone zone in zones) {
            List<PokemonDataSO> pokemonsToSpawnInZone = existingPokemons.FindAll(x => CheckPokemonType(zone.zoneTypes, x.Types) == true);

            if (pokemonsToSpawnInZone.Count == 0) continue;

            zone.InitSpawn();
            //TODO: faudra regarder pokemonsTypes dans chaque zone pour savoir quels pokemons envoyer à une zone
            zone.SpawnPokemons(pokemonsToSpawnInZone);
        }
    }

    bool CheckPokemonType(List<PokemonConstants.Types> zoneTypes, PokemonConstants.Types[] pokemonTypes)
    {
        for (int i = 0; i != pokemonTypes.Length; i++) {
            if (zoneTypes.Contains(pokemonTypes[i])) {
                return true;
            }
        }
        return false;
    }
}
