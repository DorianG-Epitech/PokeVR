using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokeballBelt : MonoBehaviour
{
    public List<Pokemon> pkmnTeam;
    public List<PokeballSlot> pokeballSlots;
    
    public void AddPokemonToBelt(PokemonData pokemonData)
    {
        var availableSlot = pokeballSlots.Find(p => !p.pokeball);
        if (availableSlot != null)
            availableSlot.AddPokeballToSlot(pokemonData);
    }
}
