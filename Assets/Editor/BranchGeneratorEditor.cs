using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BranchGenerator))]
public class BranchGeneratorEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        // Draw default fields (mesh, etc.)
        // still see the public variables
        DrawDefaultInspector();

        // Reference to the actual target component
        BranchGenerator generator = (BranchGenerator)target;

        generator.showAttractionRadius = EditorGUILayout.Toggle("Show Attraction Radius", generator.showAttractionRadius);
        generator.showKillRadius = EditorGUILayout.Toggle("Show Kill Radius", generator.showKillRadius);
        generator.showLines = EditorGUILayout.Toggle("Show Lines", generator.showLines);

        if (GUI.changed)
            SceneView.RepaintAll(); // refresh gizmos immediately

        // Add a button
        if (GUILayout.Button("Generate Branches"))
        {
            generator.SpaceColonization();

            // Mark scene dirty so Unity knows something changed
            EditorUtility.SetDirty(generator);
        }  
    }
}
