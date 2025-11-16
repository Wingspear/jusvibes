using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Drives the QuarkGlow shader based on audio bands from AudioReactiveVFX,
/// the current VFX Graph colors, and an external hover signal.
/// </summary>
public class QuarkGlowShaderDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioReactiveVFX audioReactive;
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader Property Names")]
    [SerializeField] private string primaryColorProperty = "PrimaryColor";
    [SerializeField] private string secondaryColorProperty = "SecondaryColor";
    [SerializeField] private string accentColorProperty = "AccentColor";
    [SerializeField] private string glowProperty = "_GlowPulse";
    [SerializeField] private string trebleProperty = "_TrebleAmount";
    [SerializeField] private string hoverProperty = "_Hover";

    [Header("VFX Color Property Names")]
    [SerializeField] private string vfxPrimaryColorProperty = "PrimaryColor";
    [SerializeField] private string vfxSecondaryColorProperty = "SecondaryColor";
    [SerializeField] private string vfxAccentColorProperty = "AccentColor";

    [Header("Tuning")]
    [SerializeField] private float glowSmoothing = 10f;
    [SerializeField] private float trebleSmoothing = 10f;
    [SerializeField] private float hoverSmoothing = 10f;
    [SerializeField] private float maxGlowFromBass = 1f;
    [SerializeField] private float maxTrebleInfluence = 1f;

    private float smoothedGlow;
    private float smoothedTreble;
    private float smoothedHover;
    private float targetHover;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (vfx == null)
        {
            vfx = GetComponentInParent<VisualEffect>();
        }
    }

    private void Update()
    {
        if (targetRenderer == null)
        {
            return;
        }

        var mat = targetRenderer.material;

        // Sync colors from VFX Graph (which are already driven by AudioReactiveVFX)
        if (vfx != null)
        {
            if (!string.IsNullOrEmpty(vfxPrimaryColorProperty) && !string.IsNullOrEmpty(primaryColorProperty))
            {
                var c = (Color)vfx.GetVector4(vfxPrimaryColorProperty);
                mat.SetColor(primaryColorProperty, c);
            }

            if (!string.IsNullOrEmpty(vfxSecondaryColorProperty) && !string.IsNullOrEmpty(secondaryColorProperty))
            {
                var c = (Color)vfx.GetVector4(vfxSecondaryColorProperty);
                mat.SetColor(secondaryColorProperty, c);
            }

            if (!string.IsNullOrEmpty(vfxAccentColorProperty) && !string.IsNullOrEmpty(accentColorProperty))
            {
                var c = (Color)vfx.GetVector4(vfxAccentColorProperty);
                mat.SetColor(accentColorProperty, c);
            }
        }

        // Audio-driven parameters (bass / treble)
        if (audioReactive != null)
        {
            audioReactive.GetFrequencyValues(out var bass, out _, out var treble);

            float targetGlow = Mathf.Clamp01(bass) * maxGlowFromBass;
            float targetTreble = Mathf.Clamp01(treble) * maxTrebleInfluence;

            smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, Time.deltaTime * glowSmoothing);
            smoothedTreble = Mathf.Lerp(smoothedTreble, targetTreble, Time.deltaTime * trebleSmoothing);

            if (!string.IsNullOrEmpty(glowProperty))
            {
                mat.SetFloat(glowProperty, smoothedGlow);
            }

            if (!string.IsNullOrEmpty(trebleProperty))
            {
                mat.SetFloat(trebleProperty, smoothedTreble);
            }
        }

        // Hover-driven brightness bump
        smoothedHover = Mathf.Lerp(smoothedHover, targetHover, Time.deltaTime * hoverSmoothing);
        if (!string.IsNullOrEmpty(hoverProperty))
        {
            mat.SetFloat(hoverProperty, smoothedHover);
        }
    }

    /// <summary>
    /// Call this from your hover logic (e.g. pointer enter/exit)
    /// to control how bright the hover glow is.
    /// </summary>
    public void SetHover(bool isHovering)
    {
        targetHover = isHovering ? 1f : 0f;
    }
}
