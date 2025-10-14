using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance = null;

    public Action<WeaponCommand, AttackDataSO, int, List<AttackSequenceSO>> Attack;

    [SerializeField]
    private List<AttackSequenceSO> AttackSequences = null;
    [SerializeField]
    private AttackDataSO _defaultSwordAttack = null;
    [SerializeField]
    private AttackDataSO _defaultShieldAttack = null;
    [Space]
    [SerializeField]
    private bool _useTimeout = true;
    [SerializeField]
    private float _comboTimeout = 0.5f;
    private List<WeaponCommand> _commandHistory = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public AttackDataSO GetNextAttack(WeaponCommand command)
    {
        AttackDataSO resultingAttack = null;

        if (_useTimeout)
        {
            if(_commandHistory.Count > 0 && Time.time - _commandHistory.Last().Timestamp > _comboTimeout)
            {
                _commandHistory.Clear();
            }
        }

        _commandHistory.Add(command);

        var matchingSequences = AttackSequences
              .Where(seq => IsPrefix(seq.CommandSequence, _commandHistory, seq))
              .OrderByDescending(seq => seq.CommandSequence.Count)
              .ToList();

        List<AttackSequenceSO> finishedSequences = null;
        if (matchingSequences.Count > 0)
        {
            int sequenceCount = _commandHistory.Count;
            resultingAttack = matchingSequences[0].CommandSequence[sequenceCount - 1].AttackData;

            finishedSequences = matchingSequences.Where(x => x.CommandSequence.Count == sequenceCount).ToList();
            if (finishedSequences.Count > 1)
            {
                for (int i = 0; i < finishedSequences.Count; i++)
                {
                    Debug.Log("Finished " + finishedSequences[i].name);
                }
                Debug.LogWarning("There should probably not be a multiple of finished sequences at once");
            }
        }
        else
        {
            _commandHistory.Clear();
            _commandHistory.Add(command);
            switch (command.WeaponType)
            {
                case WeaponType.Any:
                    Debug.LogError("Tried to return WeaponType.Any as default attack");
                    break;
                case WeaponType.Sword:
                    resultingAttack = _defaultSwordAttack;
                    break;
                case WeaponType.Shield:
                    resultingAttack = _defaultShieldAttack;
                    break;
            }
        }

        Attack?.Invoke(command, resultingAttack, _commandHistory.Count, finishedSequences);

        return resultingAttack;
    }

    static bool IsPrefix(List<WeaponCommandAndAttack> sequence, List<WeaponCommand> currentInput, AttackSequenceSO nameForDebugging = null)
    {
        if (currentInput.Count > sequence.Count)
        {
            //Debug.Log(nameForDebugging.name + " not valid, too short sequence");
            return false;
        }

        for (int i = 0; i < currentInput.Count; i++)
        {
            if (!sequence[i].WeaponCommand.CompareValues(currentInput[i]))
            {
                //Debug.Log(nameForDebugging.name + " not valid, command mismatch");
                return false;
            }
        }

        //Debug.Log(nameForDebugging.name + " still valid sequence");
        return true;
    }
}