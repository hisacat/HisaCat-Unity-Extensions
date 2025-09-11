using UnityEngine;

namespace HisaCat
{
    [DefaultExecutionOrder(short.MinValue)]
    public class DestroySelfOnAwake : MonoBehaviour
    {
        private void Awake() => Destroy(this.gameObject);
    }
}
