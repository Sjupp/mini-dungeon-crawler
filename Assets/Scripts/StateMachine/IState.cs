using UnityEngine;

public interface IState
{
    void Tick();
    void OnEnter(IState state);
    void OnExit(IState state);
}