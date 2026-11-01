using System;

using Oathfall.Core;

using TMPro;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem.UI;

using UnityEngine.UI;



namespace Oathfall.Gameplay

{

    public sealed partial class RuntimeHud : MonoBehaviour

    {

        private Action<HeroId> selectHero;

        private GameObject selectionPanel;

        private TextMeshProUGUI title;

        private TextMeshProUGUI runStatus;

        private TextMeshProUGUI selectionDescription;

        private TextMeshProUGUI message;

        private GameObject inventoryPanel, pausePanel;

        private TextMeshProUGUI inventoryDetails;

        private readonly System.Collections.Generic.List<Button> bagButtons = new System.Collections.Generic.List<Button>();

        private readonly System.Collections.Generic.List<Button> equipButtons = new System.Collections.Generic.List<Button>();

        private float messageUntil;private int selectedBagIndex=-1;private Button discardButton;

        private Image healthFill;private GameObject vitals;private TextMeshProUGUI healthLabel,abilityLabel;private Button primaryHero;private readonly Sprite[] equipmentIcons=new Sprite[6];private GameObject shortcuts;private int screenWidth,screenHeight;

        private static readonly string[] ItemNames={"Warden sword","Oath shield","Iron helm","Warden cuirass","Wayfarer greaves","Oathkeeper seal"};

        private void Update(){if(screenWidth!=Screen.width||screenHeight!=Screen.height){screenWidth=Screen.width;screenHeight=Screen.height;ApplyScale();}}

        public bool InventoryOpen => inventoryPanel != null && inventoryPanel.activeSelf;

        public void ClosePanels() { if(inventoryPanel!=null)inventoryPanel.SetActive(false); if(pausePanel!=null)pausePanel.SetActive(false); Time.timeScale=1; }

        public void TogglePause() { if(InventoryOpen){ToggleInventory();return;} pausePanel.SetActive(!pausePanel.activeSelf);Time.timeScale=pausePanel.activeSelf?0:1;if(pausePanel.activeSelf)pausePanel.GetComponentInChildren<Button>().Select();RefreshRunVisibility(); }

        public void ToggleInventory() { if(pausePanel.activeSelf)pausePanel.SetActive(false);inventoryPanel.SetActive(!inventoryPanel.activeSelf);Oathfall.Presentation.Soundscape.Play(InventoryOpen?Oathfall.Presentation.SoundCue.Open:Oathfall.Presentation.SoundCue.Close);Time.timeScale=InventoryOpen?0:1;if(!InventoryOpen)EventSystem.current.SetSelectedGameObject(null);RefreshInventory();if(InventoryOpen){if(OathfallGame.Instance.Inventory.Items.Count>0)bagButtons[0].Select();else inventoryClose.Select();}RefreshRunVisibility(); }

        private void RefreshRunVisibility()
        {
            bool visible=!selectionPanel.activeSelf&&!InventoryOpen&&!pausePanel.activeSelf;
            vitals.SetActive(visible);shortcuts.SetActive(visible);runStatus.gameObject.SetActive(visible);message.gameObject.SetActive(visible);abilityLabel.gameObject.SetActive(visible);abilityCards.SetActive(visible);
        }

        public void RefreshInventory()

        {

            RefreshGearUI();
        }

        public static Color ItemColor(ItemRarity rarity)=>rarity==ItemRarity.Rare?new Color(.93f,.74f,.3f):rarity==ItemRarity.Enchanted?new Color(.42f,.7f,.95f):new Color(.8f,.83f,.78f);

        private static string Describe(ItemInstance item)

        {

            var text=item.Rarity+" "+ItemNames[(int)item.Slot]+"  Lv."+item.ItemLevel;

            text+="\nBase +"+(item.Slot==ItemSlot.Weapon?(4+item.ItemLevel*2)+" damage":item.Slot==ItemSlot.Charm?(8+item.ItemLevel*2)+" health":(2+item.ItemLevel)+" armor");

            foreach(var affix in item.Affixes)text+="\n+"+affix.Value.ToString("0.0")+(affix.AffixId=="move-speed"?"% ":" ")+affix.AffixId;

            return text;

        }



        public bool IsRunHudVisible => runStatus != null && runStatus.gameObject.activeSelf && message != null && message.gameObject.activeSelf;



        public static RuntimeHud Create(Action<HeroId> onSelectHero)

