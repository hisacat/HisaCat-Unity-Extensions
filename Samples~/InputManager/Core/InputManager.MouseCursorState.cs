using HisaCat.Collections;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class MouseCursorStateTicket
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    lockMouseCursorTickets.Clear();
                }
            }

#pragma warning restore IDE0051
#endif
            public readonly object Owner = null;
            public readonly CursorLockMode LockState = CursorLockMode.None;
            public readonly bool Visible = true;
            public readonly string LogMessage = null;
            public readonly string StackTrace = null;
            public MouseCursorStateTicket(object owner, CursorLockMode lockState, bool visible, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.LockState = lockState;
                this.Visible = visible;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }

            internal SimpleLinkedList<MouseCursorStateTicket>.Node Node = null;
            internal void SetNode(SimpleLinkedList<MouseCursorStateTicket>.Node node)
            {
                if (ConditionLog.LogError(this.Node != null, $"[{nameof(MouseCursorStateTicket)}] {nameof(SetNode)}: Node already set!"))
                    return;

                this.Node = node;
            }
            internal void ClearNode() => this.Node = null;
        }
        private static SimpleLinkedList<MouseCursorStateTicket> lockMouseCursorTickets = new();
        public static CursorLockMode CurrentLockState => lockMouseCursorTickets.Count <= 0 ? CursorLockMode.None : lockMouseCursorTickets.Last.Value.LockState;
        public static bool CurrentVisible => lockMouseCursorTickets.Count <= 0 ? true : lockMouseCursorTickets.Last.Value.Visible;
        public static MouseCursorStateTicket SetLockMouseState(object owner, CursorLockMode lockState, bool visible, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetLockMouseState)}: From {(owner == null ? "null" : owner.ToString())}\n" +
                $"LogMessage: {logMessage}\n" +
                $"StackTrace:\n" +
                $"{stackTrace}");

            var ticket = new MouseCursorStateTicket(owner, lockState, visible, logMessage, stackTrace);
            var node = lockMouseCursorTickets.AddLast(ticket);
            ticket.SetNode(node);

            UpdateLockMouseCursorStatus();

            return ticket;
        }
        public static void ClearLockMouseState(MouseCursorStateTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearLockMouseState)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\n" +
                $"LogMessage: {ticket.LogMessage}\n" +
                $"StackTrace:\n" +
                $"{ticket.StackTrace}");

            lockMouseCursorTickets.Remove(ticket.Node);
            ticket.ClearNode();

            UpdateLockMouseCursorStatus();
        }
        private static void UpdateLockMouseCursorStatus()
        {
            if (Instance == null) return;
            Cursor.lockState = CurrentLockState;
            Cursor.visible = CurrentVisible;
        }
    }
}
