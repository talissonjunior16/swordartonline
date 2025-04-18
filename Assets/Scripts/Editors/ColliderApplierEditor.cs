using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColliderApplier))]
public class ColliderApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button to apply the collider
        ColliderApplier colliderApplier = (ColliderApplier)target;
        if (GUILayout.Button("Apply Collider"))
        {
            colliderApplier.ApplyCollider();
        }
    }
}