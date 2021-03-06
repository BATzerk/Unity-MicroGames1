using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager {
	// Actions and Event Variables
	public delegate void NoParamAction ();
	public delegate void BoolAction (bool _bool);
	public delegate void StringAction (string _str);
	public delegate void IntAction (int _int);
	public delegate void AudioClipAction (AudioClip _clip);

    public event NoParamAction AnyButtonClickEvent; // NOTE: Kinda weird this is in EventManager, not a sound class. :P
	public event NoParamAction ScreenSizeChangedEvent;
	public event NoParamAction RetryButtonClickEvent;
	public event NoParamAction QuitGameplayButtonClickEvent;
	public event BoolAction SetDebugUIVisibleEvent;
	public event AudioClipAction TriggerAudioClipEvent;
	public event IntAction LevelJumpButtonClickEvent;


	// Events
	public void OnScreenSizeChanged () { if (ScreenSizeChangedEvent!=null) { ScreenSizeChangedEvent (); } }

    public void OnAnyButtonClick() { AnyButtonClickEvent?.Invoke(); }
	public void OnLevelJumpButtonClick(int levelIndexChange) { if (LevelJumpButtonClickEvent!=null) { LevelJumpButtonClickEvent (levelIndexChange); } }
	public void OnRetryButtonClick() { if (RetryButtonClickEvent!=null) { RetryButtonClickEvent(); } }
	public void OnSetDebugUIVisible(bool isVisible) { if (SetDebugUIVisibleEvent!=null) { SetDebugUIVisibleEvent(isVisible); } }
	public void OnQuitGameplayButtonClick() { if (QuitGameplayButtonClickEvent!=null) { QuitGameplayButtonClickEvent(); } }

	public void TriggerAudioClip (AudioClip _clip) {if (TriggerAudioClipEvent != null) {TriggerAudioClipEvent (_clip);}}


	// Game-specific
    public delegate void Spool_SpoolAction(SpoolOut.Spool spool);
    public event Spool_SpoolAction Spool_PathChangedEvent;
    public void Spool_OnSpoolPathChangedEvent(SpoolOut.Spool spool) { if (Spool_PathChangedEvent!=null) { Spool_PathChangedEvent(spool); } }
    
    public delegate void SlideAndStick_LevelAction(SlideAndStick.Level level);
    public event SlideAndStick_LevelAction SlideAndStick_StartLevelEvent;
    public void SlideAndStick_OnStartLevel(SlideAndStick.Level level) { if (SlideAndStick_StartLevelEvent!=null) { SlideAndStick_StartLevelEvent(level); } }

}



