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

        // Add a button
        if (GUILayout.Button("Generate Attractors"))
        {
            generator.GenerateAttractors();

            // Mark scene dirty so Unity knows something changed
            EditorUtility.SetDirty(generator);
        }  
    }
}
