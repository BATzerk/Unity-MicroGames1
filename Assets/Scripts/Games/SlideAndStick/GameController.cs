using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SlideAndStick {
    /** Note that this GameController /manually/ trickles down Update calls. To ensure no accidental Update calls when we shouldn't have any (i.e. when blocked by LevSel). */
	public class GameController : BaseGameController {
		// Overrideables
		override public string MyGameName() { return GameNames.SlideAndStick; }
        // Constants
        private const int NumStartingUndos = 30;//TODO: Balance this.
        public const int NumUndosFromRewardVideo = 30; // TODO: Balance this.
		// Components
		[SerializeField] public RandGenParams randGenParams=null;
		private Level level;
        // Properties
        private List<BoardData> debug_prevBoardDatas=new List<BoardData>(); // for making rand lvls. Press E to restore the last level, in case we pressed R accidentally and lost it.
        public int NumUndosLeft { get; private set; }
        // References
        [SerializeField] private GameObject go_levelContainer=null;
        [SerializeField] private GameBackground background=null;
        [SerializeField] private CoreMenuController coreMenuController=null;
        [SerializeField] private FUEController fueController=null;
        [SerializeField] private GameObject go_toggleLevSelButton=null;
        [SerializeField] private GetUndosPopup getUndosPopup=null;
        [SerializeField] private SlideAndStickSfxController sfxController=null;

		// Getters (Public)
        public FUEController FUEController { get { return fueController; } }
        public SlideAndStickSfxController SFXController { get { return sfxController; } }
        public bool CanAffordUndo() { return NumUndosLeft > 0; }
        public bool CanTouchBoard() {
            if (!FUEController.CanTouchBoard) { return false; } // FUE's locked us out? No movin'.
            if (getUndosPopup.IsOpen) { return false; } // Popup in the way? No movin'.
            return true; // Sure, sounds ok.
        }
        // Getters (Private)
        private bool IsBlockedByLevSel { get { return coreMenuController.IsGameControllerBlockedByLevSel(); } }
        private LevelAddress currAddress { get { return level==null?LevelAddress.zero : level.MyAddress; } }
        private PackData CurrentPackData { get { return levelsManager.GetPackData(currAddress); } }
        private LevelsManager levelsManager { get { return LevelsManager.Instance; } }


        // ----------------------------------------------------------------
        //  Ads
        // ----------------------------------------------------------------
        // Constants
        private const int NumPlaysBetweenAds = 12;//3
        private const int MinSecondsBetweenAds = 5 * 60;//45 // we WON'T show ads more frequently than this.
        // Properties
        private static int PlaysUntilAd = NumPlaysBetweenAds;
        private static float timeWhenMayShowAd = 0; // in Time.unscaledTime. When we show ad, this is Time.unscaledTime+MinSecondsBetweenAds
        // Doers
        private void MaybeShowAd() {
            if (IsRateGamePopupLevel()) { return; } // Don't show an ad if we're asking 'em to rate the game.
            // Decrement PlaysUntilAd.
            PlaysUntilAd --;
            // Ok to show one??
            //print(Time.frameCount + " MaybeShowAd. " + PlaysUntilAd + "     timeWhenMayShowAd, Time.unscaledTime: " + timeWhenMayShowAd + ", " + Time.unscaledTime);
            if (PlaysUntilAd <= 0 && Time.unscaledTime>timeWhenMayShowAd) {
                ShowAd();
            }
        }
        private void ShowAd() {
            // Show an ad!
            AdManager.instance.showInterstitial();
            //print(Time.frameCount + " ShowAd!");
            // Reset PlaysUntilAd.
            PlaysUntilAd = NumPlaysBetweenAds;
            timeWhenMayShowAd = Time.unscaledTime + MinSecondsBetweenAds;
        }
        


        // ----------------------------------------------------------------
        //  Start
        // ----------------------------------------------------------------
        override protected void Start() {
            base.Start();
            
            // In the editor? Reload levels!
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
    //      AssetDatabase.ImportAsset(LevelLoader.LevelsFilePath(MyGameName(), true));
            #endif
            
            // Load saved values.
            NumUndosLeft = SaveStorage.GetInt(SaveKeys.SlideAndStick_NumUndosLeft, NumStartingUndos);
            
            // Start at the level we've most recently played!
            bool debug_supressAnimation = Input.GetKey(KeyCode.A); // DEBUG for TESTING!
            LevelData ld = levelsManager.GetLastPlayedLevelData();
            SetCurrentLevel(ld, !debug_supressAnimation);
            
            // Add event listeners!
            GameManagers.Instance.EventManager.LevelJumpButtonClickEvent += OnLevelJumpButtonClick;
            GameManagers.Instance.EventManager.QuitGameplayButtonClickEvent += OnQuitGameplayButtonClick;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        }
        override protected void OnDestroy() {
            base.OnDestroy();
            
            // Remove event listeners!
            GameManagers.Instance.EventManager.LevelJumpButtonClickEvent -= OnLevelJumpButtonClick;
            GameManagers.Instance.EventManager.QuitGameplayButtonClickEvent -= OnQuitGameplayButtonClick;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
        }
        
        private void OnQuitGameplayButtonClick() {
            OpenScene(SceneNames.LevelSelect(MyGameName()));
        }
        private void OnLevelJumpButtonClick(int levelIndexChange) {
            ChangeLevel(levelIndexChange);
        }
        
        
        

        // ----------------------------------------------------------------
        //  Undo Management
        // ----------------------------------------------------------------
        public void DecrementNumUndosLeft() {
            SetNumUndosLeft(NumUndosLeft - 1);
        }
        private void SetNumUndosLeft(int val) {
            NumUndosLeft = val;
            SaveStorage.SetInt(SaveKeys.SlideAndStick_NumUndosLeft, NumUndosLeft);
            level.UndoMoveInputController.OnNumUndosLeftChanged();
        }
        public void OpenGetUndosPopup() {
            getUndosPopup.Open();
        }
        
        public void ShowRewardVideoForUndos() {
            // In editor? PRETEND we showed the video. Otherwise, show it!
            if (Application.isEditor) { RewardedVideoAdRewardedEvent(new IronSourcePlacement("test","test",1)); }
            else { AdManager.instance.showRewardVideo(); }
        }
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement placement) {
            Debug.Log("Reward video complete! Name: " + placement.getRewardName() + ", amount: " + placement.getRewardAmount());
            // Give player undos!
            SetNumUndosLeft(NumUndosLeft + NumUndosFromRewardVideo);
            // Close the popup automatically.
            getUndosPopup.Close();
        }
        
        
        
        // ----------------------------------------------------------------
        //  Rate-Game Popup
        // ----------------------------------------------------------------
        private void MaybeShowRateGamePopup() {
            if (IsRateGamePopupLevel()) {
                //TODO: Kurt.
            }
        }
        private bool IsRateGamePopupLevel() {
            //TODO: Kurt.
            return false;
            // Note: currAddress.mode should be 0, but I forget what the collection value for difficulties is. Make sure to add a breakpoint and make sure the collection value is right!
            // e.g. if (currAddress.mode==0 && currAddress.collection==4 && currAddress.pack==0 && currAddress.level==10) { return true; }
        }
        
        

		// ----------------------------------------------------------------
		//  Doers - Loading Level
		// ----------------------------------------------------------------
        private IEnumerator Coroutine_JustWonLevel() {
            yield return new WaitForSecondsRealtime(0.32f);

            // Wait until there's no touch on the screen.
            while (inputController.IsTouchHold()) { yield return null; }
            
            // Show LevelCompletePopup, and wait until we press its Next button!
            LevelCompletePopup popup = level.LevelUI.LevelCompletePopup;
            popup.Appear();
            while (!popup.DidPressNextButton) { yield return null; }
            
            // NOT last level in pack? Start next level!
            if (!levelsManager.IsLastLevelInPack(currAddress)) {
                StartNextLevel();
            }
            else {
				bool wasTutorial = levelsManager.IsTutorial(currAddress);
                // What's the next level we can play? Start it!
                RolloverPackStartNextLevel();
				// WAS the tutorial?? Tell FUEController!
				if (wasTutorial) {
					fueController.ForcePlayerToOpenLevSel();
				}
				// Was NOT tutorial. Open LevSel!
				else {
                	coreMenuController.OpenLevSelController(false);
				}
            }
        }

        public void RestartLevel() { SetCurrentLevel(currAddress, false); }
        //private void StartPrevLevel() {
        //    LevelData data = levelsManager.GetLevelData(currAddress.PreviousLevel);
        //    if (data != null) { SetCurrentLevel(data); }
        //}
        private void StartNextLevel() {
            LevelData data = levelsManager.GetLevelData(currAddress.NextLevel);
            if (data != null) { SetCurrentLevel(data, true); }
        }
        private void RolloverPackStartNextLevel() {
            LevelData data = levelsManager.GetRolloverPackNextLevelData(currAddress);
            if (data != null) { SetCurrentLevel(data, false); } // Note: Don't animate; LevSel's covering us now.
        }
        private void ChangeCollection(int change) {
            SetCurrentLevel(new LevelAddress(currAddress.mode, currAddress.collection+change, 0, 0));
        }
        private void ChangePack(int change) {
            SetCurrentLevel(new LevelAddress(currAddress.mode, currAddress.collection, currAddress.pack+change, 0));
        }
        private void ChangeLevel(int change) {
            SetCurrentLevel(currAddress + new LevelAddress(0, 0, 0, change));
        }
        public void SetCurrentLevel(LevelAddress address, bool doAnimate=false) {
            if (Application.isEditor) { // In editor? Noice. Reload all levels from file so we can update during runtime!
                levelsManager.Reset();
            }
            address = address.NoNegatives();
            LevelData ld = levelsManager.GetLevelData(address);
            if (ld == null) { Debug.LogError("Requested LevelData doesn't exist! Address: " + address.ToString()); } // Useful feedback for dev.
            SetCurrentLevel(ld, doAnimate);
        }
    
        private void SetCurrentLevel(LevelData levelData, bool doAnimate=false) {
			Level oldLevel = level;

			InitializeLevel(levelData);
            
            // Animate in/out!
			if (doAnimate) {
                level.AnimateIn();
                if (oldLevel != null) {
                    oldLevel.AnimateOut();
                }
                background.SpeedUpParticlesforLevelTrans();
            }
            // No animating.
			else {
                level.AnimateInBoard();
                if (oldLevel != null) {
                    oldLevel.DestroySelf();
                }
            }
		}
        
        //private IEnumerator Coroutine_AnimateLevelsInOut(Level l, Level oldLevel) {
        //    // Animate the dudes in/out.
        //    // Speed up/slow down the particles.
        //        StartCoroutine(Coroutine_AnimateLevelsInOut(level, oldLevel));
        //}
        
        private void InitializeLevel(LevelData ld) {
            if (ld == null) {
                Debug.LogError ("Can't load the requested level! Can't find its LevelData.");
                if (level == null) { // If there's no currentLevel, yikes! Default us to something.
                    ld = levelsManager.GetFallbackEmptyLevelData();
                }
                else { return; } // If there IS a currentLevel, let's just stay there!
            }
    
            // Instantiate the Level from the provided LevelData!
            level = Instantiate(resourcesHandler.slideAndStick_level).GetComponent<Level>();
            level.Initialize(this, go_levelContainer.transform, ld);
            debug_prevBoardDatas.Add(level.Board.SerializeAsData());
            
            // Reset basic stuff
            SetIsPaused(false);
            // Tell people!
            fueController.OnStartLevel(level);
            levelsManager.selectedAddress = currAddress; // for consistency.
            bool isTutorial = levelsManager.IsTutorial(currAddress);
            go_toggleLevSelButton.SetActive(!isTutorial); // hide menu button in tutorial!
            getUndosPopup.Close();
            // Save values!
            //SaveStorage.SetString(SaveKeys.SlideAndStick_LastPlayedLevelLocal(currAddress), currAddress.ToString());
            SaveStorage.SetString(SaveKeys.SlideAndStick_LastPlayedLevelGlobal, currAddress.ToString());
            
            // Maybe show ad/popup!
            MaybeShowAd();
            MaybeShowRateGamePopup();
        }
        
        

		// ----------------------------------------------------------------
		//  Game Events
		// ----------------------------------------------------------------
        public void OnBoardGoalsSatisfied() {
            WinLevel();
        }
        private void WinLevel() {
            // Tell people!
            levelsManager.OnCompleteLevel(currAddress);
            level.OnWinLevel();
            fueController.OnCompleteLevel();
            sfxController.OnCompleteLevel();
            // Analytics!
            Dictionary<string, object> custAnalyParams = new Dictionary<string, object> {
                { "AB_IsEasies", ABTestsManager.Instance.IsEasies }
            };
            AnalyticsController.Instance.OnWinLevel(MyGameName(), currAddress, custAnalyParams);
            
            // #forchristian Here's where WinLevel is called! Put what you need here.
            // Note: currAddress has mode (not used), collection (difficulty), pack (board size), and level.
            //ChristianFunAnalyticsHandler.DispatchOneGroovyEvent(currAddress.collection, currAddress.pack, currAddress.level);
            
            if (levelsManager.IsLastLevelInPack(currAddress)) {
                OnCompleteLastLevelInPack();
            }
            // Preemptively save the NEXT level as the last one played! In case we quit now and reopen the game (we don't wanna open to the lvl we just beat).
            SaveStorage.SetString(SaveKeys.SlideAndStick_LastPlayedLevelGlobal, currAddress.NextLevel.ToString());
            // Start next level business!
            StartCoroutine(Coroutine_JustWonLevel());
        }
        private void OnCompleteLastLevelInPack() {
            // We just completed the tutorial?? Save that!!
            if (levelsManager.IsTutorial(currAddress)) {
                SaveStorage.SetBool(SaveKeys.SlideAndStick_DidCompleteTutorial, true);
            }
        }



        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        override protected void Update () {
            // DEBUG! S = Save screenshot.
            if (Input.GetKeyDown(KeyCode.S)) { ScreenCapture.CaptureScreenshot("screenshot.png"); }
            
            if (!IsBlockedByLevSel) { return; }
            base.Update();
            // Update my dependencies!
            level.DependentUpdate();
            fueController.DependentUpdate();
        }
        private void FixedUpdate() {
            if (!IsBlockedByLevSel) { return; }
            // Update my dependencies!
            level.DependentFixedUpdate();
        }

        // ----------------------------------------------------------------
        //  Input
        // ----------------------------------------------------------------
        override protected void RegisterButtonInput() {
            base.RegisterButtonInput();

            // R = Reload Level (without reloading scene)
            if (Input.GetKeyDown(KeyCode.R)) {
                RestartLevel();
                return;
            }
    
            // DEBUG
            bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool isKey_ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (isKey_shift) {
                //if (Input.GetKeyDown(KeyCode.S)) { levelsManager.Debug_SaveReplaceXMLLayout(level); }
            }
            else if (isKey_ctrl) {
                if (Input.GetKeyDown(KeyCode.LeftBracket))  { ChangeCollection(-1); return; }
                if (Input.GetKeyDown(KeyCode.RightBracket)) { ChangeCollection( 1); return; }
            }
            else if (isKey_alt) {
                if (Input.GetKeyDown(KeyCode.LeftBracket))  { ChangePack(-1); return; }
                if (Input.GetKeyDown(KeyCode.RightBracket)) { ChangePack( 1); return; }
            }
            else {
                if (Input.GetKeyDown(KeyCode.P))            { ChangeLevel(-10); return; } // P = Back 10 levels.
                if (Input.GetKeyDown(KeyCode.LeftBracket))  { ChangeLevel( -1); return; } // [ = Back 1 level.
                if (Input.GetKeyDown(KeyCode.RightBracket)) { ChangeLevel(  1); return; } // ] = Ahead 1 level.
                if (Input.GetKeyDown(KeyCode.Backslash))    { ChangeLevel( 10); return; } // \ = Ahead 10 levels.
                
                // A = Maybe show an ad!
                if (Input.GetKeyDown(KeyCode.A)) {
                    MaybeShowAd();
                }
                // W = Win!
                if (Input.GetKeyDown(KeyCode.W)) {
                    WinLevel();
                }
                // E = Restore prev Board! In case we had one we liked but accidentally made a new one.
                if (Input.GetKeyDown(KeyCode.E)) {
                    Debug_RestorePrevBoard();
                    return;
                }
            }
            
        }
        
        
        public void RecedeIntoBackground() {
            LeanTween.cancel(go_levelContainer);
            float duration = 0.7f;
            Vector3 posTo = new Vector3(100, 0,0);//200
            LeanTween.scale(go_levelContainer, Vector3.one*0.74f, duration).setEaseOutQuart();
            LeanTween.moveLocal(go_levelContainer, posTo, duration).setEaseOutQuart();
			// Tell people.
			fueController.OnGameControllerRecedeIntoBackground();
        }
        public void ReturnToForeground() {
            LeanTween.cancel(go_levelContainer.gameObject);
            float duration = 0.5f;
            Vector3 posTo = new Vector3(0,0,0);
            LeanTween.scale(go_levelContainer, Vector3.one, duration).setEaseOutQuart();
            LeanTween.moveLocal(go_levelContainer, posTo, duration).setEaseOutQuart();
        }
        
        
        
        

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        if (UnityEditor.EditorApplication.isPlaying)  {
            CoreMenuController.Debug_HideMenuOnStart = true; // Tell CoreMenuController NOT to show the Combi menu when we reload the scene.
            ReloadScene();
        }
    }
#endif
        private void Debug_RestorePrevBoard() {
            if (debug_prevBoardDatas.Count > 1) {
                BoardData snapshot = debug_prevBoardDatas[debug_prevBoardDatas.Count-2];
                debug_prevBoardDatas.RemoveAt(debug_prevBoardDatas.Count-1);
                level.Debug_RemakeBoardAndViewFromArbitrarySnapshot(snapshot);
            }
        }


	}


}



