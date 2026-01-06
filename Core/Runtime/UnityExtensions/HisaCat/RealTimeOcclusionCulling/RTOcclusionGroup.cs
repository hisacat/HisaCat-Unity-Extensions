using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    // ID
    // 하나의 Room 단위로 컬링합니다.
    // 카메라는 하나의 Room에 속해 있어야 합니다.
    // 기본적으로 해당 Room을 제외한 다른 Room을 모두 컬링하며,
    // 해당 Room과 인접한 Room중, Occluder에 의해 가려지지 않는 오브젝트들만이 컬링에서 제외되어야 합니다
    // (활성화 대상 Room만을 찾고, 이후 룸 내부에 존재하는 객체만을 대상으로 기존 RTOcclusion의 로직을 활용하면 될 듯)

    // 또한 카메라의 Far plane또한 계산에 염두에 두어야 합니다.
    // 거리 비교에 대해서는, 성능 확보를 위해 셀에 의한 공간 해싱(유사 Spatial Hash), 혹은 kd-tree / 옥트리 등을 활용해야 합니다.
    public class RTOcclusionGroup : RTOcclusionBase
    {
        protected override string ComponentName => nameof(RTOcclusionGroup);

        // TODO: Renderers global logic?
        public IReadOnlyList<RTOccluder> Occluders => this.m_Occluders;
        [SerializeField] private List<RTOccluder> m_Occluders = new();
        public IReadOnlyList<RTOccludee> Occludees => this.m_Occludees;
        [SerializeField] private List<RTOccludee> m_Occludees = new();

        public bool IsCulled { get; private set; }
        public void SetCulling(bool cull, bool force = false)
        {
            if (force == false && this.IsCulled == cull) return;

            this.IsCulled = cull;

            for (int i = 0; i < this.Renderers.Count; i++)
            {
                var renderer = this.Renderers[i];
                if (renderer == null) continue;

                renderer.forceRenderingOff = cull;
            }
        }

        // TODO Check contain cells
    }
}
