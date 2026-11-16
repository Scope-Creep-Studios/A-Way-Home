using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroOutroBackground : MonoBehaviour
{
    private SpriteRenderer background;
    private float curAlphaVal = 0.1f;
    private float curColorVal = 1f;
    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(curColorVal <= 0.58f) {
            background.color = new Color(curColorVal, curColorVal, curColorVal, curAlphaVal);
            curAlphaVal += 0.1f * Time.deltaTime;
        }
        else{
            background.color = new Color(curColorVal, curColorVal, curColorVal, curAlphaVal);
            curAlphaVal += 0.1f * Time.deltaTime;
            curColorVal -= 0.1f * Time.deltaTime;
        }
    }
}
