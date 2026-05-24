using System;
using UnityEngine;

namespace HisaCat.CoroutineExtensions
{
    public abstract class WaitForSecondsOrUntilBase : CustomYieldInstruction
    {
        private readonly float m_Seconds;
        private readonly Func<bool> m_Predicate;

        private float m_EndTime;
        private bool m_Started;

        public WaitForSecondsOrUntilBase(float seconds, Func<bool> predicate)
        {
            if (seconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(seconds));

            this.m_Seconds = seconds;
            this.m_Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public override sealed bool keepWaiting
        {
            get
            {
                var currentTime = this.ReferenceTime;

                if (this.m_Started == false)
                {
                    this.m_EndTime = currentTime + this.m_Seconds;
                    this.m_Started = true;
                }

                return currentTime < this.m_EndTime && this.m_Predicate() == false;
            }
        }

        protected abstract float ReferenceTime { get; }
    }

    public sealed class WaitForSecondsOrUntil : WaitForSecondsOrUntilBase
    {
        public WaitForSecondsOrUntil(float seconds, Func<bool> predicate) : base(seconds, predicate) { }

        protected override float ReferenceTime => Time.time;
    }

    public sealed class WaitForSecondsRealtimeOrUntil : WaitForSecondsOrUntilBase
    {
        public WaitForSecondsRealtimeOrUntil(float seconds, Func<bool> predicate) : base(seconds, predicate) { }

        protected override float ReferenceTime => Time.realtimeSinceStartup;
    }
}
