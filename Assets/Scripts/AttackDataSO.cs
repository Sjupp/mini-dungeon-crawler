using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Create Attack")]
public class AttackDataSO : ScriptableObject
{
    public GameObject Hitbox;
    public GameObject VFX;
    public AttackTimeline AttackTimeline;
    public string AnimationName;
    public int Damage;

    //public float Sharpness; ???
    //public float Sturdiness; ???
}

[System.Serializable]
public struct AttackTimeline
{
    public float Windup;
    public float Duration;
    public float Winddown;
}
