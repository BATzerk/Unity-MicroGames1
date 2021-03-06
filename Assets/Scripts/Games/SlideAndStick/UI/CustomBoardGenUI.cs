﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlideAndStick {
    public class CustomBoardGenUI : MonoBehaviour {
        // Components
        [SerializeField] private CanvasGroup cg_savedPopup=null;
        [SerializeField] private GameObject go_params=null;
        [SerializeField] private GameObject go_paintPallete=null;
        [SerializeField] private GameObject go_saveButtons=null;
        [SerializeField] private Slider sl_numColors=null;
        [SerializeField] private Slider sl_stickinessMin=null;
        [SerializeField] private Slider sl_stickinessMax=null;
        [SerializeField] private Slider sl_percentTiles=null;
        [SerializeField] private TextMeshProUGUI t_numColors=null;
        [SerializeField] private TextMeshProUGUI t_stickinessMin=null;
        [SerializeField] private TextMeshProUGUI t_stickinessMax=null;
        [SerializeField] private TextMeshProUGUI t_percentTiles=null;
        [SerializeField] private TextMeshProUGUI t_savedPopup=null;
        // Properties
        private string customLayoutXMLs; // all the XML nodes in one big-ass string.
        // References
        [SerializeField] private Level level=null;
        
        // Getters (Public)
        static public int GetNumLayouts(string savedLayoutsString) {
            return TextUtils.CountOccurances(savedLayoutsString, "layout=");
        }
        // Getters (Private)
        private GameController gameController { get { return level.GameController; } }
        private RandGenParams rgp { get { return gameController.randGenParams; } }
        //private bool AreLayoutsIdentical(BoardData bdA, BoardData bdB) {
        //    return bdA.Debug_GetLayout(true) == bdB.Debug_GetLayout(true);
        //}


        // ----------------------------------------------------------------
        //  Start
        // ----------------------------------------------------------------
        private void Start() {
            // Load layouts!
            LoadCustomLayouts();
            // Load params!
            UpdateSliderValuesFromParams();
            UpdateParamsTextsFromValues();
            // Hide t_savedPopup.
            SetSavedPopupAlpha(0);
        }
        

        // ----------------------------------------------------------------
        //  UI Doers
        // ----------------------------------------------------------------
        public void ToggleParamsVisible() {
            go_params.SetActive(!go_params.activeSelf);
        }
        public void TogglePaintPaletteVisible() {
            go_paintPallete.SetActive(!go_paintPallete.activeSelf);
        }
        public void ToggleSaveButtonsVisible() {
            go_saveButtons.SetActive(!go_saveButtons.activeSelf);
        }
        private void HideSaveButtons() {
            go_saveButtons.SetActive(false);
        }
        private void UpdateParamsTextsFromValues() {
            t_numColors.text = rgp.NumColors.ToString();
            t_stickinessMin.text = rgp.StickinessMin.ToString();
            t_stickinessMax.text = rgp.StickinessMax.ToString();
            t_percentTiles.text = (int)(100*rgp.PercentTiles) + "%";
        }
        private void UpdateSliderValuesFromParams() {
            sl_numColors.value = rgp.NumColors;
            sl_stickinessMin.value = rgp.StickinessMin;
            sl_stickinessMax.value = rgp.StickinessMax;
            sl_percentTiles.value = rgp.PercentTiles;
        }
        private void SetSavedPopupAlpha(float alpha) {
            cg_savedPopup.alpha = alpha;
        }
        
        // ----------------------------------------------------------------
        //  UI Events
        // ----------------------------------------------------------------
        public void OnSlVal_NumColors() {
            rgp.NumColors = (int)sl_numColors.value;
            UpdateParamsTextsFromValues();
        }
        public void OnSlVal_PercentTiles() {
            rgp.PercentTiles = sl_percentTiles.value;
            UpdateParamsTextsFromValues();
        }
        public void OnSlVal_StickinessMin() {
            SetStickinessMinAndMax((int)sl_stickinessMin.value, rgp.StickinessMax);
        }
        public void OnSlVal_StickinessMax() {
            SetStickinessMinAndMax(rgp.StickinessMin, (int)sl_stickinessMax.value);
        }
        private void SetStickinessMinAndMax(int _min, int _max) {
            rgp.StickinessMin = Mathf.Min(_min, _max);
            rgp.StickinessMax = Mathf.Max(_min, _max);
            UpdateSliderValuesFromParams(); // in case one of the above has changed, update the slider poses.
            UpdateParamsTextsFromValues();
        }
        public void OnClickPrevLevel() {
            GameManagers.Instance.EventManager.OnLevelJumpButtonClick(-1);
        }
        public void OnClickNextLevel() {
            GameManagers.Instance.EventManager.OnLevelJumpButtonClick(1);
        }
        

        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void LoadCustomLayouts() {
            customLayoutXMLs = SaveStorage.GetString(SaveKeys.SlideAndStick_Debug_CustomLayouts);
        }
        private void SaveCustomLayouts() {
            SaveStorage.SetString(SaveKeys.SlideAndStick_Debug_CustomLayouts, customLayoutXMLs);
        }
        
        public void MakeNewLayout() {
            level.GameController.RestartLevel();
        }
        
        private void CopyLayoutsAsXMLToClipboard() {
            GameUtils.CopyToClipboard(customLayoutXMLs);
        }
        private void ShowPopupText() {
            int numLayouts = GetNumLayouts(customLayoutXMLs);
            t_savedPopup.text = numLayouts + " layouts copied to clipboard";
            SetSavedPopupAlpha(1);
            LeanTween.value(gameObject, SetSavedPopupAlpha, 1,0, 1.4f).setDelay(1.5f);
        }
        
        public void AddLayout(int difficulty) {
            // First off, REMOVE any layouts identical to this new one. We're replacing it with this updated difficulty.
            MaybeRemoveLastIdenticalLayoutFromXMLString();
        
            // Set the Board's difficulty and add to big XML string.
            level.Board.Debug_SetDifficulty(difficulty);
            customLayoutXMLs += level.Board.Debug_GetAsXML(true);
            
            // Saaaaave!
            SaveCustomLayouts();
            CopyLayoutsAsXMLToClipboard();
            ShowPopupText();
            HideSaveButtons();
        }
        private void MaybeRemoveLastIdenticalLayoutFromXMLString() {
            // First off, if this exact layout is the SAME as the last saved fella, REMOVE the last saved fella! 
            string boardLayoutString = level.Board.Debug_GetLayout(true);
            if (customLayoutXMLs.Contains(boardLayoutString)) {
                int startIndex = customLayoutXMLs.LastIndexOf("    <Level", System.StringComparison.InvariantCulture);
                customLayoutXMLs = customLayoutXMLs.Substring(0, startIndex);
            }
        }
        
        
        
    }
}