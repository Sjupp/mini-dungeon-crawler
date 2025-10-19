using UnityEngine;

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
    [SerializeField]
    private float _duration = 0f;
    public override string BlockName => "Shift";
    public override float Duration { get => _duration; set => _duration = value; }
}
