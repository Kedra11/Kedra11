using UnityEngine;

namespace AntEmpire.Core
{
    /// <summary>
    /// Snaps the main camera between the two game views: the surface meadow
    /// and the underground "ant farm" cutaway. Lives on the camera object;
    /// the HUD button calls Toggle().
    /// </summary>
    public class CameraViewSwitcher : MonoBehaviour
    {
        private Pose _surfacePose;
        private Pose _undergroundPose;
        private bool _configured;

        public bool IsUnderground { get; private set; }

        public void Configure(Pose surfacePose, Pose undergroundPose)
        {
            _surfacePose = surfacePose;
            _undergroundPose = undergroundPose;
            _configured = true;
        }

        public void Toggle()
        {
            if (!_configured)
            {
                return;
            }

            IsUnderground = !IsUnderground;
            Pose target = IsUnderground ? _undergroundPose : _surfacePose;
            transform.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}
