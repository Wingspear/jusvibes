using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class QuarkManager : Singleton<QuarkManager>
{
    [SerializeField] private RoomScanner roomScanner;
    [SerializeField] private Quark quarkPrefab;
    [SerializeField] private Transform quarkSpawnParent;
    [SerializeField] private OVRSkeleton leftSkeleton;
    [SerializeField] private Transform _head; // main camera
    
    private Quark spawnedQuark = null;

    private bool lastPalmFacingFace = false; // to prevent log spam

    public void SpawnQuark(Vector3 pos, Quaternion rot, Transform parent)
    {
        spawnedQuark = Instantiate(quarkPrefab, pos, rot, parent);
        spawnedQuark.gameObject.SetActive(false);

        Debug.Log($"[QuarkManager] Spawned new Quark at {pos} (inactive).");
    }

    public async Task GenerateMusicForQuark(Quark quark)
    {
        Debug.Log("[QuarkManager] Generating music for Quark...");
        await roomScanner.ScanAndPlayMusic(quark.Audio);
    }

    public void OnQuarkGrabbed(Quark quark, bool isFirstGrab)
    {
        Debug.Log($"[QuarkManager] Quark grabbed. First grab: {isFirstGrab}");

        if (isFirstGrab)
        {
            spawnedQuark = null;
            StartCoroutine(SpawnNewQuark());
        }
    }

    IEnumerator SpawnNewQuark(float delay = 3f)
    {
        Debug.Log($"[QuarkManager] Waiting {delay}s before spawning a new Quark...");
        yield return new WaitForSeconds(delay);

        SpawnQuark(Vector3.zero, Quaternion.identity, quarkSpawnParent);
    }

    private void Update()
    {
        if (leftSkeleton == null)
        {
            Debug.LogWarning("[QuarkManager] Left skeleton reference missing!");
            return;
        }

        if (!leftSkeleton.IsInitialized)
        {
            Debug.LogWarning("[QuarkManager] Left skeleton not initialized yet.");
            return;
        }

        // Get the wrist joint (palm reference)
        var wrist = leftSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot];
        if (wrist == null)
        {
            Debug.LogWarning("[QuarkManager] Wrist bone is NULL — cannot compute palm direction!");
            return;
        }

        Pose palmPose = new Pose(wrist.Transform.position, wrist.Transform.rotation);

        // Debug positions
        Debug.DrawLine(palmPose.position, palmPose.position + palmPose.rotation * Vector3.forward * 0.1f, Color.blue);
        Debug.DrawLine(_head.position, palmPose.position, Color.green);

        // Compute palm-to-face dot product
        Vector3 palmNormal = palmPose.rotation * Vector3.forward;
        Vector3 headToPalm = (palmPose.position - _head.position).normalized;
        float dot = Vector3.Dot(palmNormal, headToPalm);

        bool palmFacingFace = dot > 0.65f;

        // Only log transitions to avoid spam
        if (palmFacingFace != lastPalmFacingFace)
        {
            Debug.Log($"[QuarkManager] PalmFacingFace changed → {palmFacingFace}  (dot: {dot:F3})");
            lastPalmFacingFace = palmFacingFace;
        }

        // Optional: continuous debug value
        // Debug.Log($"[QuarkManager] dot={dot:F3}");

        if (spawnedQuark != null)
        {
            spawnedQuark.gameObject.SetActive(palmFacingFace);
        }
    }
}
