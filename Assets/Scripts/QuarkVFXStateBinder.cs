using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Bridges the Quark gameplay state to the VFX Graph by
/// writing the current QuarkState enum value into an int
/// property on the associated VisualEffect.
/// </summary>
public class QuarkVFXStateBinder : MonoBehaviour
{
    [SerializeField] private Quark quark;
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private string quarkStatePropertyName = "QuarkState";

    private QuarkState lastState;

    private void Awake()
    {
        if (quark == null)
        {
            quark = GetComponent<Quark>();
        }

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
        }
    }

    private void OnEnable()
    {
        PushStateToVfx();
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
            PushStateToVfx();
        }
    }

    private void PushStateToVfx()
    {
        if (vfx == null)
        {
            return;
        }

        vfx.SetInt(quarkStatePropertyName, (int)lastState);
    }
}
