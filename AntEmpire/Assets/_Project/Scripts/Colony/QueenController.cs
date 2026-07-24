using UnityEngine;

namespace AntEmpire.Colony
{
    /// <summary>
    /// The queen: heart of the colony. Her level drives birth rate,
    /// population cap and unlocks (rooms, ant roles).
    /// </summary>
    public class QueenController : MonoBehaviour
    {
        // TODO: read/write queenLevel through SaveData (via GameManager).
        // TODO: larva production timer -> spawns new workers while Food lasts.
        // TODO: queen evolution visuals (star levels), driven by config data.
        // TODO: population cap = f(queenLevel, Queen Chamber room level).
    }
}
