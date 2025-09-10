using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace HisaCat.FishNet
{
    public class OwnerSyncVar<T> : SyncVar<T>
    {
        public struct OwnerSyncVarSettings
        {
            public float SendRate;
            public Channel Channel;

            private bool IsDefault() => SendRate == default && Channel == default;
            private const float DefaultSendRate = 0.1f;
            private const Channel DefaultChannel = Channel.Reliable;
            public OwnerSyncVarSettings(float sendRate)
            {
                this.SendRate = sendRate;
                this.Channel = DefaultChannel;
            }
            public OwnerSyncVarSettings(Channel channel)
            {
                this.SendRate = DefaultSendRate;
                this.Channel = channel;
            }
            public OwnerSyncVarSettings(float sendRate, Channel channel)
            {
                this.SendRate = sendRate;
                this.Channel = channel;
            }

            public SyncTypeSettings ToSyncTypeSettings()
            {
                bool isDefault = this.IsDefault();
                return new(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner,
                    isDefault ? DefaultSendRate : this.SendRate,
                    isDefault ? DefaultChannel : this.Channel);
            }
        }

        public OwnerSyncVar(OwnerSyncVarSettings settings = new()) : this(default, settings) { }
        public OwnerSyncVar(T initialValue, OwnerSyncVarSettings settings = new()) : base(initialValue, settings.ToSyncTypeSettings()) { }
        public new T Value { get => base.Value; }
        public void SetValueFromRpc(T value) => base.Value = value;
    }

    public static class OwnerSyncVarExtensions
    {
        public static bool ValidateSetOwnerSyncVar(this NetworkBehaviour behaviour)
        {
            if (behaviour.IsController == false)
            {
                Debug.LogError("Cannot set OwnerSync var because IsController is false.", behaviour);
                return false;
            }
            return true;
        }
    }
}
