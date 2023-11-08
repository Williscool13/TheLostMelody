using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformSystem : MonoBehaviour
{
    public static PlayerPlatformSystem Instance;

    [SerializeField] float SpawnPlatformCooldown;
    [SerializeField] Vector2 mainOffset;
    [SerializeField] float platformLifetime;
    [SerializeField] RaycastEngine raycastEngine;
    [SerializeField] GameObject platformPrefab;
    float inputOnTimestamp;
    float cooldownTimestamp;

    GameObject platform;
    Collider2D platformCollider;

    float platformTime;

    private void Awake() {
        if (PlayerPlatformSystem.Instance != null) {
            Debug.LogWarning("2 data managers in scene, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            PlayerPlatformSystem.Instance = this;
        }
    }


    void Start()
    {
        platform = Instantiate(platformPrefab);
        platformCollider = platform.GetComponent<Collider2D>();
        Object.DontDestroyOnLoad(platform);
    }


    bool placed = false;
    void Update()
    {
        if (!GameDataManager.Instance.PlayerAbilityData.abilities[4]) {
            return;
        }
        if (Input.GetButtonDown("Platform")) {
            inputOnTimestamp = Time.time + 0.1f;
        }

        if (placed) {
            platformTime += Time.deltaTime;
            if (platformTime > platformLifetime) {
                platformCollider.enabled = false;
            }
        }

    }
    private void FixedUpdate() {
        if (inputOnTimestamp > Time.time && cooldownTimestamp < Time.time) {
            Debug.Log("Doing");
            bool flip = raycastEngine.GetRendererXFlip();
            Vector2 offset;
            if (flip) {
                offset = mainOffset * -1;
            }
            else {
                offset = mainOffset;
            }

            Vector2 targetPosition = (Vector2)transform.position + offset;

            platform.transform.position = targetPosition;
            platformCollider.enabled = true;
            placed = true;
            platformTime = 0;
            inputOnTimestamp = 0;
            cooldownTimestamp = Time.time + SpawnPlatformCooldown;
        }
    }

    public void ResetPlatformPosition() {
        platform.transform.position = new Vector2(-100, -100);
    }
}
