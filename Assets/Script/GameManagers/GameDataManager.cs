using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDataManager : MonoBehaviour
{
    [SerializeField] Slider staminaSlider;
    [SerializeField] int staminaIncrementMagnitude;

    [SerializeField] public int[] staminaLimits;
    public static GameDataManager Instance { get; private set; }


    public SceneZeroData SceneZeroData { get; private set; }

    public PlayerAbilityData PlayerAbilityData { get;  set; }

    public DeathData PreviousDeathData { get; set; }

    public float deathCount = 0;
    public float jumpCount = 0;
    public float dashCount = 0;
    public float gameStartTimestamp = 0;
    private void Awake() {
        if (GameDataManager.Instance != null) {
            Debug.LogWarning("2 data managers in scene, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            GameDataManager.Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        SceneZeroData = new();
        PlayerAbilityData = new();
        PreviousDeathData = new("none");

        gameStartTimestamp = Time.time;
    }


    public void SetStaminaSlider(float val) {
        staminaSlider.value = val;
    }
    public void IncrementStaminaSlider(float val) {
        int oldVal = Mathf.FloorToInt(staminaSlider.value * staminaIncrementMagnitude);
        int newVal = Mathf.FloorToInt(val * staminaIncrementMagnitude);

        if (newVal != oldVal) {
            staminaSlider.value = val;
        }
    }
}


public class PlayerAbilityData {
    public bool[] abilities;

    public PlayerAbilityData() {
        // 0: Wall Cling
        // 1: Dash
        // 2: Double Jump
        // 3: water walking
        // 4: placeable platform
        abilities = new bool[] { false, false, false, false, false };
        //abilities = new bool[] { true, true, true, true, false };

    }
}

public struct DeathData {
    public string CauseOfDeath { get; private set; }

    public DeathData(string cause) {
        CauseOfDeath = cause;
    }   
}

public class SceneZeroData {
    public bool[] flowerInteracted;
    public bool spiritFirstInteracted;


    public SceneZeroData() {
        flowerInteracted = new bool[] { false, false, false, false, false };
        spiritFirstInteracted = false;
    }
}

public struct SceneThreeData {
    public bool[] batPickedUp;
    public SceneThreeData(bool dummy) {
        batPickedUp = new bool[] {
            false, false, false, false,
            false, false, false, false,
            false, false, false, false,
            false, false, false, false,
        };
    }
}