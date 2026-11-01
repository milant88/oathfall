using Oathfall.Core;
using Oathfall.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Oathfall.Gameplay
{
    public sealed partial class RuntimeHud
    {
        private GearPreview gearPreview;
        private TextMeshProUGUI gearName,gearMeta,equippedDescription,gearStats,bagCount,previewCaption,gearFeedback;
        private UnityEngine.UI.Button equipSelected,inventoryClose;
        private GameObject abilityCards;
        private readonly TextMeshProUGUI[] abilityCardsText=new TextMeshProUGUI[6];
        private static readonly Color Gold=new Color(.77f,.62f,.37f),Ink=new Color(.035f,.049f,.057f),PanelColor=new Color(.065f,.081f,.087f);
        private UnityEngine.UI.Image Block(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var obj=new GameObject(name,typeof(UnityEngine.UI.Image));obj.transform.SetParent(parent,false);var image=obj.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;SetRect(image.rectTransform,new Vector2(0,1),new Vector2(0,1),position,size);return image;
        }
        private TextMeshProUGUI Label(Transform parent,string name,string value,Vector2 position,Vector2 size,int font,Color color)
        {
            var t=CreateText(name,parent,font,TextAlignmentOptions.TopLeft);t.text=value;t.color=color;SetRect(t.rectTransform,new Vector2(0,1),new Vector2(0,1),position,size);return t;
        }
        private void Frame(GameObject obj,Color color)
        {
            var outline=obj.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=color;outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=false;
        }
        private UnityEngine.UI.Button ActionButton(Transform parent,string text,Vector2 position,Vector2 size,System.Action action)
        {
            var b=InventoryButton(parent,position,size);SetShortcutLabel(b,text);b.GetComponent<UnityEngine.UI.Image>().color=PanelColor;Frame(b.gameObject,new Color(.26f,.28f,.25f));b.onClick.AddListener(()=>action());b.GetComponentInChildren<TextMeshProUGUI>().fontSize=19;return b;
        }
        private void BuildGearUI()
        {
            inventoryPanel=Panel("Equipment",new Vector2(1680,900));inventoryPanel.GetComponent<UnityEngine.UI.Image>().color=new Color(.023f,.032f,.038f,.99f);Frame(inventoryPanel,Gold);
            Label(inventoryPanel.transform,"Equipment heading","THE WARDEN'S ARMOURY",new Vector2(28,-25),new Vector2(1200,50),34,Gold);
            Label(inventoryPanel.transform,"Equipment subtitle","Inspect your finds. Shape the next encounter.",new Vector2(29,-70),new Vector2(1000,30),18,new Color(.64f,.70f,.71f));
            inventoryClose=ActionButton(inventoryPanel.transform,"RETURN  [I / Y]",new Vector2(1430,-26),new Vector2(220,48),ToggleInventory);
            Block(inventoryPanel.transform,"Header rule",new Vector2(28,-110),new Vector2(1624,1),Gold*.7f);
            Block(inventoryPanel.transform,"Preview field",new Vector2(28,-130),new Vector2(420,390),Ink);
            var rawObject=new GameObject("3D selected gear",typeof(UnityEngine.UI.RawImage));rawObject.transform.SetParent(inventoryPanel.transform,false);var raw=rawObject.GetComponent<UnityEngine.UI.RawImage>();SetRect(raw.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(45,-137),new Vector2(386,365));
            gearPreview=rawObject.AddComponent<GearPreview>();gearPreview.Initialize(raw);
            previewCaption=Label(inventoryPanel.transform,"Preview hint","WARDEN SWORD  /  3D INSPECTION",new Vector2(45,-520),new Vector2(400,28),17,Gold);
            Label(inventoryPanel.transform,"Rotate hint","Drag to rotate · scroll to zoom",new Vector2(45,-550),new Vector2(390,25),16,new Color(.65f,.72f,.73f));
            ActionButton(inventoryPanel.transform,"<",new Vector2(46,-580),new Vector2(62,35),()=>gearPreview.Rotate(-35));ActionButton(inventoryPanel.transform,">",new Vector2(122,-580),new Vector2(62,35),()=>gearPreview.Rotate(35));
            Label(inventoryPanel.transform,"Worn gear heading","EQUIPPED",new Vector2(28,-643),new Vector2(400,30),19,Gold);
            for(int i=0;i<6;i++)
            {
                int index=i;var b=InventoryButton(inventoryPanel.transform,new Vector2(28+i%2*214,-682-i/2*65),new Vector2(202,56));b.onClick.AddListener(()=>InspectEquipped((ItemSlot)index));equipButtons.Add(b);
                b.GetComponent<UnityEngine.UI.Image>().color=PanelColor;Frame(b.gameObject,new Color(.22f,.27f,.27f));var icon=b.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();SetRect(icon.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(5,-4),new Vector2(48,48));
                var t=b.GetComponentInChildren<TextMeshProUGUI>();t.fontSize=15;t.alignment=TextAlignmentOptions.MidlineLeft;t.rectTransform.anchoredPosition=new Vector2(26,0);t.rectTransform.sizeDelta=new Vector2(-62,-6);
            }
            bagCount=Label(inventoryPanel.transform,"Bag heading","SATCHEL  0 / 24",new Vector2(482,-140),new Vector2(620,35),22,Gold);
            for(int i=0;i<24;i++)
            {
                int index=i;var b=InventoryButton(inventoryPanel.transform,new Vector2(482+i%6*104,-188-i/6*140),new Vector2(94,128));bagButtons.Add(b);b.onClick.AddListener(()=>Compare(index));b.GetComponent<UnityEngine.UI.Image>().color=PanelColor;
                Frame(b.gameObject,new Color(.19f,.24f,.25f));var icon=b.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();SetRect(icon.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(5,-5),new Vector2(84,84));
                var text=b.GetComponentInChildren<TextMeshProUGUI>();text.fontSize=14;text.alignment=TextAlignmentOptions.Center;SetRect(text.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(3,-92),new Vector2(88,32));
                var events=b.gameObject.AddComponent<EventTrigger>();var entry=new EventTrigger.Entry{eventID=EventTriggerType.Select};entry.callback.AddListener(_=>Compare(index));events.triggers.Add(entry);
            }
            gearStats=Label(inventoryPanel.transform,"Total stats","",new Vector2(482,-770),new Vector2(620,95),19,new Color(.8f,.85f,.83f));
            Block(inventoryPanel.transform,"Inspection separator",new Vector2(1130,-140),new Vector2(1,705),new Color(.24f,.28f,.27f));
            gearName=Label(inventoryPanel.transform,"Selected item","Choose a find",new Vector2(1160,-145),new Vector2(490,75),30,Gold);
            gearMeta=Label(inventoryPanel.transform,"Selected rarity","",new Vector2(1160,-220),new Vector2(470,42),17,new Color(.68f,.75f,.75f));
            inventoryDetails=Label(inventoryPanel.transform,"Comparison","Select an item from the satchel to inspect its model and compare the actual values.",new Vector2(1160,-275),new Vector2(470,240),21,new Color(.88f,.89f,.83f));
            equippedDescription=Label(inventoryPanel.transform,"Equipped comparison","",new Vector2(1160,-530),new Vector2(470,160),18,new Color(.67f,.75f,.73f));
            equipSelected=ActionButton(inventoryPanel.transform,"EQUIP SELECTED",new Vector2(1160,-725),new Vector2(472,54),()=>{if(selectedBagIndex>=0)OathfallGame.Instance.Equip(selectedBagIndex);});equipSelected.interactable=false;equipSelected.GetComponent<UnityEngine.UI.Image>().color=new Color(.30f,.24f,.13f);
            discardButton=ActionButton(inventoryPanel.transform,"DISCARD SELECTED",new Vector2(1160,-795),new Vector2(472,44),()=>{OathfallGame.Instance.Discard(selectedBagIndex);});discardButton.interactable=false;
            gearFeedback=Label(inventoryPanel.transform,"Equipment feedback","",new Vector2(28,-866),new Vector2(1600,26),17,Gold);
            inventoryPanel.SetActive(false);
        }
        private void RefreshGearUI()
        {
            var game=OathfallGame.Instance;if(game==null||inventoryPanel==null)return;
            selectedBagIndex=-1;discardButton.interactable=false;equipSelected.interactable=false;SetShortcutLabel(discardButton,"DISCARD SELECTED");SetShortcutLabel(equipSelected,"EQUIP SELECTED");equipSelected.onClick.RemoveAllListeners();equipSelected.onClick.AddListener(()=>{if(selectedBagIndex>=0)game.Equip(selectedBagIndex);});
            var bag=game.Inventory;bagCount.text="SATCHEL  "+bag.Items.Count+" / 24";
            for(int i=0;i<24;i++)
            {
                bool occupied=i<bag.Items.Count;var b=bagButtons[i];var text=b.GetComponentInChildren<TextMeshProUGUI>();text.text=occupied?bag.Items[i].Slot+"\nLv. "+bag.Items[i].ItemLevel:"—";text.color=occupied?ItemColor(bag.Items[i].Rarity):new Color(.3f,.36f,.36f);b.interactable=occupied;var icon=b.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();icon.sprite=occupied?equipmentIcons[(int)bag.Items[i].Slot]:null;icon.enabled=occupied;b.GetComponent<UnityEngine.UI.Outline>().effectColor=occupied?ItemColor(bag.Items[i].Rarity)*.65f:new Color(.15f,.20f,.21f);
            }
            for(int i=0;i<6;i++){var item=bag.Equipment[(ItemSlot)i];var b=equipButtons[i];b.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().sprite=equipmentIcons[i];b.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().enabled=true;b.GetComponentInChildren<TextMeshProUGUI>().text=((ItemSlot)i)+"\n"+(item==null?"Unassigned":"Lv. "+item.ItemLevel+" · "+item.Rarity);}
            var stats=EquipmentStats.Calculate(bag.Equipment);gearStats.text=$"COMBAT ATTRIBUTES\nDamage  {stats.Damage:0.0}     Armour  {stats.Armor:0.0}\nHealth  {stats.Health:0}     Movement  {stats.MoveSpeed:0.00} m/s";
            gearName.text="Choose a find";gearMeta.text="INSPECT BEFORE EQUIPPING";inventoryDetails.text="Select an item to inspect its 3D model, properties and effect on your attributes.";equippedDescription.text="Equipment changes are manual.\nSelect a worn slot to inspect or remove it.";
            if(InventoryOpen){if(bag.Items.Count>0){bagButtons[0].Select();SelectGear(0);}else inventoryClose.Select();}
        }
        private void SelectGear(int index)
        {
            var bag=OathfallGame.Instance.Inventory;selectedBagIndex=index;
            if(index<0||index>=bag.Items.Count){selectedBagIndex=-1;discardButton.interactable=false;equipSelected.interactable=false;return;}
            var item=bag.Items[index];var current=bag.Equipment[item.Slot];gearPreview.Show(item.Slot);previewCaption.text=ItemNames[(int)item.Slot].ToUpperInvariant()+" / 3D INSPECTION";
            gearName.text=ItemNames[(int)item.Slot];gearName.color=ItemColor(item.Rarity);gearMeta.text=item.Rarity.ToString().ToUpperInvariant()+"  /  "+item.Slot.ToString().ToUpperInvariant()+"  /  LEVEL "+item.ItemLevel;
            inventoryDetails.text=Describe(item).Substring(Describe(item).IndexOf('\n')+1).Replace("Base +","IMPLICIT\n+").Replace("max-health","maximum health").Replace("move-speed","movement speed");
            var a=EquipmentStats.Calculate(bag.Equipment);var b=EquipmentStats.PreviewSwap(bag.Equipment,item);
            equippedDescription.text="IF EQUIPPED\n"+Delta("Damage",a.Damage,b.Damage)+"   "+Delta("Armour",a.Armor,b.Armor)+"\n"+Delta("Health",a.Health,b.Health)+"   "+Delta("Speed",a.MoveSpeed,b.MoveSpeed)+"\nReplacing: "+(current==null?"empty slot":current.Rarity+" Lv. "+current.ItemLevel);
            discardButton.interactable=equipSelected.interactable=true;SetShortcutLabel(discardButton,"DISCARD: "+item.Slot);SetShortcutLabel(equipSelected,"EQUIP SELECTED");equipSelected.onClick.RemoveAllListeners();equipSelected.onClick.AddListener(()=>OathfallGame.Instance.Equip(selectedBagIndex));
        }
        private static string Delta(string name,float oldValue,float next)=>name+" <color="+(next>oldValue?"#9FC9A0":next<oldValue?"#DD9984":"#CCD5D1")+">"+(next-oldValue>=0?"+":"")+(next-oldValue).ToString("0.0")+"</color>";
        private void InspectEquipped(ItemSlot slot)
        {
            selectedBagIndex=-1;discardButton.interactable=false;SetShortcutLabel(discardButton,"DISCARD SELECTED");gearPreview.Show(slot);var item=OathfallGame.Instance.Inventory.Equipment[slot];previewCaption.text=ItemNames[(int)slot].ToUpperInvariant()+" / 3D INSPECTION";
            gearName.text=ItemNames[(int)slot];gearName.color=Gold;gearMeta.text=item==null?"CATALOG MODEL · SLOT EMPTY":"CURRENTLY EQUIPPED";inventoryDetails.text=item==null?"No item assigned to this slot.":Describe(item);equippedDescription.text="Removing equipment returns it to the satchel. A free slot is required.";equipSelected.interactable=item!=null;SetShortcutLabel(equipSelected,"UNEQUIP "+slot);equipSelected.onClick.RemoveAllListeners();equipSelected.onClick.AddListener(()=>OathfallGame.Instance.Unequip(slot));
        }
        private void PolishHud()
        {
            vitals.GetComponent<UnityEngine.UI.Image>().color=Ink;Frame(vitals,Gold);healthFill.color=new Color(.40f,.095f,.065f);healthLabel.fontSize=19;healthLabel.color=new Color(.95f,.88f,.72f);
            runStatus.fontSize=22;runStatus.rectTransform.sizeDelta=new Vector2(1120,110);runStatus.color=new Color(.88f,.83f,.68f);
            abilityLabel.fontSize=14;abilityLabel.color=new Color(.69f,.74f,.73f);abilityLabel.rectTransform.sizeDelta=new Vector2(900,22);
            abilityCards=new GameObject("Combat action cards",typeof(RectTransform));abilityCards.transform.SetParent(transform,false);SetRect(abilityCards.GetComponent<RectTransform>(),new Vector2(1,0),new Vector2(1,0),new Vector2(-28,58),new Vector2(816,76));
            string[] labels={"ATTACK","BLOCK","DODGE","SHIELD BASH","CLEAVING ARC","DRAUGHT"};string[] keys={"LMB / RT","RMB / LT","SPACE / A","Q / LB","E / RB","F / D-UP"};
            for(int i=0;i<6;i++)
            {
                var panel=Block(abilityCards.transform,labels[i],new Vector2(i*136,0),new Vector2(126,76),Ink);Frame(panel.gameObject,new Color(.32f,.30f,.23f));
                Label(panel.transform,"Binding",keys[i],new Vector2(9,-7),new Vector2(114,22),14,Gold);
                abilityCardsText[i]=Label(panel.transform,"Action",labels[i],new Vector2(9,-32),new Vector2(112,40),15,new Color(.85f,.87f,.82f));
            }
            inventoryPanel.transform.SetAsLastSibling();pausePanel.transform.SetAsLastSibling();
        }
        private void AudioSetting(string label,float y,System.Func<float> get,System.Action<float> set)
        {
            var b=ActionButton(pausePanel.transform,label+"  "+Mathf.RoundToInt(get()*100)+"%",new Vector2(25,y),new Vector2(590,56),()=>{});
            b.onClick.AddListener(()=>{set(get()>=.99f?0:get()+.25f);SetShortcutLabel(b,label+"  "+Mathf.RoundToInt(get()*100)+"%");Soundscape.Play(SoundCue.Click);});
        }
        private void UpdateAbilityCards(PlayerController player)
        {
            if(abilityCardsText[0]==null)return;abilityLabel.text="I / Y  Equipment     ESC / START  Pause";
            abilityCardsText[3].text="SHIELD BASH\n"+(player.BashCooldown>0?player.BashCooldown.ToString("0.0")+"s":"READY");
            abilityCardsText[4].text="CLEAVING ARC\n"+(player.CleaveCooldown>0?player.CleaveCooldown.ToString("0.0")+"s":"READY");
            abilityCardsText[5].text="DRAUGHT\n"+OathfallGame.Instance.Flasks+" / 3";
        }
    }
}
