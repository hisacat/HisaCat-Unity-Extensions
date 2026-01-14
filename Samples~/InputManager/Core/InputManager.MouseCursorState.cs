using HisaCat.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class MouseCursorStateTicket
        {
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
        }
        private static LinkedList<MouseCursorStateTicket> lockMouseCursorTickets = new();
        public static CursorLockMode CurrentLockState => lockMouseCursorTickets.Count <= 0 ? CursorLockMode.None : lockMouseCursorTickets.Last.Value.LockState;
        public static bool CurrentVisible => lockMouseCursorTickets.Count <= 0 ? true : lockMouseCursorTickets.Last.Value.Visible;
        public static MouseCursorStateTicket SetLockMouseState(object owner, CursorLockMode lockState, bool visible, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetLockMouseState)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new MouseCursorStateTicket(owner, lockState, visible, logMessage, stackTrace);
            lockMouseCursorTickets.AddLast(ticket);

            UpdateLockMouseCursorStatus();

            return ticket;
        }
        public static void ClearLockMouseState(MouseCursorStateTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearLockMouseState)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            lockMouseCursorTickets.Remove(ticket);

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
