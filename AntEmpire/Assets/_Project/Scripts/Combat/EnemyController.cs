using System;
using System.Collections;
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

        // Defence: up to half the colony rushes the spider; the spider
        // strikes back, killing a nearby worker every few seconds (workers
        // are weak fighters — real Soldier ants come later).
        private const float AggroRadius = 10f;
        private const float RecruitInterval = 0.5f;
        private const float StrikeInterval = 2.5f;
        private const float StrikeRadius = 2.0f;

        private ResourceManager _resources;
        private AntSpawner _spawner;
        private Vector3 _nestPosition;
        private Vector3 _fleeDirection;
        private float _stealTimer;
        private float _recruitTimer;
        private float _strikeTimer = 1.2f; // first strike lands soon after ants engage

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
                case Phase.Done:
                    return;
                case Phase.Approach:
                    if (StepTowards(_nestPosition, NestStopDistance))
                    {
                        CurrentPhase = Phase.Steal;
                    }
                    RecruitDefenders();
                    TakeBites();
                    StrikeBack();
                    break;

                case Phase.Steal:
                    _stealTimer -= Time.deltaTime;
                    RecruitDefenders();
                    TakeBites();
                    StrikeBack();
                    if (CurrentPhase == Phase.Steal && _stealTimer <= 0f)
                    {
                        GrabFood();
                        ReleaseDefenders();
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

        /// <summary>Send nearby workers into the fight, capped at half the
        /// colony so gathering never fully stops.</summary>
        private void RecruitDefenders()
        {
            _recruitTimer -= Time.deltaTime;
            if (_recruitTimer > 0f || _spawner == null)
            {
                return;
            }
            _recruitTimer = RecruitInterval;

            int maxDefenders = Mathf.CeilToInt(_spawner.Ants.Count * 0.5f);
            int defending = 0;
            for (int i = 0; i < _spawner.Ants.Count; i++)
            {
                if (_spawner.Ants[i].CombatTarget == this)
                {
                    defending++;
                }
            }

            float aggroSqr = AggroRadius * AggroRadius;
            for (int i = 0; i < _spawner.Ants.Count && defending < maxDefenders; i++)
            {
                AntController ant = _spawner.Ants[i];
                if (ant.CombatTarget == null &&
                    (ant.transform.position - transform.position).sqrMagnitude <= aggroSqr)
                {
                    ant.EnterCombat(this);
                    defending++;
                }
            }
        }

        /// <summary>The spider fights back: kills one worker in melee range
        /// every few seconds. The timer only runs while an ant is actually
        /// in range, so the first kill lands shortly after the swarm arrives.
        /// It never takes the colony's last worker.</summary>
        private void StrikeBack()
        {
            if (CurrentPhase == Phase.Done || _spawner == null || _spawner.Ants.Count <= 1)
            {
                return;
            }

            AntController victim = null;
            float strikeSqr = StrikeRadius * StrikeRadius;
            for (int i = 0; i < _spawner.Ants.Count; i++)
            {
                AntController ant = _spawner.Ants[i];
                if ((ant.transform.position - transform.position).sqrMagnitude <= strikeSqr)
                {
                    victim = ant;
                    break;
                }
            }

            if (victim == null)
            {
                return; // nobody in reach — hold the strike
            }

            _strikeTimer -= Time.deltaTime;
            if (_strikeTimer > 0f)
            {
                return;
            }
            _strikeTimer = StrikeInterval;
            _spawner.KillWorker(victim);
        }

        private void ReleaseDefenders()
        {
            if (_spawner == null)
            {
                return;
            }
            for (int i = 0; i < _spawner.Ants.Count; i++)
            {
                if (_spawner.Ants[i].CombatTarget == this)
                {
                    _spawner.Ants[i].ExitCombat();
                }
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
            ReleaseDefenders();
            Finished?.Invoke(this, defeated);

            if (defeated)
            {
                StartCoroutine(DeathCollapse());
            }
            else
            {
                Destroy(gameObject); // it walked off-screen anyway
            }
        }

        /// <summary>Quick squash-flat death animation, then despawn.</summary>
        private IEnumerator DeathCollapse()
        {
            Vector3 startScale = transform.localScale;
            const float duration = 0.45f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.localScale = new Vector3(
                    startScale.x * (1f + 0.3f * k),
                    startScale.y * (1f - 0.92f * k),
                    startScale.z * (1f + 0.3f * k));
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
