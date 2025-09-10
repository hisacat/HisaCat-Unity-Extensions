#if MIRROR
using UnityEngine;
using UnityEditor;
using Mirror;

namespace HisaCat.Mirror
{
    [CustomEditor(typeof(NetworkIdentity))]
    public class NetworkIdentityInspector : Editor
    {
        private NetworkIdentity Target;
        private void OnEnable()
        {
            this.Target = target as NetworkIdentity;
        }
        private void OnDisable()
        {
            this.Target = null;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (this.Target == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detail View", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"isLocalPlayer: {this.Target.isLocalPlayer}");

            if (NetworkServer.active == false)
                EditorGUILayout.LabelField($"Client Id: Available only on the server");
            else
                EditorGUILayout.LabelField($"Client id: {(this.Target.connectionToClient == null ? "null" : this.Target.connectionToClient.connectionId)}");
        }
    }
}
#endif
