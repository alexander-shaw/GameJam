using System;
using UnityEngine;

public class BattleEntity : MonoBehaviour {
    [Header("Settings")]
    public float health = 100f;
    public float energy = 50f;
    public float attackPower = 20f;
    public float defense = 10f;

    [Header("Attack")]
    public string attack1Name;
    public float attack1Damage;
    public string attack2Name;
    public float attack2Damage;

    public void Start() {
        throw new NotImplementedException();
    }
}