using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Zone : MonoBehaviour
{
    public List<PokemonConstants.Types> zoneTypes;
    public Terrain terrain;
    public List<PokemonDataSO> zonePokemons;

    private BoxCollider zoneCollider;
    public int zoneDivider = 500;
    private int nbPokemons;
    
    public void InitSpawn()
    {
        zoneCollider = GetComponent<BoxCollider>();

        nbPokemons = (int)((transform.localScale.x * transform.localScale.z) / zoneDivider);
    }

    public void SpawnPokemons(List<PokemonDataSO> zonePokemons)
    {
        this.zonePokemons = zonePokemons;
        for (int i = 0; i != nbPokemons; i++) {
            Vector3 pos = new Vector3(
                Random.Range(zoneCollider.bounds.min.x, zoneCollider.bounds.max.x),
                Random.Range(zoneCollider.bounds.min.y, zoneCollider.bounds.max.y),
                Random.Range(zoneCollider.bounds.min.z, zoneCollider.bounds.max.z)
            );
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 0.5f;
            SpawnPokemon(zonePokemons[Random.Range(0, zonePokemons.Count)], pos);
        }
    }

    [Server]
    void SpawnPokemon(PokemonDataSO pokemon, Vector3 position)
    {
        Pokemon newPokemonObject = Instantiate(SpawnManager.Instance.pokemonPrefab, position, Quaternion.identity);

        newPokemonObject.InitSpawn(pokemon, zoneCollider);

        NetworkServer.Spawn(newPokemonObject.gameObject);
    }
}
