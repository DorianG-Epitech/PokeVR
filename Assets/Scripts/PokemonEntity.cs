using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonEntity : MonoBehaviour
{
    private Animator _animator;
    private static readonly int attack1 = Animator.StringToHash("Attack");
    private static readonly int beAttacked = Animator.StringToHash("BeAttacked");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void Attack(PokemonEntity target)
    {
        _animator.SetTrigger(attack1);
        if (target != null)
            target.BeAttacked();
    }
    
    public void BeAttacked()
    {
        _animator.SetTrigger(beAttacked);
    }
}
