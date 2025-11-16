using UnityEngine;

public class SceneMeshColliderAdder : MonoBehaviour
{
    // This will be called by Room Mesh Event
    public void AddCollider(MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        var go = meshFilter.gameObject;

        var collider = go.GetComponent<MeshCollider>();
        if (collider == null)
            collider = go.AddComponent<MeshCollider>();

        collider.sharedMesh = meshFilter.sharedMesh;
        collider.convex = false;   // keep non-convex for environment
    }
}