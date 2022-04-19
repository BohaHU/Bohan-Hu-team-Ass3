using System.Collections;
using UnityEngine;

namespace OnRailsShooter
{
	/// <summary>
	/// This script is used to make the player camera shake when being hit by an enemy
	/// </summary>
	public class ORSCameraShake : MonoBehaviour
	{
        // How far should the camera angle turn
        internal Vector3 cameraTurn = Vector3.zero;

        [Tooltip("How fast the camera returns to the default angle")]
        public float turnSpeed = 10;

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
            // If camera turn is not 0, retun it to 0 based on turn speed
            if ( cameraTurn != Vector3.zero )
            {
                // Change the camera turn towards 0
                cameraTurn = Vector3.Slerp(cameraTurn, Vector3.zero, Time.deltaTime * turnSpeed);

                // Set the rotation of the camera based on camera turn
                Camera.main.transform.localEulerAngles = cameraTurn;
            }
            else if ( Camera.main.transform.localEulerAngles != Vector3.zero )
            {
                // Reset the camera to 0
                Camera.main.transform.localEulerAngles = Vector3.zero;
            }
		}
	}
}