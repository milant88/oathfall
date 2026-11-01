using UnityEngine;
using Oathfall.Core;
namespace Oathfall.Presentation
{
    [CreateAssetMenu(menuName="Oathfall/Presentation/Gear catalog")]
    public sealed class GearCatalog:ScriptableObject
    {
        public GameObject[] models=new GameObject[6];
        public Sprite[] icons=new Sprite[6];
        public GameObject Model(ItemSlot slot)=>models[(int)slot];
        public Sprite Icon(ItemSlot slot)=>icons[(int)slot];
        public static GearCatalog Load()=>Resources.Load<GearCatalog>("OathfallGear");
    }
}
