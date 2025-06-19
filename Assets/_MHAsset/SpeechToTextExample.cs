using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Demo class for speech-to-text functionality in Unity
public class SpeechToTextDemo : MonoBehaviour, ISpeechToTextListener
{
	// UI references
	public TextMeshProUGUI SpeechText;
	public Button StartSpeechToTextButton, StopSpeechToTextButton;
	public Slider VoiceLevelSlider;
	public bool PreferOfflineRecognition;

	private float normalizedVoiceLevel;

	// Initialize speech-to-text and button listeners
	private void Awake()
	{
		SpeechToText.Initialize( "en-US" );

		StartSpeechToTextButton.onClick.AddListener( StartSpeechToText );
		StopSpeechToTextButton.onClick.AddListener( StopSpeechToText );
	}

	// Update UI button states and voice level slider
	private void Update()
	{
		StartSpeechToTextButton.interactable = SpeechToText.IsServiceAvailable( PreferOfflineRecognition ) && !SpeechToText.IsBusy();
		StopSpeechToTextButton.interactable = SpeechToText.IsBusy();

		// Smoothly animate the voice level slider
		VoiceLevelSlider.value = Mathf.Lerp( VoiceLevelSlider.value, normalizedVoiceLevel, 15f * Time.unscaledDeltaTime );
	}

	// Change the recognition language
	public void ChangeLanguage( string preferredLanguage )
	{
		if( !SpeechToText.Initialize( preferredLanguage ) )
			SpeechText.text = "Couldn't initialize with language: " + preferredLanguage;
	}

	// Start speech-to-text session after permission check
	public void StartSpeechToText()
	{
		SpeechToText.RequestPermissionAsync( ( permission ) =>
		{
			if( permission == SpeechToText.Permission.Granted )
			{
				if( SpeechToText.Start( this, preferOfflineRecognition: PreferOfflineRecognition ) )
					SpeechText.text = "";
				else
					SpeechText.text = "Couldn't start speech recognition session!";
			}
			else
				SpeechText.text = "Permission is denied!";
		} );
	}

	// Stop the current speech-to-text session
	public void StopSpeechToText()
	{
		SpeechToText.ForceStop();
	}

	// Called when ready to start listening
	void ISpeechToTextListener.OnReadyForSpeech()
	{
		Debug.Log( "OnReadyForSpeech" );
	}

	// Called when speech input begins
	void ISpeechToTextListener.OnBeginningOfSpeech()
	{
		Debug.Log( "OnBeginningOfSpeech" );
	}

	// Called when the voice level changes (for UI feedback)
	void ISpeechToTextListener.OnVoiceLevelChanged( float normalizedVoiceLevel )
	{
		// Note: On Android, initial beep may trigger this callback. Consider ignoring for ~0.5s.
		this.normalizedVoiceLevel = normalizedVoiceLevel;
	}

	// Called with partial speech recognition results
	void ISpeechToTextListener.OnPartialResultReceived( string spokenText )
	{
		Debug.Log( "OnPartialResultReceived: " + spokenText );
		SpeechText.text = spokenText;
	}

	// Called with final speech recognition result or error
	void ISpeechToTextListener.OnResultReceived( string spokenText, int? errorCode )
	{
		Debug.Log( "OnResultReceived: " + spokenText + ( errorCode.HasValue ? ( " --- Error: " + errorCode ) : "" ) );
		SpeechText.text = spokenText;
		normalizedVoiceLevel = 0f;

		// Recommended: handle error codes and session results for better UX
	}
}