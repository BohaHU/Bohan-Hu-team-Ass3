using UnityEngine;
using System.Collections;

namespace OnRailsShooter
{
	/// <summary>
	/// This script animates the UI while the game is paused
	/// </summary>
	public class ORSLookAt : MonoBehaviour
	{
        static Transform targetCamera;

		// Use this for initialization
		void Start()
		{
            if ( targetCamera == null ) targetCamera = Camera.main.transform;

            transform.LookAt(targetCamera);
		}
		
		// Update is called once per frame
		void Update()
		{
            transform.LookAt(targetCamera);
        }
	}
}

