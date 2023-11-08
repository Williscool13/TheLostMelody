using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStaminaSystem : MonoBehaviour
{
    [SerializeField] RaycastEngine player;


    [SerializeField] float jumpStaminaCost;
    [SerializeField] float jumpSecondStaminaCost;
    [SerializeField] float dashStaminaCost;

    [SerializeField] float totalStamina;
    [SerializeField] float currentStamina;
    // Start is called before the first frame update
    void Start()
    {
        RefillStamina();
    }

    float refillTimestamp;
    float refillCooldown = 1.0f;
    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate() {
        
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        if (mementoIndex > 4) {
            if (refillTimestamp < Time.time) {
                FillStamina();
                refillTimestamp = Time.time + refillCooldown;
            }
        }
    }
    public void FillStamina() {
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        int maxStam = GameDataManager.Instance.staminaLimits[mementoIndex];
        currentStamina = maxStam;
        totalStamina = maxStam;
        GameDataManager.Instance.IncrementStaminaSlider(Mathf.InverseLerp(0, totalStamina, currentStamina));
    }
    public void RefillStamina() {
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        int maxStam = GameDataManager.Instance.staminaLimits[mementoIndex];
        currentStamina = maxStam;
        totalStamina = maxStam;
        //currentStamina = totalStamina;
        RaycastEngine.Instance.InputValid_StaminaManager = true;
        GameDataManager.Instance.IncrementStaminaSlider(Mathf.InverseLerp(0, totalStamina, currentStamina));
    }

    public void JumpCast() {
        currentStamina -= jumpStaminaCost;
        CastCheckStamina();
    }
    public void SecondJumpCast() {
        currentStamina -= jumpSecondStaminaCost;
        CastCheckStamina();
    }
    public void DashCast() {
        currentStamina -= dashStaminaCost;
        CastCheckStamina();
    }

    /// <summary>
    /// Cast when the player uses their life force to help another unit
    /// </summary>
    public void DepleteStamina(string cause) {
        currentStamina = 0;
        GameDataManager.Instance.IncrementStaminaSlider(Mathf.InverseLerp(0, totalStamina, currentStamina));

        player.InputValid_StaminaManager = false;
        if (cause == "memento") {
            player.Love();
        } else {
            GameDataManager.Instance.deathCount += 1;
            player.Death();
        }
        GameDataManager.Instance.PreviousDeathData = new(cause);
        GameSceneManager.Instance.DeathChangeScene();
    }
    bool lastAction = false;
    void CastCheckStamina() {
        GameDataManager.Instance.IncrementStaminaSlider(Mathf.InverseLerp(0, totalStamina, currentStamina));
        if (lastAction) {
            lastAction = false;

            DepleteStamina("stamina");
        } else if (currentStamina <= 0) {
            currentStamina = 0;
            lastAction = true;
        }
    }

    /// <summary>
    /// called only by thing
    /// </summary>
    /// <param name="value"></param>
    public void ReduceStamina(int value) {
        if (!lastAction) {
            currentStamina -= value;
            CastCheckStamina();
        }
    }

    public void MementoDepleteStamina() {


        RaycastEngine player = RaycastEngine.Instance;
        player.Death();
        GameDataManager.Instance.PreviousDeathData = new("memento");
        GameSceneManager.Instance.DeathChangeScene();
        //GameDialogueManager.Instance.StartDialogue(payload);
    }
}
