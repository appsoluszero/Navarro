using UnityEngine;

public class Boss_SlimeQueen : MonoBehaviour
{
    [SerializeField] private bossState currentBossState;
    int attackType = 2;

    void Start() {
        currentBossState = bossState.Attack;
    }

    void Update() {
        transform.GetChild(attackType).GetComponent<IBossAttack>().AttackLogicRunner();
    }
}

public enum bossState {
    Idle,
    PrepareAttack,
    Attack,
    Charging
}
