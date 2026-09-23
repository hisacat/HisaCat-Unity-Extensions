using HisaCat.Collections;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class CursorTextureTicket
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    cursorTextureTickets.Clear();
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
            public CursorTextureTicket(object owner, Texture2D texture, Vector2 hotspot, CursorMode mode, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.Texture = texture;
                this.Hotspot = hotspot;
                this.Mode = mode;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }

            internal SimpleLinkedList<CursorTextureTicket>.Node Node = null;
            internal void SetNode(SimpleLinkedList<CursorTextureTicket>.Node node)
            {
                if (ConditionLog.LogError(this.Node != null, $"[{nameof(CursorTextureTicket)}] {nameof(SetNode)}: Node already set!"))
                    return;

                this.Node = node;
            }
            internal void ClearNode() => this.Node = null;
        }
        private static SimpleLinkedList<CursorTextureTicket> cursorTextureTickets = new();
        public static Texture2D CurrentCursorTexture => cursorTextureTickets.Count <= 0 ? null : cursorTextureTickets.Last.Value.Texture;
        public static Vector2 CurrentCursorHotspot => cursorTextureTickets.Count <= 0 ? Vector2.zero : cursorTextureTickets.Last.Value.Hotspot;
        public static CursorMode CurrentCursorMode => cursorTextureTickets.Count <= 0 ? CursorMode.Auto : cursorTextureTickets.Last.Value.Mode;
        public static CursorTextureTicket SetCursorTexture(object owner, Texture2D texture, Vector2 hotspot, CursorMode mode, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetCursorTexture)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new CursorTextureTicket(owner, texture, hotspot, mode, logMessage, stackTrace);
            var node = cursorTextureTickets.AddLast(ticket);
            ticket.SetNode(node);

            UpdateCursorTexture();

            return ticket;
        }
        public static void ClearCursorTexture(CursorTextureTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearCursorTexture)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            cursorTextureTickets.Remove(ticket.Node);
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
