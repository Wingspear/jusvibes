# Audio-Reactive VFX - Parameter Ranges & Volume Pulse Guide

## ✅ NEW FEATURES ADDED

### 🔊 Volume Spike Detection with Smooth Radius Pulsing
The orb now pulses its radius smoothly when volume spikes are detected across all frequencies!

---

## 📊 Parameter Ranges & Value Mapping

### **Frequency Bands (Normalized 0-1)**

Audio analysis produces values from 0-1, which are then mapped to custom output ranges:

| Parameter | Input (Normalized) | Default Output Range | VFX Parameter |
|-----------|-------------------|----------------------|---------------|
| **Bass** | 0.0 - 1.0 | `0.0 - 1.0` | `AudioBass` |
| **Mid** | 0.0 - 1.0 | `0.0 - 1.0` | `AudioMid` |
| **Treble** | 0.0 - 1.0 | `0.0 - 1.0` | `AudioTreble` |
| **Energy** | Calculated | `50 - 400` | `Energy` |
| **Turbulence** | Calculated | `0.5 - 5.0` | `TurbulenceIntensity` |
| **Radius** | From AdaptiveOrbRadius | `0.2 - 1.5` meters | `ParticleBoundary_radius` |

### **Customizable Ranges (Inspector)**

All ranges can be adjusted in the Inspector under **"Parameter Ranges & Fallbacks"**:

```csharp
// Frequency band output ranges
bassRange: (0, 1)         // Min/Max for bass output
midRange: (0, 1)          // Min/Max for mid output  
trebleRange: (0, 1)       // Min/Max for treble output

// Calculated parameter ranges
energyRange: (50, 400)    // Particle spawn rate limits
turbulenceRange: (0.5, 5) // Turbulence intensity limits
radiusRange: (0.2, 1.5)   // Radius size limits in meters
```

---

## 🎯 Fallback Values (When No Audio)

When music isn't playing, these values are applied to keep the orb visible and stable:

| Parameter | Fallback Value | Purpose |
|-----------|---------------|---------|
| `fallbackBass` | `0.1` | Minimal bass presence |
| `fallbackMid` | `0.1` | Minimal mid presence |
| `fallbackTreble` | `0.1` | Minimal treble presence |
| `fallbackEnergy` | `100` | Moderate particle spawn rate |
| `fallbackTurbulence` | `1.0` | Baseline movement |
| `fallbackRadius` | `0.4m` | Comfortable default size |

**Why fallbacks?** Without audio, the orb would collapse to zero energy and become invisible. Fallbacks ensure it remains visible and gently animated.

---

## 🔊 Volume Spike Detection

### **How It Works**

1. **Calculate Total Volume**: Sum of all FFT frequency bins
2. **Track History**: Running average of volume over time
3. **Detect Spikes**: When `totalVolume > averageVolume × threshold`
4. **Trigger Pulse**: Start smooth radius animation
5. **Cooldown**: Prevent rapid re-triggers

### **Configuration**

```csharp
[Header("Volume Spike Detection")]
enableVolumePulse: true              // Toggle feature on/off
volumeSpikeThreshold: 1.5            // How much above average triggers (1.5× = 50% louder)
volumeSpikeCooldown: 0.15s           // Minimum time between pulses
radiusPulseAmount: 0.3m              // How much radius expands (meters)
radiusPulseDuration: 0.4s            // How long pulse lasts
radiusPulseCurve: AnimationCurve     // Smooth ease-in-out curve
```

### **Pulse Animation**

The radius pulse uses an **AnimationCurve** for smooth expansion/contraction:

```
Default Curve: EaseInOut (0→1→0)
Duration: 0.4 seconds

Radius progression:
  t=0.0s:  baseRadius + 0.0m         (start)
  t=0.1s:  baseRadius + 0.15m        (expanding)
  t=0.2s:  baseRadius + 0.3m         (peak)
  t=0.3s:  baseRadius + 0.15m        (contracting)
  t=0.4s:  baseRadius + 0.0m         (back to base)
```

The curve is **editable in Inspector** - you can customize the pulse shape!

---

## 🎛️ How Parameters Flow

### **1. Audio Input → Analysis**
```
AudioSource.clip playing
    ↓
FFT Analysis (512 samples)
    ↓
Extract: Bass (0-250Hz), Mid (250-2kHz), Treble (2k-20kHz), TotalVolume
    ↓
Normalize to 0-1 range
```

