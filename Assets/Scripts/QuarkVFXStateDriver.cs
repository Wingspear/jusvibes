using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Drives high-level VFX parameters (radius, spawn rate, etc.)
/// based on the current QuarkState. The VFX Graph stays "dumb":
/// it just consumes these floats and the QuarkState int.
/// </summary>
public class QuarkVFXStateDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Quark quark;
    [SerializeField] private VisualEffect vfx;

    [Header("VFX Property Names")]
    [SerializeField] private string radiusProperty = "Radius";
    [SerializeField] private string spawnRateProperty = "SpawnRate";
    [SerializeField] private string particleSizeProperty = "ParticleSize";
    [SerializeField] private string turbulenceProperty = "TurbulenceStrength";
    [SerializeField] private string vortexProperty = "VortexStrength";

    [Header("State Profiles")]
    [SerializeField] private QuarkVFXProfile spawned = new QuarkVFXProfile
    {
        radius = 0.25f,
        spawnRate = 250f,
        particleSize = 0.5f,
        turbulence = 0.8f,
        vortex = 0.1f
    };

    [SerializeField] private QuarkVFXProfile transition = new QuarkVFXProfile
    {
        radius = 0.15f,
        spawnRate = 200f,
        particleSize = 0.4f,
        turbulence = 0.5f,
        vortex = 0.2f
    };

    [SerializeField] private QuarkVFXProfile load = new QuarkVFXProfile
    {
        radius = 0.4f,
        spawnRate = 500f,
        particleSize = 0.8f,
        turbulence = 1.8f,
        vortex = 1.0f
    };

    [SerializeField] private QuarkVFXProfile reactive = new QuarkVFXProfile
    {
        radius = 0.6f,
        spawnRate = 900f,
        particleSize = 1.0f,
        turbulence = 2.0f,
        vortex = 0.3f
    };

    [SerializeField] private QuarkVFXProfile idle = new QuarkVFXProfile
    {
        radius = 0.7f,
        spawnRate = 350f,
        particleSize = 0.9f,
        turbulence = 0.9f,
        vortex = 0.2f
    };

    [Header("Transition Settings")]
    [SerializeField] private float transitionTime = 0.4f;

    private QuarkState lastState;
    private QuarkVFXProfile currentProfile;
    private QuarkVFXProfile targetProfile;
    private float transitionProgress;

    private void Awake()
    {
        if (quark == null)
        {
            quark = GetComponentInParent<Quark>();
        }

        if (vfx == null)
        {
            vfx = GetComponentInChildren<VisualEffect>();
        }

        if (quark != null)
        {
            lastState = quark.state;
            currentProfile = targetProfile = GetProfile(lastState);
            ApplyProfile(currentProfile);
        }
    }

    private void Update()
    {
        if (quark == null || vfx == null)
        {
            return;
        }

        if (quark.state != lastState)
        {
            lastState = quark.state;
            targetProfile = GetProfile(lastState);
            transitionProgress = 0f;
        }

        if (transitionTime <= 0f)
        {
            currentProfile = targetProfile;
            ApplyProfile(currentProfile);
            return;
        }

        if (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionTime;
            float t = Mathf.Clamp01(transitionProgress);
            float s = Mathf.SmoothStep(0f, 1f, t);

            currentProfile.radius = Mathf.Lerp(currentProfile.radius, targetProfile.radius, s);
            currentProfile.spawnRate = Mathf.Lerp(currentProfile.spawnRate, targetProfile.spawnRate, s);
            currentProfile.particleSize = Mathf.Lerp(currentProfile.particleSize, targetProfile.particleSize, s);
            currentProfile.turbulence = Mathf.Lerp(currentProfile.turbulence, targetProfile.turbulence, s);
            currentProfile.vortex = Mathf.Lerp(currentProfile.vortex, targetProfile.vortex, s);

            ApplyProfile(currentProfile);
        }
    }

    private QuarkVFXProfile GetProfile(QuarkState state)
    {
        switch (state)
        {
            case QuarkState.Spawned:
                return spawned;
            case QuarkState.Transition:
                return transition;
            case QuarkState.Load:
                return load;
            case QuarkState.Reactive:
                return reactive;
            case QuarkState.Idle:
                return idle;
            default:
                return idle;
        }
    }

    private void ApplyProfile(QuarkVFXProfile profile)
    {
        if (vfx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(radiusProperty))
        {
            vfx.SetFloat(radiusProperty, profile.radius);
        }

        if (!string.IsNullOrEmpty(spawnRateProperty))
        {
            vfx.SetFloat(spawnRateProperty, profile.spawnRate);
        }

        if (!string.IsNullOrEmpty(particleSizeProperty))
        {
            vfx.SetFloat(particleSizeProperty, profile.particleSize);
        }

        if (!string.IsNullOrEmpty(turbulenceProperty))
        {
            vfx.SetFloat(turbulenceProperty, profile.turbulence);
        }

        if (!string.IsNullOrEmpty(vortexProperty))
        {
            vfx.SetFloat(vortexProperty, profile.vortex);
        }
    }
}

[System.Serializable]
public struct QuarkVFXProfile
{
    public float radius;
    public float spawnRate;
    public float particleSize;
    public float turbulence;
    public float vortex;
}

