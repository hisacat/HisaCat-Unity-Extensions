namespace HisaCat.RealTimeOcclusionCulling
{
    [System.Flags]
    public enum CullingState
    {
        None = 0,
        Group = 1,
        Occluder = 2,
        All = ~None,
    }
}