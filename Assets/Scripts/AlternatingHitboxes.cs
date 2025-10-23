using System.Collections.Generic;
using UnityEngine;

public class AlternatingHitboxes : MonoBehaviour
{
    [SerializeField]
    private Hitbox _hitboxPrefab = null;

    private Queue<Hitbox> _availableHitboxes = new Queue<Hitbox>();
    private Dictionary<HitboxBlock, Hitbox> _assignedHitboxBlocks = new();

    public void ActivateHitbox(HitboxBlock hitboxBlock, DamageInfo damageInfo)
    {
        if (_availableHitboxes.TryDequeue(out Hitbox hitbox))
        {
            _assignedHitboxBlocks.Add(hitboxBlock, hitbox);
            hitbox.Activate(hitboxBlock, damageInfo);
        }
        else
        {
            var newHitbox = Instantiate(_hitboxPrefab, transform);
            _assignedHitboxBlocks.Add(hitboxBlock, newHitbox);
            newHitbox.Activate(hitboxBlock, damageInfo);
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
