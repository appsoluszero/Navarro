using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DamageEffectData", menuName = "Navarro/DamageEffectData", order = 0)]
public class DamageEffectData : ScriptableObject {
    [Header("Flash on hit")]
    public bool enable = true;
    public AnimationCurve flashCurve;
    public int frameCount;

    [Header("Blood")]
    public ParticleSystem blood_particle;
    public Vector3 relativePosition;
}
