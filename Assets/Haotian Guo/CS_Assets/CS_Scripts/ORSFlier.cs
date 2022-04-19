using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines an object ( ie: enemy ) that can fly randomly within a limited area
    /// </summary>
    public class ORSFlier : MonoBehaviour
    {
        // Hold the transform and initial position of the slyer
        internal Transform thisTransform;
        internal Vector3 initialPosition;

        [Tooltip("The center of the flying area relative to the flier's initial position")]
        public Vector3 flyingAreaCenter = Vector3.zero;

        [Tooltip("The size of the flying area. The flier chooses a random location within this area to fly to")]
        public Vector3 flyingAreaSize = new Vector3(2, 2, 2);
        internal Vector3 moveTarget;
        internal bool isMoving = false;

        [Tooltip("How long to wait before moving to a new random location within the flying area")]
        public Vector2 moveDelayRange = new Vector2(2,4);
        internal float moveDelayCount;

        [Tooltip("How fast the flier moves to the new location")]
        public float moveSpeed = 10;
        
        void Start()
        {
            thisTransform = transform;

            // Set the initial position of the flier
            initialPosition = thisTransform.position;

            // Set the center of the flying area relative to the initial position
            flyingAreaCenter += initialPosition;

            // Move the flier to the center of the flying area
            MoveToTarget(flyingAreaCenter);
        }

        void Update()
        {
            // If the flier is not moving, count down to the next time it needs to move
            if ( isMoving == false )
            {
                // Count down to the next move
                moveDelayCount -= Time.deltaTime;

                // If the time to move has come, choose a new random target location and move to it
                if ( moveDelayCount <= 0 )
                {
                    // Move to a random location within the flying area limits 
                    StartCoroutine(MoveToTarget(flyingAreaCenter + new Vector3(Random.Range(-flyingAreaSize.x * 0.5f, flyingAreaSize.x * 0.5f), Random.Range(-flyingAreaSize.y * 0.5f, flyingAreaSize.y * 0.5f), Random.Range(-flyingAreaSize.z * 0.5f, flyingAreaSize.z * 0.5f))));

                    // Set a delay time for the next move
                    moveDelayCount = Random.Range(moveDelayRange.x, moveDelayRange.y);
                }
            }
        }

        /// <summary>
        /// Moves to the target location
        /// </summary>
        /// <param name="moveTarget"></param>
        /// <returns></returns>
        IEnumerator MoveToTarget( Vector3 moveTarget )
        {
            // We start moving
            isMoving = true;

            // While we are far from the target location, keep moving towards it
            while ( Vector3.Distance(thisTransform.position, moveTarget) > 0.5f )
            {
                // Wait a little to animate the effect
                yield return new WaitForSeconds(Time.deltaTime);

                // Move close to the target location
                thisTransform.position = Vector3.Slerp(thisTransform.position, moveTarget, Time.deltaTime * moveSpeed);
            }

            // We stop moving
            isMoving = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;

            // Show the flying area relative to the initial position of the flier
            if ( Application.isPlaying == true ) Gizmos.DrawWireCube( flyingAreaCenter, flyingAreaSize);
            else Gizmos.DrawWireCube(transform.position + flyingAreaCenter, flyingAreaSize);

            Gizmos.color = Color.white;

            // Draw a line between the position of the flier and the center of the flying area, so that we know where it will fly to at first
            if ( Application.isPlaying == true ) Gizmos.DrawLine(transform.position, flyingAreaCenter);
            else Gizmos.DrawLine(transform.position, transform.position + flyingAreaCenter);
        }
    }
}