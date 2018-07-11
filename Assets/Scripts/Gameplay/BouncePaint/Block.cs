﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BouncePaint {
    public class Block : MonoBehaviour {
        // Components
        [SerializeField] private Image i_body=null;
		[SerializeField] private Image i_hitBox=null;
		[SerializeField] private RectTransform myRectTransform=null;
		private TextMeshProUGUI t_numHitsReq=null; // This is added from a prefab if needed!
        // Properties
        private bool isPainted=false;
        private bool isPaintable=true; // set to FALSE for Blocks that can be bounced on but NOT painted (and they're always satisfied, of course).
        private Player ballTargetingMe=null;
		private Rect hitBox;
        public Vector2 posDipOffset; //HACK temp! private. when we get bounced on, this is set to like (0,-16). Eases back to (0,0). Added to our center pos.
        private Vector2 posDanceOffset=Vector2.zero; // for post-level celebration.
        private Vector2 size;
		// Properties (Specials)
		private bool doTap=true; // only FALSE for the black Blocks we DON'T wanna tap on!
		private bool doTravel;
		private float travelSpeed=1f; // for TRAVELING Blocks.
		private float travelOscVal; // for TRAVELING Blocks.
		private int numHitsReq;
		private Vector2 centerA,centerB; // for TRAVELING Blocks.
        // References
		[SerializeField] private Sprite s_bodyDontTap;
        private GameController gameController;
        private Level myLevel;

		// Getters (Public)
        public bool DoTap { get { return doTap; } }
        public bool IsPainted { get { return isPainted; } }
        public bool IsSatisfied { get { return isPainted || !isPaintable; } }
        public bool IsAvailable { get { return !isPainted && ballTargetingMe==null; } } // I'm available if A) I'm unpainted, and B) Nobody's planning to hit me!
        public float BlockTop { get { return center.y + size.y*0.5f; } } // The VISUAL top of the block.
        public int NumHitsReq { get { return numHitsReq; } }
        public Rect HitBox { get { return hitBox; } }
        public Player BallTargetingMe {
            get { return ballTargetingMe; }
            set { ballTargetingMe = value; }
        }
		public Vector2 GetPredictedPos(float timeFromNow) {
			if (!doTravel) { return HitBox.center; } // Oh, if I'm NOT a traveler, just return my current position.
			float predTravelOscVal = travelOscVal + travelSpeed*timeFromNow; // we use FixedUpdate, so this is actually reliable.
			float predTravelLoc = MathUtils.Sin01(predTravelOscVal);
			return Vector2.Lerp(centerA,centerB, predTravelLoc);
		}

        // Getters (Private)
        private Color bodyColor {
            get { return i_body.color; }
            set { i_body.color = value; }
		}
		private Vector2 center {
			get { return myRectTransform.anchoredPosition; }
			set {
				myRectTransform.anchoredPosition = value;
				UpdateHitBoxCenter();
			}
		}
        private void UpdateHitBoxCenter() {
			hitBox.center = new Vector2(
                center.x,
                center.y + (size.y+hitBox.height)*0.5f - 15);
		}


        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
		public void Initialize(GameController _gameController, Level _myLevel,
			Vector2 _size,
			Vector2 _centerA,Vector2 _centerB
		) {
            gameController = _gameController;
            myLevel = _myLevel;
            this.transform.SetParent(myLevel.transform);
            this.transform.localScale = Vector3.one;
            this.transform.localEulerAngles = Vector3.zero;
			myRectTransform.anchoredPosition = Vector2.zero;
			// Assign properties
			centerA = _centerA;
			centerB = _centerB;
			posDipOffset = Vector2.zero;
			ApplyPos();

            size = _size;
            // Make hitBox!
            hitBox = new Rect();
            hitBox.size = new Vector2(size.x, 38);
			UpdateHitBoxCenter();
            // Fudgily bloat the hitBox's width a bit (in case the block or ball are moving fast horizontally).
            hitBox.size += new Vector2(40, 0);
			hitBox.center += new Vector2(-20, 0);

			// Now put/size me where I belong!
			myRectTransform.sizeDelta = size;
            i_hitBox.rectTransform.sizeDelta = hitBox.size;
            i_hitBox.rectTransform.anchoredPosition = hitBox.center - myRectTransform.anchoredPosition;

            // TEMP! Default value sloppy in here.
            SetSpeed(1, 0);
        }
        public Block SetHitsReq(int _numHitsReq) {
            numHitsReq = _numHitsReq;
            // Hits-required text
            if (numHitsReq > 1) {
                t_numHitsReq = Instantiate(ResourcesHandler.Instance.bouncePaint_blockNumHitsReqText).GetComponent<TextMeshProUGUI>();
                t_numHitsReq.rectTransform.SetParent(this.myRectTransform);
                t_numHitsReq.rectTransform.localScale = Vector2.one;
                t_numHitsReq.rectTransform.localEulerAngles = Vector2.zero;
                t_numHitsReq.rectTransform.offsetMin = t_numHitsReq.rectTransform.offsetMax = Vector2.zero;
                t_numHitsReq.color = new Color(0,0,0, 0.5f);
            }
            UpdateNumHitsReqText();
            return this;
        }
        public Block SetDontTap() {
            doTap = false;
            // if (!doTap) {
            //  i_body.sprite = s_bodyDontTap;
            // }
            SetIntentionVisuals(false);
            return this;
        }
        public Block SetUnpaintable() {
            isPaintable = false;
            return this;
        }
        public Block SetSpeed(float _speed, float _startLocOffset=0) {
            travelSpeed = _speed*0.03f; // awkward scaling down the speed here.
            doTravel = travelSpeed != 0;
            travelOscVal = _startLocOffset;
            ApplyPos();
            return this;
        }


        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
		public void SetIntentionVisuals(bool isPlayerComingToMe) {
			if (isPainted) { return; } // Already painted? We don't have to do anything.

			// if (isPlayerComingToMe) {
			// 	if (doTap) {
			// 		bodyColor = new Color(0.7f,0.7f,0.7f, 0.6f);
			// 	}
			// 	// else {
			// 	// 	bodyColor = new Color(0,0,0, 0.8f);
			// 	// }
			// }
			// else {
				if (doTap) {
					bodyColor = new Color(0,0,0, 0.4f); // light gray
				}
				else {
					bodyColor = new Color(0,0,0, 0.99f); // Black.
				}
			// }
		}
		private void UpdateNumHitsReqText() {
			if (t_numHitsReq != null) {
                t_numHitsReq.text = numHitsReq.ToString();
                t_numHitsReq.enabled = numHitsReq > 0;
				//if (numHitsReq <= 0) {
				//	t_numHitsReq.color = new Color(0,0,0, 0.3f);
				//}
			}
		}
		private void ApplyPos() {
			// Do travel?? Lerp me between my two center poses.
			if (doTravel) {
				float travelLoc = MathUtils.Sin01(travelOscVal);
                center = Vector2.Lerp(centerA,centerB, travelLoc) + posDipOffset + posDanceOffset;
			}
			// DON'T travel? Just put me at my one known center pos.
			else {
                center = centerA + posDipOffset + posDanceOffset;
			}
		}


        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        /// Paints me! :)
		public void OnPlayerBounceOnMe(Color playerColor) {
			// If I'm paintable AND not yet painted...!
            if (isPaintable && !isPainted) {
				// Hit me, Paul!
				numHitsReq --;
				UpdateNumHitsReqText();
				// That was the last straw, Cady??
				if (numHitsReq <= 0) {
					// Paint me!
	                isPainted = true;
	                bodyColor = playerColor;
				}
            }
            // Push me down!
			posDipOffset += new Vector2(0, -16f);
        }
		/// Called when Player taps to jump while in me, BUT I'm a don't-tap Block!
		public void OnPlayerPressJumpOnMeInappropriately() {
			bodyColor = new Color(0.6f,0f,0.06f, 1f);
		}
        //public void OnPlayerPaintedABlock() {
        //    // TEST!
        //    doTap = !DoTap;
        //    SetIntentionVisuals(false);
        //}



        // ----------------------------------------------------------------
        //  FixedUpdate
        // ----------------------------------------------------------------
        private void FixedUpdate() {
            if (myLevel.IsAnimatingIn) { return; } // Animating in? Don't move.
			UpdateTravel();
            UpdatePosOffsets();
			ApplyPos();
        }
		private void UpdateTravel() {
			if (doTravel) {
				travelOscVal += travelSpeed;
			}
		}
        private void UpdatePosOffsets() {
            // Dip
			Vector2 posDipTarget = Vector2.zero;
			if (posDipOffset != posDipTarget) {
				posDipOffset += new Vector2(
					(posDipTarget.x-posDipOffset.x) * 0.24f,
					(posDipTarget.y-posDipOffset.y) * 0.24f);
			}
            // Dance
            if (gameController.IsLevelComplete) {
                posDanceOffset = new Vector2(0, Mathf.Sin(Time.time*4f+center.x*0.016f) * 26f);
            }
        }

    }
}