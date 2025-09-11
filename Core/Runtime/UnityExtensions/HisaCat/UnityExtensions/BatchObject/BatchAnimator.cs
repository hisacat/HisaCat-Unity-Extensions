using HisaCat.UnityExtensions;
using System;
using UnityEngine;

namespace HisaCat.Collections
{
    [System.Serializable]
    public class BatchAnimator : BatchBehaviourBase<Animator>
    {
        public void BatchSetFloat(string name, float value)
            => IterateObjects((name, value), static (Animator obj, (string name, float value) args) => obj.SetFloat(args.name, args.value));
        public void BatchSetFloat(int id, float value)
            => IterateObjects((id, value), static (Animator obj, (int id, float value) args) => obj.SetFloat(args.id, args.value));
        public void BatchSetFloatSafe(string name, float value)
            => IterateObjects((name, value), static (Animator obj, (string name, float value) args) => obj.TrySetFloat(args.name, args.value));
        public void BatchSetFloatSafe(int id, float value)
            => IterateObjects((id, value), static (Animator obj, (int id, float value) args) => obj.TrySetFloat(args.id, args.value));

        public void BatchSetBool(string name, bool value)
            => IterateObjects((name, value), static (Animator obj, (string name, bool value) args) => obj.SetBool(args.name, args.value));
        public void BatchSetBool(int id, bool value)
            => IterateObjects((id, value), static (Animator obj, (int id, bool value) args) => obj.SetBool(args.id, args.value));
        public void BatchSetBoolSafe(string name, bool value)
            => IterateObjects((name, value), static (Animator obj, (string name, bool value) args) => obj.TrySetBool(args.name, args.value));
        public void BatchSetBoolSafe(int id, bool value)
            => IterateObjects((id, value), static (Animator obj, (int id, bool value) args) => obj.TrySetBool(args.id, args.value));

        public void BatchSetInteger(string name, int value)
            => IterateObjects((name, value), static (Animator obj, (string name, int value) args) => obj.SetInteger(args.name, args.value));
        public void BatchSetInteger(int id, int value)
            => IterateObjects((id, value), static (Animator obj, (int id, int value) args) => obj.SetInteger(args.id, args.value));
        public void BatchSetIntegerSafe(string name, int value)
            => IterateObjects((name, value), static (Animator obj, (string name, int value) args) => obj.TrySetInteger(args.name, args.value));
        public void BatchSetIntegerSafe(int id, int value)
            => IterateObjects((id, value), static (Animator obj, (int id, int value) args) => obj.TrySetInteger(args.id, args.value));

        public void BatchSetTrigger(string name)
            => IterateObjects(name, static (Animator obj, string name) => obj.SetTrigger(name));
        public void BatchSetTrigger(int id)
            => IterateObjects(id, static (Animator obj, int id) => obj.SetTrigger(id));
        public void BatchSetTriggerSafe(string name)
            => IterateObjects(name, static (Animator obj, string name) => obj.TrySetTrigger(name));
        public void BatchSetTriggerSafe(int id)
            => IterateObjects(id, static (Animator obj, int id) => obj.TrySetTrigger(id));

        public void BatchResetTrigger(string name)
            => IterateObjects(name, static (Animator obj, string name) => obj.ResetTrigger(name));
        public void BatchResetTrigger(int id)
            => IterateObjects(id, static (Animator obj, int id) => obj.ResetTrigger(id));
        public void BatchResetTriggerSafe(string name)
            => IterateObjects(name, static (Animator obj, string name) => obj.TryResetTrigger(name));
        public void BatchResetTriggerSafe(int id)
            => IterateObjects(id, static (Animator obj, int id) => obj.TryResetTrigger(id));

        public void BatchSetLayerWeight(int layerIndex, float weight)
            => IterateObjects((layerIndex, weight), static (Animator obj, (int layerIndex, float weight) args) => obj.SetLayerWeight(args.layerIndex, args.weight));
    }
}
