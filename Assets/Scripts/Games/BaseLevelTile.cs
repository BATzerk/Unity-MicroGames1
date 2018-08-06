﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class BaseLevelTile : MonoBehaviour {
	// Components
	[SerializeField] private   Button myButton=null;
	[SerializeField] protected Image i_backing=null;
	[SerializeField] private   Text t_levelNumber=null;
	[SerializeField] private   RectTransform myRectTransform=null;
	// Properties
	private bool isLocked;
	private int levelIndex;
	// References
	protected BaseLevelSelectController levelSelectController;

	// Getters (Public)
	public int LevelIndex { get { return levelIndex; } }
	// Getters (Protected)
	protected string MyGameName() { return levelSelectController.MyGameName(); }
	protected Vector2 pos {
		get { return myRectTransform.anchoredPosition; }
		set { myRectTransform.anchoredPosition = value; }
	}
	protected bool IsLocked { get { return isLocked; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(BaseLevelSelectController _levelSelectController, Transform tf_parent, int _levelIndex, bool _isLocked) {
		levelSelectController = _levelSelectController;
		levelIndex = _levelIndex;
		isLocked = _isLocked;

		GameUtils.ParentAndReset(this.gameObject, tf_parent);
		this.gameObject.name = "LevelTile " + levelIndex;

		// Visuals!
		UpdateLockedVisuals();
		t_levelNumber.text = levelIndex.ToString();
	}
	private void UpdateLockedVisuals() {
		myButton.interactable = !isLocked;
		t_levelNumber.color = isLocked ? new Color(0,0,0, 0.5f) : new Color(0,0,0, 0.9f);
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetPosSize(Vector2 _pos, Vector2 _size) {
		pos = _pos;
		myRectTransform.sizeDelta = _size;
	}
	private void StartGameAtLevel() {
		SaveStorage.SetInt(SaveKeys.LastLevelPlayed(MyGameName()), levelIndex);
		UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Gameplay(MyGameName()));
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnClick() {
		if (!isLocked) {
			StartGameAtLevel();
		}
	}


	// ----------------------------------------------------------------
	//  Debug
	// ----------------------------------------------------------------
	public void Debug_UnlockMe() {
		isLocked = false;
		UpdateLockedVisuals();
	}



}
