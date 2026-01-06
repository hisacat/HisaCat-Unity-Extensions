using System.Collections.Generic;

namespace HisaCat.RealTimeOcclusionCulling.V2
{
    public class Room
    {
        public string Name { get; set; }
        public IReadOnlyList<Room> Neighbors { get; set; }
    }
}
