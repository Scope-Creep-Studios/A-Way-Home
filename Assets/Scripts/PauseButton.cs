using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    bool paused = false;

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI(){
        if(paused) {
            GUILayout.Label("Game is paused!");
            if(GUILayout.Button("Resume")) {
                paused = togglePause();
            }
        }
        else if (!paused){
            if(GUILayout.Button("PAUSE")) {
                paused = togglePause();
            }
        }
    }

    bool togglePause() {
        if(Time.timeScale == 0f) {
            Time.timeScale = 1f;
            return(false);
        }
        else {
            Time.timeScale = 0f;
            Time.timeScale = 0f;
            return(true);
        }
    }
}
