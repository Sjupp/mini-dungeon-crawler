using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Create Attack")]
public class AttackDataSO : ScriptableObject
{
    // old
    public int Damage;
    public MovementState MovementState = MovementState.Free;
    public string AnimationName;
    public List<AnimationThing> AnimationThings = new();
    public Vector3 MovementShift;
    public float ShiftDuration;

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

    [SerializeReference]
    public List<AttackBlock> Blocks = new();

    public float TotalDurationOverride = -1f;
    public float GetTotalDuration()
    {
        if (TotalDurationOverride != -1)
        {
            return TotalDurationOverride;
        }
        else
        {
            return Mathf.Max(.1f, Blocks.Max(b => b.StartTime + b.Duration));
        }
    }
}

[System.Serializable]
public class AttackTimeline
{
    public float Windup;
    public float Duration;
    public float Winddown;

    public float Total => Windup + Duration + Winddown;
}

public enum AnimationType
{
    PlayerAnimation,
    ItemAnimation
}

public class AnimationThing
{
    public AnimationType AnimationType;
    public string AnimationName;
    public float Delay;
}





[System.Serializable]
public abstract class AttackBlock
{
    public float StartTime;
    public abstract string BlockName { get; }
    public abstract float Duration { get; set; }
}

[System.Serializable]
public class AnimationBlock : AttackBlock
{
    public AnimationType AnimationType;
    public AnimationClip AnimationClip;
    public override string BlockName => "Animation";
    public override float Duration { get => AnimationClip != null ? AnimationClip.length : 0f; set { } }
}

[System.Serializable]
public class HitboxBlock : AttackBlock
{
    public Vector3 Position;
    public Vector3 Scale;
    //public Vector3 Rotation;
    [SerializeField]
    private float _duration = 0f;

    public override string BlockName => "Hitbox";
    public override float Duration { get => _duration; set => _duration = value; }
}

[System.Serializable]
public class VFXBlock : AttackBlock
{
    public ParticleSystem VFX;
    public Vector3 VFXPosition;
    public Vector3 VFXScale = Vector3.one;
    public float VFXRotationZ;
    [SerializeField]
    private float _duration = 0f;

    public override string BlockName => "VFX";
    public override float Duration { get => _duration; set => _duration = value; }
}

[System.Serializable]
public class ShiftBlock : AttackBlock
{
    public Vector3 PositionRelative;
    private float _duration = 0f;

    public override string BlockName => "Shift";
    public override float Duration { get => _duration; set => _duration = value; }

}