using UnityEngine;

public class VoiceDebug : MonoBehaviour
{
    public void HandleUserText(string text)
    {
        Debug.Log("[VoiceDebug] User said: " + text);
    }
}