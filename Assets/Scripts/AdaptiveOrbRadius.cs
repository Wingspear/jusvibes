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
    [SerializeField] private float maxRadius = 0.6f;
    [SerializeField] private float minRadius = 0.15f;
    [SerializeField] private float padding = 0.1f;

    [Header("Animation")]
    [SerializeField] private float adjustSpeed = 5f;
    [SerializeField] private float updateInterval = 0.5f; // Continuous updates every 0.5s

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

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
        // Check if it's time to recalculate
        if (Time.time - lastUpdateTime >= updateInterval)
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
    /// Uses Physics.OverlapSphere to test progressively smaller radii.
    /// </summary>
    /// <param name="position">Center position to test from</param>
    /// <returns>The largest safe radius, clamped between minRadius and maxRadius</returns>
    private float ComputeAvailableRadius(Vector3 position)
    {
        float testRadius = maxRadius;
        float step = 0.05f; // 5cm steps for reasonable precision

        // Test from max down to min, finding first radius that fits
        while (testRadius >= minRadius)
        {
            // Check if a sphere of this radius (minus padding) would overlap any obstacles
            Collider[] overlaps = Physics.OverlapSphere(position, testRadius - padding, obstacleMask);
            
            if (overlaps.Length == 0)
            {
                // Found a radius that fits!
                return Mathf.Clamp(testRadius, minRadius, maxRadius);
            }
            
            testRadius -= step;
        }

        // Fallback to minimum radius if nothing fits
        return minRadius;
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

