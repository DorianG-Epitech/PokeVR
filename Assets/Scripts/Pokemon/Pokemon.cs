using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PokemonData
{
    public PokemonDataSO data;
    public int id;
    public List<MoveDataSO> moves = new List<MoveDataSO>();
}

[RequireComponent(typeof (NetworkIdentity))]
[RequireComponent(typeof (NetworkTransform))]
[RequireComponent(typeof (NetworkAnimator))]
public class Pokemon : NetworkBehaviour
{
    public PokemonDataSO data;

    public BoxCollider zone;

    [SyncVar]
    public int id;

    [Header("Stats")]
    public int hp;
    public int attack;
    public int defense;
    public int specialAttack;
    public int specialDefense;
    public int speed;

    [Header("BattleStats")]
    [SyncVar]
    public int bHp;
    [SyncVar]
    public int bAttack;
    [SyncVar]
    public int bDefense;
    [SyncVar]
    public int bSpecialAttack;
    [SyncVar]
    public int bSpecialDefense;
    [SyncVar]
    public int bSpeed;
    [SyncVar]
    public int evasivness;
    [SyncVar]
    public int precision;
    [SyncVar]
    public int critRate;
    [SyncVar]
    public MoveDataSO.MoveAilment ailment = MoveDataSO.MoveAilment.none;
    [SyncVar]
    public int roundSinceAilment = 0;

    [Header("Moves")]
    public readonly SyncList<MoveDataSO> moves = new SyncList<MoveDataSO>();

    [Header("Model")]
    public float modelScale = 0.007f;
    public float minFlyingOffset = 2.0f;
    public float maxFlyingOffset = 5.0f;

    [SyncVar]
    private float _flyingOffset;
    private SkinnedMeshRenderer[] _meshRenderers;

    private void Start()
    {
        // Destroy the temporary animator
        Destroy(GetComponent<Animator>());

        // Set parent object name
        transform.name = data.Name;

        if (isServer) StartServer();

        InitStats();
        InitModel();

        bHp = hp;
        ResetBattleStats();
    }

    private void StartServer()
    {
        _flyingOffset = UnityEngine.Random.Range(minFlyingOffset, maxFlyingOffset);
    }

    public override void OnStartClient()
    {
        if (!isServer)
        {
            // Add the pokemon data
            data = SpawnManager.Instance.existingPokemons.Find(i => i.Id == id);

            // Destroy all the server side components
            Destroy(GetComponent<NavMeshAgent>());
        }
    }

    public PokemonData GetData()
    {
        PokemonData copy = new PokemonData();
        copy.id = id;
        copy.data = data;
        foreach (var move in moves)
            copy.moves.Add(move);
        return copy;
    }

    private void FixedUpdate()
    {
        UpdateColliders();
    }

    /// <summary>
    /// Init pokemon values when spawning
    /// </summary>
    /// <param name="data">pokemon data</param>
    /// <param name="boxCollider">pokemon zone</param>
    public void InitSpawn(PokemonDataSO data, BoxCollider boxCollider)
    {
        this.data = data;
        id = data.Id;
        zone = boxCollider;
        InitMoves();
    }

    /// <summary>
    /// Init the pokemon statistics from the given data
    /// </summary>
    private void InitStats()
    {
        hp = Array.Find(data.Stats, element => element.name == "hp").value;
        attack = Array.Find(data.Stats, element => element.name == "attack").value;
        defense = Array.Find(data.Stats, element => element.name == "defense").value;
        specialAttack = Array.Find(data.Stats, element => element.name == "special-attack").value;
        specialDefense = Array.Find(data.Stats, element => element.name == "special-defense").value;
        speed = Array.Find(data.Stats, element => element.name == "speed").value;

        hp = (((2 * hp + 31) * 50) / 100) + 50 + 10;
        attack = (((2 * attack + 31) * 50) / 100) + 5;
        defense = (((2 * defense + 31) * 50) / 100) + 5;
        specialAttack = (((2 * defense + 31) * 50) / 100) + 5;
        specialDefense = (((2 * defense + 31) * 50) / 100) + 5;
        speed = (((2 * defense + 31) * 50) / 100) + 5;
    }

    /// <summary>
    /// Pick 4 moves from the moves pool and add it to the pokemon moves set
    /// </summary>
    public void InitMoves()
    {
        List<int> movesIndex = new List<int>();

        while (movesIndex.Count < 4)
        {
            int newIndex = UnityEngine.Random.Range(0, data.Moves.Length);
            if (movesIndex.Contains(newIndex)) continue;
            movesIndex.Add(newIndex);
            // if (data.Moves[newIndex].Category == MoveDataSO.MoveCategory.damage || data.Moves[newIndex].Category == MoveDataSO.MoveCategory.ailment || data.Moves[newIndex].Category == MoveDataSO.MoveCategory.damageAilment)
                // movesIndex.Add(newIndex);
        }

        foreach (int index in movesIndex)
            moves.Add(data.Moves[index]);
    }

    /// <summary>
    /// Init the pokmemon model
    /// </summary>
    private void InitModel()
    {
        GameObject model = Instantiate(data.Model);

        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        GetComponent<NetworkAnimator>().animator = model.GetComponentInChildren<Animator>();

        // Add flying offset
        if (data.IsFlying)
        {
            model.transform.localPosition = new Vector3(0.0f, _flyingOffset, 0.0f);
        }

        // Create mesh colliders
        _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var mesh in _meshRenderers)
        {
            mesh.gameObject.AddComponent<BoxCollider>();
        }

        model.transform.localScale = new Vector3(modelScale, modelScale, modelScale);
    }

    private void UpdateColliders()
    {
        foreach (var mesh in _meshRenderers)
        {
            var collider = mesh.GetComponent<BoxCollider>();

            var size = collider.transform.InverseTransformVector(mesh.bounds.size);
            if (size.x >= 0) size.x *= -1;
            if (size.y >= 0) size.y *= -1;
            if (size.z >= 0) size.z *= -1;
            collider.center = collider.transform.InverseTransformPoint(mesh.bounds.center);
        }
    }

    private void ResetBattleStats()
    {
        bAttack = attack;
        bDefense = defense;
        bSpecialAttack = specialAttack;
        bSpecialDefense = specialDefense;
        bSpeed = speed;
        precision = 100;
        evasivness = 100;
        critRate = 0;
        if (ailment != MoveDataSO.MoveAilment.freeze && ailment != MoveDataSO.MoveAilment.poison && ailment != MoveDataSO.MoveAilment.paralysis && ailment != MoveDataSO.MoveAilment.sleep && ailment != MoveDataSO.MoveAilment.burn) {
            ailment = MoveDataSO.MoveAilment.none;
            roundSinceAilment = 0;
        }   
    }
}
