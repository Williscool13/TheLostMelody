using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CountUpdate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI jumpCount;
    [SerializeField] TextMeshProUGUI dashCount;
    [SerializeField] TextMeshProUGUI deathCount;

    [SerializeField] TextMeshProUGUI runTime;

    // Start is called before the first frame update
    void Start()
    {
        jumpCount.text = GameDataManager.Instance.jumpCount.ToString();
        dashCount.text = GameDataManager.Instance.dashCount.ToString();
        deathCount.text = GameDataManager.Instance.deathCount.ToString();

        runTime.text = Mathf.Round(Time.time - GameDataManager.Instance.gameStartTimestamp).ToString() + " seconds";
    }
}
