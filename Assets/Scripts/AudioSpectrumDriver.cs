using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Extremely simple audio spectrum driver.
/// - Reads AudioSource spectrum each frame
/// - Outputs normalized bass/mid/treble and overall volume
/// - Writes directly to VFX Graph exposed properties:
///   AudioBass, AudioMid, AudioTreble, Energy
/// </summary>
public class AudioSpectrumDriver : MonoBehaviour
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private VisualEffect vfx;
	[SerializeField] private int fftSize = 1024;
	[SerializeField] private float smooth = 10f; // higher = smoother

	private float[] spectrum;
	private float bass, mid, treble, volume;

	// VFX exposed property names (must match VFX Graph)
	private const string PROP_BASS = "AudioBass";
	private const string PROP_MID = "AudioMid";
	private const string PROP_TREBLE = "AudioTreble";
	private const string PROP_ENERGY = "Energy";

	private void Awake()
	{
		if (vfx == null)
		{
			vfx = GetComponent<VisualEffect>();
		}
	}

	private void Update()
	{
		if (spectrum == null || spectrum.Length != fftSize)
		{
			spectrum = new float[fftSize];
		}

		// If no audio or not playing - output zeros
		if (audioSource == null || audioSource.clip == null || (!audioSource.isPlaying && audioSource.time <= 0f))
		{
			WriteToVfx(0f, 0f, 0f, 0f);
			return;
		}

		audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

		// Frequency band indices based on current output sample rate
		float nyquist = AudioSettings.outputSampleRate * 0.5f;
		float hzPerBin = nyquist / fftSize;

		int iBassMax = Mathf.Clamp(Mathf.FloorToInt(250f / hzPerBin), 1, fftSize - 1);
		int iMidMin = iBassMax + 1;
		int iMidMax = Mathf.Clamp(Mathf.FloorToInt(2000f / hzPerBin), iMidMin + 1, fftSize - 1);
		int iTrebleMin = iMidMax + 1;

		float rawBass = Sum(spectrum, 0, iBassMax);
		float rawMid = Sum(spectrum, iMidMin, iMidMax);
		float rawTreble = Sum(spectrum, iTrebleMin, fftSize - 1);
		float rawVol = rawBass + rawMid + rawTreble;

		// Simple scaling and smoothing to 0..1
		// Scale chosen empirically for typical FFT magnitudes
		const float scale = 40f;
		float dt = Mathf.Clamp01(Time.deltaTime * smooth);

		bass = Mathf.Lerp(bass, Mathf.Clamp01(rawBass * scale), dt);
		mid = Mathf.Lerp(mid, Mathf.Clamp01(rawMid * scale), dt);
		treble = Mathf.Lerp(treble, Mathf.Clamp01(rawTreble * scale), dt);
		volume = Mathf.Lerp(volume, Mathf.Clamp01(rawVol * scale * 0.5f), dt);

		WriteToVfx(bass, mid, treble, volume);
	}

	private static float Sum(float[] data, int start, int endInclusive)
	{
		float s = 0f;
		for (int i = start; i <= endInclusive && i < data.Length; i++)
		{
			s += data[i];
		}
		return s;
	}

	private void WriteToVfx(float b, float m, float t, float vol)
	{
		if (vfx == null) return;
		vfx.SetFloat(PROP_BASS, b);
		vfx.SetFloat(PROP_MID, m);
		vfx.SetFloat(PROP_TREBLE, t);
		vfx.SetFloat(PROP_ENERGY, Mathf.Lerp(80f, 350f, vol)); // map to Energy range
	}
}


