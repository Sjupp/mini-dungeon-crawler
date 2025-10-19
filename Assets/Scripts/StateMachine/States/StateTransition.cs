using UnityEngine;

[System.Serializable]
public class StateTransition
{
    [SerializeField]
    public BaseState State;
    [SerializeField]
    public int Weight;
}