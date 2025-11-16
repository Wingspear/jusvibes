using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Music-reactive VFX controller that analyzes audio and drives VFX Graph parameters.
/// Handles frequency analysis, beat detection, and dynamic color gradients.
/// </summary>
public class AudioReactiveVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private MusicGenerator musicGenerator;

    [Header("Frequency Analysis")]
    [SerializeField] private int fftSize = 512;
    [SerializeField] private float bassBoost = 1.5f;
    [SerializeField] private float midBoost = 1.2f;
    [SerializeField] private float trebleBoost = 1.0f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float sensitivity = 50f;

    [Header("Beat Detection")]
    [SerializeField] private bool enableBeatDetection = true;
    [SerializeField] private float beatThreshold = 1.3f;
    [SerializeField] private float beatCooldown = 0.2f;
    [SerializeField] private float beatEnergyPulse = 200f;
    [SerializeField] private float beatPulseDuration = 0.1f;

    [Header("Volume Spike Detection")]
    [SerializeField] private bool enableVolumePulse = true;
    [SerializeField] private float volumeSpikeThreshold = 1.5f;
    [SerializeField] private float volumeSpikeCooldown = 0.15f;
    [SerializeField] private float radiusPulseAmount = 0.3f;
    [SerializeField] private float radiusPulseDuration = 0.4f;
    [SerializeField] private AnimationCurve radiusPulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Color System")]
    [SerializeField] private ColorGradientMode colorMode = ColorGradientMode.SpectrumCycle;
    [SerializeField] private VFXColorPreset colorPreset;
    [SerializeField] private float colorCycleSpeed = 1f;
    [SerializeField] private float colorSaturation = 0.8f;
    [SerializeField] private float colorBrightness = 1f;
    [SerializeField] private float colorHueOffset = 0.1f; // Offset between primary/secondary colors

    [Header("Parameter Mapping")]
    [SerializeField] private bool updateAudioBands = true;
    [SerializeField] private bool updateColors = true;
    [SerializeField] private bool modulateEnergy = true;
    [SerializeField] private bool modulateTurbulence = true;
    [SerializeField] private bool modulateRadius = true;
    [SerializeField] private float baseEnergy = 100f;
    [SerializeField] private float energyMultiplier = 2f;

    [Header("Parameter Ranges & Fallbacks")]
    [Tooltip("Fallback values when no audio is playing")]
    [SerializeField] private float fallbackBass = 0.1f;
    [SerializeField] private float fallbackMid = 0.1f;
    [SerializeField] private float fallbackTreble = 0.1f;
    [SerializeField] private float fallbackEnergy = 100f;
    [SerializeField] private float fallbackTurbulence = 1f;
    [SerializeField] private float fallbackRadius = 0.4f;
    [SerializeField] private float fallbackInnerRadius = 1.6f;
    [Tooltip("Output ranges for normalized values")]
    [SerializeField] private Vector2 bassRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 midRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 trebleRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 energyRange = new Vector2(50f, 400f);
    [SerializeField] private Vector2 turbulenceRange = new Vector2(0.5f, 5f);
    [SerializeField] private Vector2 radiusRange = new Vector2(0.2f, 1.5f);
    [SerializeField] private Vector2 innerRadiusRange = new Vector2(0.8f, 1.8f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Internal state
    private float[] spectrumData;
    private float bassValue, midValue, trebleValue;
    private float smoothBass, smoothMid, smoothTreble;
    private float bassHistory;
    private float bassAverage;
    private float lastBeatTime;
    private float beatPulseTimer;
    private Color currentPrimary, currentSecondary, currentAccent;
    private float colorTime;
    
    // Volume spike detection
    private float totalVolume;
    private float volumeHistory;
    private float volumeAverage;
    private float lastVolumeSpikeTime;
    private float radiusPulseTimer;
    private float baseRadius;
    private float baseInnerRadius;
    private AdaptiveOrbRadius adaptiveRadius;

    // VFX Parameter names
    private const string PARAM_AUDIO_BASS = "AudioBass";
    private const string PARAM_AUDIO_MID = "AudioMid";
    private const string PARAM_AUDIO_TREBLE = "AudioTreble";
    private const string PARAM_PRIMARY_COLOR = "PrimaryColor";
    private const string PARAM_SECONDARY_COLOR = "SecondaryColor";
    private const string PARAM_ACCENT_COLOR = "AccentColor";
    private const string PARAM_ENERGY = "Energy";
    private const string PARAM_TURBULENCE = "TurbulenceIntensity";
    private const string PARAM_RADIUS = "ParticleBoundary_radius";
    private const string PARAM_INNER_RADIUS = "ParticleInternal_radius";

    public enum ColorGradientMode
    {
        SpectrumCycle,      // Cycle through rainbow over track duration
        PresetGradient,     // Use VFXColorPreset asset
        FrequencyMapped     // Map frequency bands to colors
    }

    private void Start()
    {
        spectrumData = new float[fftSize];
        
        // Auto-wire references if not set
        if (vfx == null)
        {
            vfx = GetComponent<VisualEffect>();
        }

        if (musicGenerator == null)
        {
            musicGenerator = FindObjectOfType<MusicGenerator>();
        }

        if (audioSource == null && musicGenerator != null)
        {
            audioSource = musicGenerator.GetComponent<AudioSource>();
        }

        // Get AdaptiveOrbRadius component if it exists
        adaptiveRadius = GetComponent<AdaptiveOrbRadius>();

        if (audioSource == null)
        {
            Debug.LogWarning("[AudioReactiveVFX] No AudioSource found. Audio reactivity will not work.");
        }

        if (vfx == null)
        {
            Debug.LogWarning("[AudioReactiveVFX] No VisualEffect found. Cannot drive VFX parameters.");
        }

        // Initialize colors
        currentPrimary = Color.blue;
        currentSecondary = Color.red;
        currentAccent = Color.white;

        // Initialize base radii
        baseRadius = fallbackRadius;
        baseInnerRadius = fallbackInnerRadius;
        
        // Apply fallback values initially
        ApplyFallbackValues();
    }

    private void Update()
    {
        if (audioSource == null || !audioSource.isPlaying || audioSource.clip == null)
        {
            ApplyFallbackValues();
            return;
        }

        AnalyzeFrequencies();
        DetectBeats();
        DetectVolumeSpikes();
        UpdateColors();
        UpdateRadiusPulse();
        ApplyToVFX();
    }

    /// <summary>
    /// Analyze audio spectrum and extract frequency bands.
    /// </summary>
    private void AnalyzeFrequencies()
    {
        // Get spectrum data
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // Calculate total volume across all frequencies
        totalVolume = 0f;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            totalVolume += spectrumData[i];
        }
        totalVolume *= sensitivity;

        // Extract frequency bands
        // Bass: 0-250Hz (bins 0-10 at 22050Hz sample rate with 512 FFT)
        // Mid: 250-2000Hz (bins 11-80)
        // Treble: 2000-20000Hz (bins 81-511)
        
        bassValue = 0f;
        for (int i = 0; i < 11; i++)
        {
            bassValue += spectrumData[i];
        }
        bassValue *= bassBoost * sensitivity;

        midValue = 0f;
        for (int i = 11; i < 81; i++)
        {
            midValue += spectrumData[i];
        }
        midValue *= midBoost * sensitivity;

        trebleValue = 0f;
        for (int i = 81; i < spectrumData.Length; i++)
        {
            trebleValue += spectrumData[i];
        }
        trebleValue *= trebleBoost * sensitivity;

        // Smooth the values to avoid jitter
        smoothBass = Mathf.Lerp(smoothBass, bassValue, Time.deltaTime * smoothSpeed);
        smoothMid = Mathf.Lerp(smoothMid, midValue, Time.deltaTime * smoothSpeed);
        smoothTreble = Mathf.Lerp(smoothTreble, trebleValue, Time.deltaTime * smoothSpeed);

        // Clamp to 0-1 range
        smoothBass = Mathf.Clamp01(smoothBass);
        smoothMid = Mathf.Clamp01(smoothMid);
        smoothTreble = Mathf.Clamp01(smoothTreble);
        
        // Debug logging
        if (enableDebugLogs && Time.frameCount % 30 == 0) // Log every 30 frames
        {
            Debug.Log($"[Audio] Bass: {smoothBass:F3} | Mid: {smoothMid:F3} | Treble: {smoothTreble:F3}");
        }
    }

    /// <summary>
    /// Detect beats from bass frequency spikes.
    /// </summary>
    private void DetectBeats()
    {
        if (!enableBeatDetection)
        {
            beatPulseTimer = 0f;
            return;
        }

        // Track bass history for average calculation
        bassHistory = Mathf.Lerp(bassHistory, bassValue, Time.deltaTime * 2f);
        bassAverage = bassHistory;

        // Detect beat if bass exceeds threshold and cooldown has passed
        if (bassValue > bassAverage * beatThreshold && 
            Time.time - lastBeatTime > beatCooldown)
        {
            OnBeatDetected();
        }

        // Update pulse timer
        if (beatPulseTimer > 0f)
        {
            beatPulseTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Called when a beat is detected.
    /// </summary>
    private void OnBeatDetected()
    {
        lastBeatTime = Time.time;
        beatPulseTimer = beatPulseDuration;

        if (enableDebugLogs)
        {
            Debug.Log($"[AudioReactiveVFX] Beat detected! Bass: {bassValue:F2}");
        }
    }

    /// <summary>
    /// Detect volume spikes across all frequencies.
    /// </summary>
    private void DetectVolumeSpikes()
    {
        if (!enableVolumePulse)
        {
            radiusPulseTimer = 0f;
            return;
        }

        // Track volume history for average calculation
        volumeHistory = Mathf.Lerp(volumeHistory, totalVolume, Time.deltaTime * 2f);
        volumeAverage = volumeHistory;

        // Detect volume spike if total exceeds threshold and cooldown has passed
        if (totalVolume > volumeAverage * volumeSpikeThreshold && 
            Time.time - lastVolumeSpikeTime > volumeSpikeCooldown)
        {
            OnVolumeSpikeDetected();
        }
    }

    /// <summary>
    /// Called when a volume spike is detected.
    /// </summary>
    private void OnVolumeSpikeDetected()
    {
        lastVolumeSpikeTime = Time.time;
        radiusPulseTimer = radiusPulseDuration;

        if (enableDebugLogs)
        {
            Debug.Log($"[AudioReactiveVFX] Volume spike detected! Total: {totalVolume:F2}");
        }
    }

    /// <summary>
    /// Update radius pulse animation from volume spikes.
    /// </summary>
    private void UpdateRadiusPulse()
    {
        if (!modulateRadius || !enableVolumePulse)
        {
            baseRadius = fallbackRadius;
            baseInnerRadius = fallbackInnerRadius;
            return;
        }

        // Update pulse timer
        if (radiusPulseTimer > 0f)
        {
            radiusPulseTimer -= Time.deltaTime;
        }

        // Calculate base radius from AdaptiveOrbRadius if available
        if (adaptiveRadius != null)
        {
            baseRadius = adaptiveRadius.GetCurrentRadius();
        }
        else
        {
            baseRadius = fallbackRadius;
        }
        
        // Inner radius is always based on fallback (not spatially adaptive)
        baseInnerRadius = fallbackInnerRadius;
    }

    /// <summary>
    /// Update color gradients based on selected mode.
    /// </summary>
    private void UpdateColors()
    {
        if (!updateColors) return;

        switch (colorMode)
        {
            case ColorGradientMode.SpectrumCycle:
                UpdateSpectrumCycle();
                break;

            case ColorGradientMode.PresetGradient:
                UpdatePresetGradient();
                break;

            case ColorGradientMode.FrequencyMapped:
                UpdateFrequencyMappedColors();
                break;
        }
    }

    /// <summary>
    /// Cycle through rainbow spectrum based on track time.
    /// </summary>
    private void UpdateSpectrumCycle()
    {
        // Calculate normalized time through the track
        float normalizedTime = 0f;
        if (audioSource.clip != null && audioSource.clip.length > 0)
        {
            normalizedTime = (audioSource.time / audioSource.clip.length) * colorCycleSpeed;
        }
        else
        {
            // Fallback to continuous time-based cycling
            colorTime += Time.deltaTime * colorCycleSpeed * 0.1f;
            normalizedTime = colorTime % 1f;
        }

        // Create rainbow colors with HSV
        currentPrimary = Color.HSVToRGB((normalizedTime) % 1f, colorSaturation, colorBrightness);
        currentSecondary = Color.HSVToRGB((normalizedTime + colorHueOffset) % 1f, colorSaturation, colorBrightness);
        currentAccent = Color.HSVToRGB((normalizedTime + colorHueOffset * 2f) % 1f, colorSaturation, colorBrightness);
    }

    /// <summary>
    /// Sample colors from preset gradient.
    /// </summary>
    private void UpdatePresetGradient()
    {
        if (colorPreset == null)
        {
            UpdateSpectrumCycle(); // Fallback
            return;
        }

        float normalizedTime = 0f;
        if (audioSource.clip != null && audioSource.clip.length > 0)
        {
            normalizedTime = audioSource.time / audioSource.clip.length;
        }

        colorPreset.SampleColors(normalizedTime, out currentPrimary, out currentSecondary, out currentAccent);
    }

    /// <summary>
    /// Map frequency bands to colors (bass=red, mid=green, treble=blue).
    /// </summary>
    private void UpdateFrequencyMappedColors()
    {
        currentPrimary = new Color(smoothBass, smoothMid, smoothTreble, 1f);
        currentSecondary = new Color(smoothTreble, smoothBass, smoothMid, 1f);
        currentAccent = new Color(smoothMid, smoothTreble, smoothBass, 1f);
    }

    /// <summary>
    /// Apply all computed values to VFX Graph parameters.
    /// </summary>
    private void ApplyToVFX()
    {
        if (vfx == null) return;

        // Update frequency bands (normalized 0-1 mapped to custom ranges)
        if (updateAudioBands)
        {
            float mappedBass = Mathf.Lerp(bassRange.x, bassRange.y, smoothBass);
            float mappedMid = Mathf.Lerp(midRange.x, midRange.y, smoothMid);
            float mappedTreble = Mathf.Lerp(trebleRange.x, trebleRange.y, smoothTreble);
            
            vfx.SetFloat(PARAM_AUDIO_BASS, mappedBass);
            vfx.SetFloat(PARAM_AUDIO_MID, mappedMid);
            vfx.SetFloat(PARAM_AUDIO_TREBLE, mappedTreble);
        }

        // Update colors
        if (updateColors)
        {
            vfx.SetVector4(PARAM_PRIMARY_COLOR, currentPrimary);
            vfx.SetVector4(PARAM_SECONDARY_COLOR, currentSecondary);
            vfx.SetVector4(PARAM_ACCENT_COLOR, currentAccent);
        }

        // Modulate energy
        if (modulateEnergy)
        {
            float energyValue = baseEnergy;
            
            // Add beat pulse
            if (beatPulseTimer > 0f)
            {
                float pulseAmount = (beatPulseTimer / beatPulseDuration) * beatEnergyPulse;
                energyValue += pulseAmount;
            }
            
            // Add frequency-based modulation
            energyValue += (smoothBass * 0.8f + smoothMid * 0.3f) * energyMultiplier * 50f;
            
            // Clamp to energy range
            energyValue = Mathf.Clamp(energyValue, energyRange.x, energyRange.y);
            
            vfx.SetFloat(PARAM_ENERGY, energyValue);
        }

        // Modulate turbulence
        if (modulateTurbulence)
        {
            float turbulence = 1f + (smoothMid + smoothTreble) * 2f;
            // Map to turbulence range
            turbulence = Mathf.Clamp(turbulence, turbulenceRange.x, turbulenceRange.y);
            vfx.SetFloat(PARAM_TURBULENCE, turbulence);
        }

        // Modulate outer radius directly with bass frequency (smooth continuous response)
        if (modulateRadius)
        {
            // Map smoothBass (0-1) directly to radius range for immediate visual feedback
            float targetOuterRadius = Mathf.Lerp(radiusRange.x, radiusRange.y, smoothBass);
            
            // Keep inner radius static for now
            float targetInnerRadius = baseInnerRadius;
            
            // Clamp to safe ranges
            targetOuterRadius = Mathf.Clamp(targetOuterRadius, radiusRange.x, radiusRange.y);
            targetInnerRadius = Mathf.Clamp(targetInnerRadius, innerRadiusRange.x, innerRadiusRange.y);
            
            if (enableDebugLogs && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[Radius] Bass: {smoothBass:F3} -> Outer: {targetOuterRadius:F2}m (Range: {radiusRange.x}-{radiusRange.y})");
            }
            
            vfx.SetFloat(PARAM_RADIUS, targetOuterRadius);
            vfx.SetFloat(PARAM_INNER_RADIUS, targetInnerRadius);
        }
    }
    
    /// <summary>
    /// Apply fallback values when audio is not playing.
    /// </summary>
    private void ApplyFallbackValues()
    {
        if (vfx == null) return;

        if (updateAudioBands)
        {
            vfx.SetFloat(PARAM_AUDIO_BASS, fallbackBass);
            vfx.SetFloat(PARAM_AUDIO_MID, fallbackMid);
            vfx.SetFloat(PARAM_AUDIO_TREBLE, fallbackTreble);
        }

        if (modulateEnergy)
        {
            vfx.SetFloat(PARAM_ENERGY, fallbackEnergy);
        }

        if (modulateTurbulence)
        {
            vfx.SetFloat(PARAM_TURBULENCE, fallbackTurbulence);
        }

        if (modulateRadius)
        {
            vfx.SetFloat(PARAM_RADIUS, fallbackRadius);
            vfx.SetFloat(PARAM_INNER_RADIUS, fallbackInnerRadius);
        }
    }

    /// <summary>
    /// Get current frequency band values (for debugging or external use).
    /// </summary>
    public void GetFrequencyValues(out float bass, out float mid, out float treble)
    {
        bass = smoothBass;
        mid = smoothMid;
        treble = smoothTreble;
    }

    /// <summary>
    /// Check if a beat was recently detected.
    /// </summary>
    public bool IsBeatActive()
    {
        return beatPulseTimer > 0f;
    }

    private void OnValidate()
    {
        // Ensure valid FFT size (must be power of 2)
        if (fftSize != 64 && fftSize != 128 && fftSize != 256 && 
            fftSize != 512 && fftSize != 1024 && fftSize != 2048 && 
            fftSize != 4096 && fftSize != 8192)
        {
            fftSize = 512;
        }

        // Clamp values
        beatThreshold = Mathf.Max(1f, beatThreshold);
        beatCooldown = Mathf.Max(0.05f, beatCooldown);
        colorSaturation = Mathf.Clamp01(colorSaturation);
        colorBrightness = Mathf.Clamp(0.1f, 1f, colorBrightness);
    }
}

