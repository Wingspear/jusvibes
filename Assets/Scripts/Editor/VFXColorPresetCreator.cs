using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to create default VFX color presets.
/// </summary>
public class VFXColorPresetCreator
{
    [MenuItem("QUARK/Create Default Rainbow Preset")]
    public static void CreateDefaultRainbowPreset()
    {
        VFXColorPreset preset = ScriptableObject.CreateInstance<VFXColorPreset>();
        preset.presetName = "Rainbow Spectrum";
        preset.description = "Default rainbow color cycle that transitions through the full spectrum";

        // Create rainbow gradient for primary color
        GradientColorKey[] primaryColorKeys = new GradientColorKey[7];
        primaryColorKeys[0] = new GradientColorKey(new Color(1f, 0f, 0f), 0f);      // Red
        primaryColorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.17f); // Orange
        primaryColorKeys[2] = new GradientColorKey(new Color(1f, 1f, 0f), 0.33f);   // Yellow
        primaryColorKeys[3] = new GradientColorKey(new Color(0f, 1f, 0f), 0.5f);    // Green
        primaryColorKeys[4] = new GradientColorKey(new Color(0f, 0f, 1f), 0.67f);   // Blue
        primaryColorKeys[5] = new GradientColorKey(new Color(0.5f, 0f, 1f), 0.83f); // Purple
        primaryColorKeys[6] = new GradientColorKey(new Color(1f, 0f, 0f), 1f);      // Back to Red

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        preset.primaryGradient = new Gradient();
        preset.primaryGradient.SetKeys(primaryColorKeys, alphaKeys);

