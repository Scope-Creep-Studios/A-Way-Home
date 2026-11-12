using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsCamera : MonoBehaviour
{
    [SerializeField]
    private float xPos = 9;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x >= 300) {
            transform.position = new Vector3 (400, 0, -10);
        }
        else {
            transform.position = new Vector3 (xPos, 0, -10);
            xPos += 5f * Time.deltaTime;
        }
    }
}
