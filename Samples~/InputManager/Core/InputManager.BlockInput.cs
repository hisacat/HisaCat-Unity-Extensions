using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public class BlockInputTicket
        {
            public readonly object Owner = null;
            public readonly string LogMessage = null;
            public readonly string StackTrace = null;
            public BlockInputTicket(object owner, string logMessage, string stackTrace)
            {
                this.Owner = owner;
                this.LogMessage = logMessage;
                this.StackTrace = stackTrace;
            }
        }
        private static HashSet<BlockInputTicket> blockInputTickets = new HashSet<BlockInputTicket>();
        public static bool IsInputBlocked => blockInputTickets.Count > 0;
        public static BlockInputTicket SetBlockInput(object owner, string logMessage = null)
        {
            string stackTrace = null;

#if UNITY_EDITOR
            stackTrace = StackTraceUtility.ExtractStackTrace();
#endif

            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(SetBlockInput)}: From {(owner == null ? "null" : owner.ToString())}\r\n" +
                $"LogMessage: {logMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{stackTrace}");

            var ticket = new BlockInputTicket(owner, logMessage, stackTrace);
            blockInputTickets.Add(ticket);

            UpdateBlockInputStatus();

            return ticket;
        }
        public static void ClearBlockInput(BlockInputTicket ticket)
        {
            ManagedDebug.Log($"[{nameof(InputManager)}] {nameof(ClearBlockInput)}: From {(ticket.Owner == null ? "null" : ticket.Owner.ToString())}\r\n" +
                $"LogMessage: {ticket.LogMessage}\r\n" +
                $"StackTrace:\r\n" +
                $"{ticket.StackTrace}");

            blockInputTickets.Remove(ticket);

            UpdateBlockInputStatus();
        }
        private static void UpdateBlockInputStatus()
        {
            if (Instance == null) return;
            Instance.m_EventSystem.enabled = !IsInputBlocked;
        }
    }
}
