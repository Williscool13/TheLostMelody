using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudEnable : MonoBehaviour
{
    [SerializeField] Collider2D col;
    // Start is called before the first frame update
    void Start()
    {

        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        if (mementoIndex > 4) {
            col.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
