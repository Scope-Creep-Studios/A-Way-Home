using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroOutroCamera : MonoBehaviour
{
    [SerializeField]
    private float yPos = 0;
    [SerializeField]
    private float scrollSpeed = -0.2f;
    [SerializeField]
    private float yEnd = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < yEnd) {
            //enter new scene
        }
        else {
            transform.position = new Vector3 (0, yPos, -10f);
            yPos += Time.deltaTime * scrollSpeed;
        }

    }
}
