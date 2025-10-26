using System.Collections.Generic;
using UnityEngine;

public class AlternatingHitboxes
{
    private Queue<Hitbox> _availableHitboxes = new Queue<Hitbox>();
    private Dictionary<HitboxBlock, Hitbox> _assignedHitboxBlocks = new();

    public void ActivateHitbox(HitboxBlock hitboxBlock, DamageInfo damageInfo, Transform transform)
    {
        if (_availableHitboxes.TryDequeue(out Hitbox hitbox))
        {
            _assignedHitboxBlocks.Add(hitboxBlock, hitbox);
            hitbox.Activate(hitboxBlock, damageInfo);
        }
        else
        {
            var go = new GameObject("Hitbox");
            go.transform.SetParent(transform, false);
            var hitboxComponent = go.AddComponent<Hitbox>();
            var colliderComponent = go.AddComponent<BoxCollider2D>();
            colliderComponent.isTrigger = true;
            hitboxComponent.Init(colliderComponent);

            _assignedHitboxBlocks.Add(hitboxBlock, hitboxComponent);
            hitboxComponent.Activate(hitboxBlock, damageInfo);
        }
    }
    
    public void Cancel(HitboxBlock hitboxBlock)
    {
        if (_assignedHitboxBlocks.TryGetValue(hitboxBlock, out Hitbox hitbox))
        {
            hitbox.Cancel();
            _availableHitboxes.Enqueue(hitbox);
        }
        bool success = _assignedHitboxBlocks.Remove(hitboxBlock);
    }
}
