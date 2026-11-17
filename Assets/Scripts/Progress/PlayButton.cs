using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public string nextSceneName = "Opening Sequence";
    // Start is called before the first frame update

    public void StartGame() {
        AudioController.Instance.Play(SoundCategory.Interactable,0);
        SceneManager.LoadScene(nextSceneName);
    }
}
