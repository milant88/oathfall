using Oathfall.Core;
namespace Oathfall.Gameplay
{
    public interface IGameSession
    {
        bool RunActive { get; }
        bool InputBlocked { get; }
        PlayerController Player { get; }
        PickupResult TryCollect(ItemInstance item);
        void OnCombatantDefeated(Combatant combatant);
    }
    public static class GameplaySession { public static IGameSession Current { get; set; } }
}
