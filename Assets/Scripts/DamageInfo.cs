using UnityEngine;

public class DamageInfo
{
    public int Damage;
    public Transform SourceTransform;
    public Faction SourceFaction;

    public DamageInfo() { }
    public DamageInfo(int damage, Transform sourceTransform, Faction sourceFaction)
    {
        Damage = damage;
        SourceTransform = sourceTransform;
        SourceFaction = sourceFaction;
    }
}
