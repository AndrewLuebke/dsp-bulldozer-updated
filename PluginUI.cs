using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulldozer
{
    public class UIElements : MonoBehaviour
    {
        private const string DEFAULT_TIP_MESSAGE_PT1 =
            "Adds foundation to entire planet. Any existing foundation decoration will be lost.\n";

        private const string DEFAULT_TIP_MESSAGE_LAT_PT1 =
            "Adds foundation to all locations in selected latitude range.\n";

        private const string DEFAULT_TIP_MESSAGE_VEINS_NO_ALTER = "Veins will not be raised or lowered";

        private const string DEFAULT_TIP_MESSAGE_DESTROY_FACTORY =
            "Destroy all factory machines";

        private const string DEFAULT_TIP_MESSAGE_DESTROY_FACTORY_IN_LAT =
            "Destroy all factory machines in selected latitude range";

        // Layout of Bulldozer's controls, left of the game's foundation button:
        // [Bulldozer button (scaled down)] ButtonGap [checkbox column] CheckBoxGap [foundation button]
        // The space between the build menu's left edge and the foundation button is narrow, so the button is
        // drawn smaller than the game's buttons and settings are opened by right-clicking it.
        private const float BulldozeButtonScale = 0.75f;
        private const float ButtonGap = 4f;
        private const float CheckBoxGap = 6f;
        private const float CheckBoxWidth = 20f;
        private RectTransform _bulldozeButtonHolder;
        private static readonly Vector3[] WorldCorners = new Vector3[4];

        public static ManualLogSource logger;
        private readonly List<GameObject> gameObjectsToDestroy = new();

        public GameObject DrawEquatorCheck;
        public Sprite spriteChecked;
        public Sprite spriteUnChecked;

        public UIButton PaveActionButton;
        public RectTransform BulldozeButton;
        public Text countText;
        public Text buttonHotkeyTextComponent;
        public Transform CountTransform;
        public CheckboxControl drawEquatorCheckboxButton;
        private bool _alterVeinsField;
        private Transform _bulldozeIcon;
        private bool _destroyFactoryMachines;

        // use to track whether checkbox needs to be synced
        private bool _drawEquatorField = true;
        private Image _iconImage;

        private bool _techUnlocked;
        private bool _readyToGo;
        [NonSerialized] public Image AlterVeinsCheckBoxImage;
        [NonSerialized] public Image CheckBoxImage;

        private bool currentTipMessageIsDefault = true;
        [NonSerialized] public Image DestroyMachinesCheckBoxImage;
        private Text hover;
        private UIButton mainActionButton;
        public int initPercent;
        private RectTransform buttonOneRectTransform;
        private GameObject AlterVeinsCheck;
        private GameObject DestroyMachinesCheck;
        private bool _initialized;

        public bool IsUsable => _initialized
                                && BulldozeButton != null
                                && PaveActionButton != null
                                && PaveActionButton.button != null
                                && mainActionButton != null
                                && mainActionButton.button != null;

        public bool TechUnlockedState
        {
            get => _techUnlocked;
            set
            {
                _techUnlocked = value;
                if (mainActionButton == null || mainActionButton.button == null)
                {
                    return;
                }

                if (!_techUnlocked)
                {
                    mainActionButton.button.interactable = false;
                    mainActionButton.tips.tipText = "Research Universe Exploration 3 to Unlock";
                }
                else
                {
                    mainActionButton.button.interactable = true;
                    mainActionButton.tips.tipText = ConstructTipMessageDependentOnConfig();
                }
            }
        }

        public bool ReadyForAction
        {
            set
            {
                _readyToGo = value;
                if (mainActionButton == null || mainActionButton.button == null)
                {
                    return;
                }

                if (_techUnlocked && !_readyToGo)
                {
                    mainActionButton.button.interactable = false;
                    mainActionButton.tips.tipText = $"Initializing ({initPercent}% complete)";
                }
                else if (_readyToGo && _techUnlocked)
                {
                    mainActionButton.button.interactable = true;
                    mainActionButton.tips.tipText = ConstructTipMessageDependentOnConfig();
                }
            }
        }

        public void Update()
        {
            if (!IsUsable)
            {
                return;
            }

            if (_drawEquatorField != PluginConfig.addGuideLines.Value)
            {
                // value might have been updated in config manager plugin ui
                if (CheckBoxImage != null)
                    CheckBoxImage.sprite = PluginConfig.addGuideLines.Value ? spriteChecked : spriteUnChecked;
                _drawEquatorField = PluginConfig.addGuideLines.Value;
            }

            if (_alterVeinsField != PluginConfig.alterVeinState.Value)
            {
                // sync checkbox with externally changed value
                if (AlterVeinsCheckBoxImage != null)
                    AlterVeinsCheckBoxImage.sprite = PluginConfig.alterVeinState.Value ? spriteChecked : spriteUnChecked;
                _alterVeinsField = PluginConfig.alterVeinState.Value;
            }

            if (_destroyFactoryMachines != PluginConfig.destroyFactoryAssemblers.Value)
            {
                // sync checkbox with externally changed value
                if (DestroyMachinesCheckBoxImage != null)
                    DestroyMachinesCheckBoxImage.sprite = PluginConfig.destroyFactoryAssemblers.Value ? spriteChecked : spriteUnChecked;
                _destroyFactoryMachines = PluginConfig.destroyFactoryAssemblers.Value;
            }

            if (!_techUnlocked && PluginConfig.disableTechRequirement.Value)
            {
                TechUnlockedState = true;
            }

            if (_destroyFactoryMachines)
            {
                if (mainActionButton != null && currentTipMessageIsDefault)
                {
                    currentTipMessageIsDefault = false;
                    if (PluginConfig.IsLatConstrained())
                    {
                        mainActionButton.tips.tipText = DEFAULT_TIP_MESSAGE_DESTROY_FACTORY_IN_LAT;
                    }
                    else
                    {
                        mainActionButton.tips.tipText = DEFAULT_TIP_MESSAGE_DESTROY_FACTORY;
                    }
                }
            }
            else if (mainActionButton != null && !currentTipMessageIsDefault)
            {
                currentTipMessageIsDefault = true;
                mainActionButton.tips.tipText = ConstructTipMessageDependentOnConfig();
            }
        }

        private string ConstructTipMessageDependentOnConfig()
        {
            var veinsAlterMessage = $"Will attempt to {PluginConfig.GetCurrentVeinsRaiseState()} all veins for planet.";
            if (PluginConfig.IsLatConstrained())
            {
                veinsAlterMessage = $"Will attempt to {PluginConfig.GetCurrentVeinsRaiseState()} veins in selected latitude range ({PluginConfig.GetLatRangeString()})";
            }

            var part2 = PluginConfig.alterVeinState.Value ? veinsAlterMessage : DEFAULT_TIP_MESSAGE_VEINS_NO_ALTER;
            currentTipMessageIsDefault = !PluginConfig.alterVeinState.Value;
            if (PluginConfig.IsLatConstrained())
            {
                return DEFAULT_TIP_MESSAGE_LAT_PT1 + part2;
            }

            return DEFAULT_TIP_MESSAGE_PT1 + part2;
        }

        // Position of the game's foundation button as the game laid it out, captured the first time this session.
        // Older versions moved the button in sandbox mode and persisted its original position to the config,
        // which meant a value saved under an older game layout kept forcing the button back to that layout after
        // DSP rearranged the Environment Modification row (0.10.35 added the terrain restoration button).
        private static Vector2? _originalFoundationButtonPosition;

        private static void ResetButtonPos(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            if (_originalFoundationButtonPosition == null)
            {
                _originalFoundationButtonPosition = rectTransform.anchoredPosition;
                return;
            }

            if (rectTransform.anchoredPosition != _originalFoundationButtonPosition.Value)
            {
                Log.Debug($"Setting button back to original {_originalFoundationButtonPosition.Value}");
                rectTransform.anchoredPosition = _originalFoundationButtonPosition.Value;
            }
        }


        public bool TryAddBulldozeComponents(RectTransform environmentModificationContainer, GameObject foundationButton,
            Action<int> action, out string failureReason)
        {
            failureReason = null;
            try
            {
                Unload();
                if (environmentModificationContainer == null)
                {
                    failureReason = "Unable to initialize Bulldozer UI: Environment Modification container is null.";
                    return false;
                }

                if (!InitOnOffSprites(out failureReason) || !InitActionButton(foundationButton, action, out failureReason))
                {
                    Unload();
                    return false;
                }

                InitDrawEquatorCheckbox(environmentModificationContainer, foundationButton);
                InitAlterVeinsCheckbox(environmentModificationContainer);
                InitDestroyMachinesCheckbox(environmentModificationContainer);

                if (CheckBoxImage == null || AlterVeinsCheckBoxImage == null || DestroyMachinesCheckBoxImage == null)
                {
                    failureReason = "Unable to initialize Bulldozer UI: one or more custom controls could not be created.";
                    Unload();
                    return false;
                }

                _initialized = true;
                return true;
            }
            catch (Exception e)
            {
                failureReason = $"Unable to initialize Bulldozer UI controls: {e.Message}";
                Unload();
                return false;
            }
        }

        private bool InitActionButton(GameObject buttonToCopy, Action<int> action, out string failureReason)
        {
            failureReason = null;
            if (buttonToCopy == null)
            {
                failureReason = "Unable to initialize Bulldozer UI: foundation button is null.";
                return false;
            }

            buttonOneRectTransform = buttonToCopy.gameObject.GetComponent<RectTransform>();
            if (buttonOneRectTransform == null)
            {
                failureReason = "Unable to initialize Bulldozer UI: foundation button has no RectTransform.";
                return false;
            }

            countText = null;
            ResetButtonPos(buttonOneRectTransform);
            BulldozeButton = CopyButton(buttonOneRectTransform, Vector2.zero, out countText,
                Helper.GetSprite("bulldoze"), action);
            if (BulldozeButton == null || mainActionButton == null || mainActionButton.button == null)
            {
                failureReason = "Unable to initialize Bulldozer UI: foundation button does not contain the expected action control.";
                return false;
            }

            gameObjectsToDestroy.Add(BulldozeButton.gameObject);
            WrapBulldozeButtonInHolder();
            PaveActionButton = mainActionButton;
            if (countText != null)
            {
                gameObjectsToDestroy.Add(countText.gameObject);
            }

            return true;
        }

        private bool InitOnOffSprites(out string failureReason)
        {
            failureReason = null;
            Texture2D texOff = Resources.Load<Texture2D>("ui/textures/sprites/icons/checkbox-off");
            Texture2D texOn = Resources.Load<Texture2D>("ui/textures/sprites/icons/checkbox-on");
            if (texOff == null || texOn == null)
            {
                failureReason = "Unable to initialize Bulldozer UI: checkbox sprites could not be loaded.";
                return false;
            }
            spriteChecked = Sprite.Create(texOn, new Rect(0, 0, texOn.width, texOn.height), new Vector2(0.5f, 0.5f));
            spriteUnChecked = Sprite.Create(texOff, new Rect(0, 0, texOff.width, texOff.height), new Vector2(0.5f, 0.5f));
            return true;
        }

        private void InitDrawEquatorCheckbox(RectTransform environmentModificationContainer, GameObject button1)
        {
            DrawEquatorCheck = new GameObject("Draw equator line");
            gameObjectsToDestroy.Add(DrawEquatorCheck);
            RectTransform rect = DrawEquatorCheck.AddComponent<RectTransform>();
            rect.SetParent(environmentModificationContainer.transform, false);

            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(20, 20);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(GetCheckBoxXValue(), -90);
            drawEquatorCheckboxButton = rect.gameObject.AddComponent<CheckboxControl>();
            drawEquatorCheckboxButton.HoverText = "Add painted guidelines at various locations (equator, tropics, meridians configurable)";
            drawEquatorCheckboxButton.onClick += OnDrawEquatorCheckClick;
            gameObjectsToDestroy.Add(drawEquatorCheckboxButton.gameObject);

            if (countText != null)
            {
                hover = Instantiate(countText, environmentModificationContainer.transform, true);
                gameObjectsToDestroy.Add(hover.gameObject);
                var copiedRectTransform = hover.GetComponent<RectTransform>();
                var parentRect = environmentModificationContainer.GetComponent<RectTransform>();
                copiedRectTransform.anchorMin = new Vector2(0, 1);
                copiedRectTransform.anchorMax = new Vector2(0, 1);
                copiedRectTransform.sizeDelta = new Vector2(500, 20);
                copiedRectTransform.anchoredPosition = new Vector2(620, parentRect.transform.position.y - 115);
                drawEquatorCheckboxButton.textObject = hover;
            }

            gameObjectsToDestroy.Add(drawEquatorCheckboxButton.gameObject);

            CheckBoxImage = drawEquatorCheckboxButton.gameObject.AddComponent<Image>();
            CheckBoxImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
            gameObjectsToDestroy.Add(CheckBoxImage.gameObject);

            CheckBoxImage.sprite = PluginConfig.addGuideLines.Value ? spriteChecked : spriteUnChecked;
        }

        private void InitAlterVeinsCheckbox(RectTransform environmentModificationContainer)
        {
            AlterVeinsCheck = new GameObject("Repave");
            gameObjectsToDestroy.Add(AlterVeinsCheck);
            RectTransform rect = AlterVeinsCheck.AddComponent<RectTransform>();
            rect.SetParent(environmentModificationContainer.transform, false);

            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(20, 20);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(GetCheckBoxXValue(), -105);
            var repaveCheckboxButton = rect.gameObject.AddComponent<CheckboxControl>();
            repaveCheckboxButton.HoverText = "Check to attempt to raise or lower all veins. No foundation will be placed";

            if (countText != null)
            {
                var repaveHover = Instantiate(countText, environmentModificationContainer.transform, true);
                gameObjectsToDestroy.Add(repaveHover.gameObject);
                var copiedRectTransform = repaveHover.GetComponent<RectTransform>();
                var parentRect = environmentModificationContainer.GetComponent<RectTransform>();
                copiedRectTransform.anchorMin = new Vector2(0, 1);
                copiedRectTransform.anchorMax = new Vector2(0, 1);
                copiedRectTransform.sizeDelta = new Vector2(500, 20);
                copiedRectTransform.anchoredPosition = new Vector2(700, parentRect.transform.position.y - 115);
                repaveCheckboxButton.textObject = repaveHover;
            }

            gameObjectsToDestroy.Add(repaveCheckboxButton.gameObject);

            AlterVeinsCheckBoxImage = repaveCheckboxButton.gameObject.AddComponent<Image>();
            AlterVeinsCheckBoxImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
            gameObjectsToDestroy.Add(AlterVeinsCheckBoxImage.gameObject);

            AlterVeinsCheckBoxImage.sprite = PluginConfig.alterVeinState.Value ? spriteChecked : spriteUnChecked;
            repaveCheckboxButton.onClick += OnAlterVeinCheckClick;
        }

        private void InitDestroyMachinesCheckbox(RectTransform environmentModificationContainer)
        {
            DestroyMachinesCheck = new GameObject("DestroyMachines");
            gameObjectsToDestroy.Add(DestroyMachinesCheck);
            RectTransform rect = DestroyMachinesCheck.AddComponent<RectTransform>();
            rect.SetParent(environmentModificationContainer.transform, false);

            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(20, 20);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(GetCheckBoxXValue(), -120);
            var destroyMachinesButton = rect.gameObject.AddComponent<CheckboxControl>();
            destroyMachinesButton.HoverText = "Clear all factory machines. Skip adding foundation.";

            if (countText != null)
            {
                var destroyHover = Instantiate(countText, environmentModificationContainer.transform, true);
                gameObjectsToDestroy.Add(destroyHover.gameObject);
                var copiedRectTransform = destroyHover.GetComponent<RectTransform>();
                var parentRect = environmentModificationContainer.GetComponent<RectTransform>();
                copiedRectTransform.anchorMin = new Vector2(0, 1);
                copiedRectTransform.anchorMax = new Vector2(0, 1);
                copiedRectTransform.sizeDelta = new Vector2(800, 20);
                copiedRectTransform.anchoredPosition = new Vector2(700, parentRect.transform.position.y - 115);
                destroyMachinesButton.textObject = destroyHover;
            }

            gameObjectsToDestroy.Add(destroyMachinesButton.gameObject);

            DestroyMachinesCheckBoxImage = destroyMachinesButton.gameObject.AddComponent<Image>();
            DestroyMachinesCheckBoxImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
            gameObjectsToDestroy.Add(DestroyMachinesCheckBoxImage.gameObject);

            DestroyMachinesCheckBoxImage.sprite = PluginConfig.destroyFactoryAssemblers.Value ? spriteChecked : spriteUnChecked;
            destroyMachinesButton.onClick += OnDestroyMachinesCheckClick;
        }

        /// <summary>
        /// X position (from the container's left edge, matching the checkboxes' top-left anchors) of the column of
        /// option checkboxes and the config icon. The column sits directly to the left of the game's foundation
        /// button, between it and the Bulldozer button. DSP 0.10.35 filled the space to the right of the
        /// foundation button (terrain restoration, vegetation transplant, vein raise/bury), so the old fixed
        /// position put the checkboxes on top of the vegetation transplant button.
        /// </summary>
        private float GetCheckBoxXValue()
        {
            var container = buttonOneRectTransform != null ? buttonOneRectTransform.parent as RectTransform : null;
            if (container == null)
            {
                return 350;
            }

            buttonOneRectTransform.GetWorldCorners(WorldCorners);
            var foundationLeft = container.InverseTransformPoint(WorldCorners[0]).x - container.rect.xMin;
            return foundationLeft - CheckBoxGap - CheckBoxWidth;
        }

        /// <summary>
        /// Shrinks the Bulldozer button and places it directly left of the checkbox column, vertically centred on
        /// the game's foundation button. The space to the right of the foundation button is taken by the game's
        /// own controls since DSP 0.10.35 and is split across several containers, so the left side is the only
        /// spot that does not depend on how the game lays out the rest of the row. Positions are measured from the
        /// rendered corners so the result does not depend on the button's pivot or anchors.
        /// </summary>
        private void PositionBulldozeButton()
        {
            var container = buttonOneRectTransform.parent as RectTransform;
            if (container == null)
            {
                return;
            }

            // Scale and move the holder, not the button: the game's UIButton animates the button's own scale
            // (hover/press effects) and resets it to 1, which undid a scale applied to the button directly.
            var holder = _bulldozeButtonHolder != null ? _bulldozeButtonHolder : BulldozeButton;
            holder.localScale = Vector3.one * BulldozeButtonScale;
            holder.anchoredPosition = buttonOneRectTransform.anchoredPosition;

            buttonOneRectTransform.GetWorldCorners(WorldCorners);
            var foundationMin = (Vector2)container.InverseTransformPoint(WorldCorners[0]);
            var foundationMax = (Vector2)container.InverseTransformPoint(WorldCorners[2]);
            holder.GetWorldCorners(WorldCorners);
            var bulldozeMin = (Vector2)container.InverseTransformPoint(WorldCorners[0]);
            var bulldozeMax = (Vector2)container.InverseTransformPoint(WorldCorners[2]);

            var targetRight = container.rect.xMin + GetCheckBoxXValue() - ButtonGap;
            var targetCenterY = (foundationMin.y + foundationMax.y) / 2;
            var delta = new Vector2(targetRight - bulldozeMax.x, targetCenterY - (bulldozeMin.y + bulldozeMax.y) / 2);
            holder.anchoredPosition += delta;
        }

        /// <summary>
        /// Puts the Bulldozer button inside an empty holder with the foundation button's size and anchors, with the
        /// button stretched to fill it, so the holder can be scaled without the game's button animations undoing it.
        /// </summary>
        private void WrapBulldozeButtonInHolder()
        {
            var parent = BulldozeButton.parent;
            var holderObject = new GameObject("BulldozerButtonHolder", typeof(RectTransform));
            _bulldozeButtonHolder = holderObject.GetComponent<RectTransform>();
            _bulldozeButtonHolder.SetParent(parent, false);
            _bulldozeButtonHolder.anchorMin = buttonOneRectTransform.anchorMin;
            _bulldozeButtonHolder.anchorMax = buttonOneRectTransform.anchorMax;
            _bulldozeButtonHolder.pivot = buttonOneRectTransform.pivot;
            _bulldozeButtonHolder.sizeDelta = buttonOneRectTransform.sizeDelta;
            _bulldozeButtonHolder.anchoredPosition = buttonOneRectTransform.anchoredPosition;
            gameObjectsToDestroy.Add(holderObject);

            BulldozeButton.SetParent(_bulldozeButtonHolder, false);
            BulldozeButton.anchorMin = Vector2.zero;
            BulldozeButton.anchorMax = Vector2.one;
            BulldozeButton.pivot = buttonOneRectTransform.pivot;
            BulldozeButton.offsetMin = Vector2.zero;
            BulldozeButton.offsetMax = Vector2.zero;
            BulldozeButton.localScale = Vector3.one;
        }

        public void Unload()
        {
            _initialized = false;
            if (buttonOneRectTransform != null)
            {
                ResetButtonPos(buttonOneRectTransform);
            }

            try
            {
                while (gameObjectsToDestroy.Count > 0)
                {
                    Destroy(gameObjectsToDestroy[0]);
                    gameObjectsToDestroy.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"failed to do unload {e.Message}");
                logger.LogWarning(e.StackTrace);
            }

            BulldozeButton = null;
            _bulldozeButtonHolder = null;
            PaveActionButton = null;
            mainActionButton = null;
            countText = null;
            buttonOneRectTransform = null;
            CheckBoxImage = null;
            AlterVeinsCheckBoxImage = null;
            DestroyMachinesCheckBoxImage = null;
        }

        public RectTransform CopyButton(RectTransform rectTransform, Vector2 positionDelta, out Text countComponent, Sprite newIcon, Action<int> action)
        {
            countComponent = null;
            if (rectTransform == null || newIcon == null || action == null)
            {
                return null;
            }

            var copied = Instantiate(rectTransform, rectTransform.transform.parent, false);
            var copiedRectTransform = copied.GetComponent<RectTransform>();
            if (copiedRectTransform == null)
            {
                Destroy(copied.gameObject);
                return null;
            }

            copiedRectTransform.anchorMin = rectTransform.anchorMin;
            copiedRectTransform.anchorMax = rectTransform.anchorMax;
            copiedRectTransform.sizeDelta = rectTransform.sizeDelta;
            copiedRectTransform.anchoredPosition = rectTransform.anchoredPosition + positionDelta;


            _bulldozeIcon = copiedRectTransform.transform.Find("icon");
            if (_bulldozeIcon != null)
            {
                gameObjectsToDestroy.Add(_bulldozeIcon.gameObject);
                _iconImage = _bulldozeIcon.GetComponentInChildren<Image>();
                if (_iconImage != null)
                {
                    _iconImage.sprite = newIcon;
                    gameObjectsToDestroy.Add(_iconImage.gameObject);
                }
            }

            var buttonHotkeyText = copiedRectTransform.transform.Find("text");
            if (buttonHotkeyText != null)
            {
                buttonHotkeyTextComponent = buttonHotkeyText.GetComponentInChildren<Text>();
                if (buttonHotkeyTextComponent != null)
                {
                    buttonHotkeyTextComponent.text = "";
                }
            }

            mainActionButton = copiedRectTransform.GetComponentInChildren<UIButton>();
            if (mainActionButton == null || mainActionButton.button == null)
            {
                Destroy(copied.gameObject);
                return null;
            }

            mainActionButton.tips.tipTitle = "Bulldoze (right-click for settings)";
            mainActionButton.tips.tipText = ConstructTipMessageDependentOnConfig();
            mainActionButton.tips.offset = new Vector2(mainActionButton.tips.offset.x, mainActionButton.tips.offset.y + 100);
            mainActionButton.button.onClick.RemoveAllListeners();
            mainActionButton.button.onClick.AddListener(delegate { action(1); });
            // Unity buttons only react to the left mouse button; right-click opens Bulldozer's settings window.
            var rightClick = mainActionButton.button.gameObject.AddComponent<RightClickHandler>();
            rightClick.OnRightClick = () => { PluginConfigWindow.visible = !PluginConfigWindow.visible; };

            CountTransform = copiedRectTransform.transform.Find("count");
            if (CountTransform != null)
            {
                countComponent = CountTransform.GetComponentInChildren<Text>();
                if (countComponent != null)
                {
                    countComponent.text = "0";
                    return copied;
                }
            }

            return copied;
        }

        public void Show(bool inittedThisTime = false)
        {
            if (!IsUsable)
            {
                return;
            }

            // Bulldozer sits to the left of the foundation button, so the game's own button no longer needs to be
            // shifted to make room (older versions moved it left in sandbox mode).
            if (buttonOneRectTransform != null)
            {
                ResetButtonPos(buttonOneRectTransform);
            }

            RepositionElements();

            if (BulldozeButton != null)
                BulldozeButton.gameObject.SetActive(true);
            if (PaveActionButton != null)
                PaveActionButton.gameObject.SetActive(true);
            if (CheckBoxImage != null)
                CheckBoxImage.gameObject.SetActive(true);
            if (AlterVeinsCheckBoxImage != null)
            {
                AlterVeinsCheckBoxImage.gameObject.SetActive(true);
            }

            if (DestroyMachinesCheckBoxImage != null)
                DestroyMachinesCheckBoxImage.gameObject.SetActive(true);
        }

        private void RepositionElements()
        {
            if (BulldozeButton == null || buttonOneRectTransform == null)
                return;

            PositionBulldozeButton();
            if (DrawEquatorCheck != null)
            {
                RectTransform drawEquatorRect = DrawEquatorCheck.GetComponent<RectTransform>();
                if (drawEquatorRect != null)
                    drawEquatorRect.anchoredPosition = new Vector2(GetCheckBoxXValue(), drawEquatorRect.anchoredPosition.y);
            }
            if (AlterVeinsCheck != null)
            {
                RectTransform rect = AlterVeinsCheck.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = new Vector2(GetCheckBoxXValue(), -105);
            }
            if (DestroyMachinesCheck != null)
            {
                RectTransform rect = DestroyMachinesCheck.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = new Vector2(GetCheckBoxXValue(), -120);
            }
        }

        public void Hide()
        {
            if (!IsUsable)
            {
                return;
            }

            if (GameMain.sandboxToolsEnabled && buttonOneRectTransform != null)
                ResetButtonPos(buttonOneRectTransform);
            if (BulldozeButton != null)
                BulldozeButton.gameObject.SetActive(false);
            if (PaveActionButton != null)
                PaveActionButton.gameObject.SetActive(false);
            if (CheckBoxImage != null)
                CheckBoxImage.gameObject.SetActive(false);
            if (AlterVeinsCheckBoxImage != null)
                AlterVeinsCheckBoxImage.gameObject.SetActive(false);
            if (DestroyMachinesCheckBoxImage != null)
                DestroyMachinesCheckBoxImage.gameObject.SetActive(false);
        }

        public bool IsShowing()
        {
            if (!IsUsable || BulldozeButton.gameObject == false)
                return false;
            return BulldozeButton.gameObject.activeSelf;
        }

        private void OnDrawEquatorCheckClick(PointerEventData data)
        {
            OnDrawEquatorCheckClickImpl();
        }

        private void OnDrawEquatorCheckClickImpl(bool explicitDisable = false)
        {
            if (!IsUsable || CheckBoxImage == null)
            {
                return;
            }

            if (explicitDisable)
                PluginConfig.addGuideLines.Value = false;
            else
                PluginConfig.addGuideLines.Value = !PluginConfig.addGuideLines.Value;
            CheckBoxImage.sprite = PluginConfig.addGuideLines.Value ? spriteChecked : spriteUnChecked;
            _drawEquatorField = PluginConfig.addGuideLines.Value;
            if (PluginConfig.addGuideLines.Value)
            {
                if (PluginConfig.alterVeinState.Value)
                {
                    OnAlterVeinCheckClickImpl(true);
                }

                if (PluginConfig.destroyFactoryAssemblers.Value)
                {
                    OnDestroyMachinesCheckClickImpl(true);
                }
            }
        }

        private void OnAlterVeinCheckClick(PointerEventData obj)
        {
            OnAlterVeinCheckClickImpl();
        }

        private void OnAlterVeinCheckClickImpl(bool explicitDisable = false)
        {
            if (!IsUsable || AlterVeinsCheckBoxImage == null)
            {
                return;
            }

            if (explicitDisable)
                PluginConfig.alterVeinState.Value = false;
            else
                PluginConfig.alterVeinState.Value = !PluginConfig.alterVeinState.Value;
            AlterVeinsCheckBoxImage.sprite = PluginConfig.alterVeinState.Value ? spriteChecked : spriteUnChecked;
            _alterVeinsField = PluginConfig.alterVeinState.Value;
            // turn off the other check if this is clicked since it can be confusing
            if (PluginConfig.alterVeinState.Value)
            {
                if (PluginConfig.addGuideLines.Value)
                {
                    OnDrawEquatorCheckClickImpl(true);
                }

                if (PluginConfig.destroyFactoryAssemblers.Value)
                {
                    OnDestroyMachinesCheckClickImpl(true);
                }
            }
        }

        private void OnDestroyMachinesCheckClick(PointerEventData obj)
        {
            OnDestroyMachinesCheckClickImpl();
        }

        private void OnDestroyMachinesCheckClickImpl(bool explicitDisable = false)
        {
            if (!IsUsable || DestroyMachinesCheckBoxImage == null)
            {
                return;
            }

            if (explicitDisable)
                PluginConfig.destroyFactoryAssemblers.Value = false;
            else
                PluginConfig.destroyFactoryAssemblers.Value = !PluginConfig.destroyFactoryAssemblers.Value;

            DestroyMachinesCheckBoxImage.sprite = PluginConfig.destroyFactoryAssemblers.Value ? spriteChecked : spriteUnChecked;
            _destroyFactoryMachines = PluginConfig.destroyFactoryAssemblers.Value;
            if (PluginConfig.destroyFactoryAssemblers.Value)
            {
                // disable others
                if (PluginConfig.addGuideLines.Value)
                {
                    OnDrawEquatorCheckClickImpl(true);
                }

                if (PluginConfig.alterVeinState.Value)
                {
                    OnAlterVeinCheckClickImpl(true);
                }
            }
        }
    }


    public class Helper
    {
        public static Sprite GetSprite(string spriteName)
        {
            var readTex = BulldozeIcon.GetIconTexture(spriteName);

            return Sprite.Create(readTex, new Rect(0f, 0, readTex.width, readTex.height),
                new Vector2(readTex.width, readTex.height), 1000);
        }
    }
}
