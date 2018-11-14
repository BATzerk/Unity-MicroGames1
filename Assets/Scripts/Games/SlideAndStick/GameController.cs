using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SlideAndStick {
    /** Note that this GameController /manually/ trickles down Update calls. To ensure no accidental Update calls when we shouldn't have any (i.e. when blocked by LevSel). */
	public class GameController : BaseGameController {
		// Overrideables
		override public string MyGameName() { return GameNames.SlideAndStick; }
		// Components
		[SerializeField] public RandGenParams randGenParams=null;
		private Level level;
        // Properties
        private List<BoardData> debug_prevBoardDatas=new List<BoardData>(); // for making rand lvls. Press E to restore the last level, in case we pressed R accidentally and lost it.
		// References
        [SerializeField] private CoreMenuController coreMenuController;
		[SerializeField] private FUEController fueController=null;

		// Getters (Public)
        public FUEController FUEController { get { return fueController; } }
        // Getters (Private)
        private bool IsBlockedByLevSel { get { return coreMenuController.IsGameControllerBlockedByLevSel(); } }
        private LevelAddress currAddress { get { return level==null?LevelAddress.zero : level.MyAddress; } }
        private PackData CurrentPackData { get { return levelsManager.GetPackData(currAddress); } }
        private LevelsManager levelsManager { get { return LevelsManager.Instance; } }


        

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
            
            // Start at the level we've most recently played!
            SetCurrentLevel(levelsManager.GetLastPlayedLevelAddress());
            
            // Add event listeners!
            GameManagers.Instance.EventManager.LevelJumpButtonClickEvent += OnLevelJumpButtonClick;
            GameManagers.Instance.EventManager.QuitGameplayButtonClickEvent += OnQuitGameplayButtonClick;
        }
        override protected void OnDestroy() {
            base.OnDestroy();
            
            // Remove event listeners!
            GameManagers.Instance.EventManager.LevelJumpButtonClickEvent -= OnLevelJumpButtonClick;
            GameManagers.Instance.EventManager.QuitGameplayButtonClickEvent -= OnQuitGameplayButtonClick;
        }
        
        private void OnQuitGameplayButtonClick() {
            OpenScene(SceneNames.LevelSelect(MyGameName()));
        }
        private void OnLevelJumpButtonClick(int levelIndexChange) {
            ChangeLevel(levelIndexChange);
        }
        
        
        //public void Open() {
        //    // Start at the level we've most recently played!
        //    SetCurrentLevel(levelsManager.GetLastPlayedLevelAddress());
        //}
        

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
            
            //yield return new WaitForSecondsRealtime(0.1f);
            StartNextLevel();
        }

        private void RestartLevel() { SetCurrentLevel(currAddress, false); }
        private void StartPrevLevel() {
            LevelData data = levelsManager.GetLevelData(currAddress.PreviousLevel);
            if (data != null) { SetCurrentLevel(data); }
        }
        private void StartNextLevel() {
            if (levelsManager.IsLastLevelInPack(currAddress)) {
                OnCompleteLastLevelInPack();
            }
            else {
                SetCurrentLevel(currAddress.NextLevel, true);
            }
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
            }
            // No animating.
			else {
                level.AnimateInBoard();
                if (oldLevel != null) {
                    oldLevel.DestroySelf();
                }
            }
		}
        
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
            level.Initialize(this, canvas.transform, ld);
            debug_prevBoardDatas.Add(level.Board.SerializeAsData());
            
            // Reset basic stuff
            SetIsPaused(false);
            // Tell people!
            fueController.OnStartLevel(level);
            levelsManager.selectedAddress = currAddress; // for consistency.
            // Save values!
            SaveStorage.SetString(SaveKeys.SlideAndStick_LastPlayedLevelAddress(currAddress), currAddress.ToString());
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
            FBAnalyticsController.Instance.OnWinLevel(MyGameName(), currAddress);
            StartCoroutine(Coroutine_JustWonLevel());
        }
        private void OnCompleteLastLevelInPack() {
            //// We just completed the tutorial?? Save that!!
            //if (currAddress.mode == GameModes.TutorialIndex) {
            //    SaveStorage.SetInt(SaveKeys.SlideAndStick_DidCompleteTutorial, 1);
            //}

			// What's the next level we can play?
			//TODO: This

            // Bring us to LevelSelect!
			coreMenuController.OpenLevSelController(false);
        }



        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        override protected void Update () {
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
                if (Input.GetKeyDown(KeyCode.S)) { levelsManager.Debug_SaveReplaceXMLLayout(level); }
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
                
                // E = Restore prev Board! In case we had one we liked but accidentally made a new one.
                if (Input.GetKeyDown(KeyCode.E)) {
                    Debug_RestorePrevBoard();
                    return;
                }
            }
            
        }
        
        
        // Debug
        private void Debug_RestorePrevBoard() {
            if (debug_prevBoardDatas.Count > 1) {
                BoardData snapshot = debug_prevBoardDatas[debug_prevBoardDatas.Count-2];
                debug_prevBoardDatas.RemoveAt(debug_prevBoardDatas.Count-1);
                level.Debug_RemakeBoardAndViewFromArbitrarySnapshot(snapshot);
            }
        }


	}


}



