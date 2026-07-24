using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// MonoBehaviour on every ant prefab. Thin wrapper: holds references and
    /// delegates all decisions to AntStateMachine.
    /// </summary>
    public class AntController : MonoBehaviour
    {
        // TODO: hold AntStateMachine instance and tick it on a slow timer
        //       (e.g. 5–10 Hz), not every frame — mobile performance.
        // TODO: reference AntTypeData (ScriptableObject) for stats.
        // TODO: expose current evolution level for AntVisualController.
        // TODO: movement — simple lerp/steering toward target, no NavMesh in MVP.
    }
}
