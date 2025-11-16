using System.IO;
using System.Threading.Tasks;
using Meta.XR;
using Sirenix.OdinInspector;
using UnityEngine;

public static class PassthroughCameraExtensions
{
    public static async Task<bool> SaveCurrentCameraImageAsync(this PassthroughCameraAccess camera, string filePath,
        bool jpg = false)
    {
        if (!camera.IsPlaying)
        {
            Debug.LogError("No camera frame available yet.");
            return false;
        }

        var tex = camera.GetTexture() as Texture2D;
        if (tex == null)
        {
            Debug.LogError("Camera texture is null or not a Texture2D.");
            return false;
        }

        // --- MAIN THREAD: Copy texture ---
        Texture2D copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        copy.SetPixels32(tex.GetPixels32());
        copy.Apply();

        // --- MAIN THREAD: Encode ---
        byte[] bytes = jpg ? copy.EncodeToJPG(95) : copy.EncodeToPNG();

        // Cleanup texture copy (Unity object)
        Object.Destroy(copy);

        // Ensure directory exists
        string dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // --- BACKGROUND THREAD: Write file ---
        try
        {
            await File.WriteAllBytesAsync(filePath, bytes);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to write file: " + ex);
            return false;
        }

        Debug.Log("Saved passthrough camera image → " + filePath);
        return true;
    }
}

public class CaptureController : MonoBehaviour
{
    [SerializeField] private PassthroughCameraAccess camAccess;

    [Button(30)]
    public async void CapturePhotoTest()
    {
        await camAccess.SaveCurrentCameraImageAsync(Application.persistentDataPath + "/capture.png");
    }

    public async Task CapturePhoto()
    {
        await camAccess.SaveCurrentCameraImageAsync(Application.persistentDataPath + "/capture.png");
    }
}
