using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingTrigger : MonoBehaviour
{
    [SerializeField]
    private string NextSceneName = "Credits";

    void Awake() {

    }
    void Update(){

    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            {
                SceneManager.LoadScene(NextSceneName);
            }
        }
    }
}
