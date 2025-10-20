using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseState : MonoBehaviour, IState
{
    [SerializeReference]
    public List<AttackBlock> AttackBlocks = new();
    [SerializeField]
    public List<StateTransition> StateTransitions = new();

    public abstract void OnEnter(IState state);
    public abstract void OnExit(IState state);
    public abstract void Tick();
    public virtual BaseState WeightedTransitionState()
    {
        int totalWeight = StateTransitions.Sum(x => x.Weight);
        int rand = Random.Range(0, totalWeight);
        int value = 0;
        for (var i = 0; i < StateTransitions.Count; i++)
        {
            var transition = StateTransitions[i];
            value += transition.Weight;
            if (rand < value)
            {
                return transition.State;
            }
        }
        return null;
    }
}