using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera cam;
    Transform player;


    public static CameraController Instance { get; set; }
    void Start()
    {
        player = RaycastEngine.Instance.transform;

        if (CameraController.Instance != null) {
            Debug.LogWarning("2 camera controllers in scene, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            CameraController.Instance = this;
            Object.DontDestroyOnLoad(this);
        }
    }



    private void FixedUpdate() {
        Vector3 targetPos = player.position;
        targetPos.z = -10;
        targetPos.y += 1.3f;
        targetPos.x += 2.2f;
        transform.position = targetPos;

        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        if (mementoIndex > 4) {
            cam.orthographicSize = 9.5f;
        }
    }
}
