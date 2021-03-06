﻿using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpoolOut {
    /** Note that this GameController /manually/ trickles down Update calls. To ensure no accidental Update calls when we shouldn't have any (i.e. when blocked by LevSel). */
	public class GameController : BaseGameController {
		// Overrideables
		override public string MyGameName() { return GameNames.SpoolOut; }
		// Components
		[SerializeField] public RandGenParams randGenParams=null;
		private Level level;
		// References
        [SerializeField] private CoreMenuController coreMenuController=null;
        //[SerializeField] private GameObject go_toggleLevSelButton=null;

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
			bool debug_supressAnimation = true; // TEMP DEBUG.
            SetCurrentLevel(levelsManager.GetLastPlayedLevelAddress(), !debug_supressAnimation);
            
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
                // What's the next level we can play?
                RolloverPackStartNextLevel();
                // Bring us to LevelSelect!
                coreMenuController.OpenLevSelController(false);
            }
        }

        public void RestartLevel() { SetCurrentLevel(currAddress, false); }
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
            }
            // No animating.
			else {
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
            level = Instantiate(resourcesHandler.spoolOut_level).GetComponent<Level>();
            level.Initialize(this, go_levelContainer.transform, ld);
            
            // Reset basic stuff
            SetIsPaused(false);
            // Tell people!
            levelsManager.selectedAddress = currAddress; // for consistency.
            //bool isTutorial = levelsManager.IsTutorial(currAddress);
            //go_toggleLevSelButton.SetActive(!isTutorial); // hide menu button in tutorial!
            // Save values!
            //SaveStorage.SetString(SaveKeys.SpoolOut_LastPlayedLevelLocal(currAddress), currAddress.ToString());
            SaveStorage.SetString(SaveKeys.SpoolOut_LastPlayedLevelGlobal, currAddress.ToString());
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
            AnalyticsController.Instance.OnWinLevel(MyGameName(), currAddress);
            if (levelsManager.IsLastLevelInPack(currAddress)) {
                OnCompleteLastLevelInPack();
            }
            // Preemptively save the NEXT level as the last one played! In case we quit now and reopen the game (we don't wanna open to the lvl we just beat).
            SaveStorage.SetString(SaveKeys.SpoolOut_LastPlayedLevelGlobal, currAddress.NextLevel.ToString());
            // Start next level business!
            StartCoroutine(Coroutine_JustWonLevel());
        }
        private void OnCompleteLastLevelInPack() {
            // We just completed the tutorial?? Save that!!
            if (levelsManager.IsTutorial(currAddress)) {
                SaveStorage.SetInt(SaveKeys.SpoolOut_DidCompleteTutorial, 1);
            }
        }



        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        override protected void Update () {
            if (!IsBlockedByLevSel) { return; }
            base.Update();
            // Update my dependencies!
            level.DependentUpdate();
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
            }
            
        }
        
        
        [SerializeField] private GameObject go_levelContainer=null;
        public void RecedeIntoBackground() {
            LeanTween.cancel(go_levelContainer);
            float duration = 0.7f;
            Vector3 posTo = new Vector3(100, 0,0);//200
            LeanTween.scale(go_levelContainer, Vector3.one*0.74f, duration).setEaseOutQuart();
            LeanTween.moveLocal(go_levelContainer, posTo, duration).setEaseOutQuart();
        }
        public void ReturnToForeground() {
            LeanTween.cancel(go_levelContainer.gameObject);
            float duration = 0.5f;
            Vector3 posTo = new Vector3(0,0,0);
            LeanTween.scale(go_levelContainer, Vector3.one, duration).setEaseOutQuart();
            LeanTween.moveLocal(go_levelContainer, posTo, duration).setEaseOutQuart();
        }


	}


}



