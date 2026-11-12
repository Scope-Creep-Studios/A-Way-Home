using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroOutroText : MonoBehaviour
{
    [SerializeField]
    private float yPos = 0;
    [SerializeField]
    private float scrollSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3 (0, yPos, 0);
        yPos += Time.deltaTime * scrollSpeed;
    }
}
