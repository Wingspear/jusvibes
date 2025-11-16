using System;
using System.IO;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class MusicGenerator : MonoBehaviour
{
    private string apiKey = "";
    
    [Header("Suno API")] [SerializeField] private SunoConfig sunoConfig;
    [SerializeField] private string callBackUrl = "https://dummy-url.com/callback";
    [SerializeField] private string model = "V5";

    [TextArea]
    public string prompt = "";

    private const string GenerateUrl = "https://api.sunoapi.org/api/v1/generate";
    private const string RecordInfoUrl = "https://api.sunoapi.org/api/v1/generate/record-info";

    // ---------- Odin button entry (for inspector testing) ----------

    [Button(30)]
    public async void TestGenerate(AudioSource audioSource, string prompt)
    {
        await GenerateMusic(audioSource, prompt);
    }

    // ---------- Public async API ----------

    /// <summary>
    /// Full pipeline: load config → call Suno → poll → download → play.
    /// This Task completes only when audio is playing (or if something fails).
    /// </summary>
    public async Task GenerateMusic(AudioSource audioSource, string userPrompt)
    {
        apiKey = sunoConfig.sunoApiKey;
        Debug.Log("Setting api key to " + apiKey);
        await GenerateAndPlayAsync(audioSource, userPrompt);
    }

    // ---------- DTOs ----------

    [Serializable]
    private class GenerateRequestBody
    {
        public bool customMode;
        public bool instrumental;
        public string model;
        public string callBackUrl;
        public string prompt;

        public string style = null;
        public string title = null;
        public string personaId = null;
        public string negativeTags = null;
        public string vocalGender = null;
        public float styleWeight = 0f;
        public float weirdnessConstraint = 0f;
        public float audioWeight = 0f;
    }

    [Serializable]
    private class GenerateResponseData
    {
        public string taskId;
    }

    [Serializable]
    private class GenerateResponse
    {
        public int code;
        public string msg;
        public GenerateResponseData data;
    }

    [Serializable]
    private class SunoTrack
    {
        public string audioUrl;
        public string streamAudioUrl;
        public string title;
        public string id;
    }

    [Serializable]
    private class RecordInfoInner
    {
        public SunoTrack[] sunoData;
    }

    [Serializable]
    private class RecordInfoData
    {
        public string taskId;
        public string status;
        public RecordInfoInner response;
    }

    [Serializable]
    private class RecordInfoResponse
    {
        public int code;
        public string msg;
        public RecordInfoData data;
    }
    
    // ---------- Async pipeline ----------

    private async Task GenerateAndPlayAsync(AudioSource audioSource, string userPrompt)
    {
        // 1. POST /generate
        string taskId = await CallGenerateEndpointAsync(userPrompt);
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("GenerateAndPlayAsync: Failed to get taskId.");
            return;
        }

        // 2. Poll until FIRST_SUCCESS (audio ready)
        string streamUrl = await GetMusicUrlAsync(taskId);
        if (string.IsNullOrEmpty(streamUrl))
        {
            Debug.LogError("GenerateAndPlayAsync: Failed to get stream URL.");
            return;
        }

        // 3. Download & play
        await DownloadAndPlayAsync(audioSource, streamUrl);
    }

    private async Task<string> CallGenerateEndpointAsync(string userPrompt)
    {
        var body = new GenerateRequestBody
        {
            customMode = true,
            instrumental = true,
            model = model,
            callBackUrl = callBackUrl,
            prompt = userPrompt
        };

        string json = JsonUtility.ToJson(body);

        using (var request = new UnityWebRequest(GenerateUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            await AwaitRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Generate Error: " + request.error);
                Debug.LogError(request.downloadHandler.text);
                return null;
            }

            var resp = JsonUtility.FromJson<GenerateResponse>(request.downloadHandler.text);
            if (resp == null || resp.code != 200 || resp.data == null)
            {
                Debug.LogError("Suno error: " + (resp != null ? resp.msg : "null response"));
                return null;
            }

            string taskId = resp.data.taskId;
            Debug.Log("Task ID: " + taskId);
            return taskId;
        }
    }

    private async Task<string> GetMusicUrlAsync(string taskId)
    {
        string url = $"{RecordInfoUrl}?taskId={taskId}";

        while (true)
        {
            using (var req = UnityWebRequest.Get(url))
            {
                req.SetRequestHeader("Authorization", "Bearer " + apiKey);

                await AwaitRequest(req);

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Poll Error: " + req.error);
                    return null;
                }

                var resp = JsonUtility.FromJson<RecordInfoResponse>(req.downloadHandler.text);

                if (resp == null || resp.data == null)
                {
                    Debug.LogError("Bad poll response: " + req.downloadHandler.text);
                    return null;
                }

                Debug.Log("Status: " + resp.data.status);

                // Better to wait for FIRST_SUCCESS (audio ready)
                if (resp.data.status == "FIRST_SUCCESS" || resp.data.status == "TEXT_SUCCESS")
                {
                    string streamUrl = resp.data.response.sunoData[0].streamAudioUrl;
                    Debug.Log("🎵 STREAM URL READY: " + streamUrl);
                    return streamUrl;
                }
            }

            // wait 3s between polls
            await Task.Delay(3000);
        }
    }

    private async Task DownloadAndPlayAsync(AudioSource audioSource, string url)
    {
        if (audioSource == null)
        {
            Debug.LogError("MusicGenerator: No AudioSource to play audio.");
            return;
        }

        using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            await AwaitRequest(req);

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio download error: " + req.error);
                return;
            }

            var clip = DownloadHandlerAudioClip.GetContent(req);

            if (clip == null)
            {
                Debug.LogError("Failed to decode audio clip from URL: " + url);
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();

            Debug.Log("▶️ Playing generated track. Length: " + clip.length + "s");
        }
    }

    // ---------- Helper: await UnityWebRequest ----------

    private static Task AwaitRequest(UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = request.SendWebRequest();

        op.completed += _ => tcs.TrySetResult(true);

        return tcs.Task;
    }
}
