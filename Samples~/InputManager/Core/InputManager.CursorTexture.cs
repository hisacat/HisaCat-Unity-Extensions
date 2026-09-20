using HisaCat.Collections;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class CursorIconTicket
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    cursorIconTickets.Clear();
                }
            }

#pragma warning restore IDE0051
#endif
            public readonly object Owner = null;
            public readonly Texture2D Texture = null;
            public readonly Vector2 Hotspot = Vector2.zero;
            public readonly CursorMode Mode = CursorMode.Auto;
            public readonly string LogMessage = null;
            public readonly string StackTrace = null;
            public CursorIconTicket(object owner, Texture2D texture, Vector2 hotspot, CursorMode mode, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.Texture = texture;
                this.Hotspot = hotspot;
                this.Mode = mode;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }

            internal SimpleLinkedList<CursorIconTicket>.Node Node = null;
            internal void SetNode(SimpleLinkedList<CursorIconTicket>.Node node)
            {
                if (ConditionLog.LogError(this.Node != null, $"[{nameof(CursorIconTicket)}] {nameof(SetNode)}: Node already set!"))
                    return;

                this.Node = node;
            }
            internal void ClearNode() => this.Node = null;
        }
        private static SimpleLinkedList<CursorIconTicket> cursorIconTickets = new();
        public static Texture2D CurrentCursorTexture => cursorIconTickets.Count <= 0 ? null : cursorIconTickets.Last.Value.Texture;
        public static Vector2 CurrentCursorHotspot => cursorIconTickets.Count <= 0 ? Vector2.zero : cursorIconTickets.Last.Value.Hotspot;
        public static CursorMode CurrentCursorMode => cursorIconTickets.Count <= 0 ? CursorMode.Auto : cursorIconTickets.Last.Value.Mode;
        public static CursorIconTicket SetCursorTexture(object owner, Texture2D texture, Vector2 hotspot, CursorMode mode, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetCursorTexture)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new CursorIconTicket(owner, texture, hotspot, mode, logMessage, stackTrace);
            var node = cursorIconTickets.AddLast(ticket);
            ticket.SetNode(node);

            UpdateCursorTexture();

            return ticket;
        }
        public static void ClearCursorIcon(CursorIconTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearCursorIcon)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            cursorIconTickets.Remove(ticket.Node);
            ticket.ClearNode();

            UpdateCursorTexture();
        }
        private static void UpdateCursorTexture()
        {
            if (Instance == null) return;
            Cursor.SetCursor(CurrentCursorTexture, CurrentCursorHotspot, CurrentCursorMode);
        }
    }
}
