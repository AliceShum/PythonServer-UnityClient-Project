using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//进度条自己不停地跑
public class ProgressSlider : MonoBehaviour
{
    Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = 0;
    }

    void Update()
    {
        if (slider == null) return;
        slider.value += Time.deltaTime * 0.2f;
        if (slider.value >= 1f)
        {
            slider.value = 0f;
        }
    }
}