        {

            var canvasObject = new GameObject("Oathfall HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = canvasObject.GetComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution = new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            if (FindFirstObjectByType<EventSystem>() == null)

            {

                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

                if (Application.isPlaying)

                {

                    DontDestroyOnLoad(eventSystem);

                }

            }



            var hud = canvasObject.AddComponent<RuntimeHud>();

            hud.selectHero = onSelectHero;

            hud.Build();

            return hud;

        }



        public void ShowHeroSelection(string overrideMessage = null)

        {

            runStatus.gameObject.SetActive(false);

            message.gameObject.SetActive(false);

            selectionPanel.SetActive(true);shortcuts.SetActive(false);vitals.SetActive(false);abilityLabel.gameObject.SetActive(false);primaryHero.Select();if(abilityCards!=null)abilityCards.SetActive(false);

            title.text = "OATHFALL";

            selectionDescription.text = overrideMessage ?? "Choose a hero for the Fallen Oath Shrine.";

        }



        public void HideHeroSelection()

        {

            selectionPanel.SetActive(false);shortcuts.SetActive(true);vitals.SetActive(true);abilityLabel.gameObject.SetActive(true);

            if(abilityCards!=null)abilityCards.SetActive(true);
            runStatus.gameObject.SetActive(true);

            message.gameObject.SetActive(true);

        }



        public void UpdateRun(PlayerController player, int defeatedEnemies, int corruption)

        {

            RefreshRunVisibility();

            var combatant = player.GetComponent<Combatant>();

            runStatus.text = "HOLLOWWOOD  /  "+(OathfallGame.Instance.Location.ToUpperInvariant())+"  "+Mathf.Min(OathfallGame.Instance.Stage+1,Oathfall.Core.ExpeditionContent.Stops.Length)+"/"+Oathfall.Core.ExpeditionContent.Stops.Length+"  "+System.TimeSpan.FromSeconds(OathfallGame.Instance.Elapsed).ToString(@"mm\:ss")+"\n<size=21>"+OathfallGame.Instance.Objective+"</size>";

            healthFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(combatant.Health/combatant.MaxHealth),1);

            healthLabel.text=$"DAWNWARDEN    {Mathf.CeilToInt(combatant.Health)} / {Mathf.CeilToInt(combatant.MaxHealth)}";

            abilityLabel.text=$"<color=#D6B67D>Q / LB</color>  SHIELD BASH  {(player.BashCooldown>0?player.BashCooldown.ToString("0.0")+"s":"READY")}     <color=#D6B67D>E / RB</color>  CLEAVING ARC  {(player.CleaveCooldown>0?player.CleaveCooldown.ToString("0.0")+"s":"READY")}\n<size=18>SPACE / A  Dodge     RMB / LT  Block     I / Y  Equipment     F / D-PAD UP  Draught: {OathfallGame.Instance.Flasks}</size>";

            UpdateAbilityCards(player);
            if(Time.unscaledTime>messageUntil){message.text="";if(gearFeedback!=null)gearFeedback.text="";}

        }



        public void ShowMessage(string text)

        {

            message.text = text;

            if(InventoryOpen&&gearFeedback!=null)gearFeedback.text=text;

            messageUntil=Time.unscaledTime+6;

        }



        private void Build()

        {

            for(int i=0;i<6;i++)equipmentIcons[i]=Resources.Load<Sprite>("OathfallIcons/"+(ItemSlot)i);

            runStatus = CreateText("Status", transform, 28, TextAlignmentOptions.Left);

            SetRect(runStatus.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(1400f, 125f));

            runStatus.fontSize=26;runStatus.color=new Color(.88f,.83f,.67f);

            message = CreateText("Message", transform, 22, TextAlignmentOptions.Left);

            SetRect(message.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 28f), new Vector2(1200f, 60f));



            selectionPanel = new GameObject("Hero Selection", typeof(Image));

            selectionPanel.transform.SetParent(transform, false);

            var background = selectionPanel.GetComponent<Image>();

            background.color = new Color(0.035f, 0.05f, 0.06f, 0.93f);

            var panelRect = selectionPanel.GetComponent<RectTransform>();

            SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 570f));



            title = CreateText("Title", selectionPanel.transform, 62, TextAlignmentOptions.Center);

            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(780f, 88f));

            selectionDescription = CreateText("Description", selectionPanel.transform, 24, TextAlignmentOptions.Center);

            SetRect(selectionDescription.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(780f, 74f));



            CreateHeroButton(HeroId.Dawnwarden, "1  Dawnwarden\nSword and shield\nAVAILABLE", new Vector2(-260f, -35f), new Color(0.50f, 0.28f, 0.06f), true);

            CreateHeroButton(HeroId.Thornstrider, "Thornstrider\nCOMING IN A LATER SLICE", new Vector2(0f, -35f), new Color(0.10f, 0.16f, 0.11f), false);

            CreateHeroButton(HeroId.Runebinder, "Runebinder\nCOMING IN A LATER SLICE", new Vector2(260f, -35f), new Color(0.10f, 0.12f, 0.18f), false);

            var help = CreateText("Help", selectionPanel.transform, 18, TextAlignmentOptions.Center);

            help.text = "Move: WASD / left stick   Attack: left mouse / right trigger   Block: right mouse / left trigger   Dodge: Space / South button";

            SetRect(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(830f, 60f));

            vitals=new GameObject("Health",typeof(Image));vitals.transform.SetParent(transform,false);vitals.GetComponent<Image>().color=new Color(.06f,.075f,.065f,.93f);

            SetRect(vitals.GetComponent<RectTransform>(),new Vector2(0,0),new Vector2(0,0),new Vector2(28,28),new Vector2(410,62));

            var fill=new GameObject("Health Fill",typeof(Image));fill.transform.SetParent(vitals.transform,false);healthFill=fill.GetComponent<Image>();healthFill.color=new Color(.5f,.16f,.12f);SetRect(healthFill.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(-6,-6));

            healthLabel=CreateText("Health value",vitals.transform,22,TextAlignmentOptions.Center);SetRect(healthLabel.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);

            abilityLabel=CreateText("Ability slots",transform,22,TextAlignmentOptions.BottomRight);SetRect(abilityLabel.rectTransform,new Vector2(1,0),new Vector2(1,0),new Vector2(-28,28),new Vector2(1000,72));

            message.rectTransform.anchoredPosition=new Vector2(28,105);message.fontSize=21;

            BuildInventory();

            shortcuts=new GameObject("Run shortcuts",typeof(RectTransform));shortcuts.transform.SetParent(transform,false);SetRect(shortcuts.GetComponent<RectTransform>(),Vector2.one,Vector2.one,new Vector2(-28,-24),new Vector2(280,56));

            var equipment=InventoryButton(shortcuts.transform,Vector2.zero,new Vector2(136,56));SetShortcutLabel(equipment,"EQUIPMENT\n[I]");equipment.onClick.AddListener(ToggleInventory);equipment.navigation=new Navigation{mode=Navigation.Mode.None};

            var pause=InventoryButton(shortcuts.transform,new Vector2(144,0),new Vector2(136,56));SetShortcutLabel(pause,"PAUSE\n[ESC]");pause.onClick.AddListener(TogglePause);pause.navigation=new Navigation{mode=Navigation.Mode.None};
            PolishHud();

        }



        private void BuildInventory()

        {

            BuildGearUI();

            pausePanel=Panel("Pause",new Vector2(640,760));var text=CreateText("PauseLabel",pausePanel.transform,32,TextAlignmentOptions.Center);text.text="PAUSED  /  SETTINGS";SetRect(text.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-25),new Vector2(600,60));

            SettingButton("Resume",-105,TogglePause);

            SettingButton("UI scale — cycle 85 / 100 / 115%",-185,()=>{var x=Oathfall.Presentation.PresentationSettings.UiScale;Oathfall.Presentation.PresentationSettings.UiScale=x<.99f?1:x<1.1f?1.15f:.85f;ApplyScale();});

            SettingButton("Hit flashes — cycle Off / Reduced / Full",-265,()=>{var x=Oathfall.Presentation.PresentationSettings.Flash;Oathfall.Presentation.PresentationSettings.Flash=x<.1f?.5f:x<.9f?1:0;});

            SettingButton("Camera shake — cycle Off / Reduced / Full",-345,()=>{var x=Oathfall.Presentation.PresentationSettings.Shake;Oathfall.Presentation.PresentationSettings.Shake=x<.1f?.35f:x<.9f?1:0;});

            AudioSetting("Master volume",-415,()=>Oathfall.Presentation.PresentationSettings.Master,v=>Oathfall.Presentation.PresentationSettings.Master=v);
            AudioSetting("Music",-485,()=>Oathfall.Presentation.PresentationSettings.Music,v=>Oathfall.Presentation.PresentationSettings.Music=v);
            AudioSetting("Forest ambience",-555,()=>Oathfall.Presentation.PresentationSettings.Ambience,v=>Oathfall.Presentation.PresentationSettings.Ambience=v);
            AudioSetting("Combat / UI effects",-625,()=>Oathfall.Presentation.PresentationSettings.Sfx,v=>Oathfall.Presentation.PresentationSettings.Sfx=v);
            pausePanel.SetActive(false);ApplyScale();

        }

        private static void SetShortcutLabel(Button button,string label)
        {
            var text=button.GetComponentInChildren<TextMeshProUGUI>();text.text=label;
            // Shortcuts have no item icon; reserve the full width for their label.
            text.rectTransform.anchoredPosition=Vector2.zero;text.rectTransform.sizeDelta=new Vector2(-12,-8);
        }
        private void SettingButton(string label,float y,Action action){var b=InventoryButton(pausePanel.transform,new Vector2(25,y),new Vector2(590,65));SetShortcutLabel(b,label);b.onClick.AddListener(()=>action());}

        private void ApplyScale(){var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=Mathf.Min(Mathf.Min(Screen.width/1920f,Screen.height/1080f)*Oathfall.Presentation.PresentationSettings.UiScale,Mathf.Min(Screen.width/1720f,Screen.height/950f));}

        private void Compare(int index)

        {
            SelectGear(index);
        }
        private GameObject Panel(string name,Vector2 size)

        {

            var panel=new GameObject(name,typeof(Image));panel.transform.SetParent(transform,false);panel.GetComponent<Image>().color=new Color(.035f,.055f,.052f,.98f);

            SetRect(panel.GetComponent<RectTransform>(),Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,size);return panel;

        }

        private Button InventoryButton(Transform parent,Vector2 position,Vector2 size)

        {

            var obj=new GameObject("Item",typeof(Image),typeof(Button));obj.transform.SetParent(parent,false);obj.GetComponent<Image>().color=new Color(.14f,.19f,.17f);

            SetRect(obj.GetComponent<RectTransform>(),new Vector2(0,1),new Vector2(0,1),position,size);

            var icon=new GameObject("Icon",typeof(Image));icon.transform.SetParent(obj.transform,false);icon.GetComponent<Image>().raycastTarget=false;SetRect(icon.GetComponent<RectTransform>(),new Vector2(0,1),new Vector2(0,1),new Vector2(5,-5),new Vector2(36,36));icon.GetComponent<Image>().enabled=false;

            var text=CreateText("ItemLabel",obj.transform,15,TextAlignmentOptions.Center);SetRect(text.rectTransform,Vector2.zero,Vector2.one,new Vector2(20,0),new Vector2(-52,-8));return obj.GetComponent<Button>();

        }



        private void CreateHeroButton(HeroId hero, string label, Vector2 anchoredPosition, Color color, bool available)

        {

            var buttonObject = new GameObject(hero.ToString(), typeof(Image), typeof(Button));

            buttonObject.transform.SetParent(selectionPanel.transform, false);

            var image = buttonObject.GetComponent<Image>();

            image.color = color;

            var button = buttonObject.GetComponent<Button>();

            button.targetGraphic = image;

            button.interactable = available;if(available)primaryHero=button;

            button.onClick.AddListener(() => selectHero?.Invoke(hero));

            var rect = buttonObject.GetComponent<RectTransform>();

            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(230f, 200f));

            var text = CreateText("Label", buttonObject.transform, 22, TextAlignmentOptions.Center);

            text.text = label;

            text.color = available ? Color.white : new Color(0.63f, 0.67f, 0.70f);

            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        }



        private static TextMeshProUGUI CreateText(string name, Transform parent, float size, TextAlignmentOptions alignment)

        {

            var textObject = new GameObject(name, typeof(TextMeshProUGUI));

            textObject.transform.SetParent(parent, false);

            var text = textObject.GetComponent<TextMeshProUGUI>();

            text.font = TMP_Settings.defaultFontAsset;

            text.fontSize = size;

            text.alignment = alignment;

            text.color = Color.white;

            text.raycastTarget = false;

            return text;

        }



        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)

        {

            rect.anchorMin = anchorMin;

            rect.anchorMax = anchorMax;

            rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = position;

            rect.sizeDelta = size;

        }

    }

}

