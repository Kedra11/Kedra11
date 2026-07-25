using AntEmpire.Colony;
using AntEmpire.Combat;
using AntEmpire.Core;
using AntEmpire.Events;
using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Soldier ant: doesn't gather. Patrols around the nest entrance and
    /// rushes any active enemy the moment it appears. In melee it deals
    /// triple a worker's bite (the enemy counts soldiers separately) and
    /// dies in the workers' place — the spider strikes soldiers first.
    /// </summary>
    public class SoldierController : MonoBehaviour
    {
        public const float DamagePerSecond = 3f;

        private const float MoveSpeed = 2.3f;
        private const float MeleeStopDistance = 1.1f;
        private const float PatrolRadius = 3.5f;

        private Vector3 _patrolTarget;
        private float _repathTimer;

        private void Start()
        {
            PickPatrolPoint();
        }

        private void Update()
        {
            EnemyController enemy = EnemyController.Active;
            if (enemy != null)
            {
                StepTowards(enemy.transform.position, MeleeStopDistance);
                return;
            }

            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f || StepTowards(_patrolTarget, 0.3f))
            {
                PickPatrolPoint();
            }
        }

        private void PickPatrolPoint()
        {
            Vector3 center = NestEntrance.Instance != null ? NestEntrance.Instance.Position : Vector3.zero;
            Vector2 offset = Random.insideUnitCircle * PatrolRadius;
            _patrolTarget = center + new Vector3(offset.x, 0f, offset.y);
            _repathTimer = Random.Range(2f, 4.5f);
        }

        private bool StepTowards(Vector3 destination, float arriveDistance)
        {
            destination.y = transform.position.y;
            Vector3 toTarget = destination - transform.position;
            if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                return true;
            }

            float speed = MoveSpeed * RainEvent.SpeedFactor *
                (GameManager.Instance != null ? GameManager.Instance.RoomService.SpeedBonus : 1f);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 540f * Time.deltaTime);
            return false;
        }
    }
}
