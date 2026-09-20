using HisaCat.Collections;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class CursorStateTicket
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    cursorStateTickets.Clear();
                }
            }

#pragma warning restore IDE0051
#endif
            public readonly object Owner = null;
            public readonly CursorLockMode LockMode = CursorLockMode.None;
            public readonly bool Visible = true;
            public readonly string LogMessage = null;
            public readonly string StackTrace = null;
            public CursorStateTicket(object owner, CursorLockMode lockState, bool visible, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.LockMode = lockState;
                this.Visible = visible;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }

            internal SimpleLinkedList<CursorStateTicket>.Node Node = null;
            internal void SetNode(SimpleLinkedList<CursorStateTicket>.Node node)
            {
                if (ConditionLog.LogError(this.Node != null, $"[{nameof(CursorStateTicket)}] {nameof(SetNode)}: Node already set!"))
                    return;

                this.Node = node;
            }
            internal void ClearNode() => this.Node = null;
        }
        private static SimpleLinkedList<CursorStateTicket> cursorStateTickets = new();
        public static CursorLockMode CurrentCursorLockMode => cursorStateTickets.Count <= 0 ? CursorLockMode.None : cursorStateTickets.Last.Value.LockMode;
        public static bool CurrentCursorVisible => cursorStateTickets.Count <= 0 ? true : cursorStateTickets.Last.Value.Visible;
        public static CursorStateTicket SetCursorState(object owner, CursorLockMode lockState, bool visible, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetCursorState)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new CursorStateTicket(owner, lockState, visible, logMessage, stackTrace);
            var node = cursorStateTickets.AddLast(ticket);
            ticket.SetNode(node);

            UpdateCursorStatus();

            return ticket;
        }
        public static void ClearCursorState(CursorStateTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearCursorState)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            cursorStateTickets.Remove(ticket.Node);
            ticket.ClearNode();

            UpdateCursorStatus();
        }
        private static void UpdateCursorStatus()
        {
            if (Instance == null) return;
            Cursor.lockState = CurrentCursorLockMode;
            Cursor.visible = CurrentCursorVisible;
        }
    }
}
