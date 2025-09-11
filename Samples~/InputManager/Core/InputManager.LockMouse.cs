using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class LockMouseCursorTicket
        {
            public readonly object Owner = null;
            public readonly string LogMessage = null;
            public readonly string StackTrace = null;
            public LockMouseCursorTicket(object owner, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }
        }
        private static HashSet<LockMouseCursorTicket> lockMouseCursorTickets = new HashSet<LockMouseCursorTicket>();
        public static bool IsMouseCursorLocked => lockMouseCursorTickets.Count > 0;
        public static LockMouseCursorTicket SetLockMouseCursor(object owner, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetLockMouseCursor)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new LockMouseCursorTicket(owner, logMessage, stackTrace);
            lockMouseCursorTickets.Add(ticket);

            UpdateLockMouseCursorStatus();

            return ticket;
        }
        public static void ClearLockMouseCursor(LockMouseCursorTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearLockMouseCursor)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            lockMouseCursorTickets.Remove(ticket);

            UpdateLockMouseCursorStatus();
        }
        private static void UpdateLockMouseCursorStatus()
        {
            if (Instance == null) return;
            if (IsMouseCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
