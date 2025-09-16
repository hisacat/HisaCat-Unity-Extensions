using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace HisaCat.UnityExtensions
{
    internal static class InternalChildrenUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult[] GetChildrenArray<T, TResult>(T obj, System.Func<T, int> getChildCount, System.Func<T, int, TResult> getChild)
        {
            int count = getChildCount(obj);
            var arr = new TResult[count];
            for (int i = 0; i < count; i++)
                arr[i] = getChild(obj, i);
            return arr;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TResult> GetNestedAllChildrenList<T, TResult>(T obj, System.Func<T, int> getChildCount, System.Func<T, int, T> getChild, System.Func<T, TResult> toResult)
        {
            var allChildren = new List<TResult>();
            Collect(obj, allChildren);
            void Collect(T current, List<TResult> list)
            {
                int count = getChildCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = getChild(current, i);
                    list.Add(toResult(child));
                    Collect(child, list);
                }
            }
            return allChildren;
        }
    }

    public static class ObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSafeName(this Object obj) => obj == null ? "null" : obj.name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InstantiateWithOriginalName<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            var obj = Object.Instantiate(original, parent, worldPositionStays);
            obj.name = original.name;
            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InstantiateWithOriginalName<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            var obj = Object.Instantiate(original, position, rotation, parent);
            obj.name = original.name;
            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InstantiateWithOriginalName<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            var obj = Object.Instantiate(original, position, rotation);
            obj.name = original.name;
            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InstantiateWithOriginalName<T>(T original) where T : Object
        {
            var obj = Object.Instantiate(original);
            obj.name = original.name;
            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Object obj) => obj == null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this Object obj) => obj != null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActuallyNull(this Object obj) => ReferenceEquals(obj, null);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDestroyed(this Object obj) => obj == null && ReferenceEquals(obj, null) == false;
    }

    public static class GameObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform SafeTransform(this GameObject go) => go == null ? null : go.transform;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject[] GetChildrenArray(this GameObject go)
            => InternalChildrenUtil.GetChildrenArray(go.transform, tr => tr.childCount, (tr, i) => tr.GetChild(i).gameObject);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> GetNestedAllChidrenList(this GameObject go)
            => InternalChildrenUtil.GetNestedAllChildrenList(go.transform, tr => tr.childCount, (tr, i) => tr.GetChild(i), tr => tr.gameObject);
    }

    public static class TransformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject SafeGameObject(this Transform transform) => transform == null ? null : transform.gameObject;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform[] GetChildrenArray(this Transform t)
            => InternalChildrenUtil.GetChildrenArray(t, tr => tr.childCount, (tr, i) => tr.GetChild(i));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Transform> GetNestedAllChidrenList(this Transform t)
            => InternalChildrenUtil.GetNestedAllChildrenList(t, tr => tr.childCount, (tr, i) => tr.GetChild(i), tr => tr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyAllChildren(this Transform transform)
        {
            var childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetTransform(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScaleX(this Transform transform, float x)
        {
            var val = transform.localScale;
            val.x = x;
            transform.localScale = val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScaleY(this Transform transform, float y)
        {
            var val = transform.localScale;
            val.y = y;
            transform.localScale = val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            var val = transform.localScale;
            val.z = z;
            transform.localScale = val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetFullPath(this Transform transform)
        {
            string path = "/" + transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = "/" + transform.name + path;
            }
            return path;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsParentOf(this Transform transform, Transform parent)
        {
            if (parent == null) return false;
            return transform.IsChildOf(parent);
        }
    }

    public static class RectTransformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetHorizontalStretch(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1, rt.anchorMax.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVerticalStretch(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(rt.anchorMin.x, 0);
            rt.anchorMax = new Vector2(rt.anchorMax.x, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStretchAll(this RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStretchAll(this RectTransform rt, float left, float right, float top, float bottom)
        {
            SetStretchAll(rt);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(right, top);
        }
    }

    public static class ComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SafeGetComponent<T>(this Component component) where T : Component => component == null ? null : component.GetComponent<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] SafeGetComponents<T>(this Component component) where T : Component => component == null ? null : component.GetComponents<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SafeGetComponentInChildren<T>(this Component component, bool includeInactive = false) where T : Component => component == null ? null : component.GetComponentInChildren<T>(includeInactive);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] SafeGetComponentsInChildren<T>(this Component component, bool includeInactive = false) where T : Component => component == null ? null : component.GetComponentsInChildren<T>(includeInactive);
    }

    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(this Vector3 a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static class SceneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> GetAllGameObjectsRecursiveInScene(this Scene scene)
        {
            var allObjects = new List<GameObject>();
            var roots = scene.GetRootGameObjects();
            int rootCount = roots.Length;
            for (int i = 0; i < rootCount; i++)
            {
                var root = roots[i];
                allObjects.Add(root);
                var children = root.GetNestedAllChidrenList();
                allObjects.AddRange(children);
            }
            return allObjects;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> GetAllGameObjectsRecursive()
        {
            var allObjects = new List<GameObject>();
            var sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                allObjects.AddRange(scene.GetAllGameObjectsRecursiveInScene());
            }
            return allObjects;
        }
    }

    public static class UpdateExtensions
    {
        /// <summary>
        /// Caching values on each frame for supports access history values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class HistoryFrame<T> where T : struct
        {
            /// <summary>
            /// Container of value.
            /// </summary>
            public readonly MonoBehaviour Container;
            /// <summary>
            /// Max capturing duration.
            /// </summary>
            public readonly float Duration;
            /// <summary>
            /// Captured histroy values.
            /// </summary>
            public IReadOnlyList<KeyValuePair<float, T>> StackedValues => stackedValues;
            private readonly List<KeyValuePair<float, T>> stackedValues;

            private readonly bool updateValueManually;

            /// <summary>
            /// Creates HistoryFrame
            /// </summary>
            /// <param name="container">Container of value.</param>
            /// <param name="duration">Max capturing duration.</param>
            /// <param name="updateValueManually">Trues if will update value manually with call <see cref="Update(T)"/></param>
            /// <exception cref="System.ArgumentException"></exception>
            public HistoryFrame(MonoBehaviour container, float duration, bool updateValueManually = false, System.Func<T> getValue = null)
            {
                this.Container = container;
                this.Duration = duration;

                this.stackedValues = new List<KeyValuePair<float, T>>();

                this.updateValueManually = updateValueManually;

                if (this.Duration <= 0)
                    throw new System.ArgumentException($"{nameof(this.Duration)} cannot be bigger then zero!");

                if (this.updateValueManually == false)
                {
                    if (getValue == null)
                        throw new System.ArgumentException($"For use {nameof(this.updateValueManually)}, {nameof(getValue)} function requried.");

                    IEnumerator UpdateValueAutomaticallyRoutine()
                    {
                        while (true)
                        {
                            yield return CachedYieldInstruction.WaitForEndOfFrame();
                            UpdateInternal(getValue());
                        }
                    }
                    this.Container.StartCoroutine(UpdateValueAutomaticallyRoutine());
                }
            }
            /// <summary>
            /// Update value. It normally called from Update function of Behaviour.
            /// </summary>
            /// <param name="value"></param>
            public void Update(T value)
            {
                if (this.updateValueManually == false)
                    throw new System.InvalidOperationException($"{nameof(Update)} function only supports when {nameof(this.updateValueManually)} is true!");

                UpdateInternal(value);
            }
            private void UpdateInternal(T value)
            {
                this.stackedValues.Add(new KeyValuePair<float, T>(Time.time, value));

                while (Time.time - this.stackedValues[0].Key >= Duration)
                    this.stackedValues.RemoveAt(0);
            }

            /// <summary>
            /// Clear captured values.
            /// </summary>
            /// <param name="startValue"></param>
            public void Reset(T startValue)
            {
                this.stackedValues.Clear();
            }
        }
    }

    public static class GizmosExtensions
    {
        /// <summary>
        /// Draws a wire arc.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dir">The direction from which the anglesRange is taken into account</param>
        /// <param name="anglesRange">The angle range, in degrees.</param>
        /// <param name="radius"></param>
        /// <param name="maxSteps">How many steps to use to draw the arc.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawWireArc(bool useY, Vector3 position, Vector3 dir, float anglesRange, float radius, float maxSteps = 20)
        {
            var srcAngles = GetAnglesFromDir(useY, position, dir);
            var initialPos = position;
            var posA = initialPos;
            var stepAngles = anglesRange / maxSteps;
            var angle = srcAngles - anglesRange / 2;
            for (var i = 0; i <= maxSteps; i++)
            {
                var rad = Mathf.Deg2Rad * angle;
                var posB = initialPos;
                posB += useY ?
                    new Vector3(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad), 0) :
                    new Vector3(radius * Mathf.Cos(rad), 0, radius * Mathf.Sin(rad));

                Gizmos.DrawLine(posA, posB);

                angle += stepAngles;
                posA = posB;
            }
            Gizmos.DrawLine(posA, initialPos);
        }

        private static float GetAnglesFromDir(bool useY, Vector3 position, Vector3 dir)
        {
            var forwardLimitPos = position + dir;
            var srcAngles = useY ?
                Mathf.Rad2Deg * Mathf.Atan2(forwardLimitPos.y - position.y, forwardLimitPos.x - position.x) :
                Mathf.Rad2Deg * Mathf.Atan2(forwardLimitPos.z - position.z, forwardLimitPos.x - position.x);

            return srcAngles;
        }

        public static class DrawArrow
        {
            [Conditional("UNITY_EDITOR")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
            {
                Gizmos.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            }

            [Conditional("UNITY_EDITOR")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
            {
                Gizmos.color = color;
                Gizmos.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            }

            [Conditional("UNITY_EDITOR")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
            {
                Debug.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Debug.DrawRay(pos + direction, right * arrowHeadLength);
                Debug.DrawRay(pos + direction, left * arrowHeadLength);
            }

            [Conditional("UNITY_EDITOR")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
            {
                Debug.DrawRay(pos, direction, color);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
                Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
            }
        }
    }

    public static class Unity3DExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetDirection3D(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSqrLen(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude;
        }

        #region Fast distance comparison
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinDistanceFast(Vector3 a, Vector3 b, float distance)
            => (a - b).sqrMagnitude < distance * distance;
        public static bool IsWithinOrAtDistanceFast(Vector3 a, Vector3 b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;
        public static bool IsBeyondDistanceFast(Vector3 a, Vector3 b, float distance)
            => (a - b).sqrMagnitude > distance * distance;
        public static bool IsBeyondOrAtDistanceFast(Vector3 a, Vector3 b, float distance)
            => (a - b).sqrMagnitude >= distance * distance;
        public static bool IsWithinDistanceFast(Vector3Int a, Vector3Int b, float distance)
            => (a - b).sqrMagnitude < distance * distance;
        public static bool IsWithinOrAtDistanceFast(Vector3Int a, Vector3Int b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;
        public static bool IsBeyondDistanceFast(Vector3Int a, Vector3Int b, float distance)
            => (a - b).sqrMagnitude > distance * distance;
        public static bool IsBeyondOrAtDistanceFast(Vector3Int a, Vector3Int b, float distance)
            => (a - b).sqrMagnitude >= distance * distance;
        #endregion Fast distance comparison
    }

    public static class Unity2DExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetDirection2D(Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSqrLen(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSqrLen(Vector2Int a, Vector2Int b)
        {
            return ((Vector2)a - (Vector2)b).sqrMagnitude;
        }

        #region Fast distance comparison
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinDistanceFast(Vector2 a, Vector2 b, float distance)
            => (a - b).sqrMagnitude < distance * distance;
        public static bool IsWithinOrAtDistanceFast(Vector2 a, Vector2 b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;
        public static bool IsBeyondDistanceFast(Vector2 a, Vector2 b, float distance)
            => (a - b).sqrMagnitude > distance * distance;
        public static bool IsBeyondOrAtDistanceFast(Vector2 a, Vector2 b, float distance)
            => (a - b).sqrMagnitude >= distance * distance;
        public static bool IsWithinDistanceFast(Vector2Int a, Vector2Int b, float distance)
            => (a - b).sqrMagnitude < distance * distance;
        public static bool IsWithinOrAtDistanceFast(Vector2Int a, Vector2Int b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;
        public static bool IsBeyondDistanceFast(Vector2Int a, Vector2Int b, float distance)
            => (a - b).sqrMagnitude > distance * distance;
        public static bool IsBeyondOrAtDistanceFast(Vector2Int a, Vector2Int b, float distance)
            => (a - b).sqrMagnitude >= distance * distance;
        #endregion Fast distance comparison

        /// <summary>
        /// 하나의 선분에서 특정 위치에 가장 인접한 위치를 구하는 함수.
        /// </summary>
        /// <param name="lineStart">선분 시작점</param>
        /// <param name="lineEnd">선분 끝점</param>
        /// <param name="point">대상 위치</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetNearPointInLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            var line = lineEnd - lineStart;
            var lineDir = line.normalized;
            var v = point - lineStart;
            var cyanDot = Vector2.Dot(v, lineDir);
            cyanDot = Mathf.Clamp(cyanDot, 0, line.magnitude);
            var nearPoint = lineStart + lineDir * cyanDot;
            return nearPoint;
        }

        /// <summary>
        /// 0 is up(1,0). clockwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Deg2Normal(float degree, float magnitude = 1f) => Rad2Normal(degree * Mathf.Deg2Rad, magnitude);
        /// <summary>
        /// 0 is up(1,0). clockwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Rad2Normal(float radian, float magnitude = 1f)
        {
            return new Vector2(Mathf.Sin(radian) * magnitude, Mathf.Cos(radian) * magnitude);
        }
        /// <summary>
        /// 0 is up(1,0). clockwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalToDeg(Vector2 normal) => NormalToRad(normal) * Mathf.Rad2Deg;
        /// <summary>
        /// 0 is up(1,0). clockwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalToRad(Vector2 normal)
        {
            return Mathf.Atan2(normal.x, normal.y);
        }
    }

    public static class RecursiveGameObjectUtil
    {
        public enum RecursiveIgnoreState : int
        {
            None,
            IgnoreSelfOnly,
            IgnoreSelfAndChildren,
            IgnoreChildrenOnly,
        }

        /// <summary>
        /// Invoke action for gameObject and its children.
        /// If action returns false, the action will be stopped.
        /// </summary>
        /// <param name="gameObject">The gameObject to invoke action for.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="shouldIgnore">The function to determine if the gameObject should be ignored.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvokeActionRecursive(this GameObject gameObject, System.Func<GameObject, bool> action, System.Func<GameObject, RecursiveIgnoreState> shouldIgnore = null)
        {
            var ignoreState = shouldIgnore == null ? RecursiveIgnoreState.None : shouldIgnore(gameObject);

            bool ignoreSelf = false;
            bool ignoreChildren = false;
            switch (ignoreState)
            {
                case RecursiveIgnoreState.None:
                    (ignoreSelf, ignoreChildren) = (false, false);
                    break;
                case RecursiveIgnoreState.IgnoreSelfOnly:
                    (ignoreSelf, ignoreChildren) = (true, false);
                    break;
                case RecursiveIgnoreState.IgnoreSelfAndChildren:
                    (ignoreSelf, ignoreChildren) = (true, true);
                    return;
                case RecursiveIgnoreState.IgnoreChildrenOnly:
                    (ignoreSelf, ignoreChildren) = (false, true);
                    return;
            }

            if (ignoreSelf == false)
            {
                var shouldContinue = action(gameObject);
                if (shouldContinue == false) return;
            }

            if (ignoreChildren) return;

            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                InvokeActionRecursive(child, action, shouldIgnore);
            }
        }

        /// <summary>
        /// Invoke action for gameObject and its children.
        /// If action returns false, the action will be stopped.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <param name="gameObject">The gameObject to invoke action for.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="shouldIgnore">The function to determine if the gameObject should be ignored.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentRecursive<T>(this GameObject gameObject, System.Func<GameObject, T, bool> action, System.Func<GameObject, RecursiveIgnoreState> shouldIgnore = null)
        {
            InvokeActionRecursive(gameObject, Action, shouldIgnore);
            bool Action(GameObject gameObject) => action.Invoke(gameObject, gameObject.GetComponent<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLayerRecursive(this GameObject gameObject, int layer, System.Func<GameObject, RecursiveIgnoreState> shouldIgnore = null)
        {
            InvokeActionRecursive(gameObject, Action, shouldIgnore);
            bool Action(GameObject gameObject)
            {
                gameObject.layer = layer;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsStaticRecursive(this GameObject gameObject, bool isStatic, System.Func<GameObject, RecursiveIgnoreState> shouldIgnore = null)
        {
            if (Application.isPlaying)
                Debug.LogWarning("Setting StaticEditorFlags at runtime has no effect on these systems.");

            InvokeActionRecursive(gameObject, Action, shouldIgnore);
            bool Action(GameObject gameObject)
            {
                gameObject.isStatic = isStatic;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRendererShadowCastingModeRecursive(this GameObject gameObject, ShadowCastingMode shadowCastingMode, System.Func<GameObject, RecursiveIgnoreState> shouldIgnore = null)
        {
            GetComponentRecursive<Renderer>(gameObject, Action, shouldIgnore);
            bool Action(GameObject gameObject, Renderer renderer)
            {
                if (renderer != null) renderer.shadowCastingMode = shadowCastingMode;
                return true;
            }
        }
    }

    public static class LayerMaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLayerInMask(this LayerMask layerMask, int layer) => layerMask == (layerMask | (1 << layer));
    }

    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandom<T>(this IList<T> array)
        {
            return array[Random.Range(0, array.Count)];
        }

        /// <summary>
        /// Iterate elements from first to last.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachFast<T>(this IList<T> array, System.Action<T> action)
        {
            int count = array.Count;
            for (int i = 0; i < count; i++)
                action.Invoke(array[i]);
        }
        /// <summary>
        /// Iterate elements from last to first.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachFromEndFast<T>(this IList<T> array, System.Action<T> action)
        {
            int count = array.Count;
            for (int i = count - 1; i >= 0; i--)
                action.Invoke(array[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LastElement<T>(this IList<T> array) => array[^1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIndexing<T>(this IList<T> array, int index, out T result)
        {
            if (index < 0 || index >= array.Count)
            {
                result = default;
                return false;
            }
            result = array[index];
            return true;
        }
    }

    public static class ListExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefaultNonAlloc<T>(this List<T> list, System.Func<T, bool> predicate)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var element = list[i];
                if (predicate(element)) return element;
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefaultNonAlloc<T>(this IReadOnlyList<T> list, System.Func<T, bool> predicate)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var element = list[i];
                if (predicate(element)) return element;
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyNonAlloc<T>(this IReadOnlyList<T> list, System.Func<T, bool> predicate)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var element = list[i];
                if (predicate(element)) return true;
            }
            return false;
        }
    }

    public static class RandomExtensions
    {
        public struct RandomTableArg<T>
        {
            public float weight;
            public T row;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandomTable<T>(params RandomTableArg<T>[] args)
        {
            var weightSum = args.Sum(e => e.weight);
            var weightTarget = Random.Range(0, weightSum);
            foreach (var current in args)
            {
                weightTarget -= current.weight;
                if (weightTarget <= 0)
                    return current.row;
            }
            //NOTE: This defence logic is basically not executed.
            return args[0].row;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandomTable<T>(IEnumerable<T> table, System.Func<T, float> getWeight)
            => PickRandomTable(table.Select(e => new RandomTableArg<T>() { weight = getWeight(e), row = e }).ToArray());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandomTable<T>(float weight0, T row0, float weight1, T row1)
            => PickRandomTable(new RandomTableArg<T>[] { new() { weight = weight0, row = row0 }, new() { weight = weight1, row = row1 } });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandomTable<T>(float weight0, T row0, float weight1, T row1, float weight2, T row2)
            => PickRandomTable(new RandomTableArg<T>[] { new() { weight = weight0, row = row0 }, new() { weight = weight1, row = row1 }, new() { weight = weight2, row = row2 } });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandomTableTuple<T>(params System.Tuple<float, T>[] tables)
            => PickRandomTable(tables.Select(e => new RandomTableArg<T>() { weight = e.Item1, row = e.Item2 }).ToArray());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateRandomASCIIString(int length)
        {
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                // Uses 32 ~ 126 (Space, A-Z, a-z, numbers, special characters)
                result += (char)Random.Range(62, 126 + 1);
            }

            return result;
        }
    }

    public static class PlayerPrefsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBool(string key, bool value)
            => PlayerPrefs.SetInt(key, value ? 1 : 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBool(string key) => GetBool(key, false);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public static class LinqExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LinqIndexOf<T>(this IEnumerable<T> enumerable, T item)
        {
            int index = 0;
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Equals(item))
                        return index;
                    index++;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new System.ArgumentNullException("source");
            if (selector == null) throw new System.ArgumentNullException("selector");
            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new System.InvalidOperationException("Sequence contains no elements");
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new System.ArgumentNullException("source");
            if (selector == null) throw new System.ArgumentNullException("selector");
            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new System.InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }
    }

    public static class LocalizationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string KeyToLocalized(this string key) => Localization.LocalizationManager.Load(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string KeyToLocalized(this string key, SystemLanguage language) => Localization.LocalizationManager.Load(language, key);
    }

    public static class ColorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetAlpha(this Color color, float alpha) => new(color.r, color.g, color.b, alpha);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 SetAlpha(this Color32 color, byte alpha) => new(color.r, color.g, color.b, alpha);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetRed(this Color color, float red) => new(red, color.g, color.b, color.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 SetRed(this Color32 color, byte red) => new(red, color.g, color.b, color.a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetGreen(this Color color, float green) => new(color.r, green, color.b, color.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 SetGreen(this Color32 color, byte green) => new(color.r, green, color.b, color.a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetBlue(this Color color, float blue) => new(color.r, color.g, blue, color.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 SetBlue(this Color32 color, byte blue) => new(color.r, color.g, blue, color.a);
    }

    public static class StringExtensions
    {
        public readonly struct NamedFormatArgument
        {
            public readonly string Name;
            public readonly object Value;
            public NamedFormatArgument(string name, object value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NamedFormat(this string input, NamedFormatArgument[] arguments)
        {
            int count = arguments.Length;
            for (int i = 0; i < count; i++)
            {
                var argument = arguments[i];

                var pattern = @"({" + System.Text.RegularExpressions.Regex.Escape(argument.Name) + @")+(:+.*?}|})";
                var matches = System.Text.RegularExpressions.Regex.Matches(input, pattern);
                var matchesCount = matches.Count;
                for (int j = 0; j < matchesCount; j++)
                {
                    var match = matches[j];
                    var capture = match.Captures.Count <= 0 ? null : match.Captures[0];
                    if (capture == null) continue;

                    var format = capture.Value.Replace(argument.Name, "0");
                    string result;

                    result = string.Format(format, argument.Value);

                    input = input.Replace(capture.Value, result);
                }
            }

            return input;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NamedFormat(this string input, object p)
        {
            var properties = System.ComponentModel.TypeDescriptor.GetProperties(p);
            int count = properties.Count;
            var arguments = new NamedFormatArgument[count];
            for (int i = 0; i < count; i++)
            {
                var prop = properties[i];
                arguments[i] = new(prop.Name, prop.GetValue(p));
            }

            return NamedFormat(input, arguments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitLine(this string input)
        {
            return input.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);
        }
    }

    public static class PhysicsExtensions
    {
        private static RaycastHitComparer raycastHitComparer = new();
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                raycastHitComparer = new();
            }
        }
#pragma warning restore IDE0051
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SortByClosest(this RaycastHit[] hits, int count)
        {
            System.Array.Sort(hits, 0, count, raycastHitComparer);
        }
        public class RaycastHitComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        public static bool IntersectsBounds(this Collider colliderA, Collider colliderB)
        {
            return colliderA.bounds.Intersects(colliderB.bounds);
        }

        public static bool Overlap(this Collider colliderA, Collider colliderB) => colliderA.Overlap(colliderB, out var _, out var _);
        public static bool Overlap(this Collider colliderA, Collider colliderB, out Vector3 direction, out float distance)
        {
            return Physics.ComputePenetration(
                colliderA, colliderA.transform.position, colliderA.transform.rotation,
                colliderB, colliderB.transform.position, colliderB.transform.rotation,
                out direction, out distance);
        }
    }

    public static class ParentConstraintExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearSources(this ParentConstraint parentConstraint)
        {
            var count = parentConstraint.sourceCount;
            for (int i = count - 1; i >= 0; i--)
                parentConstraint.RemoveSource(i);
        }
    }

    public static class DebugExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogCondition(bool condition, string message, LogType logType)
        {
            if (condition)
            {
                switch (logType)
                {
                    case LogType.Log:
                        Debug.Log(message);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(message);
                        break;
                    case LogType.Error:
                        Debug.LogError(message);
                        break;
                    default:
                        Debug.LogError($"[{nameof(DebugExtensions)}] {nameof(LogCondition)}: Dose not supports log type \"{logType}\"");
                        break;
                }
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ManagedLogCondition(bool condition, string message, LogType logType)
        {
            if (condition)
            {
                switch (logType)
                {
                    case LogType.Log:
                        ManagedDebug.Log(message);
                        break;
                    case LogType.Warning:
                        ManagedDebug.LogWarning(message);
                        break;
                    case LogType.Error:
                        ManagedDebug.LogError(message);
                        break;
                    default:
                        Debug.LogError($"[{nameof(DebugExtensions)}] {nameof(ManagedLogCondition)}: Dose not supports log type \"{logType}\"");
                        break;
                }
                return true;
            }
            return false;
        }
    }

    public static class AnimationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAllClips(this Animation animation)
        {
            List<string> names = new();
            foreach (AnimationState state in animation)
                names.Add(state.name);
            foreach (var name in names)
                animation.RemoveClip(name);
            names.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Play(this Animation animation, AnimationClip clip, float normalizedTime, float speed)
            => Play(animation, clip, clip.name, normalizedTime, speed);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Play(this Animation animation, AnimationClip clip, string newName, float normalizedTime, float speed)
        {
            var state = Sample(animation, clip, newName, normalizedTime);
            state.speed = speed;
            animation.Play(newName);
            return state;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Play(this Animation animation, AnimationClip clip, float normalizedTime, float speed, PlayMode playMode)
            => Play(animation, clip, clip.name, normalizedTime, speed, playMode);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Play(this Animation animation, AnimationClip clip, string newName, float normalizedTime, float speed, PlayMode playMode)
        {
            var state = Sample(animation, clip, newName, normalizedTime);
            state.speed = speed;
            animation.Play(newName, playMode);
            return state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Sample(this Animation animation, AnimationClip clip, float normalizedTime)
            => Sample(animation, clip, clip.name, normalizedTime);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationState Sample(this Animation animation, AnimationClip clip, string newName, float normalizedTime)
        {
            // Set animation clip.
            animation.AddClip(clip, newName);
            animation.clip = clip;

            // Set parameters and sample once.
            var state = animation[newName];
            state.normalizedTime = normalizedTime;
            animation.Sample();

            return state;
        }
    }

    public static class AnimatorExtensions
    {
        public struct AnimatorParameterCache
        {
            public readonly int NameHash;
            public readonly bool BoolValue;
            public readonly int IntValue;
            public readonly float FloatValue;
            public readonly AnimatorControllerParameterType Type;
            public AnimatorParameterCache(int nameHash, bool boolValue)
            {
                this.NameHash = nameHash;
                this.BoolValue = boolValue;
                this.IntValue = default;
                this.FloatValue = default;
                this.Type = AnimatorControllerParameterType.Bool;
            }
            public AnimatorParameterCache(int nameHash, int intValue)
            {
                this.NameHash = nameHash;
                this.BoolValue = default;
                this.IntValue = intValue;
                this.FloatValue = default;
                this.Type = AnimatorControllerParameterType.Int;
            }
            public AnimatorParameterCache(int nameHash, float floatValue)
            {
                this.NameHash = nameHash;
                this.BoolValue = default;
                this.IntValue = default;
                this.FloatValue = floatValue;
                this.Type = AnimatorControllerParameterType.Float;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceController(this Animator animator, RuntimeAnimatorController controller, bool keepParameters, bool keepLayerWeights)
        {
            if (animator.runtimeAnimatorController == controller)
            {
                keepParameters = false;
                keepLayerWeights = false;
            }

            AnimatorParameterCache[] parameterCache = null;
            if (keepParameters)
            {
                parameterCache = new AnimatorParameterCache[animator.parameterCount];
                for (int i = 0; i < animator.parameterCount; i++)
                {
                    var parameter = animator.parameters[i];
                    var nameHash = parameter.nameHash;
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            parameterCache[i] = new(nameHash, animator.GetBool(nameHash));
                            break;
                        case AnimatorControllerParameterType.Int:
                            parameterCache[i] = new(nameHash, animator.GetInteger(nameHash));
                            break;
                        case AnimatorControllerParameterType.Float:
                            parameterCache[i] = new(nameHash, animator.GetFloat(nameHash));
                            break;
                    }
                }
            }
            float[] layerWeights = null;
            if (keepLayerWeights)
            {
                layerWeights = new float[animator.layerCount];
                for (int i = 0; i < animator.layerCount; i++)
                    layerWeights[i] = animator.GetLayerWeight(i);
            }

            animator.runtimeAnimatorController = controller;

            if (keepParameters)
            {
                for (int i = 0; i < parameterCache.Length; i++)
                {
                    var cache = parameterCache[i];
                    switch (cache.Type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            animator.SetBool(cache.NameHash, cache.BoolValue);
                            break;
                        case AnimatorControllerParameterType.Int:
                            animator.SetInteger(cache.NameHash, cache.IntValue);
                            break;
                        case AnimatorControllerParameterType.Float:
                            animator.SetFloat(cache.NameHash, cache.FloatValue);
                            break;
                    }
                }
            }
            if (keepLayerWeights)
            {
                for (int i = 0; i < layerWeights.Length; i++)
                    animator.SetLayerWeight(i, layerWeights[i]);
            }
        }

        private static readonly Dictionary<RuntimeAnimatorController, Dictionary<int, AnimatorControllerParameter>> AnimatorControllerParameterCache = new();
        private static readonly Dictionary<string, int> StringHashCache = new();
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                AnimatorControllerParameterCache.Clear();
                StringHashCache.Clear();
            }
        }
#pragma warning restore IDE0051
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAnimatorControllerParameterCache()
        {
            AnimatorControllerParameterCache.Clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<int, AnimatorControllerParameter> InitializeParameterCache(Animator animator)
        {
            // Animator가 꺼져있는 상태에서는 정상적으로 Parameter를 가져올 수 없음으로, 아무것도 하지 않습니다.
            if (animator.isActiveAndEnabled == false) return null;
            if (animator.runtimeAnimatorController == null) return null;
            if (AnimatorControllerParameterCache.TryGetValue(animator.runtimeAnimatorController, out var parameters) == false)
            {
                parameters = new Dictionary<int, AnimatorControllerParameter>();
                for (int i = 0; i < animator.parameterCount; i++)
                {
                    var parameter = animator.parameters[i];
                    parameters.Add(parameter.nameHash, parameter);
                }
                AnimatorControllerParameterCache.Add(animator.runtimeAnimatorController, parameters);
            }
            return parameters;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAnimatorStringHash(string name)
        {
            if (StringHashCache.TryGetValue(name, out var hash) == false)
            {
                hash = Animator.StringToHash(name);
                StringHashCache.Add(name, hash);
            }
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetBool(this Animator animator, int hash, bool value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Bool)
            {
                return false;
            }

            animator.SetBool(hash, value);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetBool(this Animator animator, string name, bool value)
            => TrySetBool(animator, GetAnimatorStringHash(name), value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBool(this Animator animator, int hash, out bool value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Bool)
            {
                value = default;
                return false;
            }

            value = animator.GetBool(hash);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetInteger(this Animator animator, int hash, int value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Int)
            {
                return false;
            }

            animator.SetInteger(hash, value);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetInteger(this Animator animator, string name, int value)
            => TrySetInteger(animator, GetAnimatorStringHash(name), value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInteger(this Animator animator, int hash, out int value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Int)
            {
                value = default;
                return false;
            }

            value = animator.GetInteger(hash);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetFloat(this Animator animator, int hash, float value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Float)
            {
                return false;
            }

            animator.SetFloat(hash, value);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetFloat(this Animator animator, string name, float value)
            => TrySetFloat(animator, GetAnimatorStringHash(name), value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFloat(this Animator animator, int hash, out float value)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Float)
            {
                value = default;
                return false;
            }

            value = animator.GetFloat(hash);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetTrigger(this Animator animator, int hash)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Trigger)
            {
                return false;
            }

            animator.SetTrigger(hash);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetTrigger(this Animator animator, string name)
            => TrySetTrigger(animator, GetAnimatorStringHash(name));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResetTrigger(this Animator animator, int hash)
        {
            var parameters = InitializeParameterCache(animator);
            if (parameters == null ||
                parameters.TryGetValue(hash, out var parameter) == false ||
                parameter.type != AnimatorControllerParameterType.Trigger)
            {
                return false;
            }

            animator.ResetTrigger(hash);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResetTrigger(this Animator animator, string name)
            => TryResetTrigger(animator, GetAnimatorStringHash(name));
    }

    public static class CharacterControllerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetBottomPoint(this CharacterController characterController)
        {
            return characterController.transform.position + characterController.center - characterController.transform.up * ((characterController.height / 2f) - characterController.radius);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTopPoint(this CharacterController characterController)
        {
            return characterController.transform.position + characterController.center + characterController.transform.up * ((characterController.height / 2f) - characterController.radius);
        }
    }

    public static class ColliderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetBottomPoint(this CapsuleCollider collider)
        {
            return collider.transform.position + collider.center - collider.transform.up * ((collider.height / 2f) - collider.radius);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTopPoint(this CapsuleCollider collider)
        {
            return collider.transform.position + collider.center + collider.transform.up * ((collider.height / 2f) - collider.radius);
        }
    }

    public static class MathExtensions
    {
        /// <summary>
        /// Returns a safe index for circular list navigation.
        /// Converts negative or out-of-bounds indices to proper circular indices.
        /// </summary>
        /// <param name="index">Desired index (can be negative or exceed bounds)</param>
        /// <param name="count">Total count of the list</param>
        /// <returns>Safe index within range 0 ~ (count-1)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCircularIndex(int index, int count)
        {
            if (count <= 0) return 0;
            return ((index % count) + count) % count;
        }

        /// <summary>
        /// Returns true if x is between min and max (inclusive).
        /// </summary>
        /// <param name="x">Value to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if x is between min and max (inclusive)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetweenInclusive(float x, float min, float max)
        {
            return x >= min && x <= max;
        }

        /// <summary>
        /// Returns true if x is between min and max (exclusive).
        /// </summary>
        /// <param name="x">Value to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if x is between min and max (exclusive)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetweenExclusive(float x, float min, float max)
        {
            return x > min && x < max;
        }

        /// <summary>
        /// Returns true if x is between min and max (inclusive).
        /// </summary>
        /// <param name="x">Value to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if x is between min and max (inclusive)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetweenInclusive(int x, int min, int max)
        {
            return x >= min && x <= max;
        }

        /// <summary>
        /// Returns true if x is between min and max (exclusive).
        /// </summary>
        /// <param name="x">Value to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if x is between min and max (exclusive)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetweenExclusive(int x, int min, int max)
        {
            return x > min && x < max;
        }
    }

    public static class HashSetExtensions
    {
        public static int AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            int added = 0;
            using (var enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (hashSet.Add(enumerator.Current))
                        added++;
                }
            }
            return added;
        }
    }
}
