using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[CustomEditor(typeof(BaseState), true)]
public class AttackBlockEditor : Editor
{
    private SerializedProperty attackBlocksProp;
    private Type[] blockTypes;
    private string[] blockTypeNames;

    private void OnEnable()
    {
        attackBlocksProp = serializedObject.FindProperty("AttackBlocks");

        // Find all concrete subclasses of AttackBlock
        blockTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => typeof(AttackBlock).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        blockTypeNames = blockTypes.Select(t => t.Name.Truncate(t.Name.Count() - 5, "")).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Custom label
        EditorGUILayout.LabelField("Attack Blocks", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);

        // Draw each AttackBlock in the list
        for (int i = 0; i < attackBlocksProp.arraySize; i++)
        {
            SerializedProperty blockProp = attackBlocksProp.GetArrayElementAtIndex(i);
            AttackBlock block = GetTargetBlockAtIndex(i);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"[{i}] {block?.BlockName ?? "Unknown Block"}", EditorStyles.boldLabel);

            // Iterate over all sub-properties of the block
            SerializedProperty iterator = blockProp.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }

            // Remove button
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                attackBlocksProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(5);

        // Horizontal add buttons (compact)
        EditorGUILayout.LabelField("Add New Block", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < blockTypes.Length; i++)
        {
            if (GUILayout.Button(blockTypeNames[i], GUILayout.Width(90)))
            {
                var newBlock = Activator.CreateInstance(blockTypes[i]) as AttackBlock;
                attackBlocksProp.arraySize++;
                var newElement = attackBlocksProp.GetArrayElementAtIndex(attackBlocksProp.arraySize - 1);
                newElement.managedReferenceValue = newBlock;
                break;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();

        // Draw the rest of the inspector
        EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);
        DrawPropertiesExcluding(serializedObject, "AttackBlocks");
    }

    // Helper method to get actual instance from target
    private AttackBlock GetTargetBlockAtIndex(int index)
    {
        if (target is BaseState baseState && baseState.AttackBlocks.Count > index)
        {
            return baseState.AttackBlocks[index];
        }
        return null;
    }
}
