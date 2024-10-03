using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class AudioSourceControl : MonoBehaviour
{
   public AudioSource bgm;
   public AudioSource ghostNormal;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (!bgm.isPlaying && !ghostNormal.isPlaying)
        {
            ghostNormal.Play();
        }
    }
}
