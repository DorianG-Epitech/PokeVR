using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : NetworkBehaviour
{
    [SyncVar]
    private bool _waitingPhase = true;

    [SyncVar]
    public bool _selectPhase = false;

    [SyncVar]
    public bool _animationPhase = false;

    [SyncVar]
    public bool _endPhase = false;

    [SyncVar]
    public bool _waitSwitch = false;

    [SyncVar]
    public bool isEnd = false;

    [SyncVar]
    public PlayerBattle _currentAction;

    [SyncVar]
    public float _timeAction;

    [SyncVar]
    public List<PlayerBattle> players;

    [SyncVar]
    public List<PlayerBattle> priorityList;

    public int[] critRateTable = new int[] { 417, 1250, 5000, 10000 };
    public Dictionary<PokemonConstants.Types, Dictionary<PokemonConstants.Types, float>> typesTable;

    public override void OnStartServer()
    {
        _waitingPhase = true;
        _selectPhase = false;
        _animationPhase = false;
        _endPhase = false;
        isEnd = false;
        _currentAction = null;
        _timeAction = 0;
        InitTableTypes();
    }

    void Update()
    {
        if (!isServer) { return; }
        if (isEnd) { return; }
        if (_waitingPhase) {
            if (players.Count == 2) {
                _waitingPhase = false;
                _selectPhase = true;
            }
        } else if (_selectPhase && !_waitSwitch) {
            SelectPhase();
        } else if (!_selectPhase && !_animationPhase && !_endPhase && !_waitSwitch) {
            SetPriorityList();
        } else if (_animationPhase && !_waitSwitch) {
            ExecuteAction();
        } else if (_endPhase) {
            ExecuteEndPhase();
        } else if (_waitSwitch) {
            foreach (PlayerBattle p in players) {
                if (p.pokemonAvailable <= 0) {
                    p.endState = PlayerBattle.EndState.Defeat;
                    getOpponent(p).endState = PlayerBattle.EndState.Victory;
                    isEnd = true;
                }
                if (p.waitPokemon) return;
            }
            _waitSwitch = false;
        }
        _timeAction -= Time.deltaTime;
    }

    public void Ready(PlayerBattle player)
    {
        if (!players.Contains(player)) {
            players.Add(player);
        }
    }

    public PlayerBattle getOpponent(PlayerBattle me)
    {
        foreach (PlayerBattle p in players) {
            if (p != me && p != null) {
                return p;
            }
        }

        return null;
    }

    public void SelectPhase()
    {
        bool endSelectPhase = false;
        int count = 0;
        foreach (PlayerBattle p in players) {
            if (p == null) {
                players.RemoveAt(count);
                break;
            } else {
                if (p.action != null) {
                    endSelectPhase = true;
                } else {
                    endSelectPhase = false;
                    break;
                }
            }
            count++;
        }
        if (endSelectPhase) {
            _selectPhase = false;
        }
    }

    public void SetPriorityList()
    {
        priorityList = new List<PlayerBattle>();
        PlayerBattle playerOne = players[0];
        PlayerBattle playerTwo = players[1];

        if (playerOne.action.action == BattleAction.ActionType.RETRIEVE || playerOne.action.action == BattleAction.ActionType.ITEM) {
            priorityList.Add(playerOne);
            priorityList.Add(playerTwo);
        } else if (playerTwo.action.action == BattleAction.ActionType.RETRIEVE || playerOne.action.action == BattleAction.ActionType.ITEM) {
            priorityList.Add(playerTwo);
            priorityList.Add(playerOne);
        } else if (playerOne.action.attack.Priority > playerTwo.action.attack.Priority) {
            priorityList.Add(playerOne);
            priorityList.Add(playerTwo);
        } else if (playerOne.action.attack.Priority < playerTwo.action.attack.Priority) {
            priorityList.Add(playerTwo);
            priorityList.Add(playerOne);
        } else if (playerOne.pokemons[playerOne.pokemonChoosed].bSpeed >= playerTwo.pokemons[playerTwo.pokemonChoosed].bSpeed) {
            priorityList.Add(playerOne);
            priorityList.Add(playerTwo);
        } else {
            priorityList.Add(playerTwo);
            priorityList.Add(playerOne);
        }
        _animationPhase = true;
    }

    public void ExecuteAction()
    {
        if (_timeAction <= 0) {
            if (priorityList.Count == 0) {
                _animationPhase = false;
                _endPhase = true;
            } else {
                _currentAction = priorityList[0];
                if (_currentAction.action.action == BattleAction.ActionType.RETRIEVE) {
                    CmdEnableDisablePokemon(_currentAction);
                    _currentAction.waitPokemon = true;
                    _waitSwitch = true;
                } else if (_currentAction.action.action == BattleAction.ActionType.ATTACK)
                    ExecuteAttack(_currentAction.pokemons[_currentAction.pokemonChoosed], getOpponent(_currentAction).pokemons[getOpponent(_currentAction).pokemonChoosed], _currentAction.action.attack);

                if (_currentAction.pokemons[_currentAction.pokemonChoosed].bHp <= 0) {
                    CmdEnableDisablePokemon(_currentAction);
                    _currentAction.waitPokemon = true;
                    _currentAction.pokemonAvailable -= 1;
                    _waitSwitch = true;
                    priorityList.Clear();
                } else if (getOpponent(_currentAction).pokemons[getOpponent(_currentAction).pokemonChoosed].bHp <= 0) {
                    CmdEnableDisablePokemon(getOpponent(_currentAction));
                    getOpponent(_currentAction).waitPokemon = true;
                    getOpponent(_currentAction).pokemonAvailable -= 1;
                    _waitSwitch = true;
                    priorityList.Clear();
                }
                _currentAction.tempShow = false;
                _currentAction.action = null;
                _timeAction = 5.0f;
                if (priorityList.Count > 0)
                    priorityList.RemoveAt(0);
            }
        }        
    }

    [Server]
    private void CmdEnableDisablePokemon(PlayerBattle actual)
    {
        actual.pokemons[actual.pokemonChoosed].transform.position = Vector3.up * 10;
    }


    public void ExecuteEndPhase()
    {
        Debug.Log("END PHASE");
        foreach (PlayerBattle p in players) {
            Pokemon actual = p.pokemons[p.pokemonChoosed];
            if (actual.ailment == MoveDataSO.MoveAilment.burn)
                actual.bHp -= (actual.hp / 16);
            else if (actual.ailment == MoveDataSO.MoveAilment.freeze && Random.Range(0, 100) <= 20) {
                actual.ailment = MoveDataSO.MoveAilment.none;
                actual.roundSinceAilment = 0;
            } else if (actual.ailment == MoveDataSO.MoveAilment.burn)
                actual.bHp -= (actual.hp / 8);
            else if (actual.ailment == MoveDataSO.MoveAilment.sleep && (Random.Range(0, 3) == 0 || actual.roundSinceAilment >= 3)) {
                actual.ailment = MoveDataSO.MoveAilment.none;
                actual.roundSinceAilment = 0;
            } 
            if (actual.ailment != MoveDataSO.MoveAilment.none)
                actual.roundSinceAilment += 1;
            if (actual.bHp <= 0) {
                CmdEnableDisablePokemon(_currentAction);
                p.waitPokemon = true;
                _waitSwitch = true;
                p.pokemonAvailable -= 1;
            }
        }

        _endPhase = false;
        _selectPhase = true;
    }

    public void ExecuteAttack(Pokemon me, Pokemon opponent, MoveDataSO attack)
    {
        List<Pokemon> defenders = new List<Pokemon>();

        Debug.Log(attack.Name);

        // Get Target
        if (attack.Target == MoveDataSO.MoveTarget.allAllies || attack.Target == MoveDataSO.MoveTarget.allPokemon || attack.Target == MoveDataSO.MoveTarget.ally || attack.Target == MoveDataSO.MoveTarget.user || attack.Target == MoveDataSO.MoveTarget.userAndAllies || attack.Target == MoveDataSO.MoveTarget.userOrAlly)
            defenders.Add(me);
        if (attack.Target == MoveDataSO.MoveTarget.allOpponents || attack.Target == MoveDataSO.MoveTarget.allOtherPokemon || attack.Target == MoveDataSO.MoveTarget.allPokemon || attack.Target == MoveDataSO.MoveTarget.randomOpponent || attack.Target == MoveDataSO.MoveTarget.selectedPokemon)
            defenders.Add(opponent);

        // Don't Attack if status
        if (me.ailment == MoveDataSO.MoveAilment.freeze || (me.ailment == MoveDataSO.MoveAilment.paralysis && Random.Range(0, 4) == 0) || me.ailment == MoveDataSO.MoveAilment.sleep)
            return;

        foreach (Pokemon p in defenders)
        {
            if (attack.Category == MoveDataSO.MoveCategory.damage)
            {
                int damage =  GetDamage(me, p, attack);
                p.bHp -= damage;
                me.bHp += GetHealRecoilDamage(damage, attack);
            } else if (attack.Category == MoveDataSO.MoveCategory.ailment)
                ApplyAilment(p, attack);
            else if (attack.Category == MoveDataSO.MoveCategory.damageAilment) {
                int damage = GetDamage(me, p, attack);
                p.bHp -= damage;
                me.bHp += GetHealRecoilDamage(damage, attack);
                ApplyAilment(p, attack);
            }
            
            // unfroze if fire type attack
            if (attack.Type == PokemonConstants.Types.fire && p.ailment == MoveDataSO.MoveAilment.freeze) {
                p.ailment = MoveDataSO.MoveAilment.none;
                p.roundSinceAilment = 0;
            }
        }
        
    }

    public int GetHealRecoilDamage(int damage, MoveDataSO attack)
    {   
        return (damage * attack.Drain) / 100;
    }

    public int GetDamage(Pokemon attacker, Pokemon defender, MoveDataSO attack)
    {
        float damage = 0;
        int att = 1;
        int def = 1;
        float cm = 1;

        // STAB (*1.5)
        foreach (PokemonConstants.Types type in attacker.data.Types) {
            if (type == attack.Type)
                cm *= 1.5f;
        }

        // Weakness / Efficiency
        foreach (PokemonConstants.Types type in defender.data.Types)
        {
            if (type != PokemonConstants.Types.unknown)
                cm *= typesTable[attack.Type][type];
        }

        // coup critique
        int T = 0;
        int critRate = attack.CritRate + attacker.critRate;
        if (critRate > 3)
            critRate = 3;
        T = critRateTable[critRate];
        if (Random.Range(0, 10000) < T)
            cm *= 1.5f;

        cm *= Random.Range(0.85f, 1f);

        if (attack.DamageClass == MoveDataSO.MoveDamageClass.physical) {
            att = attacker.bAttack;
            def = defender.bDefense;
        } else {
            att = attacker.bSpecialAttack;
            def = defender.bSpecialDefense;
        }

        damage = (((50 * 0.4f + 2) * att * attack.Power) / (def * 50) + 2) * cm;

        if (attacker.ailment == MoveDataSO.MoveAilment.burn && attack.DamageClass == MoveDataSO.MoveDamageClass.physical)
            damage /= 2;

        return (int)damage;
    }

    public void ApplyAilment(Pokemon defender, MoveDataSO attack)
    {
        foreach (PokemonConstants.Types type in defender.data.Types)
        {
            if ((type == PokemonConstants.Types.steel || type == PokemonConstants.Types.poison) && attack.Ailment == MoveDataSO.MoveAilment.poison)
                return;
            if (type == PokemonConstants.Types.fire && attack.Ailment == MoveDataSO.MoveAilment.burn)
                return;
            if (type == PokemonConstants.Types.ice && attack.Ailment == MoveDataSO.MoveAilment.freeze)
                return;
            if (type == PokemonConstants.Types.electric && attack.Ailment == MoveDataSO.MoveAilment.paralysis)
                return;
        }

        if (defender.ailment == MoveDataSO.MoveAilment.none) {
            defender.ailment = attack.Ailment;
            defender.roundSinceAilment = 0;
        }
    }

    private void InitTableTypes()
    {
        typesTable = new Dictionary<PokemonConstants.Types, Dictionary<PokemonConstants.Types, float>>();

        Dictionary<PokemonConstants.Types, float> normal = new Dictionary<PokemonConstants.Types, float>();
        normal.Add(PokemonConstants.Types.normal, 1); normal.Add(PokemonConstants.Types.fighting, 1); normal.Add(PokemonConstants.Types.flying, 1); normal.Add(PokemonConstants.Types.poison, 1);
        normal.Add(PokemonConstants.Types.ground, 1); normal.Add(PokemonConstants.Types.rock, 0.5f); normal.Add(PokemonConstants.Types.bug, 1); normal.Add(PokemonConstants.Types.ghost, 0);
        normal.Add(PokemonConstants.Types.steel, 0.5f); normal.Add(PokemonConstants.Types.fire, 1); normal.Add(PokemonConstants.Types.water, 1); normal.Add(PokemonConstants.Types.grass, 1);
        normal.Add(PokemonConstants.Types.electric, 1); normal.Add(PokemonConstants.Types.psychic, 1); normal.Add(PokemonConstants.Types.ice, 1); normal.Add(PokemonConstants.Types.dragon, 1);
        normal.Add(PokemonConstants.Types.dark, 1); normal.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.normal, normal);

        Dictionary<PokemonConstants.Types, float> fighting = new Dictionary<PokemonConstants.Types, float>();
        fighting.Add(PokemonConstants.Types.normal, 2); fighting.Add(PokemonConstants.Types.fighting, 1); fighting.Add(PokemonConstants.Types.flying, 0.5f); fighting.Add(PokemonConstants.Types.poison, 0.5f);
        fighting.Add(PokemonConstants.Types.ground, 1); fighting.Add(PokemonConstants.Types.rock, 2); fighting.Add(PokemonConstants.Types.bug, 0.5f); fighting.Add(PokemonConstants.Types.ghost, 0);
        fighting.Add(PokemonConstants.Types.steel, 2); fighting.Add(PokemonConstants.Types.fire, 1); fighting.Add(PokemonConstants.Types.water, 1); fighting.Add(PokemonConstants.Types.grass, 1);
        fighting.Add(PokemonConstants.Types.electric, 1); fighting.Add(PokemonConstants.Types.psychic, 0.5f); fighting.Add(PokemonConstants.Types.ice, 2); fighting.Add(PokemonConstants.Types.dragon, 1);
        fighting.Add(PokemonConstants.Types.dark, 2); fighting.Add(PokemonConstants.Types.fairy, 0.5f);
        typesTable.Add(PokemonConstants.Types.fighting, fighting);

        Dictionary<PokemonConstants.Types, float> flying = new Dictionary<PokemonConstants.Types, float>();
        flying.Add(PokemonConstants.Types.normal, 1); flying.Add(PokemonConstants.Types.fighting, 2); flying.Add(PokemonConstants.Types.flying, 1); flying.Add(PokemonConstants.Types.poison, 1);
        flying.Add(PokemonConstants.Types.ground, 1); flying.Add(PokemonConstants.Types.rock, 0.5f); flying.Add(PokemonConstants.Types.bug, 2); flying.Add(PokemonConstants.Types.ghost, 1);
        flying.Add(PokemonConstants.Types.steel, 0.5f); flying.Add(PokemonConstants.Types.fire, 1); flying.Add(PokemonConstants.Types.water, 1); flying.Add(PokemonConstants.Types.grass, 2);
        flying.Add(PokemonConstants.Types.electric, 0.5f); flying.Add(PokemonConstants.Types.psychic, 1); flying.Add(PokemonConstants.Types.ice, 1); flying.Add(PokemonConstants.Types.dragon, 1);
        flying.Add(PokemonConstants.Types.dark, 1); flying.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.flying, flying);

        Dictionary<PokemonConstants.Types, float> poison = new Dictionary<PokemonConstants.Types, float>();
        poison.Add(PokemonConstants.Types.normal, 1); poison.Add(PokemonConstants.Types.fighting, 1); poison.Add(PokemonConstants.Types.flying, 1); poison.Add(PokemonConstants.Types.poison, 0.5f);
        poison.Add(PokemonConstants.Types.ground, 0.5f); poison.Add(PokemonConstants.Types.rock, 0.5f); poison.Add(PokemonConstants.Types.bug, 1); poison.Add(PokemonConstants.Types.ghost, 0.5f);
        poison.Add(PokemonConstants.Types.steel, 0); poison.Add(PokemonConstants.Types.fire, 1); poison.Add(PokemonConstants.Types.water, 1); poison.Add(PokemonConstants.Types.grass, 2);
        poison.Add(PokemonConstants.Types.electric, 1); poison.Add(PokemonConstants.Types.psychic, 1); poison.Add(PokemonConstants.Types.ice, 1); poison.Add(PokemonConstants.Types.dragon, 1);
        poison.Add(PokemonConstants.Types.dark, 1); poison.Add(PokemonConstants.Types.fairy, 2);
        typesTable.Add(PokemonConstants.Types.poison, poison);

        Dictionary<PokemonConstants.Types, float> ground = new Dictionary<PokemonConstants.Types, float>();
        ground.Add(PokemonConstants.Types.normal, 1); ground.Add(PokemonConstants.Types.fighting, 1); ground.Add(PokemonConstants.Types.flying, 0); ground.Add(PokemonConstants.Types.poison, 2);
        ground.Add(PokemonConstants.Types.ground, 1); ground.Add(PokemonConstants.Types.rock, 2); ground.Add(PokemonConstants.Types.bug, 0.5f); ground.Add(PokemonConstants.Types.ghost, 1);
        ground.Add(PokemonConstants.Types.steel, 2); ground.Add(PokemonConstants.Types.fire, 2); ground.Add(PokemonConstants.Types.water, 1); ground.Add(PokemonConstants.Types.grass, 0.5f);
        ground.Add(PokemonConstants.Types.electric, 2); ground.Add(PokemonConstants.Types.psychic, 1); ground.Add(PokemonConstants.Types.ice, 1); ground.Add(PokemonConstants.Types.dragon, 1);
        ground.Add(PokemonConstants.Types.dark, 1); ground.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.ground, ground);

        Dictionary<PokemonConstants.Types, float> rock = new Dictionary<PokemonConstants.Types, float>();
        rock.Add(PokemonConstants.Types.normal, 1); rock.Add(PokemonConstants.Types.fighting, 0.5f); rock.Add(PokemonConstants.Types.flying, 2); rock.Add(PokemonConstants.Types.poison, 1);
        rock.Add(PokemonConstants.Types.ground, 0.5f); rock.Add(PokemonConstants.Types.rock, 1); rock.Add(PokemonConstants.Types.bug, 2); rock.Add(PokemonConstants.Types.ghost, 1);
        rock.Add(PokemonConstants.Types.steel, 0.5f); rock.Add(PokemonConstants.Types.fire, 2); rock.Add(PokemonConstants.Types.water, 1); rock.Add(PokemonConstants.Types.grass, 1);
        rock.Add(PokemonConstants.Types.electric, 1); rock.Add(PokemonConstants.Types.psychic, 1); rock.Add(PokemonConstants.Types.ice, 2); rock.Add(PokemonConstants.Types.dragon, 1);
        rock.Add(PokemonConstants.Types.dark, 1); rock.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.rock, rock);

        Dictionary<PokemonConstants.Types, float> bug = new Dictionary<PokemonConstants.Types, float>();
        bug.Add(PokemonConstants.Types.normal, 1); bug.Add(PokemonConstants.Types.fighting, 0.5f); bug.Add(PokemonConstants.Types.flying, 0.5f); bug.Add(PokemonConstants.Types.poison, 0.5f);
        bug.Add(PokemonConstants.Types.ground, 1); bug.Add(PokemonConstants.Types.rock, 1); bug.Add(PokemonConstants.Types.bug, 1); bug.Add(PokemonConstants.Types.ghost, 0.5f);
        bug.Add(PokemonConstants.Types.steel, 0.5f); bug.Add(PokemonConstants.Types.fire, 0.5f); bug.Add(PokemonConstants.Types.water, 1); bug.Add(PokemonConstants.Types.grass, 2);
        bug.Add(PokemonConstants.Types.electric, 1); bug.Add(PokemonConstants.Types.psychic, 2); bug.Add(PokemonConstants.Types.ice, 1); bug.Add(PokemonConstants.Types.dragon, 1);
        bug.Add(PokemonConstants.Types.dark, 2); bug.Add(PokemonConstants.Types.fairy, 0.5f);
        typesTable.Add(PokemonConstants.Types.bug, bug);

        Dictionary<PokemonConstants.Types, float> ghost = new Dictionary<PokemonConstants.Types, float>();
        ghost.Add(PokemonConstants.Types.normal, 0); ghost.Add(PokemonConstants.Types.fighting, 1); ghost.Add(PokemonConstants.Types.flying, 1); ghost.Add(PokemonConstants.Types.poison, 1);
        ghost.Add(PokemonConstants.Types.ground, 1); ghost.Add(PokemonConstants.Types.rock, 1); ghost.Add(PokemonConstants.Types.bug, 1); ghost.Add(PokemonConstants.Types.ghost, 2);
        ghost.Add(PokemonConstants.Types.steel, 1); ghost.Add(PokemonConstants.Types.fire, 1); ghost.Add(PokemonConstants.Types.water, 1); ghost.Add(PokemonConstants.Types.grass, 1);
        ghost.Add(PokemonConstants.Types.electric, 1); ghost.Add(PokemonConstants.Types.psychic, 2); ghost.Add(PokemonConstants.Types.ice, 1); ghost.Add(PokemonConstants.Types.dragon, 1);
        ghost.Add(PokemonConstants.Types.dark, 0.5f); ghost.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.ghost, ghost);

        Dictionary<PokemonConstants.Types, float> steel = new Dictionary<PokemonConstants.Types, float>();
        steel.Add(PokemonConstants.Types.normal, 1); steel.Add(PokemonConstants.Types.fighting, 1); steel.Add(PokemonConstants.Types.flying, 1); steel.Add(PokemonConstants.Types.poison, 1);
        steel.Add(PokemonConstants.Types.ground, 1); steel.Add(PokemonConstants.Types.rock, 2); steel.Add(PokemonConstants.Types.bug, 1); steel.Add(PokemonConstants.Types.ghost, 1);
        steel.Add(PokemonConstants.Types.steel, 0.5f); steel.Add(PokemonConstants.Types.fire, 0.5f); steel.Add(PokemonConstants.Types.water, 0.5f); steel.Add(PokemonConstants.Types.grass, 1);
        steel.Add(PokemonConstants.Types.electric, 0.5f); steel.Add(PokemonConstants.Types.psychic, 1); steel.Add(PokemonConstants.Types.ice, 2); steel.Add(PokemonConstants.Types.dragon, 1);
        steel.Add(PokemonConstants.Types.dark, 1); steel.Add(PokemonConstants.Types.fairy, 2);
        typesTable.Add(PokemonConstants.Types.steel, steel);

        Dictionary<PokemonConstants.Types, float> fire = new Dictionary<PokemonConstants.Types, float>();
        fire.Add(PokemonConstants.Types.normal, 1); fire.Add(PokemonConstants.Types.fighting, 1); fire.Add(PokemonConstants.Types.flying, 1); fire.Add(PokemonConstants.Types.poison, 1);
        fire.Add(PokemonConstants.Types.ground, 1); fire.Add(PokemonConstants.Types.rock, 0.5f); fire.Add(PokemonConstants.Types.bug, 2); fire.Add(PokemonConstants.Types.ghost, 1);
        fire.Add(PokemonConstants.Types.steel, 2); fire.Add(PokemonConstants.Types.fire, 0.5f); fire.Add(PokemonConstants.Types.water, 0.5f); fire.Add(PokemonConstants.Types.grass, 2);
        fire.Add(PokemonConstants.Types.electric, 1); fire.Add(PokemonConstants.Types.psychic, 1); fire.Add(PokemonConstants.Types.ice, 2); fire.Add(PokemonConstants.Types.dragon, 0.5f);
        fire.Add(PokemonConstants.Types.dark, 1); fire.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.fire, fire);

        Dictionary<PokemonConstants.Types, float> water = new Dictionary<PokemonConstants.Types, float>();
        water.Add(PokemonConstants.Types.normal, 1); water.Add(PokemonConstants.Types.fighting, 1); water.Add(PokemonConstants.Types.flying, 1); water.Add(PokemonConstants.Types.poison, 1);
        water.Add(PokemonConstants.Types.ground, 2); water.Add(PokemonConstants.Types.rock, 2); water.Add(PokemonConstants.Types.bug, 1); water.Add(PokemonConstants.Types.ghost, 1);
        water.Add(PokemonConstants.Types.steel, 1); water.Add(PokemonConstants.Types.fire, 2); water.Add(PokemonConstants.Types.water, 0.5f); water.Add(PokemonConstants.Types.grass, 0.5f);
        water.Add(PokemonConstants.Types.electric, 1); water.Add(PokemonConstants.Types.psychic, 1); water.Add(PokemonConstants.Types.ice, 1); water.Add(PokemonConstants.Types.dragon, 0.5f);
        water.Add(PokemonConstants.Types.dark, 1); water.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.water, water);

        Dictionary<PokemonConstants.Types, float> grass = new Dictionary<PokemonConstants.Types, float>();
        grass.Add(PokemonConstants.Types.normal, 1); grass.Add(PokemonConstants.Types.fighting, 1); grass.Add(PokemonConstants.Types.flying, 0.5f); grass.Add(PokemonConstants.Types.poison, 0.5f);
        grass.Add(PokemonConstants.Types.ground, 2); grass.Add(PokemonConstants.Types.rock, 2); grass.Add(PokemonConstants.Types.bug, 0.5f); grass.Add(PokemonConstants.Types.ghost, 1);
        grass.Add(PokemonConstants.Types.steel, 0.5f); grass.Add(PokemonConstants.Types.fire, 0.5f); grass.Add(PokemonConstants.Types.water, 2); grass.Add(PokemonConstants.Types.grass, 0.5f);
        grass.Add(PokemonConstants.Types.electric, 1); grass.Add(PokemonConstants.Types.psychic, 1); grass.Add(PokemonConstants.Types.ice, 1); grass.Add(PokemonConstants.Types.dragon, 0.5f);
        grass.Add(PokemonConstants.Types.dark, 1); grass.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.grass, grass);

        Dictionary<PokemonConstants.Types, float> electric = new Dictionary<PokemonConstants.Types, float>();
        electric.Add(PokemonConstants.Types.normal, 1); electric.Add(PokemonConstants.Types.fighting, 1); electric.Add(PokemonConstants.Types.flying, 2); electric.Add(PokemonConstants.Types.poison, 1);
        electric.Add(PokemonConstants.Types.ground, 0); electric.Add(PokemonConstants.Types.rock, 1); electric.Add(PokemonConstants.Types.bug, 1); electric.Add(PokemonConstants.Types.ghost, 1);
        electric.Add(PokemonConstants.Types.steel, 1); electric.Add(PokemonConstants.Types.fire, 1); electric.Add(PokemonConstants.Types.water, 2); electric.Add(PokemonConstants.Types.grass, 0.5f);
        electric.Add(PokemonConstants.Types.electric, 0.5f); electric.Add(PokemonConstants.Types.psychic, 1); electric.Add(PokemonConstants.Types.ice, 1); electric.Add(PokemonConstants.Types.dragon, 0.5f);
        electric.Add(PokemonConstants.Types.dark, 1); electric.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.electric, electric);

        Dictionary<PokemonConstants.Types, float> psychic = new Dictionary<PokemonConstants.Types, float>();
        psychic.Add(PokemonConstants.Types.normal, 1); psychic.Add(PokemonConstants.Types.fighting, 2); psychic.Add(PokemonConstants.Types.flying, 1); psychic.Add(PokemonConstants.Types.poison, 2);
        psychic.Add(PokemonConstants.Types.ground, 1); psychic.Add(PokemonConstants.Types.rock, 1); psychic.Add(PokemonConstants.Types.bug, 1); psychic.Add(PokemonConstants.Types.ghost, 1);
        psychic.Add(PokemonConstants.Types.steel, 0.5f); psychic.Add(PokemonConstants.Types.fire, 1); psychic.Add(PokemonConstants.Types.water, 1); psychic.Add(PokemonConstants.Types.grass, 1);
        psychic.Add(PokemonConstants.Types.electric, 1); psychic.Add(PokemonConstants.Types.psychic, 0.5f); psychic.Add(PokemonConstants.Types.ice, 1); psychic.Add(PokemonConstants.Types.dragon, 1);
        psychic.Add(PokemonConstants.Types.dark, 0); psychic.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.psychic, psychic);

        Dictionary<PokemonConstants.Types, float> ice = new Dictionary<PokemonConstants.Types, float>();
        ice.Add(PokemonConstants.Types.normal, 1); ice.Add(PokemonConstants.Types.fighting, 1); ice.Add(PokemonConstants.Types.flying, 2); ice.Add(PokemonConstants.Types.poison, 1);
        ice.Add(PokemonConstants.Types.ground, 2); ice.Add(PokemonConstants.Types.rock, 1); ice.Add(PokemonConstants.Types.bug, 1); ice.Add(PokemonConstants.Types.ghost, 1);
        ice.Add(PokemonConstants.Types.steel, 0.5f); ice.Add(PokemonConstants.Types.fire, 0.5f); ice.Add(PokemonConstants.Types.water, 0.5f); ice.Add(PokemonConstants.Types.grass, 2);
        ice.Add(PokemonConstants.Types.electric, 1); ice.Add(PokemonConstants.Types.psychic, 1); ice.Add(PokemonConstants.Types.ice, 0.5f); ice.Add(PokemonConstants.Types.dragon, 2);
        ice.Add(PokemonConstants.Types.dark, 1); ice.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.ice, ice);

        Dictionary<PokemonConstants.Types, float> dragon = new Dictionary<PokemonConstants.Types, float>();
        dragon.Add(PokemonConstants.Types.normal, 1); dragon.Add(PokemonConstants.Types.fighting, 1); dragon.Add(PokemonConstants.Types.flying, 1); dragon.Add(PokemonConstants.Types.poison, 1);
        dragon.Add(PokemonConstants.Types.ground, 1); dragon.Add(PokemonConstants.Types.rock, 1); dragon.Add(PokemonConstants.Types.bug, 1); dragon.Add(PokemonConstants.Types.ghost, 1);
        dragon.Add(PokemonConstants.Types.steel, 0.5f); dragon.Add(PokemonConstants.Types.fire, 1); dragon.Add(PokemonConstants.Types.water, 1); dragon.Add(PokemonConstants.Types.grass, 1);
        dragon.Add(PokemonConstants.Types.electric, 1); dragon.Add(PokemonConstants.Types.psychic, 1); dragon.Add(PokemonConstants.Types.ice, 1); dragon.Add(PokemonConstants.Types.dragon, 2);
        dragon.Add(PokemonConstants.Types.dark, 1); dragon.Add(PokemonConstants.Types.fairy, 0);
        typesTable.Add(PokemonConstants.Types.dragon, dragon);

        Dictionary<PokemonConstants.Types, float> dark = new Dictionary<PokemonConstants.Types, float>();
        dark.Add(PokemonConstants.Types.normal, 1); dark.Add(PokemonConstants.Types.fighting, 0.5f); dark.Add(PokemonConstants.Types.flying, 1); dark.Add(PokemonConstants.Types.poison, 1);
        dark.Add(PokemonConstants.Types.ground, 1); dark.Add(PokemonConstants.Types.rock, 1); dark.Add(PokemonConstants.Types.bug, 1); dark.Add(PokemonConstants.Types.ghost, 2);
        dark.Add(PokemonConstants.Types.steel, 1); dark.Add(PokemonConstants.Types.fire, 1); dark.Add(PokemonConstants.Types.water, 1); dark.Add(PokemonConstants.Types.grass, 1);
        dark.Add(PokemonConstants.Types.electric, 1); dark.Add(PokemonConstants.Types.psychic, 2); dark.Add(PokemonConstants.Types.ice, 1); dark.Add(PokemonConstants.Types.dragon, 1);
        dark.Add(PokemonConstants.Types.dark, 0.5f); dark.Add(PokemonConstants.Types.fairy, 0.5f);
        typesTable.Add(PokemonConstants.Types.dark, dark);

        Dictionary<PokemonConstants.Types, float> fairy = new Dictionary<PokemonConstants.Types, float>();
        fairy.Add(PokemonConstants.Types.normal, 1); fairy.Add(PokemonConstants.Types.fighting, 2); fairy.Add(PokemonConstants.Types.flying, 1); fairy.Add(PokemonConstants.Types.poison, 0.5f);
        fairy.Add(PokemonConstants.Types.ground, 1); fairy.Add(PokemonConstants.Types.rock, 1); fairy.Add(PokemonConstants.Types.bug, 1); fairy.Add(PokemonConstants.Types.ghost, 1);
        fairy.Add(PokemonConstants.Types.steel, 0.5f); fairy.Add(PokemonConstants.Types.fire, 0.5f); fairy.Add(PokemonConstants.Types.water, 1); fairy.Add(PokemonConstants.Types.grass, 1);
        fairy.Add(PokemonConstants.Types.electric, 1); fairy.Add(PokemonConstants.Types.psychic, 1); fairy.Add(PokemonConstants.Types.ice, 1); fairy.Add(PokemonConstants.Types.dragon, 2);
        fairy.Add(PokemonConstants.Types.dark, 2); fairy.Add(PokemonConstants.Types.fairy, 1);
        typesTable.Add(PokemonConstants.Types.fairy, fairy);
    }
}
