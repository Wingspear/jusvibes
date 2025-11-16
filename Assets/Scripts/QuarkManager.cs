using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class QuarkManager : Singleton<QuarkManager>
{
    [SerializeField] private RoomScanner roomScanner;
    [SerializeField] private Quark quarkPrefab;
    [SerializeField] private Transform quarkSpawnParent; // "palm" transform
    [SerializeField] private Transform _head;            // main camera
    
    private Quark spawnedQuark = null;
    private bool lastPalmFacingFace = false; // to prevent log spam

    private void Start()
    {
        SpawnQuark(Vector3.zero, Quaternion.identity, quarkSpawnParent);
    }

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
        if (quarkSpawnParent == null || _head == null)
        {
            Debug.LogWarning("[QuarkManager] Missing quarkSpawnParent or head reference.");
            return;
        }

        // Treat quarkSpawnParent as the “palm”
        Vector3 palmPos    = quarkSpawnParent.position;
        Vector3 palmNormal = quarkSpawnParent.up; // using .up as requested

        // Direction from head to this “palm”
        Vector3 headToPalm = (palmPos - _head.position).normalized;

        // Debug lines in scene view
        Debug.DrawLine(palmPos, palmPos + palmNormal * 0.1f, Color.blue);     // palm normal
        Debug.DrawLine(_head.position, palmPos, Color.green);                 // head -> palm

        float dot = Vector3.Dot(palmNormal, headToPalm);
        bool palmFacingFace = dot > 0.65f; // tweak threshold as needed

        // Only log when the state changes
        if (palmFacingFace != lastPalmFacingFace)
        {
            lastPalmFacingFace = palmFacingFace;
            Debug.Log($"[QuarkManager] PalmFacingFace changed → {palmFacingFace} (dot: {dot:F3})");
        }

        if (spawnedQuark != null)
        {
            spawnedQuark.gameObject.SetActive(palmFacingFace);
        }
    }
}
