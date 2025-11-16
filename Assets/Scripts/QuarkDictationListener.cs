using UnityEngine;
using Oculus.Voice.Dictation;       // AppDictationExperience
using UnityEngine.Events;

public class QuarkDictationListener : MonoBehaviour
{
    [Header("Meta Voice Dictation")]
    public AppDictationExperience dictation;   // drag your AppDictationExperience here

    [Header("Debug / Output")]
    [TextArea] public string lastPartial;
    [TextArea] public string lastFull;

    [Header("Events")]
    public UnityEvent<string> OnUserText;      // final text output for other systems

    private void OnEnable()
    {
        if (dictation == null)
        {
            Debug.LogError("[QuarkDictationListener] No AppDictationExperience assigned.");
            return;
        }

        // Subscribe to available DictationEvents
        dictation.DictationEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        dictation.DictationEvents.OnFullTranscription.AddListener(OnFullTranscription);
        dictation.DictationEvents.OnError.AddListener(OnDictationError);
    }

    private void OnDisable()
    {
        if (dictation == null) return;

        dictation.DictationEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        dictation.DictationEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        dictation.DictationEvents.OnError.RemoveListener(OnDictationError);
    }

    // Called continuously during speech
    private void OnPartialTranscription(string text)
    {
        lastPartial = text;
        Debug.Log("[Dictation] Partial: " + text);
    }

    // Called when the dictation system determines the utterance is complete
    private void OnFullTranscription(string text)
    {
        lastFull = text;
        Debug.Log("[Dictation] Full: " + text);

        // Treat this as the final result
        OnUserText?.Invoke(text);
    }

    private void OnDictationError(string error, string message)
    {
        Debug.LogError($"[Dictation] ERROR: {error} - {message}");
    }

    // Manual triggers
    public void StartDictation()
    {
        if (dictation == null) return;

        Debug.Log("[Dictation] Activate()");
        dictation.Activate();
    }

    public void StopDictation()
    {
        if (dictation == null) return;

        Debug.Log("[Dictation] Deactivate()");
        dictation.Deactivate();
    }

    // TEMP INPUT: Editor keyboard + Quest controller
    private void Update()
    {
#if UNITY_EDITOR
        // Editor: SPACE to start, ENTER to stop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[Dictation] SPACE → StartDictation()");
            StartDictation();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("[Dictation] ENTER → StopDictation()");
            StopDictation();
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        // Quest: A button → start, B button → stop
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("[Dictation] A BUTTON → StartDictation()");
            StartDictation();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("[Dictation] B BUTTON → StopDictation()");
            StopDictation();
        }
#endif
    }
}