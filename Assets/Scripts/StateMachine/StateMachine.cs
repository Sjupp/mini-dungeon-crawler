using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine
{
    private IState _currentState;
    private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();
    private List<Transition> _anyTransitions = new List<Transition>();

    private static List<Transition> EmptyTransitions = new List<Transition>(0);

    private string _currentStateName = null;
    public string currentStateName => _currentStateName;

    public void Tick()
    {
        var transition = GetTransition();
        if (transition != null)
            SetState(transition.To);

        _currentState?.Tick();
    }

    public void SetState(IState state, bool ignoreCurrentStateCheck = false)
    {
        if (state == _currentState && !ignoreCurrentStateCheck/* || _currentState is Dying*/)
            return;

        IState previousState = null;
        if (_currentState != null)
        {
            _currentState.OnExit(state);
            previousState = _currentState;
        }
        _currentState = state;

        _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
        if (_currentTransitions == null)
            _currentTransitions = EmptyTransitions;

        if (previousState != null)
            _currentState.OnEnter(previousState);
        else
            _currentState.OnEnter(null);

        _currentStateName = _currentState.ToString();
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
        {
            transitions = new List<Transition>();
            _transitions[from.GetType()] = transitions;
        }

        transitions.Add(new Transition(to, predicate));
    }

    public void AddAnyTransition(IState state, Func<bool> predicate)
    {
        _anyTransitions.Add(new Transition(state, predicate));
    }

    private class Transition
    {
        public Func<bool> Condition { get; }
        public IState To { get; }

        public Transition(IState to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }

    private Transition GetTransition()
    {
        foreach (var transition in _anyTransitions)
            if (transition.Condition())
                return transition;

        foreach (var transition in _currentTransitions)
            if (transition.Condition())
                return transition;

        return null;
    }

    public List<IState> GetAllStates()
    {
        List<IState> allStates = new List<IState>();

        foreach (var kvp in _transitions)
        {
            foreach (var transition in kvp.Value)
            {
                if (!allStates.Contains(transition.To))
                {
                    allStates.Add(transition.To);
                }
            }
        }

        foreach (var transition in _anyTransitions)
        {
            if (!allStates.Contains(transition.To))
            {
                allStates.Add(transition.To);
            }
        }

        return allStates;
    }

    public bool HasStateOfType<T>(out IState foundState) where T : IState
    {
        if (_currentState is T)
        {
            foundState = _currentState;
            return true;
        }

        foreach (var kvp in _transitions)
        {
            foreach (var transition in kvp.Value)
            {
                if (transition.To is T)
                {
                    foundState = transition.To;
                    return true;
                }
            }
        }

        foreach (var transition in _anyTransitions)
        {
            if (transition.To is T)
            {
                foundState = transition.To;
                return true;
            }
        }

        foundState = null;
        return false;
    }

}