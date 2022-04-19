using UnityEngine;
using System.Collections;

namespace OnRailsShooter
{
	/// <summary>
	/// This script creates a slow motion effect by changing the speed ( Time.timeScale ) of the game, then returns to normal
	/// </summary>
	public class ORSSlowMotion : MonoBehaviour
	{
        [Tooltip("the slowdown value that time will change to when this effect starts")]
        public float targetSpeed = 0.5f;

        [Tooltip("How fast the slowdown happens")]
        public float targetSpeedChange = 10;

        [Tooltip("How long the slowdown lasts before returning to original speed")]
        public float endTime = 2;

        [Tooltip("The sound that plays when the effect starts")]
        public AudioClip startSound;

        [Tooltip("The sound that plays when the effect ends")]
        public AudioClip endSound;
		
		[Tooltip("Should the slowmotion effect be played immediately when this object is enabled?")]
		public bool playOnEnabled = true;
		
		/// <summary>
		/// Runs when the object has been enabled. ( If it was disabled before )
		/// </summary>
		void OnEnable()
		{
            // If the object has been enabled. play the effect
            if ( playOnEnabled == true) StartCoroutine(SlowMotion());
		}


        /// <summary>
        /// Starts the slow motion effect
        /// </summary>
        public void StartSlowMotion()
        {
            StartCoroutine(SlowMotion());
        }

        /// <summary>
        /// Slows down the game for a while, and then returns to normal game speed
        /// </summary>
        /// <returns></returns>
        IEnumerator SlowMotion()
        {
            // Slow down the game
            while ( Mathf.Abs(Time.timeScale - targetSpeed) > 0.05f )
            {
                Time.timeScale = Mathf.Lerp( Time.timeScale, targetSpeed, Time.unscaledDeltaTime * targetSpeedChange);  

                // Wait a little so the effect is animated
                yield return new WaitForSeconds(Time.deltaTime);
            }

            // Set the final slowdown speed
            Time.timeScale = targetSpeed;

            // Wait until the effect needs to be reverted
            yield return new WaitForSecondsRealtime(endTime);

            // Speed the game back up to normal
            while ( Mathf.Abs(Time.timeScale) < 9.95f )
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime * targetSpeedChange);

                // Wait a little so the effect is animated
                yield return new WaitForSeconds(Time.deltaTime);
            }
            
            // Set the final normal speed
            Time.timeScale = 1;
        }

        // If the object is destroyed before the effect is over, return to normal speed
        void OnDisable()
        {
            Time.timeScale = 1;
        }

    }
}

