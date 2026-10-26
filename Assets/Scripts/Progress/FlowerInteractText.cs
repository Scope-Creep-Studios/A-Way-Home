using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowerInteractText : MonoBehaviour
{

    public GameObject LockedTextGirl;
    public GameObject LockedTextGhost;


    private bool playerNearby = false;

    private void Start()
    {
        if (LockedTextGirl != null)
        {
            LockedTextGirl.SetActive(false);
        }
        if (LockedTextGhost != null)
        {
            LockedTextGhost.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.CompareTag("Player")) {
            if (LockedTextGirl != null)
            {
                LockedTextGirl.SetActive(true);
            }
        }
        if (other.gameObject.CompareTag("Ghost")) {
            if (LockedTextGhost != null)
            {
                LockedTextGhost.SetActive(true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    { 
        if (other.gameObject.CompareTag("Player")) {
            if (LockedTextGirl != null)
            {
                LockedTextGirl.SetActive(false);
            }
        }
        if (other.gameObject.CompareTag("Ghost")) {
            if (LockedTextGhost != null)
            {
                LockedTextGhost.SetActive(false);
            }
        }
    }
}