### **2. Detection Systems**
```
Beat Detection: Bass spikes → Energy pulse
Volume Detection: Total volume spikes → Radius pulse
```

### **3. Mapping & Output**
```
Normalized values (0-1)
    ↓
Map to custom ranges
    ↓
Apply pulses/modulation
    ↓
Clamp to safe limits
    ↓
Set VFX parameters
```

---

## 🔧 Tuning Guide

### **If radius pulses are too subtle:**
- Increase `radiusPulseAmount` (try 0.5 or 0.7)
- Decrease `volumeSpikeThreshold` (try 1.2 for more sensitivity)
- Increase `radiusPulseDuration` (try 0.6 for longer pulses)

### **If radius pulses too frequently:**
- Increase `volumeSpikeCooldown` (try 0.3s)
- Increase `volumeSpikeThreshold` (try 2.0 for only big spikes)

### **If radius doesn't pulse at all:**
- Check `modulateRadius` is enabled
- Check `enableVolumePulse` is enabled
- Verify music is playing and has volume spikes
- Enable `enableDebugLogs` to see spike detections in Console

### **If frequency bands seem wrong:**
- Adjust `sensitivity` (default 50, try 30-100)
- Adjust individual `bassBoost`, `midBoost`, `trebleBoost`
- Check `smoothSpeed` (higher = less jittery, lower = more responsive)

---

## 🎨 Recommended Presets

### **Subtle Reactive (Chill Music)**
```
volumeSpikeThreshold: 1.8
radiusPulseAmount: 0.2
radiusPulseDuration: 0.6
sensitivity: 40
```

### **High Energy (Electronic/Dance)**
```
volumeSpikeThreshold: 1.3
radiusPulseAmount: 0.4
radiusPulseDuration: 0.3
sensitivity: 60
bassBoost: 2.0
```

### **Cinematic (Dramatic Music)**
```
volumeSpikeThreshold: 1.5
radiusPulseAmount: 0.5
radiusPulseDuration: 0.8
radiusPulseCurve: Custom (slow rise, fast fall)
sensitivity: 45
```

---

## 🔗 Integration with AdaptiveOrbRadius

The two systems work **together seamlessly**:

1. **AdaptiveOrbRadius** calculates safe radius based on space (0.3 - 1.5m)
2. **AudioReactiveVFX** reads that radius as `baseRadius`
3. **Volume pulses add** to the base radius temporarily
4. **Total radius** = `baseRadius + pulseAmount × curveValue`
5. **Final clamp** to `radiusRange` ensures safe limits

**Example flow:**
```
Room has 0.8m clearance
    ↓
AdaptiveOrbRadius sets radius = 0.8m
    ↓
AudioReactiveVFX reads baseRadius = 0.8m
    ↓
Volume spike detected!
    ↓
Pulse adds 0.3m temporarily
    ↓
Orb expands to 1.1m for 0.4 seconds
    ↓
Smoothly returns to 0.8m
```

---

## 📈 Value Examples in Practice

### **Quiet Section (Low Energy)**
```
Bass: 0.05 → mapped to 0.05
Mid: 0.08 → mapped to 0.08
Treble: 0.03 → mapped to 0.03
Energy: 50 (base) + 5 (freq mod) = 55
Turbulence: 1.0 + 0.11 = 1.11
Radius: 0.8m (no pulse)
```

### **Beat Drop (High Energy + Volume Spike)**
```
Bass: 0.85 → mapped to 0.85
Mid: 0.65 → mapped to 0.65
Treble: 0.40 → mapped to 0.40
Energy: 50 (base) + 200 (beat) + 127 (freq mod) = 377
Turbulence: 1.0 + 2.1 = 3.1
Radius: 0.8m + 0.3m (pulse) = 1.1m
```

### **No Audio (Fallback)**
```
Bass: 0.1 (fallback)
Mid: 0.1 (fallback)
Treble: 0.1 (fallback)
Energy: 100 (fallback)
Turbulence: 1.0 (fallback)
Radius: 0.4m (fallback)
```

---

## 🎯 Summary

**Volume spike pulsing** adds dynamic, breath-like expansion to the orb that responds to the overall loudness of the music, while **frequency bands** provide fine-grained reactivity to bass/mid/treble content. Together with **beat detection** and **adaptive sizing**, the orb becomes a living, breathing visual representation of the music! 🎵✨

