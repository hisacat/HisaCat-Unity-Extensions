using HisaCat.UnityExtensions;
using System.Linq;
using UnityEngine;

namespace HisaCat
{
    [DefaultExecutionOrder(int.MinValue)]
    public class DestroyChildenOnAwake : MonoBehaviour
    {
        [SerializeField]
        private Transform[] IgnoreObjects = null;
        private void Awake()
        {
            if (IgnoreObjects == null || IgnoreObjects.Length <= 0)
            {
                transform.DestroyAllChildren();
            }
            else
            {
                var count = transform.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    var child = transform.GetChild(i);

                    if (!IgnoreObjects.Contains(child))
                        Destroy(child.gameObject);
                }
            }
        }
    }
}
