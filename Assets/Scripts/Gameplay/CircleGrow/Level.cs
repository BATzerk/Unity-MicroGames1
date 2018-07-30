﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CircleGrow {
	public class Level : BaseLevel {
		// Constants
		public static int FirstLevelIndex = 1;
//		public static int LastLevelIndex = 101;
		// Components
        [SerializeField] private Image i_border=null;
        [SerializeField] private Image i_bounds=null;
        [SerializeField] private LevelUI levelUI=null;
        [SerializeField] private RectTransform rt_gameComponents=null; // Growers go on this!
        //[SerializeField] private LevelBoundsColliders boundsColliders=null;
		private List<Grower> growers;
		// Properties
//		private float screenShakeVolume;
        private int scoreRequired;
		private int currentGrowerIndex;
		private Rect r_levelBounds; // set to i_bounds.rect in Initialize.
		// References
		private GameController gameController;


        // Getters (Public)
        public int ScoreRequired { get { return scoreRequired; } }
        public List<Grower> Growers { get { return growers; } }
		// Getters (Private)
		private Grower currentGrower {
			get {
				if (currentGrowerIndex<0 || currentGrowerIndex>=growers.Count) { return null; } // Index outta bounds? Return null.
				return growers[currentGrowerIndex];
			}
		}
        /*
		private bool IsIllegalOverlap(Grower circle) {
			return IsCircleAtPos(circle.Pos, circle.Radius) || !IsCircleInBounds(circle.Pos, circle.Radius);
		}
		private bool IsCircleInBounds(Vector2 pos, float radius) {
			if (pos.x-radius < r_levelBounds.xMin) { return false; }
			if (pos.x+radius > r_levelBounds.xMax) { return false; }
			if (pos.y-radius < r_levelBounds.yMin) { return false; }
			if (pos.y+radius > r_levelBounds.yMax) { return false; }
			return true;
		}
		private bool IsCircleAtPos(Vector2 pos, float radius) {
			foreach (Grower c in growers) {
				if (c.Pos==pos && c.Radius==radius) { continue; } // Skip itself.
				if (DoCirclesOverlap(c.Pos,c.Radius, pos,radius)) { return true; }
			}
			return false; // We're good!
		}
		private bool DoCirclesOverlap(Grower circleA, Grower circleB) {
			return DoCirclesOverlap(circleA.Pos,circleA.Radius, circleB.Pos,circleB.Radius);
		}
		private bool DoCirclesOverlap(Vector2 posA,float radiusA, Vector2 posB,float radiusB) {
			float dist = Vector2.Distance(posA,posB);
			return dist < radiusA+radiusB;
		}
		*/



		// ----------------------------------------------------------------
		//  Initialize
		// ----------------------------------------------------------------
		public void Initialize(GameController _gameController, Transform tf_parent, int _levelIndex) {
			gameController = _gameController;
			BaseInitialize(tf_parent, _levelIndex);

            levelUI.Initialize();

			r_levelBounds = i_bounds.rectTransform.rect;
			r_levelBounds.center += new Vector2(0, r_levelBounds.height*0.5f);// Hacky center the bounds because Level and Grower anchors are currently different :P

			i_border.color = Grower.color_solid;
        }


        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void UpdateScoreUI(int scorePossible, int scoreSolidified) {
            levelUI.UpdateScoreUI(scorePossible, scoreSolidified);
        }
        public void OnLoseLevel(LoseReasons reason) {
            levelUI.OnLoseLevel(reason);
        }
        public void OnWinLevel() {
            levelUI.OnWinLevel();
        }


        // ----------------------------------------------------------------
        //  Game Doers
        // ----------------------------------------------------------------
		public void SolidifyCurrentGrower() {
			if (currentGrower == null) { return; } // Safety check.
            if (currentGrower.CurrentState != GrowerStates.Growing) { return; } // Oh, if it's only pre-growing, DON'T do anything.

			// Solidify current, and move onto the next one!
			currentGrower.Solidify();
			SetCurrentGrowerIndex(currentGrowerIndex + 1);
		}
        public void OnIllegalOverlap() {
            gameController.OnIllegalOverlap();
		}
		private void SetCurrentGrowerIndex(int _index) {
            currentGrowerIndex = _index;

			gameController.UpdateScore();

			// There IS another Grower!
			if (currentGrower != null) {
                currentGrower.StartGrowing();
			}
            // There is NOT another Grower! End the level.
			else {
                gameController.OnAllGrowersSolidified();
			}
		}





		// ----------------------------------------------------------------
		//  Update
		// ----------------------------------------------------------------
		private void Update () {
			if (Time.timeScale == 0) { return; } // No time? Do nothin'.

			if (gameController.IsGameStatePlaying) {
				GrowCurrentGrower();
			}
		}
        private void GrowCurrentGrower() {
			if (currentGrower == null) { return; } // Safety check.

            // Grow it, and update our score!
            if (currentGrower.CurrentState == GrowerStates.Growing) {
                currentGrower.GrowStep();
                gameController.UpdateScore();
            }

   //         if (IsIllegalOverlap(currentGrower)) {
			//	OnIllegalOverlap(currentGrower);
			//}
		}


		// ----------------------------------------------------------------
		//  Destroying Elements
		// ----------------------------------------------------------------
		private void DestroyLevelComponents() {
			if (growers != null) {
				for (int i=growers.Count-1; i>=0; --i) {
					Destroy(growers[i].gameObject);
				}
			}
			growers = new List<Grower>();
		}
		// ----------------------------------------------------------------
		//  Adding Elements
		// ----------------------------------------------------------------
        private void AddGrower(GrowerShapes shape, float radius, float growSpeed, float x,float y) {
            Grower newObj = Instantiate(resourcesHandler.circleGrow_grower).GetComponent<Grower>();
            newObj.Initialize(this, rt_gameComponents, new Vector2(x,y), shape, radius, growSpeed);
            growers.Add(newObj);
		}



		// ----------------------------------------------------------------
		//  Making Level!
		// ----------------------------------------------------------------
		override protected void AddLevelComponents() {
			DestroyLevelComponents(); // Just in case.
			if (resourcesHandler == null) { return; } // Safety check for runtime compile.

			// Reset values
			growers = new List<Grower>();

            // Specify default values
            GrowerShapes sh = GrowerShapes.Circle;
            float sr = 10; // startingRadius
            float gs = 0.8f; // growSpeed
            scoreRequired = 1000;

			// NOTE: All coordinates are based off of a 600x800 available playing space! :)

			int li = LevelIndex;
			int i=FirstLevelIndex;
			if (false) {}


			// Simple, together.
			else if (li == i++) {
                scoreRequired = 1000;
                AddGrower(sh,sr,gs, 0,0);
            }
            else if (li == i++) {
                scoreRequired = 1000;
                AddGrower(sh,sr,gs, 0,-150);
                AddGrower(sh,sr,gs, 0, 150);
            }
            else if (li == i++) {
                scoreRequired = 1500;
                AddGrower(sh,sr,gs,  50,-150);
                AddGrower(sh,sr,gs, -150,  0);
                AddGrower(sh,sr,gs,  50, 150);
            }


			else {
				DestroyLevelComponents();
                levelUI.t_moreLevelsComingSoon.gameObject.SetActive(true);
				Debug.LogWarning("No level data available for level: " + li);
			}

			// Start growing the first dude!
            SetCurrentGrowerIndex(0);
		}

	}
}

