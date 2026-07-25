using UnityEngine;

namespace AntEmpire.Rooms
{
    /// <summary>
    /// One room instance in the underground nest (Queen Chamber, Food
    /// Storage, Larva Nursery...). Levels and effects come from
    /// RoomTypeData ScriptableObjects; current level is stored in SaveData.
    /// </summary>
    public class RoomController : MonoBehaviour
    {
        // NOTE: room logic lives in RoomService (levels, costs, effects) and
        // the current visualization in UndergroundView. This stub remains for
        // the future authored Underground scene, where each room becomes an
        // interactive scene object (tap to open the upgrade panel).
    }
}
