using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SlimeQueen : MonoBehaviour
{
    [SerializeField] private bossState currentBossState;
    [SerializeField] private Boss_Attack_SlimeBlock AttackSweep;
}

public enum bossState {
    Idle,
    Attack1,
    Attack2,
    Attack3,
    Charging
}
