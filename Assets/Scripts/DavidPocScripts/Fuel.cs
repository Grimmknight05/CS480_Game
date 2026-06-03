using System.Collections;
using UnityEngine;

// A collectible fuel canister. Raises FuelCollectedChannel on pickup, plays a
// one-shot SFX + VFX, then hides and destroys itself. Follows the project's
// PickUpDefault precedent: hide visuals/collider first so the sound isn't cut
// off by the Destroy. Idle spin/bob is the entity's own Update (Update Method
// pattern) and is purely cosmetic.
[RequireComponent(typeof(Collider))]
public class Fuel : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private FuelCollectedChannel fuelCollectedChannel;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private GameObject pickupVFXPrefab;

    [Header("Idle Motion (cosmetic)")]
    [SerializeField] private float spinSpeed = 60f;      // degrees / second, 0 = no spin
    [SerializeField] private float bobAmplitude = 0.15f; // metres, 0 = no bob
    [SerializeField] private float bobFrequency = 1f;    // cycles / second

    // Optional: if an AudioSource is present we play through it (so it routes
    // through any mixer group / cave-muffle setup). Otherwise we fall back to
    // PlayClipAtPoint, which spawns its own temporary source that outlives us.
    private AudioSource source;
    private Vector3 startPos;
    private bool collected;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source != null) source.playOnAwake = false;
        startPos = transform.position;
    }

    // Editor convenience: auto-set the collider to a trigger when first added.
    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void Update()
    {
        if (collected) return;

        if (spinSpeed != 0f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        if (bobAmplitude != 0f)
        {
            float y = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            transform.position = startPos + Vector3.up * y;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag(playerTag)) return;
        collected = true;

        if (fuelCollectedChannel != null) fuelCollectedChannel.Raise();

        if (pickupVFXPrefab != null)
            Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);

        float sfxLength = 0f;
        if (pickupSfx != null)
        {
            sfxLength = pickupSfx.length;
            if (source != null) source.PlayOneShot(pickupSfx, sfxVolume);
            else AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);
        }

        StartCoroutine(HideThenDestroy(sfxLength));
    }

    // Hide the canister immediately so it reads as "grabbed," but keep the
    // GameObject alive long enough for a locally-played SFX to finish.
    private IEnumerator HideThenDestroy(float delay)
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;

        // If we used PlayClipAtPoint the temp source already owns the clip's
        // lifetime, so we only need to wait when playing on a local source.
        if (source != null && delay > 0f)
            yield return new WaitForSeconds(delay);
        else
            yield return null;

        Destroy(gameObject);
    }
}
