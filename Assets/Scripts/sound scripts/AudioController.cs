
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
        private static AudioController _instance;
    public static AudioController Instance {get{return _instance;}}

    [SerializeField] private SoundGroup[] soundGroups;
    private Dictionary<SoundCategory, SoundGroup> groups =new();

    private void Awake()
    {
        _instance=this;
        DontDestroyOnLoad(gameObject);

        foreach(SoundGroup group in soundGroups)
        {
            groups.Add(group.Category,group);
        }
    }
        public bool Play(SoundCategory category,int index)
        {
         return groups[category].TryPlay(index);
        }
        public bool PlayRandom(SoundCategory category)
        {
         return groups[category].TryPlayRandom();
        }
        public void ForcePlay(SoundCategory category, int index)
        {
         groups[category].FPlay(index);
        }
        public void ForcePlayRandom(SoundCategory category)
        {
         groups[category].FPlayRandom();
        }
    }

