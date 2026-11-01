using Oathfall.Core;
using UnityEngine;

namespace Oathfall.Gameplay
{
    public enum CombatTeam
    {
        Hero,
        Enemy,
    }

    public sealed class Combatant : MonoBehaviour
    {
        public CombatTeam Team { get; private set; }
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public float Armor { get; private set; }
        public bool IsDead { get; private set; }
        public event System.Action Damaged;
        public event System.Action<float,float,float> HitResolved;
        public void ApplyStats(HeroStats stats)
        {
            var missing = MaxHealth - Health;
            MaxHealth = stats.Health; Armor = stats.Armor;
            Health = Mathf.Clamp(MaxHealth - missing, 1, MaxHealth);
        }
        public float IncomingDamageMultiplier { get; set; } = 1f;

        private Renderer[] renderers;
        private Color baseColor;
        private IActorView presentation;

        public void Initialize(CombatTeam team, float maxHealth, float armor, Color color)
        {
            Team = team;
            MaxHealth = maxHealth;
            Health = maxHealth;
            Armor = armor;
            baseColor = color;
            presentation = GetComponent<IActorView>();
            renderers = GetComponentsInChildren<Renderer>();
            if (presentation == null) SetColor(color);
        }

        public DamageResult ReceiveDamage(DamageRequest request)
        {
            if (IsDead)
            {
                return new DamageResult();
            }

            var result = DamageResolver.Resolve(request, Armor, Health);
            result.FinalAmount *= Mathf.Max(0f, IncomingDamageMultiplier);
            result.WasLethal = result.FinalAmount >= Health;
            Health = Mathf.Max(0f, Health - result.FinalAmount);
            if (result.FinalAmount > 0)
            {
                Damaged?.Invoke();HitResolved?.Invoke(Health,MaxHealth,result.FinalAmount);
                if (presentation != null) presentation.FlashHit();else Flash(Color.white);
            }
            if (Health <= 0f)
            {
                Die();
            }

            return result;
        }

        public void Heal(float amount)
        {
            if (!IsDead)
            {
                Health = Mathf.Min(MaxHealth, Health + Mathf.Max(0f, amount));
            }
        }

        public void SetColor(Color color)
        {
            if (renderers == null) return;
            foreach (var currentRenderer in renderers)
            {
                if (currentRenderer.material.HasProperty("_BaseColor"))
                {
                    currentRenderer.material.SetColor("_BaseColor", color);
                }
            }
        }

        public void Flash(Color color)
        {
            SetColor(color);
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.08f);
        }

        private void RestoreColor()
        {
            SetColor(baseColor);
        }

        private void Die()
        {
            IsDead = true;
            presentation?.SetAction(PresentationAction.Death);
            GameplaySession.Current?.OnCombatantDefeated(this);
            foreach(var collider in GetComponents<Collider>()) collider.enabled = false;
            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent)) agent.enabled = false;
            Destroy(gameObject, 2f);
        }
    }
}
