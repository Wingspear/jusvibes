using Sirenix.OdinInspector;
using UnityEngine;

public class RoomScanner : MonoBehaviour
{
    [SerializeField] private CaptureController captureController;
    [SerializeField] private CaptureInsightProcessor captureInsights;
    [SerializeField] private MusicGenerator musicGenerator;
    
    [Button(30)]
    public async void Scan()
    {
        Debug.Log("Capturing photo...");
        await captureController.CapturePhoto();
        Debug.Log("Gathering capture insights...");
        string musicPrompt = await captureInsights.FetchCaptureMusicInsights();
        Debug.Log("Generating music");
        await musicGenerator.GenerateMusic(musicPrompt);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
