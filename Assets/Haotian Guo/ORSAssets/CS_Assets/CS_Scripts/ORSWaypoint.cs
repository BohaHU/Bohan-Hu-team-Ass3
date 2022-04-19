using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OnRailsShooter.Types;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines the attributes of a waypoint, the enemies/pickups that spawn when the player reaches it, the move/rotate speed when going to the next waypoint,
    /// and the animations that appear when starting to move and after finishing the movement.
    /// </summary>
    public class ORSWaypoint : MonoBehaviour
    {
        [Tooltip("The next waypoint that the player will move to from this waypoint")]
        public ORSWaypoint[] nextWaypoint = new ORSWaypoint[1];

        [Tooltip("If true, the player will rotate to look directly at the next waypoint, instead of rotating to match the rotation of the camera at the next waypoint")]
        public bool lookAtWaypoint = false;

        // Did we reach the waypoint?
        internal bool reachedWaypoint = false;

        [Tooltip("A list of all the enemies that will spawn when the player reaches this waypoint. The enemies are assigned from the scene and have a red gizmo line connecting them to the waypoint")]
        public EnemySpawn[] enemies;

        [Tooltip("Should the player stop at the waypoint and wait until all enemies have been killed before moving on?")]
        public bool waitForEnemies = true;

        [Tooltip("If true, the player will always look at any active enemies, keeping them in the camera frame")]
        public bool lookAtEnemies = true;

        [Tooltip("How much we can look around with the camera. If set to 0 we can't look around, but if set to a higher number we can freely look in all directions within the angle limit of the number")]
        public float freeLook = 0;

        [Tooltip("Should all enemies be removed when the player goes to the next waypoint?")]
        public float removeEnemiesDelay = 3;

        [Tooltip("An object the player can hide behind. While hiding, you cannot be hit and cannot shoot")]
        public ORSHittable hidingObject;

        [Tooltip("A list of all the pickup items that will spawn when the player reaches this waypoint. The pickups are assigned from the scene and have a blue gizmo line connecting them to the waypoint")]
        public ORSPickup[] pickups;

        [Tooltip("How long to wait before moving from this waypoint to the next. If there are enemies you will not move until you kill all of them")]
        public float moveDelay = 0;

        [Tooltip("How fast the player moves from this waypoint to the next. If there are enemies you will not move until you kill all of them")]
        public float moveSpeed = 10;

        [Tooltip("How fast the player move speed increases. This is used for example when doing a jump from waypoint to waypoint when going down the cliff at the start of the game, so we start with a low speed and then increase it with the acceleration to give a feel of jumping down")]
        public float moveAcceleration = 0;

        [Tooltip("How quickly the player rotates towards the angle of the next waypoint. When the player reaches the waypoint the turnspeed smoothes out and increases drastically in order to finish the transition quickly")]
        public float turnSpeed = 100;

        [Tooltip("The animation that plays when the player starts moving from a waypoint to the next")]
        public AnimationClip startAnimation;

        [Tooltip("The animation that plays when the player reaches the next waypoint")]
        public AnimationClip endAnimation;

        [Tooltip("If set to true, the player will instantly teleport to the next waypoint instead of moving based on speed")]
        public bool teleport = false;

        [Tooltip("The canvas screen that appears when reaching this waypoint. In the demo we used it at the start of the game to show the fade-from-black with a message")]
        public Canvas messageScreen;

        [Tooltip("A list of function that will be triggered when we reach this waypoint. These can be targeted on any object and chosen from any public function in any component")]
        public UnityEvent specialFunctions;

        //General use index
        internal int index;

        // This is used to animate the red ball that comes out of a waypoint into the next. This is used to tell us where the next waypoint is going
        internal float gizmoTime = 0;

        void Awake()
        {
            // Disable the local camera for this waypoint. The camera is used during edit mode to show the player how the waypoint would look from the perspective of the player, but when playing we don't need it because the player already has a camera
            GetComponent<Camera>().enabled = false;
        }


        void OnValidate()
        {
            //if (nextWaypoint.Length > 1) nextWaypoint = new ORSWaypoint[0];

            if (waitForEnemies == true) removeEnemiesDelay = 0;
        }

        /// <summary>
        /// These gizmos appear in the unity editor at all times
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            // Draw a gree line bewteen this waypoint and the next 
            for (index = 0; index < nextWaypoint.Length; index++)
            {
                if (nextWaypoint[index])
                {
                    Gizmos.DrawLine(transform.position, nextWaypoint[index].transform.position);
                }
            }

            Gizmos.color = Color.red;

            // Draw red lines between this waypoint and all the enemies associated with it
            for (index = 0; index < enemies.Length; index++)
            {
                if (enemies[index].enemy)
                {
                    Gizmos.DrawLine(transform.position, enemies[index].enemy.transform.position);
                }
            }

            Gizmos.color = Color.blue;

            // Draw the blue line between this waypoint and all the pickup items associated with it
            for (index = 0; index < pickups.Length; index++)
            {
                if (pickups[index])
                {
                    Gizmos.DrawLine(transform.position, pickups[index].transform.position);
                }
            }

            Gizmos.color = Color.magenta;

            // Draw a magenta line between this waypoint and the hiding object associated with it
            if ( hidingObject )    Gizmos.DrawLine(transform.position, hidingObject.transform.position);
        }

        /// <summary>
        /// These gizmos appear in the unity editor when we select this object
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            // Animate a red ball moving from this waypoint to the next, so that we know the direction of movement
            for (index = 0; index < nextWaypoint.Length; index++)
            {
                if (nextWaypoint[index])
                {
                    // Advance the animation time
                    gizmoTime += 0.01f;

                    // Reset the animation time
                    if (gizmoTime > 1) gizmoTime = 0;

                    // Draw the ball based on the time we passed, where 0 is this waypoint, 0.5 is halfway through between points, and 1 is the next waypoint
                    Gizmos.DrawSphere(transform.position - (transform.position - nextWaypoint[index].transform.position) * gizmoTime, 0.3f);
                }
            }
        }
    }
}