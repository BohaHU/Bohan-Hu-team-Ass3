using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines an object ( ie: enemy ) that can fly randomly within a limited area
    /// </summary>
    public class ORSEnemyPath : MonoBehaviour
    {
        // Hold the transform and initial position of the flyer
        internal Transform thisTransform;
        internal Vector3 initialPosition;

        public Transform[] pathPoints;
        internal int currentPathPoint;

        [Tooltip("How fast the flier moves to the new location")]
        public float moveSpeed = 10;

        public bool loopPath = true;

        public bool pingpongPath = false;
        internal int moveDirection = 1;

        [Tooltip("How long to wait before moving to a new random location within the flying area")]
        public Vector2 moveDelayRange = new Vector2(2,4);
        internal float moveDelayCount;

        internal bool isMoving = false;

        internal int index;

        void Start()
        {
            thisTransform = transform;

            // Set the initial position of the flier
            initialPosition = thisTransform.position;
        }
        
        void Update()
        {
            if (pathPoints.Length <= 0) return;

            // If the flier is not moving, count down to the next time it needs to move
            if ( isMoving == false )
            {
                // Count down to the next move
                moveDelayCount -= Time.deltaTime;

                // If the time to move has come, choose a new random target location and move to it
                if ( moveDelayCount <= 0 )
                {
                    if ( currentPathPoint < pathPoints.Length - 1 )
                    {
                        currentPathPoint++;
                    }
                    else
                    {
                        currentPathPoint = 0;
                    }

                    // Move to a random location within the flying area limits 
                    StartCoroutine(MoveToTarget(pathPoints[currentPathPoint].position));

                    thisTransform.LookAt(pathPoints[currentPathPoint].position);

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
                //thisTransform.position = Vector3.Slerp(thisTransform.position, moveTarget, Time.deltaTime * moveSpeed);
                thisTransform.position = Vector3.MoveTowards(thisTransform.position, moveTarget, Time.deltaTime * moveSpeed);

            }

            // We stop moving
            isMoving = false;
        }

        void OnDrawGizmosSelected()
        {
            if (pathPoints.Length <= 0) return;

            Gizmos.color = Color.magenta;

            // Show the flying area relative to the initial position of the flier
            for ( index = 0; index < pathPoints.Length; index++)
            {
                if ( index < pathPoints.Length - 1) Gizmos.DrawLine(pathPoints[index].position, pathPoints[index+1].position);
                //else Gizmos.DrawLine(pathPoints[index].position, pathPoints[0].position);
            }

            if ( loopPath == true )    Gizmos.DrawLine(pathPoints[pathPoints.Length-1].position, pathPoints[0].position);
            //if ( Application.isPlaying == true ) Gizmos.DrawWireCube( flyingAreaCenter, flyingAreaSize);
            //else Gizmos.DrawWireCube(transform.position + flyingAreaCenter, flyingAreaSize);

            Gizmos.color = Color.white;

            // Draw a line between the position of the flier and the center of the flying area, so that we know where it will fly to at first
            Gizmos.DrawLine(transform.position, pathPoints[0].position);
            //else Gizmos.DrawLine(transform.position, transform.position + flyingAreaCenter);
        }
    }
}