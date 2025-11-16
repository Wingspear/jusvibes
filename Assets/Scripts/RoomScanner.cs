using System;
using System.Collections;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class RoomScanner : MonoBehaviour
{
    [SerializeField] private CaptureController captureController;
    [SerializeField] private CaptureInsightProcessor captureInsights;
    [SerializeField] private MusicGenerator musicGenerator;
    
    [Button(30)]
    public async Task ScanAndPlayMusic(Quark quark)
    {
        Debug.Log("Capturing photo...");
        await captureController.CapturePhoto();
        int numCaptures = 1;
        Debug.Log("Gathering capture insights...");
        string musicPrompt = await captureInsights.FetchCaptureMusicInsights(numCaptures);
        VisualInsights visualInsights = await captureInsights.FetchCaptureVisualInsights(numCaptures);
        Debug.Log("Primary: " + visualInsights.primaryColor);
        Debug.Log("Secondary: " + visualInsights.secondaryColor);
        quark.InjectColors(visualInsights.primaryColor, visualInsights.secondaryColor);
        Debug.Log("Generating music");
        await musicGenerator.GenerateMusic(quark.Audio, musicPrompt);
    }
}
