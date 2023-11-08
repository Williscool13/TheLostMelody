using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInWhiteFinal : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(FadeIn());
        AudioSystem.Instance.Ensemble();
    }


    [SerializeField] Image fadeImage;
    IEnumerator FadeIn() {
        float alpha = 1;
        while (alpha > 0) {
            alpha -= Time.fixedDeltaTime * 0.5f;
            if (alpha < 0) {
                alpha = 0;
            }
            fadeImage.color = new Color(1, 1, 1, Mathf.Min(alpha, 1));
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

    }

}