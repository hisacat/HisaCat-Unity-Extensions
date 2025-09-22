using UnityEngine;
using UnityEditor;
using HisaCat.HUE.Settings;

namespace HisaCat.HUE.GizmosExtensions
{
    public static class MeshColliderGizmos
    {
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        public static void DrawGizmoSelectionHierarchy(MeshCollider meshCollider, GizmoType gizmoType)
        {
            if (HueSettings.Instance.GizmosSettings.MeshColliderSettings.ShowNonConvexWithRenderer == false)
                return;

            if (meshCollider.convex) return;

            var mesh = meshCollider.sharedMesh;
            if (mesh == null) return;

            if (!meshCollider.TryGetComponent<MeshRenderer>(out var renderer)) return;
            if (renderer == null || renderer.enabled == false) return;

            if (!meshCollider.TryGetComponent<MeshFilter>(out var meshFilter)) return;
            if (meshFilter.sharedMesh != mesh) return;

            Gizmos.color = HueSettings.Instance.GizmosSettings.MeshColliderSettings.NonConvexWithRendererColor;
            Gizmos.matrix = meshCollider.transform.localToWorldMatrix;

            int subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < subMeshCount; i++) Gizmos.DrawWireMesh(mesh, i);
        }
    }
}
