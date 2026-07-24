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
        // TODO: reference RoomTypeData (ScriptableObject) config.
        // TODO: upgrade via ResourceManager.TrySpend(cost...) — never mutate
        //       resources directly.
        // TODO: apply effects: Queen Chamber -> population cap,
        //       Food Storage -> storage capacity, Larva Nursery -> birth speed.
        // TODO: visual upgrade per level (swap/extend room prefab).
    }
}