        // Create offset gradient for secondary color (shifted hue)
        GradientColorKey[] secondaryColorKeys = new GradientColorKey[7];
        secondaryColorKeys[0] = new GradientColorKey(new Color(0f, 0f, 1f), 0f);      // Blue
        secondaryColorKeys[1] = new GradientColorKey(new Color(0.5f, 0f, 1f), 0.17f); // Purple
        secondaryColorKeys[2] = new GradientColorKey(new Color(1f, 0f, 0f), 0.33f);   // Red
        secondaryColorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f);  // Orange
        secondaryColorKeys[4] = new GradientColorKey(new Color(1f, 1f, 0f), 0.67f);   // Yellow
        secondaryColorKeys[5] = new GradientColorKey(new Color(0f, 1f, 0f), 0.83f);   // Green
        secondaryColorKeys[6] = new GradientColorKey(new Color(0f, 0f, 1f), 1f);      // Back to Blue

        preset.secondaryGradient = new Gradient();
        preset.secondaryGradient.SetKeys(secondaryColorKeys, alphaKeys);

        // Create accent gradient (bright white to colors)
        GradientColorKey[] accentColorKeys = new GradientColorKey[7];
        accentColorKeys[0] = new GradientColorKey(new Color(1f, 1f, 0f), 0f);        // Yellow
        accentColorKeys[1] = new GradientColorKey(new Color(0f, 1f, 0f), 0.17f);     // Green
        accentColorKeys[2] = new GradientColorKey(new Color(0f, 1f, 1f), 0.33f);     // Cyan
        accentColorKeys[3] = new GradientColorKey(new Color(0f, 0.5f, 1f), 0.5f);    // Light Blue
        accentColorKeys[4] = new GradientColorKey(new Color(1f, 0f, 1f), 0.67f);     // Magenta
        accentColorKeys[5] = new GradientColorKey(new Color(1f, 0.5f, 0.5f), 0.83f); // Pink
        accentColorKeys[6] = new GradientColorKey(new Color(1f, 1f, 0f), 1f);        // Back to Yellow

        preset.accentGradient = new Gradient();
        preset.accentGradient.SetKeys(accentColorKeys, alphaKeys);

        // Save asset
        string path = "Assets/Presets/DefaultRainbowSpectrum.asset";
        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created default rainbow preset at: {path}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = preset;
    }

    [MenuItem("QUARK/Create Warm Sunset Preset")]
    public static void CreateWarmSunsetPreset()
    {
        VFXColorPreset preset = ScriptableObject.CreateInstance<VFXColorPreset>();
        preset.presetName = "Warm Sunset";
        preset.description = "Warm colors transitioning from orange to purple like a sunset";

        GradientColorKey[] primaryColorKeys = new GradientColorKey[5];
        primaryColorKeys[0] = new GradientColorKey(new Color(1f, 0.4f, 0f), 0f);     // Orange
        primaryColorKeys[1] = new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0.25f); // Red-Orange
        primaryColorKeys[2] = new GradientColorKey(new Color(1f, 0f, 0.5f), 0.5f);   // Pink
        primaryColorKeys[3] = new GradientColorKey(new Color(0.6f, 0f, 0.8f), 0.75f); // Purple
        primaryColorKeys[4] = new GradientColorKey(new Color(0.2f, 0f, 0.5f), 1f);   // Dark Purple

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        preset.primaryGradient = new Gradient();
        preset.primaryGradient.SetKeys(primaryColorKeys, alphaKeys);

        GradientColorKey[] secondaryColorKeys = new GradientColorKey[5];
        secondaryColorKeys[0] = new GradientColorKey(new Color(1f, 0.6f, 0.2f), 0f);
        secondaryColorKeys[1] = new GradientColorKey(new Color(1f, 0.3f, 0.3f), 0.25f);
        secondaryColorKeys[2] = new GradientColorKey(new Color(1f, 0.2f, 0.6f), 0.5f);
        secondaryColorKeys[3] = new GradientColorKey(new Color(0.7f, 0.2f, 0.9f), 0.75f);
        secondaryColorKeys[4] = new GradientColorKey(new Color(0.3f, 0.1f, 0.6f), 1f);

        preset.secondaryGradient = new Gradient();
        preset.secondaryGradient.SetKeys(secondaryColorKeys, alphaKeys);

        GradientColorKey[] accentColorKeys = new GradientColorKey[3];
        accentColorKeys[0] = new GradientColorKey(new Color(1f, 0.8f, 0.4f), 0f);     // Light Orange
        accentColorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0.7f), 0.5f);   // Light Pink
        accentColorKeys[2] = new GradientColorKey(new Color(0.8f, 0.4f, 1f), 1f);     // Light Purple

        preset.accentGradient = new Gradient();
        preset.accentGradient.SetKeys(accentColorKeys, alphaKeys);

        string path = "Assets/Presets/WarmSunset.asset";
        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created warm sunset preset at: {path}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = preset;
    }

    [MenuItem("QUARK/Create Cool Ocean Preset")]
    public static void CreateCoolOceanPreset()
    {
        VFXColorPreset preset = ScriptableObject.CreateInstance<VFXColorPreset>();
        preset.presetName = "Cool Ocean";
        preset.description = "Cool blues and greens like ocean depths";

        GradientColorKey[] primaryColorKeys = new GradientColorKey[5];
        primaryColorKeys[0] = new GradientColorKey(new Color(0f, 0.8f, 1f), 0f);      // Cyan
        primaryColorKeys[1] = new GradientColorKey(new Color(0f, 0.5f, 1f), 0.25f);   // Light Blue
        primaryColorKeys[2] = new GradientColorKey(new Color(0f, 0.3f, 0.8f), 0.5f);  // Blue
        primaryColorKeys[3] = new GradientColorKey(new Color(0f, 0.5f, 0.5f), 0.75f); // Teal
        primaryColorKeys[4] = new GradientColorKey(new Color(0f, 0.3f, 0.3f), 1f);    // Dark Teal

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        preset.primaryGradient = new Gradient();
        preset.primaryGradient.SetKeys(primaryColorKeys, alphaKeys);

        GradientColorKey[] secondaryColorKeys = new GradientColorKey[5];
        secondaryColorKeys[0] = new GradientColorKey(new Color(0.2f, 1f, 1f), 0f);
        secondaryColorKeys[1] = new GradientColorKey(new Color(0.2f, 0.7f, 1f), 0.25f);
        secondaryColorKeys[2] = new GradientColorKey(new Color(0.1f, 0.5f, 0.9f), 0.5f);
        secondaryColorKeys[3] = new GradientColorKey(new Color(0.2f, 0.7f, 0.7f), 0.75f);
        secondaryColorKeys[4] = new GradientColorKey(new Color(0.1f, 0.4f, 0.4f), 1f);

        preset.secondaryGradient = new Gradient();
        preset.secondaryGradient.SetKeys(secondaryColorKeys, alphaKeys);

        GradientColorKey[] accentColorKeys = new GradientColorKey[3];
        accentColorKeys[0] = new GradientColorKey(new Color(0.5f, 1f, 1f), 0f);       // Light Cyan
        accentColorKeys[1] = new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0.5f);   // Light Blue
        accentColorKeys[2] = new GradientColorKey(new Color(0.4f, 1f, 0.8f), 1f);     // Aqua

        preset.accentGradient = new Gradient();
        preset.accentGradient.SetKeys(accentColorKeys, alphaKeys);

        string path = "Assets/Presets/CoolOcean.asset";
        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created cool ocean preset at: {path}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = preset;
    }
}

