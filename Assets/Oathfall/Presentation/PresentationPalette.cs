using UnityEngine;

namespace Oathfall.Presentation
{
    [CreateAssetMenu(menuName = "Oathfall/Presentation/Palette", fileName = "HollowwoodPalette")]
    public sealed class PresentationPalette : ScriptableObject
    {
        [SerializeField] private Material shrineFloor;
        [SerializeField] private Material oldStone;
        [SerializeField] private Material mossStone;
        [SerializeField] private Material bark;
        [SerializeField] private Material leaf;
        [SerializeField] private Material warmGold;
        [SerializeField] private Material ember;

        public Material ShrineFloor => shrineFloor;
        public Material OldStone => oldStone;
        public Material MossStone => mossStone;
        public Material Bark => bark;
        public Material Leaf => leaf;
        public Material WarmGold => warmGold;
        public Material Ember => ember;
    }
}
