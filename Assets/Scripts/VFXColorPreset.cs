using UnityEngine;

/// <summary>
/// ScriptableObject for storing VFX color gradient presets.
/// Can be used to define color schemes that cycle through during music playback.
/// </summary>
[CreateAssetMenu(fileName = "VFXColorPreset", menuName = "QUARK/VFX Color Preset")]
public class VFXColorPreset : ScriptableObject
{
    [Header("Preset Info")]
    public string presetName = "New Preset";
    
    [TextArea(3, 6)]
    public string description = "Color gradient preset for music-reactive VFX";

    [Header("Color Gradients")]
    [Tooltip("Primary particle color gradient over time")]
    public Gradient primaryGradient = new Gradient();
    
    [Tooltip("Secondary particle color gradient over time")]
    public Gradient secondaryGradient = new Gradient();
    
    [Tooltip("Accent particle color gradient over time")]
    public Gradient accentGradient = new Gradient();

    /// <summary>
    /// Sample colors from all gradients at a normalized time (0-1).
    /// </summary>
    public void SampleColors(float normalizedTime, out Color primary, out Color secondary, out Color accent)
    {
        primary = primaryGradient.Evaluate(normalizedTime);
        secondary = secondaryGradient.Evaluate(normalizedTime);
        accent = accentGradient.Evaluate(normalizedTime);
    }

    private void OnValidate()
    {
        // Ensure normalized time wraps correctly
        if (primaryGradient == null)
        {
            primaryGradient = new Gradient();
        }
        if (secondaryGradient == null)
        {
            secondaryGradient = new Gradient();
        }
        if (accentGradient == null)
        {
            accentGradient = new Gradient();
        }
    }
}

