using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines an item which can be picked up by the player when shooting it
    /// </summary>
    public class ORSWaypointArrow : MonoBehaviour
    {
        // These variables hold the gamecontroller, camera, and the pickup object, for easier access during gameplay
        static ORSGameController gameController;
        static Transform cameraObject;
        internal Transform thisTransform;

        // The target waypoint that the player will move to when clicking on this waypoint
        internal ORSWaypoint targetWaypoint;
        
        // These check if the item has been spawned or picked up already
        internal bool isPickedup = false;

        [Tooltip("The effect that appears when this item is picked up by the player")]
        public Transform pickupEffect;

        [Tooltip("Make the item look at the camera at all times. This is used for 2D items")]
        public bool lookAtCamera = false;

        public void Start()
        {
            // Assign this transfor for easier access
            thisTransform = this.transform;

            // Assign the camera from the scene
            if (cameraObject == null) cameraObject = Camera.main.transform;

            // Assign the gamecontroller from the scene
            if (gameController == null) gameController = (ORSGameController)FindObjectOfType(typeof(ORSGameController));

        }

        public void Update()
        {
            // Look at the camera at all times
            if (lookAtCamera) thisTransform.LookAt(cameraObject);
        }

        /// <summary>
        /// Picks up this item and gives it to the player
        /// </summary>
        public void Pickup()
        {
            // If this item has not been picked up yet, pick it up!
            if (isPickedup == false)
            {
                // The item has been picked up so it cannot be picked up again
                isPickedup = true;

                // If we have a gamecontroller, run the relevant functions on it
                if (gameController)
                {
                    gameController.SendMessage("MoveToWaypoint", targetWaypoint);
                }

                // Create a pickup effect at the position/rotation of this pickup item
                if (pickupEffect) Instantiate(pickupEffect, transform.position, transform.rotation);

                // Remove the pickup item
                Destroy(gameObject);
            }
        }
    }
}