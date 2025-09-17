using UnityEngine;

namespace HisaCat
{
    [DefaultExecutionOrder(int.MinValue)]
    public class DestroySelfOnAwake : MonoBehaviour
    {
        private void Awake() => Destroy(this.gameObject);
    }
}
