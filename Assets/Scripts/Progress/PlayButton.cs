using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public string nextSceneName = "Opening Sequence";
    public MusicMuffler cutsceneMuffle;
    // Start is called before the first frame update

    public void StartGame() {
        AudioController.Instance.Play(SoundCategory.Interactable,0);
        cutsceneMuffle.SetFilter(true,20);
        SceneManager.LoadScene(nextSceneName);
    }
}
