using System;
using AntEmpire.Ants;
using AntEmpire.Economy;
using UnityEngine;

namespace AntEmpire.Combat
{
    /// <summary>
    /// The spider (first enemy). Simple MVP behaviour, no pathfinding:
    ///
    ///   Approach — walks from the meadow edge towards the nest;
    ///   Steal    — sits by the entrance for a while; when the timer runs
    ///              out it grabs Food and flees;
    ///   Flee     — runs off the map and despawns.
    ///
    /// Combat is deliberately simple (per CLAUDE.md): every worker within
    /// bite range wounds it over time — more ants near the nest means a
    /// faster kill. Defeat rewards are handled by SpiderAttackEvent.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        public enum Phase { Approach, Steal, Flee, Done }

        private const float MoveSpeed = 1.4f;
        private const float StealDuration = 12f;
        private const float BiteRadius = 2.8f;
        private const float DamagePerWorkerPerSecond = 1f;
        private const float NestStopDistance = 1.9f;
        private const float DespawnDistance = 22f;
        private const double StealAmount = 25;

        private ResourceManager _resources;
        private AntSpawner _spawner;
        private Vector3 _nestPosition;
        private Vector3 _fleeDirection;
        private float _stealTimer;

        public Phase CurrentPhase { get; private set; } = Phase.Approach;
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public double StolenFood { get; private set; }

        /// <summary>Fired once when the spider dies or escapes: (enemy, defeated).</summary>
        public event Action<EnemyController, bool> Finished;

        public void Init(ResourceManager resources, AntSpawner spawner, Vector3 nestPosition, float health)
        {
            _resources = resources;
            _spawner = spawner;
            _nestPosition = nestPosition;
            MaxHealth = Health = health;
            _fleeDirection = (transform.position - nestPosition).normalized;
            _stealTimer = StealDuration;
        }

        private void Update()
        {
            switch (CurrentPhase)
            {
                case Phase.Approach:
                    if (StepTowards(_nestPosition, NestStopDistance))
                    {
                        CurrentPhase = Phase.Steal;
                    }
                    TakeBites();
                    break;

                case Phase.Steal:
                    _stealTimer -= Time.deltaTime;
                    TakeBites();
                    if (CurrentPhase == Phase.Steal && _stealTimer <= 0f)
                    {
                        GrabFood();
                        CurrentPhase = Phase.Flee;
                    }
                    break;

                case Phase.Flee:
                    transform.position += _fleeDirection * (MoveSpeed * 1.6f * Time.deltaTime);
                    FaceTowards(transform.position + _fleeDirection);
                    if ((transform.position - _nestPosition).sqrMagnitude > DespawnDistance * DespawnDistance)
                    {
                        Finish(defeated: false);
                    }
                    break;
            }
        }

        private void TakeBites()
        {
            if (_spawner == null)
            {
                return;
            }

            int biters = 0;
            float radiusSqr = BiteRadius * BiteRadius;
            for (int i = 0; i < _spawner.Ants.Count; i++)
            {
                if ((_spawner.Ants[i].transform.position - transform.position).sqrMagnitude <= radiusSqr)
                {
                    biters++;
                }
            }

            if (biters == 0)
            {
                return;
            }

            Health -= biters * DamagePerWorkerPerSecond * Time.deltaTime;
            if (Health <= 0f)
            {
                Health = 0f;
                Finish(defeated: true);
            }
        }

        private void GrabFood()
        {
            double amount = Math.Min(StealAmount, _resources.Get(ResourceType.Food));
            if (amount > 0 &&
                _resources.TrySpend(ResourceType.Food, amount) == ResourceTransactionResult.Success)
            {
                StolenFood = amount;
            }
        }

        private bool StepTowards(Vector3 destination, float stopDistance)
        {
            destination.y = transform.position.y;
            Vector3 toTarget = destination - transform.position;
            if (toTarget.sqrMagnitude <= stopDistance * stopDistance)
            {
                return true;
            }

            transform.position = Vector3.MoveTowards(
                transform.position, destination, MoveSpeed * Time.deltaTime);
            FaceTowards(destination);
            return false;
        }

        private void FaceTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    360f * Time.deltaTime);
            }
        }

        private void Finish(bool defeated)
        {
            if (CurrentPhase == Phase.Done)
            {
                return;
            }
            CurrentPhase = Phase.Done;
            Finished?.Invoke(this, defeated);
            Destroy(gameObject);
        }
    }
}
