using UnityEngine;
using UnityEngine.Tilemaps;

namespace HisaCat.Tilemaps
{
    public class EndlessTilemap : MonoBehaviour
    {
        [SerializeField] private GameObject m_SourceRoot = null;
        [SerializeField] private Grid m_SourceGrid = null;
        [SerializeField] private Tilemap m_SourceTilemap = null;
        //[HisaCat.ReadOnly] [SerializeField] private Vector3Int m_SourceOrigin = default;
        //[HisaCat.ReadOnly] [SerializeField] private Vector3Int m_SourceSize = default;

        [SerializeField] private GameObject m_Cam = null;

        private void Reset()
        {
            if (this.m_SourceRoot == null)
            {
                var sourceTransform = this.transform.Find("EndlessRoot"); ;
                this.m_SourceRoot = sourceTransform == null ? null : sourceTransform.gameObject;
            }

            if (this.m_SourceRoot != null)
            {
                if (this.m_SourceGrid == null) this.m_SourceGrid = this.m_SourceRoot.GetComponentInChildren<Grid>();
                if (this.m_SourceTilemap == null)
                {
                    if (this.m_SourceGrid != null)
                        this.m_SourceTilemap = this.m_SourceGrid.GetComponentInChildren<Tilemap>();
                }
            }
        }

        private GameObject cloneForLR = null;
        private GameObject cloneForBT = null;
        private GameObject cloneForCorner = null;
        private void Awake()
        {
            //Expand tilemap
            this.cloneForLR = Instantiate(this.m_SourceRoot, this.m_SourceRoot.transform.parent);
            this.cloneForBT = Instantiate(this.m_SourceRoot, this.m_SourceRoot.transform.parent);
            this.cloneForCorner = Instantiate(this.m_SourceRoot, this.m_SourceRoot.transform.parent);
        }

        private void OnDrawGizmos()
        {
            if (this.m_SourceTilemap != null)
            {
                var lb = this.m_SourceTilemap.CellToWorld(this.m_SourceTilemap.origin);
                var rt = this.m_SourceTilemap.CellToWorld(this.m_SourceTilemap.origin + this.m_SourceTilemap.size);

                Color color;
                color = Color.green; color.a = 0.3f;
                Gizmos.color = color;
                Gizmos.DrawLine(new Vector2(lb.x, lb.y), new Vector3(rt.x, lb.y));
                Gizmos.DrawLine(new Vector2(lb.x, rt.y), new Vector3(rt.x, rt.y));
                Gizmos.DrawLine(new Vector2(lb.x, lb.y), new Vector3(lb.x, rt.y));
                Gizmos.DrawLine(new Vector2(rt.x, lb.y), new Vector3(rt.x, rt.y));

                var center = this.m_SourceTilemap.CellToWorld(this.m_SourceTilemap.origin + this.m_SourceTilemap.size / 2);
                color = Color.yellow; color.a = 0.3f;
                Gizmos.color = color;
                Gizmos.DrawLine(new Vector2(center.x, lb.y), new Vector3(center.x, rt.y));
                Gizmos.DrawLine(new Vector2(lb.x, center.y), new Vector3(rt.x, center.y));
            }
        }

        public void SetCameraObject(GameObject camObject)
        {
            this.m_Cam = camObject;
        }

        private void LateUpdate()
        {
            if (this.m_Cam == null)
                return;

            UpdatePosition();
            UpdateExpand();
        }

        /// <summary>
        /// Move position it self per rect
        /// </summary>
        private void UpdatePosition()
        {
            var camPos = this.m_Cam.transform.position;
            var lbLocalPos = this.m_SourceTilemap.CellToLocal(this.m_SourceTilemap.origin);
            var rtLocalPos = this.m_SourceTilemap.CellToLocal(this.m_SourceTilemap.origin + this.m_SourceTilemap.size);
            var worldWidth = (rtLocalPos.x - lbLocalPos.x) * this.transform.lossyScale.x;
            var worldHeight = (rtLocalPos.y - lbLocalPos.y) * this.transform.lossyScale.x;

            var newPos = this.transform.position;
            newPos.x = ((int)((Mathf.Abs(camPos.x) + worldWidth / 2f) / worldWidth) * worldWidth) * (camPos.x > 0 ? 1f : -1f);
            newPos.y = ((int)((Mathf.Abs(camPos.y) + worldHeight / 2f) / worldHeight) * worldHeight) * (camPos.y > 0 ? 1f : -1f);
            this.transform.position = newPos;
        }

        /// <summary>
        /// Expand 3 grid (Left-Right, Bottom-Top, and Corners)
        /// </summary>
        private void UpdateExpand()
        {
            var camPos = this.m_Cam.transform.position;
            var centerWorldPos = this.m_SourceTilemap.CellToWorld(this.m_SourceTilemap.origin + this.m_SourceTilemap.size / 2);
            bool isRight = camPos.x > centerWorldPos.x;
            bool isTop = camPos.y > centerWorldPos.y;

            var gridLocalSize = Vector3.Scale(this.m_SourceTilemap.size, this.m_SourceGrid.cellSize);
            var sourcePosZ = this.m_SourceGrid.transform.localPosition.z;
            this.cloneForLR.transform.localPosition = new Vector3(isRight ? gridLocalSize.x : -gridLocalSize.x, 0, sourcePosZ);
            this.cloneForBT.transform.localPosition = new Vector3(0, isTop ? gridLocalSize.y : -gridLocalSize.y, sourcePosZ);
            this.cloneForCorner.transform.localPosition = new Vector3(isRight ? gridLocalSize.x : -gridLocalSize.x, isTop ? gridLocalSize.y : -gridLocalSize.y, sourcePosZ);
        }
    }
}
