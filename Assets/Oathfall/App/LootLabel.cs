using Oathfall.Core;
using TMPro;
using UnityEngine;
namespace Oathfall.Gameplay
{
    public sealed class LootLabel:MonoBehaviour
    {
        private TextMeshPro label;
        private static readonly System.Collections.Generic.List<LootLabel> labels=new System.Collections.Generic.List<LootLabel>();
        private void OnEnable()=>labels.Add(this);
        private void OnDisable()=>labels.Remove(this);
        public void Initialize(ItemInstance item)
        {
            var obj=new GameObject("Equipment label");obj.transform.SetParent(transform,false);obj.transform.localPosition=Vector3.up*.9f;
            label=obj.AddComponent<TextMeshPro>();label.font=TMP_Settings.defaultFontAsset;label.fontSize=2.8f;label.alignment=TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta=new Vector2(4,1);label.color=RuntimeHud.ItemColor(item.Rarity);label.text=item.Rarity+"\n"+item.Slot+"  Lv."+item.ItemLevel;
        }
        private void LateUpdate()
        {
            if(label==null || Camera.main==null)return;
            var player=OathfallGame.Instance?.Player;
            float distance=player==null?float.MaxValue:(player.transform.position-transform.position).sqrMagnitude;
            bool nearest=distance<16;
            if(nearest)foreach(var other in labels)if(other!=this && (other.transform.position-player.transform.position).sqrMagnitude<distance){nearest=false;break;}
            label.enabled=nearest;
            label.transform.rotation=Camera.main.transform.rotation;
        }
    }
}
