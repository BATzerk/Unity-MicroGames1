﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WaveTap {
	public class Bar : MonoBehaviour {
		// Components
		[SerializeField] private Image i_body=null;
		[SerializeField] private RectTransform myRectTransform=null;
		[SerializeField] private Text t_numHitsLeft=null;
		// Properties
		private int numHitsLeft;

		// Getters (Public)
		public float PosY { get { return Pos.y; } }
		public int NumHitsLeft { get { return numHitsLeft; } }
		// Getters (Private)
		private Color bodyColor {
			get { return i_body.color; }
			set { i_body.color = value; }
		}
		private Vector2 Pos {
			get { return myRectTransform.anchoredPosition; }
			set { myRectTransform.anchoredPosition = value; }
		}


		// ----------------------------------------------------------------
		//  Initialize
		// ----------------------------------------------------------------
//		public void Initialize(GameController _gameController, Transform tf_parent, Vector2 _pos, int _numHitsLeft) {
//			//gameController = _gameController;
//			this.transform.SetParent(tf_parent);
//			this.transform.localScale = Vector3.one;
//			this.transform.localPosition = Vector3.zero;
//			this.transform.localEulerAngles = Vector3.zero;
//
//			Pos = _pos;
//			SetNumHitsLeft(_numHitsLeft);
//		}
		public void Reset(int currentLevel) {
			Pos = Vector2.zero;
			SetNumHitsLeft(currentLevel);
		}


		// ----------------------------------------------------------------
		//  Doers
		// ----------------------------------------------------------------
		private void SetNumHitsLeft(int _numHitsLeft) {
			numHitsLeft = _numHitsLeft;
			t_numHitsLeft.text = numHitsLeft.ToString();
//			myRectTransform.sizeDelta = new Vector2(radius*2, radius*2);
			if (numHitsLeft > 0) { // Still more hits left? Rando my colo!
				float h = Random.Range(0f, 1f);
				bodyColor = new ColorHSB(h, 0.8f, 1f).ToColor();
			}
			else { // We're done being hit! Go green!
				bodyColor = Color.green;
			}
		}


		// ----------------------------------------------------------------
		//  Events
		// ----------------------------------------------------------------
		public void HitMe() {
			SetNumHitsLeft(numHitsLeft - 1);
		}
		public void OnMissMe() {
			bodyColor = Color.red;
		}

	}
}