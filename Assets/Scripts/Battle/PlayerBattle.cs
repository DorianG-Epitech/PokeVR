using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Facebook.WitAi;

public class PlayerBattle : NetworkBehaviour
{
    public enum EndState
    {
        Battle,
        Victory,
        Defeat
    }


    public Pokemon pokemonPrefab;
    [SyncVar]
    public BattleManager _battleManager;

    private bool _ready = false;

    [SyncVar]
    public BattleAction action = null;

    // [SyncVar]
    public readonly SyncList<Pokemon> pokemons = new SyncList<Pokemon>();

    [SyncVar]
    public int pokemonChoosed = 0;

    [SyncVar]
    public bool tempShow = false;

    [SyncVar]
    public bool waitPokemon = false;

    [SyncVar]
    public int pokemonAvailable;

    [SyncVar]
    public EndState endState = EndState.Battle;

    // TEMP
    public List<PokemonDataSO> existingPokemons;
    public Pokemon pokemonLoader;
    private ActivateVoice _activateVoice;

    public override void OnStartClient()
    {
        _battleManager = FindObjectOfType<BattleManager>();
        _ready = false;

        _activateVoice = GetComponentInChildren<ActivateVoice>();
        action = null;
    }

    public void InitPokemonsList()
    {
        if (!GameObject.FindObjectOfType<CaptureManager>())
            return;
        List<PokemonData> pokemons = GameObject.FindObjectOfType<CaptureManager>().GetPokemons();
        for (int i = 0; i < pokemons.Count; i++)
            pokemons[i].data = null;
        InitPlayerPokemons(pokemons);
    }

    [Command]
    public void InitPlayerPokemons(List<PokemonData> pokes)
    {
        // pokemons = pokes;
        pokemonChoosed = 0;

        foreach (Pokemon poke in pokemons)
        {
            NetworkServer.Destroy(poke.gameObject);
        }
        pokemons.Clear();
        Debug.LogError("InitPlayerPokemons: " + pokes.Count + " " + isServer.ToString());

        foreach (PokemonData p in pokes)
        {
            Pokemon pokeComponent = Instantiate(pokemonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            pokeComponent.id = p.id;
            pokeComponent.data = SpawnManager.Instance.existingPokemons.Find(i => i.Id == p.id);
            Debug.LogError("InitPlayerPokemons:" + pokeComponent.data.name);
            pokeComponent.gameObject.GetComponent<Animator>().SetBool("isFighting", true);
            NetworkServer.Spawn(pokeComponent.gameObject);
            if (pokemons.Count != 0)
                pokeComponent.transform.position = Vector3.up * 10;
            pokemons.Add(pokeComponent);
        }
        pokemonAvailable = pokemons.Count;
        _battleManager.Ready(this);
        _ready = true;
    }

    [Command]
    public void RepositionPokemons()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            pokemons[i].transform.position = Vector3.up * 10;
        }
        pokemons[0].transform.position = Vector3.zero;
    }

    public void HidePokemons()
    {
        if (!isLocalPlayer)
            return;
        foreach (Pokemon p in pokemons)
        {
            p.transform.position = new Vector3(10, 0, 0);
        }
        pokemons[0].transform.position = new Vector3(-10, 0, 0);
    }

    [ClientRpc]
    public void RpcSpawnPoke()
    {
        foreach (Pokemon p in pokemons)
        {
            // Debug.LogError("Spawning pokemon " + p.data.Name);
            p.gameObject.SetActive(false);
        }
        // if (pokemons.Count > 0) { pokemons[0].gameObject.SetActive(true); }

        // foreach (var move in poke.moves)
        //     pokeComponent.moves.Add(move);
        
    }

    private void Update()
    {
        bool pokeOnFloor = false;
        foreach (Pokemon poke in pokemons)
        {
            if (poke == null)
            {
                InitPokemonsList();
                return;
            }
            if (poke.transform.position.y == 0)
            {
                if (pokeOnFloor)
                {
                    RepositionPokemons();
                    return;
                }
                pokeOnFloor = true;
            }
                
        }
        if (!isLocalPlayer || _ready == false) { return; }
        if (_battleManager._selectPhase)
        {
            if (Input.GetKeyDown("a")) {
                Attack(pokemons[pokemonChoosed].moves[0]);
            } if (Input.GetKeyDown("r")) {
                RetrievePokemon();
            }   
        } else if (_battleManager._waitSwitch && waitPokemon) {
            if (Input.GetKeyDown("1"))
            {
                SwitchPokemon(0);
            }
            if (Input.GetKeyDown("2"))
            {
                SwitchPokemon(1);
            }
            if (Input.GetKeyDown("3"))
            {
                SwitchPokemon(2);
            }
            if (Input.GetKeyDown("4"))
            {
                SwitchPokemon(3);
            }
            if (Input.GetKeyDown("5"))
            {
                SwitchPokemon(4);
            }
            if (Input.GetKeyDown("6"))
            {
                SwitchPokemon(5);
            }
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                _activateVoice.WitActivate();
            }
        }
    }
    
    public void OnVoiceAttack(string[] values)
    {
        // Une erreur dans cette fonction donne un log d'erreur bizarre car elle est gérée par WitAI
        // Absolument tester toutes les valeurs auxquelles on accède ici
        if (values != null)
        {
            Debug.Log("OnVoiceAttack -> " + values[0] + " " + values[1] + " " + values[2]);
            foreach (var move in pokemons[pokemonChoosed].moves)
            {
                if (values[2] == move.Name)
                {
                    Debug.Log(move.Name);
                    Attack(move);
                    return;
                }
            }
            print("Did not understand");
        }
    }
    
    public void OnVoiceAttackTarget(string[] values)
    {
        if (values != null)
        {
            Debug.Log(values[0]);
            Debug.Log(values[1]);
            Debug.Log(values[2]);
            // Attack(pokemons[pokemonChoosed].moves[0]);
        }
        else
            print("Did not understand");
    }

    public void AddPokemons(Pokemon p)
    {
        if (pokemons.Count < 6)
            pokemons.Add(p);
    }

    [Client]
    public void SwitchPokemon(int nb)
    {
        if (pokemons[nb].bHp > 0)
            CmdSwitchPokemon(nb);
        else
            Debug.Log("Pokemon unavailable");
    }

    [Command]
    void CmdSwitchPokemon(int nb)
    {
        pokemons[nb].gameObject.transform.position = new Vector3Int(0,0,0);
        pokemonChoosed = nb;
        waitPokemon = false;
        _battleManager._waitSwitch = false;

    }

    [ClientRpc]
    void RpcSwitchPokemon(int nb)
    {
        pokemons[nb].gameObject.SetActive(true);
    }

    [Client]
    public void RetrievePokemon()
    {
        CmdRetrievePokemon();
    }

    [Command]
    void CmdRetrievePokemon()
    {
        action = new BattleAction();
    }

    [Client]
    public void Attack(MoveDataSO attack)
    {
        CmdAttack(attack);
    }

    [Command]
    void CmdAttack(MoveDataSO attack)
    {
        action = new BattleAction(attack);
    }


    [Client]
    public void UseItem()
    {
        CmdUseItem();
    }

    [Command]
    void CmdUseItem()
    {
        action = new BattleAction(10f);
    }

    [Client]
    public void CancelAction()
    {
        CmdCancelAction();
    }

    [Command]
    void CmdCancelAction()
    {
        action = null;
    }
}
