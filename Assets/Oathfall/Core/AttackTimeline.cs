namespace Oathfall.Core
{
    public enum AttackPhase { None, Windup, Active, Recovery }

    public readonly struct AttackTiming
    {
        public AttackTiming(float windup, float active, float recovery) { Windup = windup; Active = active; Recovery = recovery; }
        public float Windup { get; }
        public float Active { get; }
        public float Recovery { get; }
        public float Total => Windup + Active + Recovery;
        public bool CrossesActive(float previous, float current) => current >= Windup && previous < Windup + Active && current >= previous;
        public AttackPhase GetPhase(float elapsed)
        {
            const float boundaryEpsilon = .00001f;
            if (elapsed < 0f || elapsed >= Total - boundaryEpsilon) return AttackPhase.None;
            if (elapsed < Windup - boundaryEpsilon) return AttackPhase.Windup;
            return elapsed < Windup + Active - boundaryEpsilon ? AttackPhase.Active : AttackPhase.Recovery;
        }
    }
}
