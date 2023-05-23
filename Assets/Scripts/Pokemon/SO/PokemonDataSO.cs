using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Data/PokemonData")]
public class PokemonDataSO : ScriptableObject
{
    [Header("Pokemon")]
    [Tooltip("The identifier for this pokemon.")]
    public int Id = 0;
    [Tooltip("The name for this pokemon.")]
    public string Name = "";
    [Tooltip("The elemental types of this pokemon.")]
    public PokemonConstants.Types[] Types;
    [Tooltip("A list of abilities this Pok�mon could potentially have.")]
    public AbilityDataSO[] Abilities;
    [Tooltip("A list of base stat values for this Pok�mon.")]
    public PokemonConstants.Stat[] Stats;
    [Tooltip("A list of available moves.")]
    public MoveDataSO[] Moves;
    [Tooltip("The pokemon capture rate.")]
    public int CaptureRate;
    [Tooltip("The pokemon model.")]
    public GameObject Model;
    [Tooltip("The pokemon sprite.")]
    public Sprite Sprite;
    [Tooltip("Is the pokemon model flying.")]
    public bool IsFlying = false;
}
