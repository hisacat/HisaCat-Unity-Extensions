using FishNet.Object;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.FishNet
{
    public static class FishNetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotServerWithLogError(this NetworkBehaviour networkBehaviour, string functionName)
        {
            if (networkBehaviour.IsServerStarted == false)
            {
                Debug.LogError($"[{nameof(NetworkBehaviour)}] {functionName}: Server is not started!", networkBehaviour);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotControllerWithLogError(this NetworkBehaviour networkBehaviour, string functionName)
        {
            if (networkBehaviour.IsController == false)
            {
                Debug.LogError($"[{nameof(NetworkBehaviour)}] {functionName}: Client is not controller!", networkBehaviour);
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotOwnerOrServerWithLogError(this NetworkBehaviour networkBehaviour, string functionName)
        {
            if (IsOwnerOrServer(networkBehaviour) == false)
            {
                Debug.LogError($"[{nameof(NetworkBehaviour)}] {functionName}: Client is not owner or server!", networkBehaviour);
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOwnerOrServer(this NetworkBehaviour networkBehaviour) => networkBehaviour.IsOwner || networkBehaviour.IsServerStarted;
    }
}
