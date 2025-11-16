using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class QuarkManager : Singleton<QuarkManager>
{
    [Header("Quark + Room")]
    [SerializeField] private RoomScanner roomScanner;
    [SerializeField] private Quark quarkPrefab;
    [SerializeField] private Transform quarkSpawnParent; // attach b_l_wrist here

    [Header("Palm Up Detection")]
    [Tooltip("Dot threshold: 1 = exactly up, 0 = sideways, -1 = down.")]
    [Range(-1f, 1f)]
    [SerializeField] private float palmUpThreshold = 0.75f;

    private Quark spawnedQuark = null;
    private bool lastPalmUp = false; // for change-detect

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[QuarkManager] Awake");
    }

    private void Start()
    {
        SpawnQuark(quarkSpawnParent);
    }

    public void SpawnQuark(Transform parent)
    {
        spawnedQuark = Instantiate(quarkPrefab, parent);
        spawnedQuark.transform.localPosition = Vector3.zero;
        spawnedQuark.transform.localRotation = Quaternion.identity;
        spawnedQuark.gameObject.SetActive(false);

        Debug.Log("[QuarkManager] Spawned new Quark.");
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

    private IEnumerator SpawnNewQuark(float delay = 3f)
    {
        Debug.Log($"[QuarkManager] Waiting {delay}s before spawning a new Quark...");
        yield return new WaitForSeconds(delay);

        SpawnQuark(quarkSpawnParent);
    }

    private void Update()
    {
        if (quarkSpawnParent == null)
        {
            Debug.LogWarning("[QuarkManager] Missing quarkSpawnParent (b_l_wrist).");
            return;
        }

        // World-space up direction of the wrist
        Vector3 palmNormal = -quarkSpawnParent.up; // already world-space

        // Debug line in Scene view
        Debug.DrawLine(quarkSpawnParent.position,
                       quarkSpawnParent.position + palmNormal * 0.1f,
                       Color.blue);

        // Compare wrist up with global up
        float dot = Vector3.Dot(palmNormal.normalized, Vector3.up);
        bool palmUp = dot > palmUpThreshold;
        Debug.Log($"[QuarkManager] PalmUp = {palmUp} (dot: {dot:F3})");

        // Only log when state changes
        if (palmUp != lastPalmUp)
        {
            lastPalmUp = palmUp;
        }

        if (spawnedQuark != null)
        {
            spawnedQuark.gameObject.SetActive(palmUp);
        }
    }
}
