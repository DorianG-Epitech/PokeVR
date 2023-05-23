using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleAction
{
    public enum ActionType
    {
        ATTACK,
        ITEM,
        SWITCH,
        RETRIEVE
    }

    public MoveDataSO attack;
    public float item;
    public int sw;

    public ActionType action;

    public BattleAction()
    {
        action = ActionType.RETRIEVE;
    }

    public BattleAction(MoveDataSO a)
    {
        attack = a;
        action = ActionType.ATTACK;
    }

    public BattleAction(float a)
    {
        item = a;
        action = ActionType.ITEM;
    }

    public BattleAction(int a)
    {
        sw = a;
        action = ActionType.SWITCH;
    }
}
