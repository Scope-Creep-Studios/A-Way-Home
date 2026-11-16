using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRay : MonoBehaviour
{
    [SerializeField]
    private float timerDesync = 0f;

    private float currZPosition, nextZPosition, timer, alphaMod;
    private Vector3 currLocation;
    private SpriteRenderer sunrayRenderer;
    // Start is called before the first frame update
    void Start()
    {
        sunrayRenderer = GetComponent<SpriteRenderer>();
        timer = 0f + timerDesync;
        currZPosition = transform.position.z;
        currLocation = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        if (timer < 5f) {
            alphaMod = timer / 5f;
            sunrayRenderer.color = new Color(1.0f, 1.0f, 1.0f, alphaMod);
        }
        else if (timer < 10f){
            alphaMod = 1 - ((timer - 5f) / 5f);
            sunrayRenderer.color = new Color(1.0f, 1.0f, 1.0f, alphaMod);
        }
        else {
            currLocation = transform.position;
            transform.position = new Vector3 (currLocation.x, currLocation.y, nextZPosition);
            timer = 0f;
            currZPosition = currZPosition = transform.position.z;
        }
        if(currZPosition == 0f) {
            nextZPosition = 1.5f;
        }
        else if(currZPosition == 1.5f) {
            nextZPosition = 2.5f;
        }
        else if(currZPosition == 2.5f) {
            nextZPosition = 0f;
        }
        timer += 1f * Time.deltaTime;
    }
}
