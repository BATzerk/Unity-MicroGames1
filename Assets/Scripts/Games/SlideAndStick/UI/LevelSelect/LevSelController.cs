﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SlideAndStick {
	public enum MenuTransType { Push, Pop }

    public class LevSelController : MonoBehaviour {
        // Statics
        private const int TEMP_CollectionIndex_Tutorial = 2;
        // References
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup cg_scrim=null;
        [SerializeField] private CoreMenuController coreMenuController=null;
        [SerializeField] private RectTransform rt_menus=null;
        [SerializeField] private TextMeshProUGUI debug_t_deleteLayouts=null;
        [SerializeField] private ToggleLevSelButton toggleLevSelButton=null;
        [SerializeField] private BaseLevSelMenu collectionsMenu=null;
        [SerializeField] private PackSelectMenu packsMenu=null;
        // Properties
        private float menusWidth;
        public float OpenLoc { get; private set; } // how open I am! From 0 to 1.
        
        // Getters (Public)
        public static Color GetCollectionColor(int collection) {
            switch (collection) {
                // Non-Player-Facing (RandLayouts, Tests, Tutorial)
                case 0: return new ColorHSB(266/360f,0.70f,0.62f).ToColor();// 82/360f,0.60f,0.84f).ToColor();
                case 1: return new ColorHSB( 30/360f,0.32f,0.63f).ToColor();
                case 2: return new ColorHSB(210/360f,0.44f,0.87f).ToColor();
                // Packs
                case 3: return new Color( 59/255f,229/255f,196/255f);
                case 4: return new Color( 59/255f,229/255f, 80/255f);
                case 5: return new Color(255/255f,190/255f, 70/255f);
                case 6: return new Color(250/255f, 85/255f,200/255f);
                case 7: return new Color(180/255f,180/255f,180/255f);
                case 8: return new Color( 90/255f, 90/255f, 90/255f);
                default: return Color.red; // Hmm.
            }
        }
        // Getters (Private)
        private LevelsManager lm { get { return LevelsManager.Instance; } }
    
    
    
        // ----------------------------------------------------------------
        //  Start
        // ----------------------------------------------------------------
		private void Start() {
            // Set values
            menusWidth = rt_menus.rect.width;
            packsMenu.Close(MenuTransType.Pop); // TODO: No animations
            collectionsMenu.Open(MenuTransType.Pop);
        }
        
        
        // ----------------------------------------------------------------
        //  Open / Close
        // ----------------------------------------------------------------
        public void Open(bool doAnimate) {
            rt_menus.gameObject.SetActive(true);
            LeanTween.cancel(gameObject);
			// Refresh my PacksMenu manually, in case we beat any levels since last opening.
			packsMenu.ManualRefreshLevelButtons();
            Debug_UpdateDeleteLayoutsText();

            if (doAnimate) {
                LeanTween.value(gameObject, SetOpenLoc, OpenLoc,1, 0.5f).setEaseOutQuart();
            }
            else {
                SetOpenLoc(1);
            }
        }
        public void Close(bool doAnimate) {
            LeanTween.cancel(gameObject);
            if (doAnimate) {
                LeanTween.value(gameObject, SetOpenLoc, OpenLoc,0, 0.5f).setEaseOutQuart().setOnComplete(OnFinishClose);
            }
            else {
                OnFinishClose();
            }
        }
        private void OnFinishClose() {
            SetOpenLoc(0);
            rt_menus.gameObject.SetActive(false);
        }
    
        private void SetOpenLoc(float _loc) {
            OpenLoc = _loc;
            
            float scaleClosed = 1f;//1.4f
            float menusX = Mathf.Lerp(-menusWidth*scaleClosed,0, OpenLoc);
            rt_menus.anchoredPosition = new Vector2(menusX, rt_menus.anchoredPosition.y);
            rt_menus.localScale = Vector3.one * Mathf.Lerp(scaleClosed, 1f, OpenLoc);
//            float togButtonX = 16 + menusX + menusWidth;
            cg_scrim.alpha = OpenLoc;
            toggleLevSelButton.SetOpenLoc(OpenLoc);
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void OpenLevel(LevelAddress address) {
            coreMenuController.OpenLevel(address);
        }
        
        
        // ----------------------------------------------------------------
        //  Menus Within Me
        // ----------------------------------------------------------------
        public void ClosePacksMenu() {
			packsMenu.Close(MenuTransType.Pop);
			collectionsMenu.Open(MenuTransType.Pop);
        }
		private void ShowPackMenu(LevelAddress address) {
			packsMenu.Open(MenuTransType.Push);
			packsMenu.SetSelectedPack(address);
			collectionsMenu.Close(MenuTransType.Push);
        }
        
        
        // ----------------------------------------------------------------
        //  Button Events
        // ----------------------------------------------------------------
        public void OnCollectionButtonClick(int collectionIndex) {
            LevelAddress address = lm.selectedAddress;
            address.collection = collectionIndex;
			address.pack = 0; // Note: Previously selected packs are not remembered. (We could add that with a little effort, though.)
            address.level = 0;
            ShowPackMenu(address);
        }
        public void OnClickScrim() {
            if (OpenLoc > 0.5f) { // If I'm open and we click the scrim, close me!
                coreMenuController.CloseLevSelController(true);
            }
        }
        
        // ----------------------------------------------------------------
        //  Debug
        // ----------------------------------------------------------------
        private void Debug_UpdateDeleteLayoutsText() {
            string savedLayoutsString = SaveStorage.GetString(SaveKeys.SlideAndStick_Debug_CustomLayouts);
            int numLayouts = CustomBoardGenUI.GetNumLayouts(savedLayoutsString);
            debug_t_deleteLayouts.text = "delete " + numLayouts + " saved layouts";
        }
        public void CopySavedLayoutsToClipboard() {
            string savedLayoutsString = SaveStorage.GetString(SaveKeys.SlideAndStick_Debug_CustomLayouts);
            GameUtils.CopyToClipboard(savedLayoutsString);
        }
        public void DeleteSavedLayouts() {
            SaveStorage.DeleteKey(SaveKeys.SlideAndStick_Debug_CustomLayouts);
            coreMenuController.ReloadScene();
        }
        public void ClearAllSaveData() {
            GameManagers.Instance.DataManager.ClearAllSaveData ();
            LevelsManager.Instance.Reset();
            coreMenuController.ReloadScene();
        }
        
    
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlideAndStick {
    public enum MenuNames {
        CollectionSelect,
        PackSelect,
        LevelSelect,
    }
    
    public class LevSelController : MonoBehaviour {
        // Statics
        static public MenuNames startingMenuName=MenuNames.CollectionSelect; // assign this to something, and when we open the Menu scene, I'll go straight to that menu!
        private const int TEMP_CollectionIndex_Tutorial = 2;
        // Components
        [SerializeField] private CollectionSelectMenu collSelectMenu;
        [SerializeField] private PackSelectMenu packSelectMenu;
        [SerializeField] private LevelSelectMenu levelSelectMenu;
        // References
        private List<BaseSelectMenu> currentMenus=new List<BaseSelectMenu>(); // a stack.
    
        // Getters (Private)
        private BaseSelectMenu currentMenu { get { return currentMenus.Count==0 ? null : currentMenus[currentMenus.Count-1]; } }
        private LevelsManager lm { get { return LevelsManager.Instance; } }
        private BaseSelectMenu GetMenu(MenuNames _name) {
            if (_name == MenuNames.CollectionSelect) { return collSelectMenu; }
            if (_name == MenuNames.PackSelect) { return packSelectMenu; }
            if (_name == MenuNames.LevelSelect) { return levelSelectMenu; }
            Debug.LogError("Whoa. No menu of this name: " + _name);
            return null; // Hmmm.
        }
        private LevelAddress GetLastPlayedAddress(int collection) {
            LevelAddress collectionAdd = new LevelAddress(0,collection,0,0);
            string key = SaveKeys.SlideAndStick_LastPlayedLevelAddress(collectionAdd);
            if (SaveStorage.HasKey(key)) { // We've got it saved! Load 'er up.
                return LevelAddress.FromString(SaveStorage.GetString(key));
            }
            else { // Oh, there was no save data. Use collectionAdd to start at the first level in the collection.
                return collectionAdd;
            }
        }
    
    
    
        // ----------------------------------------------------------------
        //  Start / Destroy
        // ----------------------------------------------------------------
        private void Awake() {
            // Connect me and my dudes
            collSelectMenu.SetLevSelControllerRef(this);
            packSelectMenu.SetLevSelControllerRef(this);
            levelSelectMenu.SetLevSelControllerRef(this);
    
            //// For development: If we're STARTING at this scene, then default selectedAddress to something.
            //if (lm.selectedAddress == LevelAddress.undefined) { lm.selectedAddress = lm.GetLastPlayedLevelAddress(); }
        }
        private void Start () {
            /* DISABLED MainMenu business! Pressing PLAY will just start the game!
            // Have we NOT completed the tutorial?? Open it up immediately!
            if (!SaveStorage.HasKey(SaveKeys.DidCompleteTutorial)) {
                OpenTutorial();
                return;
            }
            
            // Start us off at the top-level ModeSelectMenu!
            PushMenu(modeSelectMenu);
            if (startingMenuName == MenuNames.PackSelect) { // Start us off at PackSelect?
                PushMenu(packSelectMenu);
            }
            else if (startingMenuName == MenuNames.LevelSelect) { // Start us off at LevelSelect?
                if (dataManager.selectedAddress.mode!=GameModes.LetterSmashIndex) { // Lettersmash is the exception! It DOESN'T use PackSelectMenu.
                    PushMenu(packSelectMenu);
                }
                PushMenu(levelSelectMenu);
            }
            * /
    
            // Add event listeners!
            GameManagers.Instance.EventManager.ScreenSizeChangedEvent += OnScreenSizeChanged;
        }
        private void OnDestroy () {
            // Remove event listeners!
            GameManagers.Instance.EventManager.ScreenSizeChangedEvent -= OnScreenSizeChanged;
        }
    
    
    
    
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        /** NOTE: This current push/pop system will NOT work if we want to open a duplicate menu. We only have the menus we start with. * /
        public void PopMenu() {
            if (currentMenus.Count <= 1) { return; } // Can't close out of the topmost menu, yo.
            currentMenu.Hide();
            currentMenus.RemoveAt(currentMenus.Count-1);
            currentMenu.Show();
        }
        public void PushMenu(MenuNames menuName) { PushMenu(GetMenu(menuName)); }
        private void PushMenu(BaseSelectMenu menu) {
            if (menu==null) { return; }
            if (currentMenu!=null) { currentMenu.Hide(); }
            currentMenus.Add(menu);
            currentMenu.Show();
        }
    
    
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        private void OnScreenSizeChanged () {
            currentMenu.PositionTiles ();
        }
    
    
    
        // ----------------------------------------------------------------
        //  Scene Management
        // ----------------------------------------------------------------
        private void ReloadScene () { OpenScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
        private void OpenScene (string sceneName) {
            UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
        }
    
        public void LoadLevel (LevelAddress address) {
            LevelsManager.Instance.selectedAddress = address; // Setting this is optional. Just keepin' it consistent.
            SaveStorage.SetString (SaveKeys.SlideAndStick_LastPlayedLevelAddress(address), address.ToString()); // Actually save the value! That's what GameController pulls in.
            OpenScene (SceneNames.Gameplay(GameNames.SlideAndStick));
        }
        private void OpenTutorial() {
            LoadLevel(new LevelAddress(GameModes.StandardIndex, TEMP_CollectionIndex_Tutorial,0,0));
        }
    
        public void ClearAllSaveData() {
            GameManagers.Instance.DataManager.ClearAllSaveData ();
            ReloadScene ();
        }
    
    
        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update () {
            bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    
            // DEBUG
            if (Input.GetKeyDown(KeyCode.Return)) {
                lm.Reset();
                ReloadScene ();
                return;
            }
            if (isKey_control && isKey_shift && Input.GetKeyDown(KeyCode.Delete)) {
                ClearAllSaveData();
                return;
            }
        }
    }
}

*/