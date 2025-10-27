using UnityEngine;

public class DamageInfo
{
    public int Damage;
    public int Knockback;
    public Transform SourceTransform;
    public Faction SourceFaction;

    public DamageInfo() { }
    public DamageInfo(int damage, int knockback, Transform sourceTransform, Faction sourceFaction)
    {
        Damage = damage;
        Knockback = knockback;
        SourceTransform = sourceTransform;
        SourceFaction = sourceFaction;
    }
}
