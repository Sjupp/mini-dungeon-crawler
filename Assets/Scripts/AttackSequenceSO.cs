using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sequence", menuName = "Create Sequence")]
public class AttackSequenceSO : ScriptableObject
{
    public List<WeaponCommandAndAttack> CommandSequence;
}

[System.Serializable]
public class WeaponCommandAndAttack
{
    public WeaponCommand WeaponCommand;
    public AttackDataSO AttackData;
}
