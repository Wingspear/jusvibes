using System;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public enum QuarkState
{
    Spawned, Transition, Load, Reactive, Idle
}

public class Quark : MonoBehaviour
{
    public QuarkState state = QuarkState.Spawned;
    public AudioSource Audio => quarkAudio;
    public bool HasMusic => hasMusic;
    private bool hasMusic = false;
    private QuarkState storedState = QuarkState.Spawned;
    private bool isGrabbed = false;
    
    [SerializeField] private AudioSource quarkAudio;
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioSource dropAudio;

    private void Start()
    {
        GetComponent<Grabbable>().WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDestroy()
    {
        GetComponent<Grabbable>().WhenPointerEventRaised -= OnPointerEvent;
    }

    public void InjectColors(Color primary, Color secondary)
    {
        visualEffect.SetVector4("PrimaryColor", primary);
        visualEffect.SetVector4("SecondaryColor", secondary);
    }
    
    private void OnPointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                if (!isGrabbed)
                {
                    isGrabbed = true;
                    OnGrabBegin(evt);
                }
                break;

            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                if (isGrabbed)
                {
                    isGrabbed = false;
                    OnGrabEnd(evt);
                }
                break;
        }
    }
    private void OnGrabBegin(PointerEvent evt)
    {
        pickupAudio.Play();
        if (!hasMusic)
        {
            QuarkManager.Instance.OnQuarkGrabbed(this, !HasMusic);
        }
        storedState = state;
        SetState(QuarkState.Transition);
    }

    private async void OnGrabEnd(PointerEvent evt)
    {
        dropAudio.Play();
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        transform.SetParent(null);
        transform.position = pos;
        transform.rotation = rot;
        
        if (!hasMusic)
        {
            hasMusic = true;
            SetState(QuarkState.Load);
            await QuarkManager.Instance.GenerateMusicForQuark(this);
            SetState(QuarkState.Reactive);
        }
        else
        {
            state = storedState;
        }
    }

    public void SetState(QuarkState inputState)
    {
        state = inputState;
        visualEffect.SetInt("QuarkState", (int)state);
    }
}
