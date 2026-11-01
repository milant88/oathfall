using UnityEngine;

namespace Oathfall.Presentation
{
    /// <summary>Scene-owned composition references. This keeps build inclusion explicit and avoids runtime shader discovery.</summary>
    public sealed class HollowwoodSceneContext : MonoBehaviour
    {
        [SerializeField] private PresentationPalette palette;
        [SerializeField] private ActorPresentationDefinition dawnwarden;
        [SerializeField] private ActorPresentationDefinition raider;
        [SerializeField] private ActorPresentationDefinition hexblade,breaker,guardian;
        public ActorPresentationDefinition ForRole(Oathfall.Core.EnemyRole role)=>role==Oathfall.Core.EnemyRole.Hexblade?hexblade:role==Oathfall.Core.EnemyRole.Breaker?breaker:role==Oathfall.Core.EnemyRole.Guardian?guardian:raider;
        [SerializeField] private Transform heroSpawn;
        [SerializeField] private GameObject lootPrefab;
        public GameObject LootPrefab => lootPrefab;
        [SerializeField] private Transform optionalCache;
        [SerializeField] private GameObject shortcutGate;
        public Transform OptionalCache => optionalCache;
        public GameObject ShortcutGate => shortcutGate;

        public PresentationPalette Palette => palette;
        public ActorPresentationDefinition Dawnwarden => dawnwarden;
        public ActorPresentationDefinition Raider => raider;
        public Transform HeroSpawn => heroSpawn;
    }
}
