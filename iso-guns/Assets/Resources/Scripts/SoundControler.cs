using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2021.1.12
/// </summary>
public class SoundControler : MonoBehaviour
{
    [SerializeField]
    AudioSource shoot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        shoot.Play();
    }
}
