using HisaCat.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class RTOcclusionCamera : MonoBehaviour
    {
        public const float GridSize = 5f;

        private Camera _camera = null;
        protected Camera Camera => this._camera.IsNotNull() ? this._camera : this._camera = this.GetComponent<Camera>();

        private static HashSet<RTOccluder> occluders = new();
        private static HashSet<RTOccludee> occludees = new();

        private static void ClearAndFetchAllOccluders()
        {
            occluders.Clear();
            occluders.AddRange(FindObjectsByType<RTOccluder>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }
        private static void ClearAndFetchAllOccludees()
        {
            occludees.Clear();
            occludees.AddRange(FindObjectsByType<RTOccludee>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }

        // [NOTE] HashSet을 이용 중임으로, Register 과정에서 Duplicated check를 할 필요가 없음.
        // 만약 다른 Collection을 사용할 경우, 중복 체크를 해야 함.
        public static void RegisterOccluder(RTOccluder occluder) => occluders.Add(occluder);
        public static void RegisterOccludee(RTOccludee occludee) => occludees.Add(occludee);
        public static void UnregisterOccluder(RTOccluder occluder) => occluders.Remove(occluder);
        public static void UnregisterOccludee(RTOccludee occludee) => occludees.Remove(occludee);

        private void Awake()
        {
            ClearAndFetchAllOccluders();
            ClearAndFetchAllOccludees();
        }
        private void OnDestroy()
        {
            occluders.Clear();
            occludees.Clear();
        }
    }
}