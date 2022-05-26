using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretData.asset", menuName = "Data/TurretData")]
public class TurretStatus : ScriptableObject
{
    public float viewAngle = 60.0f;
    public float viewDistance = 15.0f;
    public float lostDelayTime = 1.0f;
    public float hp = 1.0f;
    public float turnDelayTime = 1.0f;
    public float damage = 50.0f;
    public float attackDelayTime = 1.0f;
}
