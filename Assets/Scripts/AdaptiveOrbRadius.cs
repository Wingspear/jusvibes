using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Automatically adjusts the VFX Graph orb radius based on available space around the orb.
/// Uses Physics.OverlapSphere to detect obstacles and smoothly transitions to the largest safe radius.
/// </summary>
public class AdaptiveOrbRadius : MonoBehaviour
{
    [Header("VFX Configuration")]
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private string radiusPropertyName = "ParticleBoundary_radius";

    [Header("Space Detection")]
    [SerializeField] private LayerMask obstacleMask = -1; // Everything by default
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private float minRadius = 0.3f;
    [SerializeField] private float padding = 0.05f;
    [SerializeField] private bool ignoreSelfColliders = true;

    [Header("Animation")]
    [SerializeField] private float adjustSpeed = 5f;
    [SerializeField] private bool enableContinuousUpdates = true;
    [SerializeField] private float updateInterval = 2.0f; // Continuous updates interval

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool enableDebugLogs = true;

    private float currentRadius;
    private float targetRadius;
    private float lastUpdateTime;

    private void Start()
    {
        // Initialize with max radius
        currentRadius = maxRadius;
        
        // Auto-assign VFX if not set
        if (vfx == null)
        {
            vfx = GetComponent<VisualEffect>();
        }

        // Perform initial radius calculation
        RecalculateRadius();
        
        // Set initial radius immediately
        if (vfx != null)
        {
            vfx.SetFloat(radiusPropertyName, currentRadius);
        }
        
        lastUpdateTime = Time.time;
    }

    private void Update()
    {
        // Check if it's time to recalculate (only if continuous updates enabled)
        if (enableContinuousUpdates && Time.time - lastUpdateTime >= updateInterval)
        {
            RecalculateRadius();
            lastUpdateTime = Time.time;
        }

        // Smooth transition from current to target radius
        if (Mathf.Abs(currentRadius - targetRadius) > 0.001f)
        {
            currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * adjustSpeed);
            
            // Apply to VFX
            if (vfx != null)
            {
                vfx.SetFloat(radiusPropertyName, currentRadius);
            }
        }
    }

    /// <summary>
    /// Public API to force immediate radius recalculation.
    /// Call this after moving or repositioning the orb.
    /// </summary>
    public void RecalculateRadius()
    {
        targetRadius = ComputeAvailableRadius(transform.position);
    }

    /// <summary>
    /// Computes the largest safe radius that doesn't intersect obstacles.
    /// Uses binary search with Physics.OverlapSphere for efficient detection.
    /// </summary>
    /// <param name="position">Center position to test from</param>
    /// <returns>The largest safe radius, clamped between minRadius and maxRadius</returns>
    private float ComputeAvailableRadius(Vector3 position)
    {
        // Quick check: if max radius fits, return immediately (best case: 1 check)
        if (HasClearance(position, maxRadius - padding))
        {
            if (enableDebugLogs)
                Debug.Log($"[AdaptiveOrb] Max radius {maxRadius}m fits! Using max.");
            return maxRadius;
        }
        
        // Quick check: if min radius doesn't fit, return min (2 checks total)
        if (!HasClearance(position, minRadius - padding))
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[AdaptiveOrb] Even min radius {minRadius}m has obstacles. Using min anyway.");
            return minRadius;
        }
        
        // Binary search for largest radius that fits (typically 6-8 checks)
        float low = minRadius;
        float high = maxRadius;
        float bestRadius = minRadius;
        int maxIterations = 8; // Limits to ~8 checks for good precision
        
        for (int i = 0; i < maxIterations; i++)
        {
            float mid = (low + high) / 2f;
            
            if (HasClearance(position, mid - padding))
            {
                // This radius fits, try larger
                bestRadius = mid;
                low = mid;
            }
            else
            {
                // Collision detected, try smaller
                high = mid;
            }
            
            // Stop if range is small enough (within 5cm precision)
            if (high - low < 0.05f)
                break;
        }
        
        if (enableDebugLogs)
            Debug.Log($"[AdaptiveOrb] Found safe radius: {bestRadius:F2}m (range: {minRadius}-{maxRadius}m)");
        
        return bestRadius;
    }
    
    /// <summary>
    /// Checks if a sphere at the given position and radius has clearance from obstacles.
    /// </summary>
    private bool HasClearance(Vector3 position, float radius)
    {
        Collider[] overlaps = Physics.OverlapSphere(position, radius, obstacleMask);
        
        // Filter out self colliders if enabled
        if (ignoreSelfColliders && overlaps.Length > 0)
        {
            foreach (var collider in overlaps)
            {
                // If this collider is not part of our GameObject hierarchy, it's an obstacle
                if (!collider.transform.IsChildOf(transform) && collider.transform != transform)
                {
                    if (enableDebugLogs)
                        Debug.Log($"[AdaptiveOrb] Obstacle detected at radius {radius:F2}m: {collider.gameObject.name}");
                    return false;
                }
            }
            // All colliders were self colliders, so we have clearance
            return true;
        }
        
        return overlaps.Length == 0;
    }

    /// <summary>
    /// Gets the current radius being applied to the VFX.
    /// </summary>
    public float GetCurrentRadius()
    {
        return currentRadius;
    }

    /// <summary>
    /// Gets the target radius (largest safe radius based on last calculation).
    /// </summary>
    public float GetTargetRadius()
    {
        return targetRadius;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw current radius in green
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, currentRadius);

        // Draw target radius in yellow (if different)
        if (Mathf.Abs(currentRadius - targetRadius) > 0.001f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, targetRadius);
        }

        // Draw max radius in blue (faded)
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);

        // Draw min radius in red (faded)
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, minRadius);
    }

    private void OnValidate()
    {
        // Ensure min is never greater than max
        if (minRadius > maxRadius)
        {
            minRadius = maxRadius;
        }

        // Ensure padding is reasonable
        if (padding < 0)
        {
            padding = 0;
        }

        // Ensure update interval is positive
        if (updateInterval <= 0)
        {
            updateInterval = 0.1f;
        }
    }
}

