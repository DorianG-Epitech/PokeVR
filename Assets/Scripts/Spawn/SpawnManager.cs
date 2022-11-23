using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    public List<SpawnZone> spawnZones;
    public List<GameObject> objects;
    public List<PokemonDataSO> existingPokemons;
    public Pokemon pokemonLoader;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        Invoke("SpawnPokemons", 5f);
    }

    void SpawnPokemons()
    {
        if (!isServer) return;

        foreach (SpawnZone zone in spawnZones) {
            List<PokemonDataSO> zonePokemons = existingPokemons.FindAll(x => CheckPokemonType(zone.zoneTypes, x.Types) == true);

            if (zonePokemons.Count == 0) continue;

            zone.InitSpawn();
            //TODO: faudra regarder pokemonsTypes dans chaque zone pour savoir quels pokemons envoyer à une zone
            zone.SpawnPokemons(zonePokemons, pokemonLoader);
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
