namespace HisaCat.SimpleObjectPool
{
    public interface ISimpleObjectPoolable
    {
        /// <summary>
        /// Object Pool이 Active될 시
        /// </summary>
        void OnSpawnFromPool();

        /// <summary>
        /// Object Pool이 Deactive될 시
        /// </summary>
        void OnDespawnFromPool();

        /// <summary>
        /// Monobehaviour OnDestroy.
        /// Scene 전환 혹은 명시적 삭제, 실수에 의한 삭제시 ObjectPool에서 제외하기 위함.
        /// </summary>
        void RemoveFromPoolOnDestroy();
    }
}
