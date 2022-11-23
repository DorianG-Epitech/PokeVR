using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnZone : NetworkBehaviour
{
    public List<PokemonConstants.Types> zoneTypes;
    public Terrain terrain;
    public List<PokemonDataSO> zonePokemons;

    private BoxCollider spawnZoneCollider;
    private int nbPokemons;
    
    public void InitSpawn()
    {
        spawnZoneCollider = GetComponent<BoxCollider>();

        nbPokemons = (int)((transform.localScale.x * transform.localScale.z) / 500);
        Debug.Log(transform.name + ", nb pokemons: " + nbPokemons);
    }

    public void SpawnPokemons(List<PokemonDataSO> zonePokemons, Pokemon pokemonLoader)
    {
        this.zonePokemons = zonePokemons;
        for (int i = 0; i != nbPokemons; i++) {
            Vector3 pos = new Vector3(
                Random.Range(spawnZoneCollider.bounds.min.x, spawnZoneCollider.bounds.max.x),
                Random.Range(spawnZoneCollider.bounds.min.y, spawnZoneCollider.bounds.max.y),
                Random.Range(spawnZoneCollider.bounds.min.z, spawnZoneCollider.bounds.max.z)
            );
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 0.5f;
            SpawnPokemon(zonePokemons[Random.Range(0, zonePokemons.Count)], pokemonLoader, pos);
        }
    }

    [Server]
    void SpawnPokemon(PokemonDataSO pokemon, Pokemon pokemonLoader, Vector3 position)
    {
        Pokemon newPokemonObject = Instantiate(pokemonLoader, position, Quaternion.identity);

        newPokemonObject.Data = pokemon;
        newPokemonObject.Id = pokemon.Id;
        newPokemonObject.transform.name = pokemon.Name;

        NetworkServer.Spawn(newPokemonObject.gameObject);
    }
}
