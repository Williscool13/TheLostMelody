using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerInteractionSystem : MonoBehaviour
{
    [SerializeField] GameObject circleOverlapTarget;
    [SerializeField] float radius;
    [SerializeField] LayerMask layer;

    [SerializeField] AudioMixer mixer;

    float inputOnTimestamp;
    void Update()
    {
        if (Input.GetButtonDown("Up")) {
            inputOnTimestamp = Time.time + Time.fixedDeltaTime;
        }
        
        if (Input.GetKeyDown(KeyCode.Minus)) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                mixer.GetFloat("Music", out float val);
                mixer.SetFloat("Music", val - 2.0f);
            } else {
                mixer.GetFloat("SFX", out float val);
                mixer.SetFloat("SFX", val - 2.0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Equals)) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                mixer.GetFloat("Music", out float val);
                mixer.SetFloat("Music", Mathf.Max(0.8f, val + 2.0f));
            } else {
                mixer.GetFloat("SFX", out float val);
                mixer.SetFloat("SFX", Mathf.Max(0.8f, val + 2.0f));
            }
        }
        
    }

    private void FixedUpdate() {
        // find target
        Collider2D[] cols = Physics2D.OverlapCircleAll(circleOverlapTarget.transform.position, radius, layer);


        float minDistance = float.MaxValue;
        IInteractible closestInt = null;

        foreach (Collider2D col in cols) {
            RaycastHit2D dir = Physics2D.Raycast(circleOverlapTarget.transform.position, col.transform.position - circleOverlapTarget.transform.position, radius, layer);
            if (dir.collider == null || dir.collider != col || !dir.collider.CompareTag("Interactable")) { continue; }
            IInteractible i_int = dir.collider.GetComponent<IInteractible>();
            if (i_int.Interactable) {
                if (dir.distance < minDistance) {
                    minDistance = dir.distance;
                    closestInt = i_int;
                }
            }
        }

        if (closestInt != null) {
            if (inputOnTimestamp > Time.time) {
                closestInt.Interact();
                inputOnTimestamp = 0;
            }
            else {
                closestInt.Highlight(true);
            }
        }
    }
}
