using UnityEngine;
using System.Collections;

public class ShootAudio : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void shootAudio()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }
}
