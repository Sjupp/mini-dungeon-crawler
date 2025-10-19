using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Create Attack")]
public class AttackDataSO : ScriptableObject
{
    public int Damage;
    //public float Sharpness; ???
    //public float Sturdiness; ???

    public MovementState MovementState = MovementState.Free;

    public float TotalDurationOverride = -1f;

    [SerializeReference]
    public List<AttackBlock> Blocks = new();

    public float GetTotalDuration()
    {
        if (TotalDurationOverride != -1)
        {
            return TotalDurationOverride;
        }
        else
        {
            if (Blocks.Count == 0)
            {
                Debug.LogError("AttackData contains no blocks");
            }
            return Mathf.Max(.1f, Blocks.Max(b => b.StartTime + b.Duration));
        }
    }
}

public enum AnimationType
{
    PlayerAnimation,
    ItemAnimation
}