using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroOutroCamera : MonoBehaviour
{
    [SerializeField]
    private float yPos = 0;
    [SerializeField]
    private float scrollSpeed = -0.2f;
    [SerializeField]
    private float yEnd = 2f;
    [SerializeField]
    private string NextSceneName = "Credits";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < yEnd) {
            SceneManager.LoadScene(NextSceneName);
        }
        else {
            transform.position = new Vector3 (0, yPos, -10f);
            yPos += Time.deltaTime * scrollSpeed;
        }

    }
}
