using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AttackSequenceGraphWindow : EditorWindow
{
    private List<AttackSequenceSO> allSequences = new List<AttackSequenceSO>();
    private Vector2 scrollPos;

    private const float NodeWidth = 200f;
    private const float NodeHeight = 100f;
    private const float HorizontalSpacing = 250f;
    private const float VerticalSpacing = 150f;

    private WeaponType filterWeaponA = WeaponType.Any;
    private WeaponType filterWeaponB = WeaponType.Any;

    private const float FilterDropdownWidth = 150f;

    [MenuItem("Tools/Show Attack Sequence Graph")]
    private static void ShowWindow()
    {
        var window = GetWindow<AttackSequenceGraphWindow>("Attack Sequence Graph");
        window.minSize = new Vector2(800, 400);
        window.LoadAllSequences();
    }

    private void LoadAllSequences()
    {
        allSequences.Clear();

        string[] guids = AssetDatabase.FindAssets("t:AttackSequenceSO", new[] { "Assets/Data/Sequences" });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AttackSequenceSO asset = AssetDatabase.LoadAssetAtPath<AttackSequenceSO>(path);
            if (asset != null)
                allSequences.Add(asset);
        }

        Debug.Log($"Loaded {allSequences.Count} attack sequences from Assets/Data/Sequences");
    }

    private void OnGUI()
    {
        DrawFilters();

        List<AttackSequenceSO> filteredSequences = GetFilteredSequences();

        if (filteredSequences.Count == 0)
        {
            EditorGUILayout.HelpBox("No sequences match the selected filters.", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        float contentHeight = filteredSequences.Count * VerticalSpacing + 300;
        float contentWidth = GetMaxSequenceLength(filteredSequences) * HorizontalSpacing + 300;

        Rect canvasRect = GUILayoutUtility.GetRect(contentWidth, contentHeight);

        Handles.BeginGUI();

        for (int s = 0; s < filteredSequences.Count; s++)
        {
            AttackSequenceSO sequence = filteredSequences[s];

            if (sequence == null || sequence.CommandSequence == null || sequence.CommandSequence.Count == 0)
                continue;

            for (int i = 0; i < sequence.CommandSequence.Count; i++)
            {
                var cmd = sequence.CommandSequence[i];
                Rect nodeRect = GetNodeRect(i, s);
                DrawNode(cmd, nodeRect, i, sequence.name);

                if (i < sequence.CommandSequence.Count - 1)
                {
                    Rect nextNodeRect = GetNodeRect(i + 1, s);
                    DrawArrow(nodeRect, nextNodeRect);
                }
            }
        }

        Handles.EndGUI();
        EditorGUILayout.EndScrollView();
    }

    private void DrawFilters()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Weapon Type Filters", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Weapon Type A", GUILayout.Width(100));
        filterWeaponA = (WeaponType)EditorGUILayout.EnumPopup(filterWeaponA, GUILayout.Width(FilterDropdownWidth));

        GUILayout.Space(20);

        GUILayout.Label("Weapon Type B", GUILayout.Width(100));
        filterWeaponB = (WeaponType)EditorGUILayout.EnumPopup(filterWeaponB, GUILayout.Width(FilterDropdownWidth));

        GUILayout.Space(20);

        if (GUILayout.Button("Clear Filters", GUILayout.Width(120)))
        {
            filterWeaponA = WeaponType.Any;
            filterWeaponB = WeaponType.Any;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
    }

    private List<AttackSequenceSO> GetFilteredSequences()
    {
        if (filterWeaponA == WeaponType.Any && filterWeaponB == WeaponType.Any)
            return allSequences;

        return allSequences.Where(seq =>
            seq.CommandSequence.Any(cmd =>
                MatchesWeaponFilter(cmd.WeaponCommand.WeaponType)
            )).ToList();
    }

    private bool MatchesWeaponFilter(WeaponType weaponType)
    {
        if (filterWeaponA == WeaponType.Any && filterWeaponB == WeaponType.Any)
            return true;

        return weaponType == filterWeaponA || weaponType == filterWeaponB;
    }

    private int GetMaxSequenceLength(List<AttackSequenceSO> sequences)
    {
        return sequences.Max(seq => seq.CommandSequence?.Count ?? 0);
    }

    private Rect GetNodeRect(int index, int sequenceIndex)
    {
        float x = 20 + index * HorizontalSpacing;
        float y = 50 + sequenceIndex * VerticalSpacing;
        return new Rect(x, y, NodeWidth, NodeHeight);
    }

    private void DrawNode(WeaponCommandAndAttack command, Rect rect, int stepIndex, string sequenceName)
    {
        GUI.Box(rect, GUIContent.none);
        GUILayout.BeginArea(rect);
        GUILayout.Label($"{sequenceName} — Step {stepIndex + 1}", EditorStyles.boldLabel);
        GUILayout.Label($"Weapon: {command.WeaponCommand.WeaponType}", EditorStyles.label);
        GUILayout.Label($"Input: {command.WeaponCommand.InputType}", EditorStyles.label);
        GUILayout.Label($"Time: {command.WeaponCommand.Timestamp:0.00}s", EditorStyles.label);
        GUILayout.Label($"Attack: {(command.AttackData != null ? command.AttackData.name : "N/A")}", EditorStyles.label);
        GUILayout.Label($"Damage: {(command.AttackData != null ? command.AttackData.Damage : "N/A")}", EditorStyles.label);
        GUILayout.EndArea();
    }

    private void DrawArrow(Rect from, Rect to)
    {
        Vector3 startPos = new Vector3(from.xMax, from.center.y);
        Vector3 endPos = new Vector3(to.xMin, to.center.y);
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowHeadOffset = direction * 10f;

        Handles.DrawLine(startPos, endPos);

        // Draw arrowhead
        Vector3 perp = Vector3.Cross(direction, Vector3.forward) * 5f;
        Handles.DrawLine(endPos, endPos - arrowHeadOffset + perp);
        Handles.DrawLine(endPos, endPos - arrowHeadOffset - perp);
    }
}
