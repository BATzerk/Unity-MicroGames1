﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlideAndStick {
    //[RequireComponent(typeof(Button))]
    public class PackButton : MonoBehaviour {
        // Components
        [SerializeField] private Button myButton=null;
        [SerializeField] private Image i_backing=null;
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private TextMeshProUGUI t_packName=null;
        // References
        [SerializeField] private Sprite s_selected=null;
        [SerializeField] private Sprite s_unselected=null;
        private PackData myPackData;
        private PackSelectMenu packSelectMenu;
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(PackSelectMenu _psm, Transform tf_parent) {
            this.packSelectMenu = _psm;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, tf_parent);
        }
        
        
        // ----------------------------------------------------------------
        //  Spawn / Despawn
        // ----------------------------------------------------------------
        public void Despawn() {
            this.gameObject.SetActive(false);
        }
        public void Spawn(PackData _packData, bool isSelected) {
            this.gameObject.SetActive(true);
            this.myPackData = _packData;
            
            this.name = "Pack_" + myPackData.MyAddress.pack;
            t_packName.text = myPackData.PackName;
            myButton.interactable = !isSelected; // I'm not clickable if I'm selected.
            i_backing.color = packSelectMenu.CurrentPackColor;
            
            i_backing.sprite = isSelected ? s_selected : s_unselected;
            
            if (isSelected) {
                t_packName.color = Color.white;
            }
            else {
                //i_top.color = packSelectMenu.CurrentPackColor;
                //i_bottom.color = Color.Lerp(packSelectMenu.CurrentPackColor, Color.black, 0.3f); // darker bottom image for depth.
                t_packName.color = packSelectMenu.CurrentPackColor;
            }
        }
        public void SetPosSize(Vector2 _pos, Vector2 _size) {
            myRectTransform.anchoredPosition = _pos;
            myRectTransform.sizeDelta = _size;
        }
        
        
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        public void OnClick() {
            packSelectMenu.OnClickPackButton(myPackData.MyAddress);
        }
    }
}