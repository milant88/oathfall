namespace Oathfall.Core
{
    public enum PresentationAction { Idle, Move, AttackWindup, AttackActive, AttackRecover, Block, Dodge, Hit, Stagger, Death }
    public interface IActorView
    {
        PresentationAction Action { get; }
        void SetMovement(float speed, float localRight = 0, float localForward = 1);
        void SetAction(PresentationAction action);
        void SetTelegraph(bool visible);
        void PlayAttack(int combo, int ability, float duration);
        void FlashHit();
    }
}
