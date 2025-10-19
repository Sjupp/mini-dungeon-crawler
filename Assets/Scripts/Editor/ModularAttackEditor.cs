using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ModularAttackEditor : EditorWindow
{
    private AttackDataSO attackData;
    private Vector2 scrollPos;

    private float newDurationOverride;
    private const float TimelineHorizontalPadding = 20f;
    private const float BaseRowHeight = 28f;
    private const float ExtraHeightPerBlock = 12f;

    private static readonly float TimelineHeight = 30f;
    private static readonly float TimelinePadding = 1f;
    private static readonly float BlockPixelWidth = 10f;

    [MenuItem("Tools/Modular Attack Editor")]
    public static void OpenWindow()
    {
        GetWindow<ModularAttackEditor>("Modular Attack Editor");
    }

    private void OnGUI()
    {
        attackData = (AttackDataSO)EditorGUILayout.ObjectField("Attack Data", attackData, typeof(AttackDataSO), false);

        if (attackData == null)
        {
            EditorGUILayout.HelpBox("Assign an AttackDataSO to begin editing.", MessageType.Info);
            return;
        }

        // Attack-level metadata
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Attack Metadata", EditorStyles.boldLabel);

        attackData.MovementState = (MovementState)EditorGUILayout.EnumPopup("Movement State", attackData.MovementState);
        attackData.Damage = EditorGUILayout.IntField("Damage", attackData.Damage);

        EditorGUILayout.Space(10);

        // Show TotalDurationOverride as read-only
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.FloatField("Total Duration Override", attackData.TotalDurationOverride);
        EditorGUI.EndDisabledGroup();

        // Temporary field to input new value
        newDurationOverride = EditorGUILayout.FloatField("New Duration Override", newDurationOverride);

        // Apply button
        if (GUILayout.Button("Set Total Duration Override"))
        {
            Undo.RecordObject(attackData, "Change Total Duration Override");
            attackData.TotalDurationOverride = newDurationOverride;
            EditorUtility.SetDirty(attackData);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        float totalDuration = ComputeTotalDuration();
        EditorGUILayout.LabelField($"Total Duration: {totalDuration:0.00} seconds", EditorStyles.boldLabel);

        // Draw master timeline with draggable blocks
        DrawMasterTimeline(totalDuration);

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        Type[] blockTypes = { typeof(AnimationBlock), typeof(HitboxBlock), typeof(VFXBlock), typeof(ShiftBlock) };
        const float blockInspectorWidth = 300f;
        const float blockInspectorHeight = 200f;

        foreach (var blockType in blockTypes)
        {
            var blocksOfType = attackData.Blocks.Where(b => b.GetType() == blockType).ToList();

            EditorGUILayout.LabelField(blockType.Name.Replace("Block", "") + " Blocks", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            // Draw existing blocks
            foreach (var block in blocksOfType.ToList())
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Width(blockInspectorWidth), GUILayout.Height(blockInspectorHeight));
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(block.BlockName, EditorStyles.boldLabel);

                bool removed = false;
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    Undo.RecordObject(attackData, "Remove Block");
                    attackData.Blocks.Remove(block);
                    EditorUtility.SetDirty(attackData);
                    GUI.changed = true;
                    removed = true;
                }

                EditorGUILayout.EndHorizontal();

                if (removed)
                {
                    EditorGUILayout.EndVertical();
                    break;
                }

                float maxStartTime = Mathf.Max(0f, totalDuration);
                float newStartTime = EditorGUILayout.FloatField("Start Time", block.StartTime);
                newStartTime = Mathf.Clamp(newStartTime, 0f, maxStartTime);

                if (!Mathf.Approximately(newStartTime, block.StartTime))
                {
                    Undo.RecordObject(attackData, "Change Block Start Time");
                    block.StartTime = newStartTime;
                    EditorUtility.SetDirty(attackData);
                }

                DrawBlockInspector(block);

                EditorGUILayout.EndVertical();
            }

            // Right-side Add / Duplicate buttons
            EditorGUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add New", GUILayout.Height(40)))
            {
                Undo.RecordObject(attackData, $"Add {blockType.Name}");
                var newBlock = (AttackBlock)Activator.CreateInstance(blockType);
                newBlock.StartTime = 0f;
                newBlock = DefaultValues(newBlock);
                attackData.Blocks.Add(newBlock);
                EditorUtility.SetDirty(attackData);
                GUI.changed = true;

                var asd = new AnimationBlock();
            }

            if (blocksOfType.Count > 0 && GUILayout.Button("Duplicate Last", GUILayout.Height(40)))
            {
                var lastBlock = blocksOfType.Last();
                string json = JsonUtility.ToJson(lastBlock);
                var duplicatedBlock = (AttackBlock)JsonUtility.FromJson(json, blockType);

                if (duplicatedBlock != null)
                {
                    Undo.RecordObject(attackData, $"Duplicate {blockType.Name}");
                    duplicatedBlock.StartTime = lastBlock.StartTime + 0.1f; // Slight offset
                    attackData.Blocks.Add(duplicatedBlock);
                    EditorUtility.SetDirty(attackData);
                    GUI.changed = true;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(attackData);
            Repaint();
        }

        EditorGUILayout.EndScrollView();

        //GUI.FocusControl(null);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(attackData);
        }
    }

    private void DrawMasterTimeline(float totalDuration)
    {
        // Master timeline container
        float fullWidth = EditorGUIUtility.currentViewWidth;
        float outerPadding = TimelineHorizontalPadding;

        Rect timelineRect = GUILayoutUtility.GetRect(fullWidth, TimelineHeight);
        timelineRect.xMin += outerPadding;
        timelineRect.xMax -= outerPadding;

        // Draw timeline background
        EditorGUI.DrawRect(timelineRect, new Color(0.2f, 0.2f, 0.2f));
        // Timing ticks (aligned to content start)
        DrawTimingTicks(timelineRect, totalDuration);


        EditorGUILayout.Space(10);

        Type[] blockTypes = { typeof(AnimationBlock), typeof(HitboxBlock), typeof(VFXBlock), typeof(ShiftBlock)};

        foreach (var blockType in blockTypes)
        {
            var blocks = attackData.Blocks.Where(b => b.GetType() == blockType).ToList();
            int blockCount = blocks.Count;

            if (blockCount == 0) continue;

            float rowHeight = BaseRowHeight + (blockCount - 1) * ExtraHeightPerBlock;
            Rect r = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, rowHeight + TimelinePadding);

            r.xMin += TimelineHorizontalPadding;
            r.xMax -= TimelineHorizontalPadding;

            float rowContentStartX = r.x;
            float rowContentWidth = r.width;

            // Draw row background
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, rowHeight), new Color(0.15f, 0.15f, 0.15f));

            // Draw each block in its own sub-row
            for (int j = 0; j < blockCount; j++)
            {
                var block = blocks[j];

                float normalizedStart = block.StartTime / totalDuration;
                float normalizedDuration = block.Duration / totalDuration;

                float posX = rowContentStartX + normalizedStart * rowContentWidth;
                float width = Mathf.Max(normalizedDuration * rowContentWidth, BlockPixelWidth);

                float totalSpacing = (blockCount - 1); // 1px between each
                float subTrackHeight = (rowHeight - 4 - totalSpacing) / blockCount;
                float trackY = r.y + 2 + j * (subTrackHeight + 1); // 1px spacing added

                Rect blockRect = new Rect(posX, trackY, width, subTrackHeight);

                EditorGUI.DrawRect(blockRect, GetColorForType(block.GetType()));

                string blockName = block.BlockName;
                if (block is AnimationBlock animBlock && animBlock.AnimationClip != null)
                    blockName = animBlock.AnimationClip.name;

                GUI.Label(new Rect(posX + 2, trackY, width - 4, subTrackHeight), blockName, EditorStyles.whiteMiniLabel);

                // Drag and scroll logic
                EditorGUIUtility.AddCursorRect(blockRect, MouseCursor.Pan);
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                Event e = Event.current;

                switch (e.GetTypeForControl(controlId))
                {
                    case EventType.MouseDown:
                        if (blockRect.Contains(e.mousePosition))
                        {
                            GUIUtility.hotControl = controlId;
                            e.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlId)
                        {
                            Undo.RecordObject(attackData, "Move Block");
                            float deltaNormalized = e.delta.x / rowContentWidth;
                            block.StartTime += deltaNormalized * totalDuration;
                            block.StartTime = Mathf.Max(0f, block.StartTime);
                            e.Use();
                            GUI.changed = true;
                        }
                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId)
                        {
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                        break;

                    case EventType.ScrollWheel:
                        if (blockRect.Contains(e.mousePosition))
                        {
                            Undo.RecordObject(attackData, "Adjust Block Duration");

                            float scrollDelta = -e.delta.y * 0.01f;
                            float newDuration = Mathf.Max(0.01f, block.Duration + scrollDelta);

                            if (block is HitboxBlock hitbox)
                                hitbox.Duration = newDuration;
                            else if (block is VFXBlock vfx)
                                vfx.Duration = newDuration;
                            else if (block is ShiftBlock shift)
                                shift.Duration = newDuration;

                            EditorUtility.SetDirty(attackData);
                            GUI.changed = true;
                            e.Use();
                        }
                        break;
                }
            }
        }
    }

    private void DrawBlockInspector(AttackBlock block)
    {
        switch (block)
        {
            case AnimationBlock anim:
                anim.AnimationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type", anim.AnimationType);
                anim.AnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Clip", anim.AnimationClip, typeof(AnimationClip), false);
                EditorGUILayout.LabelField("Computed Duration", $"{anim.Duration:0.00} sec");
                break;

            case HitboxBlock hit:
                hit.Duration = EditorGUILayout.FloatField("Duration", hit.Duration);
                hit.Position = EditorGUILayout.Vector3Field("Position", hit.Position);
                hit.Scale = EditorGUILayout.Vector3Field("Scale", hit.Scale);
                break;
            case VFXBlock vfx:
                vfx.VFX = (ParticleSystem)EditorGUILayout.ObjectField("VFX", vfx.VFX, typeof(ParticleSystem), false);
                vfx.VFXPosition = EditorGUILayout.Vector3Field("Position", vfx.VFXPosition);
                vfx.VFXScale = EditorGUILayout.Vector3Field("Scale", vfx.VFXScale);
                vfx.VFXRotationZ = EditorGUILayout.FloatField("Rotation Z", vfx.VFXRotationZ);
                vfx.Duration = EditorGUILayout.FloatField("Duration", vfx.Duration);
                break;
            case ShiftBlock shift:
                shift.Duration = EditorGUILayout.FloatField("Duration", shift.Duration);
                shift.PositionRelative = EditorGUILayout.Vector3Field("PositionRelative", shift.PositionRelative);
                break;
            default:
                EditorGUILayout.HelpBox("Unknown block type", MessageType.Warning);
                break;
        }
    }

    private AttackBlock DefaultValues(AttackBlock block)
    {
        if (block is AnimationBlock anim)
        {
            // default values
        }
        else if (block is HitboxBlock hitbox)
        {
            hitbox.Duration = 0.2f;
            hitbox.Position = new Vector3(1f, 0.5f, 0f);
            hitbox.Scale = Vector3.one;
        }
        else if (block is VFXBlock vfx)
        {
            vfx.Duration = 0.2f;

        }
        else if (block is ShiftBlock shift)
        {
            shift.Duration = 0.2f;
            shift.PositionRelative = Vector3.right;
        }

        return block;
    }

    private Color GetColorForType(Type type)
    {
        if (type == typeof(AnimationBlock)) return new Color(0.9f, 0.6f, 0.6f); // red
        if (type == typeof(HitboxBlock)) return new Color(0.6f, 0.9f, 0.6f); // green
        if (type == typeof(VFXBlock)) return new Color(0.6f, 0.6f, 0.9f); // blue
        if (type == typeof(ShiftBlock)) return new Color(0.9f, 0.6f, 0.9f); // pink
        return new Color(0.8f, 0.8f, 0.2f); // default yellow
    }

    private float ComputeTotalDuration()
    {
        if (attackData.TotalDurationOverride != -1f)
            return attackData.TotalDurationOverride;

        if (attackData == null || attackData.Blocks == null || attackData.Blocks.Count == 0)
            return 0.1f;

        return Mathf.Max(.1f, attackData.Blocks.Max(b => b.StartTime + b.Duration));
    }

    private void DrawTimingTicks(Rect timelineRect, float totalDuration)
    {
        Handles.BeginGUI();

        float[] possibleIntervals = { 0.1f, 0.25f, 0.5f, 1f, 2f, 5f, 10f };
        float pixelPerSecond = timelineRect.width / totalDuration;
        float tickInterval = 1f;

        // Pick a good interval
        foreach (var interval in possibleIntervals)
        {
            if (interval * pixelPerSecond >= 50f)
            {
                tickInterval = interval;
                break;
            }
        }

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = 10,
            normal = { textColor = Color.white }
        };

        for (float t = 0; t <= totalDuration; t += tickInterval)
        {
            float x = timelineRect.x + t * pixelPerSecond;

            // Draw vertical tick
            Handles.color = Color.white;
            Handles.DrawLine(
                new Vector3(x, timelineRect.yMin),
                new Vector3(x, timelineRect.yMin + 8)
            );

            // Draw label under tick
            Rect labelRect = new Rect(x - 15, timelineRect.yMin + 10, 30, 16);
            GUI.Label(labelRect, t.ToString("0.#") + "s", labelStyle);
        }

        Handles.EndGUI();
    }

}
