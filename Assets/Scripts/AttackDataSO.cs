using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Create Attack")]
public class AttackDataSO : ScriptableObject
{
    public string AnimationName;
    public int Damage;

    public AttackTimeline AttackTimeline;
    [Space]
    public Vector3 HitboxPosition = new(1f, 0.5f, 0f);
    public Vector3 HitboxScale = Vector3.one;
    [Space]
    public ParticleSystem VFX;
    public Vector3 VFXPosition;
    public Vector3 VFXScale = Vector3.one;
    public float VFXRotationZ;
    public float VFXStartDelay;


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
