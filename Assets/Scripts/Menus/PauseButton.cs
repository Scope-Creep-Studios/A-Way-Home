using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    public static bool paused = false;
    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private GameObject settingsMenu;

    void Start(){
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            if(paused){
                ResumeGame();
            }
            else{
                PauseGame();
            }
        }
    }

    public void PauseGame(){
        pauseMenu.SetActive(true);
        pauseButton.SetActive(false);
        paused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame(){
        pauseMenu.SetActive(false);
        pauseButton.SetActive(true);
        settingsMenu.SetActive(false);
        paused = false;
        Time.timeScale = 1f;
    }

    public void OpenSettings(){
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings(){
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }
}
