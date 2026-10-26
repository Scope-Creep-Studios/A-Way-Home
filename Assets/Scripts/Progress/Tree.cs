using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tree : MonoBehaviour
{

    public GameObject interactText;
    public GameObject LockedTextGirl;
    public GameObject LockedTextGhost;

    [SerializeField]
    private SpiritTree UnlockBool;

    private bool playerNearby = false;

    private void Start()
    {
        if (interactText != null)
        {
            interactText.SetActive(false);
        }
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
        if (other.gameObject.CompareTag("Ghost") && UnlockBool.IsLocked == false)
        {


            if (interactText != null)
            {
                interactText.SetActive(true);
            }
        }
        if (other.gameObject.CompareTag("Player") && UnlockBool.IsLocked == true) {
            if (LockedTextGirl != null)
            {
                LockedTextGirl.SetActive(true);
            }
        }
        if (other.gameObject.CompareTag("Ghost") && UnlockBool.IsLocked == true) {
            if (LockedTextGhost != null)
            {
                LockedTextGhost.SetActive(true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    { 
        if (other.gameObject.CompareTag("Ghost") && UnlockBool.IsLocked == false)
        {

            if (interactText != null)
            {
                interactText.SetActive(false);
            }
        }
        if (other.gameObject.CompareTag("Player") && UnlockBool.IsLocked == true) {
            if (LockedTextGirl != null)
            {
                LockedTextGirl.SetActive(false);
            }
        }
        if (other.gameObject.CompareTag("Ghost") && UnlockBool.IsLocked == true) {
            if (LockedTextGhost != null)
            {
                LockedTextGhost.SetActive(false);
            }
        }
    }
}