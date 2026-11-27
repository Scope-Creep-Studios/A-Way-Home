using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool hasCollectible = false;
    public bool isPulled = false;

     public List<string> collectedItems = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddCollectedItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
        }
    }
}
