using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwatData.asset", menuName = "Data/SwatData")]
public class SwatStatus : ScriptableObject
{
    public float viewAngle = 60.0f;
    public float viewDistance = 15.0f;
    public float attackDistance = 7.0f;
    public float audioDistance = 13.0f;
    public float lostDelayTime = 1.0f;
    public float returnDelayTime = 3.0f;
    public float scoutDelayTime = 5.0f;
    public float hp = 1.0f;
    public float speed = 1.2f;
    public float damage = 50.0f;
}
