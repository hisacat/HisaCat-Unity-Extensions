using UnityEngine;

namespace HisaCat
{
    public class DestroySelfOnAwake : MonoBehaviour
    {
        private void Awake() => Destroy(this.gameObject);
    }
}
