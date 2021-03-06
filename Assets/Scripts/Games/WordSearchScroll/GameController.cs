﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WordSearchScroll {
	public class GameController : BaseGameController {
        // Overrideables
        override public string MyGameName() { return GameNames.WordSearchScroll; }
        
		// Objects
		private Level level;
		// References
//		[SerializeField] private GameUI ui=null;


		// ----------------------------------------------------------------
		//  Start / Destroy
		// ----------------------------------------------------------------
		override protected void Start () {
			base.Start();

			// Let's a-play!
			ResetLevel();
		}



        // ----------------------------------------------------------------
        //  Game Events
        // ----------------------------------------------------------------
//        public void GameOver() {
//            ui.OnGameOver();
//        }



		// ----------------------------------------------------------------
		//  Doers - Loading Level
		// ----------------------------------------------------------------
		public void ResetLevel() {
			// Destroy the previous level.
			DestroyLevel ();

			// Instantiate the Level from the provided LevelData!
			level = ((GameObject) Instantiate (resourcesHandler.wordSearchScroll_level)).GetComponent<Level>();
			level.Initialize (this, canvas.transform);

			// Tell the people!
//			ui.OnStartLevel();
			//			// Dispatch event!
			//			GameManagers.Instance.EventManager.OnStartGameAtLevel (currentLevel);
		}

		private void DestroyLevel () {
			if (level != null) {
				Destroy(level.gameObject);
				level = null;
			}
		}







	}


}



