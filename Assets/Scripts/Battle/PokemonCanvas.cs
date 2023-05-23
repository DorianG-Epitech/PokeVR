using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PokemonCanvas : MonoBehaviour
{
    [System.Serializable]
    public struct PokemonDisplay {
        public TextMeshPro pokemonName;
        public TextMeshPro pokemonHP;
        public TextMeshPro pokemonStatus;
        public List<TextMeshPro> attacksDisplay;
        public TextMeshPro remainingPokemons;
    };
    public PokemonDisplay playerDisplay;
    public PokemonDisplay otherDisplay;

    public void SetDisplay(Pokemon pokemon, bool isPlayerPokemon, int remainingPokemons = 0)
    {
        if (isPlayerPokemon) {
            SetDisplayValues(pokemon, playerDisplay, isPlayerPokemon, remainingPokemons);
        } else {
            SetDisplayValues(pokemon, otherDisplay, isPlayerPokemon, remainingPokemons);
        }
    }

    private void SetDisplayValues(Pokemon pokemon, PokemonDisplay display, bool isPlayerPokemon, int remainingPokemons)
    {
        display.pokemonName.SetText(pokemon.name);
        display.pokemonHP.SetText("" + pokemon.hp);
        display.pokemonStatus.SetText("" + pokemon.ailment);

        if (isPlayerPokemon) {
            for (int i = 0; i!= pokemon.moves.Count; i++) {
                display.attacksDisplay[i].SetText(pokemon.moves[i].Name);
            }
        }
        display.remainingPokemons.SetText("" + remainingPokemons);

    }
}
