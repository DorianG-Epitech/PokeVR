using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof (NetworkIdentity))]
[RequireComponent(typeof (NetworkTransform))]
[RequireComponent(typeof (NetworkAnimator))]
public class Pokemon : NetworkBehaviour
{
    public PokemonDataSO Data;

    [SyncVar]
    public int Id;

    [Header("Stats")]
    public int Hp;
    public int Attack;
    public int Defense;
    public int SpecialAttack;
    public int SpecialDefense;
    public int Speed;

    [Header("Moves")]
    public MoveDataSO[] Moves;

    [Header("Model")]
    public float ModelScale = 0.007f;

    public override void OnStartClient()
    {
        Data = SpawnManager.Instance.existingPokemons.Find(i => i.Id == Id);
    }

    private void Start()
    {
        // Destroy the temporary animator
        Destroy(GetComponent<Animator>());

        InitStats();
        InitMoves();
        InitModel();
    }

    /// <summary>
    /// Init the pokemon statistics from the given data
    /// </summary>
    private void InitStats()
    {
        Hp = Array.Find(Data.Stats, element => element.name == "hp").value;
        Attack = Array.Find(Data.Stats, element => element.name == "attack").value;
        Defense = Array.Find(Data.Stats, element => element.name == "defense").value;
        SpecialAttack = Array.Find(Data.Stats, element => element.name == "special-attack").value;
        SpecialDefense = Array.Find(Data.Stats, element => element.name == "special-defense").value;
        Speed = Array.Find(Data.Stats, element => element.name == "speed").value;
    }

    /// <summary>
    /// Pick 4 moves from the moves pool and add it to the pokemon moves set
    /// </summary>
    private void InitMoves()
    {
        List<int> movesIndex = new List<int>();
        List<MoveDataSO> moves = new List<MoveDataSO>();

        while (movesIndex.Count < 4)
        {
            int newIndex = UnityEngine.Random.Range(0, Data.Moves.Length - 1);
            if (movesIndex.Contains(newIndex)) continue;
            movesIndex.Add(newIndex);
        }

        foreach (int index in movesIndex)
            moves.Add(Data.Moves[index]);
        Moves = moves.ToArray();
    }

    /// <summary>
    /// Init the pokmemon model
    /// </summary>
    private void InitModel()
    {
        GameObject model = Instantiate(Data.Model) as GameObject;

        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = new Vector3(ModelScale, ModelScale, ModelScale);

        GetComponent<NetworkAnimator>().animator = model.GetComponentInChildren<Animator>();
    }
}
