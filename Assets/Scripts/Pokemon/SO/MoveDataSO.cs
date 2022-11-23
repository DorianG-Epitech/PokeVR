using UnityEngine;

[CreateAssetMenu(menuName ="Data/MoveData")]
public class MoveDataSO : ScriptableObject
{
    public enum MoveCategory
    {
        unique,
        damage,
        ailment,
        netGoodStats,
        heal,
        damageAilment,
        swagger,
        damageLower,
        damageRaise,
        damageHeal,
        ohko,
        wholeFieldEffect,
        fieldEffect,
        forceSwitch
    }

    public enum MoveDamageClass
    {
        status,
        physical,
        special
    }

    public enum MoveAilment
    {
        unknown,
        none,
        paralysis,
        sleep,
        freeze,
        burn,
        poison,
        confusion, 
        infatuation, 
        trap,
        nightmare,
        torment,
        disable,
        yawn,
        healBlock,
        noTypeImmunity,
        leechSeed,
        embargo,
        perishSong,
        Ingrain
    }

    public enum MoveTarget
    {
        specificMove,
        selectedPokemonMeFirst,
        ally,
        usersField,
        userOrAlly,
        opponentsField,
        user,
        randomOpponent,
        allOtherPokemon,
        selectedPokemon,
        allOpponents,
        entireField,
        userAndAllies,
        allPokemon,
        allAllies
    }

    [Header("Move")]
    [Tooltip("The identifier for this move.")]
    public int Id = 0;
    [Tooltip("The name for this move.")]
    public string Name = "";
    [Tooltip("The description for this move.")]
    public string Description = "";
    [Tooltip("The percent value of how likely this move is to be successful.")]
    public int Accuracy = 0;
    [Tooltip("The percent value of how likely it is this moves effect will happen.")]
    public int EffectChance = 0;
    [Tooltip("Power points. The number of times this move can be used.")]
    public int PP = 0;
    [Tooltip("A value between - 8 and 8.Sets the order in which moves are executed during battle.")]
    public int Priority = 0;
    [Tooltip("The base power of this move with a value of 0 if it does not have a base power.")]
    public int Power = 0;
    [Tooltip("The type of damage the move inflicts on the target, e.g.physical.")]
    public MoveDamageClass DamageClass = MoveDamageClass.physical;
    [Tooltip("The type of target that will receive the effects of the attack.")]
    public MoveTarget Target = MoveTarget.selectedPokemon;
    [Tooltip("The elemental type of this move.")]
    public PokemonConstants.Types Type = PokemonConstants.Types.unknown;
    
    [Header("MetaData")]
    [Tooltip("The status ailment this move inflicts on its target.")]
    public MoveAilment Ailment = MoveAilment.none;
    [Tooltip("The category of move this move falls under, e.g.damage or ailment.")]
    public MoveCategory Category = MoveCategory.unique;
    [Tooltip("The minimum number of times this move hits.Null if it always only hits once.")]
    public int MinHits = 0;
    [Tooltip("The maximum number of times this move hits. Null if it always only hits once.")]
    public int MaxHits = 0;
    [Tooltip("The minimum number of turns this move continues to take effect. Null if it always only lasts one turn.")]
    public int MinTurns = 1;
    [Tooltip("The maximum number of turns this move continues to take effect.Null if it always only lasts one turn.")]
    public int MaxTurns = 1;
    [Tooltip("HP drain(if positive) or Recoil damage(if negative), in percent of damage done.")]
    public int Drain = 0;
    [Tooltip("The amount of hp gained by the attacking Pokemon, in percent of it's maximum HP.")]
    public int Healing = 0;
    [Tooltip("Critical hit rate bonus.")]
    public int CritRate = 0;
    [Tooltip("The likelihood this attack will cause an ailment.")]
    public int AilmentChance = 0;
    [Tooltip("The likelihood this attack will cause the target Pokémon to flinch.")]
    public int FlinchChance = 0;
    [Tooltip("The likelihood this attack will cause a stat change in the target Pokémon.")]
    public int StatChance = 0;

    [Header("StatChange")]
    [Tooltip("A list of stats this moves effects and how much it effects them.")]
    public PokemonConstants.Stat[] StatChanges;
}