/*

		//		private void CheckOscillatingCirclesOverlaps() {
//			for (int i=circles.Count-1; i>=0; --i) {
//				if (circles[i].IsOscillating) {
//					if (IsCircleIllegalOverlap(circles[i])) {
//						OnCircleIllegalOverlap(circles[i]);
//					}
//				}
//			}
//		}
//		// ----------------------------------------------------------------
//		//  Events
//		// ----------------------------------------------------------------
//		public void OnWinLevel(Player winningPlayer) {
//			// Shaken, not stirred!
//			screenShakeVolume = 2f;
//		}
//		// ----------------------------------------------------------------
//		//  Update
//		// ----------------------------------------------------------------
//		private void Update() {
//			UpdateScreenShake();
//		}
//		private void UpdateScreenShake() {
//			if (screenShakeVolume != 0) {
//				// Apply!
//				//float rotation = Mathf.Sin(screenShakeVolume*5f) * screenShakeVolume*4f;
//				//this.transform.localEulerAngles = new Vector3(0,0,rotation);
//				float yOffset = Mathf.Sin(screenShakeVolume*20f) * screenShakeVolume*7f;
//				myRectTransform.anchoredPosition = new Vector3(0, yOffset, 0); // TEST
//				// Update!
//				screenShakeVolume += (0-screenShakeVolume) / 24f * TimeController.FrameTimeScale;
//				if (Mathf.Abs(screenShakeVolume) < 0.5f) { screenShakeVolume = 0; }
//			}
//		}

		private Vector2 GetBestPosForNewCircle() {
			Vector2 randPos;
			// Kinda sloppy! Brute-force-y.
			randPos = GetOpenPosForNewCircle(200);
			if (randPos.x != Mathf.NegativeInfinity) { return randPos; }
			randPos = GetOpenPosForNewCircle(100); // TO DO: CLean up this negative infinity awkwardness checksings.
			if (randPos.x != Mathf.NegativeInfinity) { return randPos; }
			randPos = GetOpenPosForNewCircle(50);
			if (randPos.x != Mathf.NegativeInfinity) { return randPos; }
			randPos = GetOpenPosForNewCircle(10);
			if (randPos.x != Mathf.NegativeInfinity) { return randPos; }
			Debug.Log("Couldn't find open pos for new circle!");
			return Vector2.negativeInfinity;
		}
		private Vector2 GetOpenPosForNewCircle(float newRadius) {
			Vector2 pos;
			int safetyCount=0;
			do {
				pos = new Vector2(Random.Range(r_levelBounds.xMin,r_levelBounds.xMax), Random.Range(r_levelBounds.yMin,r_levelBounds.yMax));
				if (CanAddCircleAtPos(pos, newRadius)) { break; }
				if (safetyCount++>499) {
					//Debug.Log("Couldn't find open pos for new circle!");
					return Vector2.negativeInfinity;
				}
			}
			while(true);
			return pos;
		}
		private bool CanAddCircleAtPos(Vector2 pos, float radius) {
			return !IsCircleAtPos(pos, radius) && IsCircleInBounds(pos, radius);
		}
        private void AddNewCircle() {
            float startingRadius = 5f;
            Vector2 randPos = GetBestPosForNewCircle();
            // Can't find a suitable position to put this circle? We're outta room!
            if (randPos.x == Mathf.NegativeInfinity) {
                SetGameOver();
            }
            // We DID find a suitable pos for this new circle! Add it!
            else {
                Circle newCircle = Instantiate(resourcesHandler.circleGrow_circle).GetComponent<Circle>();
                newCircle.Initialize(this, canvas.transform, randPos, startingRadius);
                circles.Add(newCircle);
                newCircle.SetIsOscillating(true); // Start it out oscillating up up!
            }
            // Update the score now! :)
            UpdateScore();
        }
        */