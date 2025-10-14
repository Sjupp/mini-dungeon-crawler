using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileSpriteBulkMatcher : EditorWindow
{
    private List<Tile> tileList = new List<Tile>();
    private List<Sprite> spriteList = new List<Sprite>();

    private Vector2 tileScroll, spriteScroll;

    private const float ListHeight = 150;

    [MenuItem("Tools/Bulk Tile-Sprite Matcher")]
    public static void ShowWindow()
    {
        GetWindow<TileSpriteBulkMatcher>("Bulk Tile-Sprite Matcher");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tile List", EditorStyles.boldLabel);
        DrawDropArea<Tile>(ref tileList, ref tileScroll, "Drop Tiles here");

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Sprite List", EditorStyles.boldLabel);
        DrawDropArea<Sprite>(ref spriteList, ref spriteScroll, "Drop Sprites here");

        EditorGUILayout.Space(15);

        // Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Tiles"))
            tileList.Clear();

        if (GUILayout.Button("Clear Sprites"))
            spriteList.Clear();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUI.enabled = tileList.Count > 0 && spriteList.Count > 0;

        if (GUILayout.Button("Apply Matching"))
            ApplyMatching();

        GUI.enabled = true;
    }

    // Draws a drop area that accepts multiple assets of a given type
    private void DrawDropArea<T>(ref List<T> list, ref Vector2 scroll, string label) where T : Object
    {
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, ListHeight, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, label, EditorStyles.helpBox);

        // Handle drag events
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object dragged in DragAndDrop.objectReferences)
                    {
                        if (dragged is T typedObj && !list.Contains(typedObj))
                        {
                            list.Add(typedObj);
                        }
                    }
                }

                evt.Use();
            }
        }

        // Scrollable view to display current list
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(ListHeight));
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = (T)EditorGUILayout.ObjectField($"{i}", list[i], typeof(T), false);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                list.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void ApplyMatching()
    {
        int matchCount = Mathf.Min(tileList.Count, spriteList.Count);
        int updated = 0;

        for (int i = 0; i < matchCount; i++)
        {
            if (tileList[i] != null && spriteList[i] != null)
            {
                tileList[i].sprite = spriteList[i];
                EditorUtility.SetDirty(tileList[i]);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Assigned {updated} sprites to tiles.");
    }
}
