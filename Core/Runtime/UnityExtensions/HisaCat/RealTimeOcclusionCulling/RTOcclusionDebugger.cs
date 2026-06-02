using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// RTOcclusion 시스템의 성능 통계를 화면에 표시하는 디버거입니다.
    /// </summary>
    public class RTOcclusionDebugger : MonoBehaviour
    {
        [SerializeField] private bool showStats = true;
        [SerializeField] private bool showDetailedStats = false;
        [SerializeField] private Vector2 position = new Vector2(10, 10);
        [SerializeField] private int fontSize = 14;

        private GUIStyle textStyle;

        private void OnGUI()
        {
            if (!showStats) return;
            if (Event.current.type != EventType.Repaint) return;

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.UpperLeft
                };
            }

            var stats = RTOcclusionManager.CurrentStats;

            string displayText = GetDisplayText(stats);

            // 반투명 배경
            var lines = displayText.Split('\n').Length;
            var bgRect = new Rect(position.x - 5, position.y - 5, 350, lines * (fontSize + 4) + 10);
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(bgRect, "");
            GUI.color = Color.white;

            // 텍스트 표시
            GUI.Label(new Rect(position.x, position.y, 400, 400), displayText, textStyle);
        }

        private readonly System.Text.StringBuilder sb = new();
        private string GetDisplayText(RTOcclusionManager.CullingStats stats)
        {
            // 성능에 따라 색상 변경
            string timeColor = stats.LastUpdateTimeMs < 2f ? "lime"
                             : stats.LastUpdateTimeMs < 5f ? "yellow" : "red";

            sb.Clear();
            sb.AppendLine("<b>RTOcclusion Performance</b>")
              .AppendLine($"Update Time: <color={timeColor}>{stats.LastUpdateTimeMs:F2}ms</color>")
              .AppendLine()
              .AppendLine($"Occludees: {stats.CulledOccludees}/{stats.TotalOccludees} culled")
              .AppendLine($"Occluders: {stats.CulledOccluders}/{stats.TotalOccluders} culled");

            if (showDetailedStats)
            {
                sb.AppendLine()
                  .AppendLine($"Occluders: {stats.TotalOccluders}")
                  .AppendLine($"Visible Cells: {stats.VisibleCells}")
                  .AppendLine();

                float cullingRate = stats.TotalOccludees > 0
                    ? stats.CulledOccludees / (float)stats.TotalOccludees * 100f : 0f;
                sb.AppendLine($"Culling Rate: <color=cyan>{cullingRate:F1}%</color>");

                // 예상 절감 렌더링 비용
                sb.AppendLine()
                  .AppendLine($"<b>Performance Gain:</b>")
                  .AppendLine($"Saved Draw Calls: ~{stats.CulledOccludees}");
            }
            return sb.ToString();
        }
    }
}

