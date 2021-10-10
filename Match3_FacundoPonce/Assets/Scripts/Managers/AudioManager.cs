using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AudioManager>();

            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    #endregion

    [SerializeField] AudioClip trackGameplay;
    AudioSource source;


    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }
}
