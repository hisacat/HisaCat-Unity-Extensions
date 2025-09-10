using System.Collections;
using UnityEngine;

namespace HisaCat
{
    /// <summary>
    /// Get server time from NIST as local timezone.
    /// And expect server time from cached server time using realtimeSinceStartup.
    /// NOTE: System.DateTime.Now이 아닌 (realtimeSinceStartup 혹은 Time.unscaledTime) 은 로컬 기기 설정에서의 시간 변경의 영향을 받지 않음으로,
    /// 시간 경과만을 측정할 때에 유용하게 사용될 수 있음.
    /// </summary>
    public static class ServerTime
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                LastFetchedServerTime = null;
            }
        }
#pragma warning restore IDE0051
#endif

        private struct ServerTimeCache
        {
            public readonly System.DateTime ServerLocalDataTime;
            public readonly double FetchedClientTime;
            public ServerTimeCache(System.DateTime serverLocalDataTime, double clientTime)
            {
                this.ServerLocalDataTime = serverLocalDataTime;
                this.FetchedClientTime = clientTime;
            }
        }
        private static ServerTimeCache? LastFetchedServerTime = null;

        public static bool IsFetchedServerTimeExists => LastFetchedServerTime.HasValue;

        public delegate void FetchServerTimeEventHandler(bool isSuccess, System.DateTime serverLocalDateTIme);
        public static void FetchServerTime(FetchServerTimeEventHandler result = null)
        {
            try
            {
                // https://stackoverflow.com/questions/6435099/how-to-get-datetime-from-the-internet
                using (var client = new System.Net.Sockets.TcpClient("time.nist.gov", 13))
                {
                    using (var streamReader = new System.IO.StreamReader(client.GetStream()))
                    {
                        // NOTE: Sometimes it respond 'Too many requests'
                        var response = streamReader.ReadToEnd();
                        var utcDateTimeString = response.Substring(7, 17);

                        var localDateTime = System.DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AssumeUniversal);

                        //var utcDataTime = localDateTime.ToUniversalTime();
                        //Debug.Log(utcDateTimeString);
                        //Debug.Log(localDateTime + " " + localDateTime.Kind);
                        //Debug.Log(utcDataTime + " " + utcDataTime.Kind);

                        LastFetchedServerTime = new ServerTimeCache(localDateTime, Time.realtimeSinceStartupAsDouble);
                        result?.Invoke(true, localDateTime);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ServerTime] Exception: {e}");
                result?.Invoke(false, default);
            }
        }
        public static IEnumerator FetchServerTimeRoutine(FetchServerTimeEventHandler result = null)
        {
            bool isCompleted = false;
            FetchServerTime(callback);
            void callback(bool isSuccess, System.DateTime serverLocalDateTIme)
            {
                result?.Invoke(isSuccess, serverLocalDateTIme);
                isCompleted = true;
            }
            yield return new WaitUntil(() => isCompleted);
        }

        public static (bool isSuccess, System.DateTime expectedServerLocalTime) ExpectServerTime()
        {
            if (LastFetchedServerTime.HasValue == false)
            {
                Debug.LogError($"[{nameof(ServerTime)}] Any servertime fetched yet! Must be success {nameof(FetchServerTime)} once.");
                return (isSuccess: false, expectedServerLocalTime: default);
            }

            var lastFetchedServerTime = LastFetchedServerTime.Value;
            var clientSpendTime = Time.realtimeSinceStartupAsDouble - lastFetchedServerTime.FetchedClientTime;
            var expectedTime = lastFetchedServerTime.ServerLocalDataTime.AddSeconds(clientSpendTime);
            return (isSuccess: true, expectedServerLocalTime: expectedTime);
        }
    }
}
