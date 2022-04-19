using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnRailsShooter
{

    /// <summary>
    /// This script defines an item which can be picked up by the player when shooting it
    /// </summary>
    public class ORSPickup : MonoBehaviour
    {
        // These variables hold the gamecontroller, camera, and the pickup object, for easier access during gameplay
        static ORSGameController gameController;
        static Transform cameraObject;
        internal Transform thisTransform;

        [Tooltip("The weapon that is given to the player when picking up this item")]
        public ORSWeapon weaponPickup;

        [Tooltip("The health value that is given to the player when picking up this item")]
        public int healthPickup = 0;

        [Tooltip("A pickup that is spawned at the player position when used. This could be a grenade for example")]
        public Transform throwablePickup;

        [Tooltip("The icon that appears in the inventory when picking up this item")]
        public Sprite itemIcon;

        [Tooltip("The effect that appears when this item is picked up by the player")]
        public Transform pickupEffect;

        [Tooltip("The bonus we get from picking up this item")]
        public int bonus = 0;

        [Tooltip("If set to true, the item is spawned when the game starts. If false, the item will only spawn when we reach a waypoint it is assigned to")]
        public bool isSpawned = false;

        [Tooltip("Make the item look at the camera at all times. This is used for 2D items")]
        public bool lookAtCamera = false;

        // This is used to check if the item has been picked up already
        internal bool isPickedup = false;

        void Awake()
        {
            // Deactivate the object at the start of the game, because it has not been spawned yet
            if (isSpawned == false) gameObject.SetActive(false);
        }

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
            if ( lookAtCamera ) thisTransform.LookAt(cameraObject);
        }

        /// <summary>
        /// Picks up this item and gives it to the player. If there is no empty slot in the player inventory, the item is used immediately. If this is a weapon, it is added to the weapons inventory
        /// </summary>
        public void Pickup( ORSPlayer player )
        {
            // If this item has not been picked up yet, pick it up!
            if (isPickedup == false)
            {
                // The item has been picked up so it cannot be picked up again
                isPickedup = true;

                // If this is a weapon pickup, try to add it to the weapons inventory
                if (weaponPickup) player.SendMessage("PickupWeapon", weaponPickup, SendMessageOptions.DontRequireReceiver);
                else if (player.PickupItem(this) == false) // If this is a regular item pickup, try to store it in the items inventory. If we can't store this item in the pickup item inventory on the player, use it
                {
                    // Create a throwable object at the position and rotation of the player
                    if (throwablePickup) Instantiate(throwablePickup, player.transform.position, player.transform.rotation);

                    // If we have a health pickup value, add it to the player's health, or to player 2 if it exists
                    if (healthPickup != 0)
                    {
                        if (gameController.player2Object && gameController.player2Object.health < player.health) gameController.player2Object.SendMessageUpwards("ChangeHealth", healthPickup, SendMessageOptions.DontRequireReceiver);
                        else gameController.playerObject.SendMessageUpwards("ChangeHealth", healthPickup, SendMessageOptions.DontRequireReceiver);
                    }

                    // Go to the next item in the list
                    player.NextItem();
                }

                // Add to the player score for picking up this item
                player.ChangeScore(bonus);

                // Create a pickup effect at the position/rotation of this pickup item
                if (pickupEffect) Instantiate( pickupEffect, transform.position, transform.rotation);

                // Remove the pickup item
                gameObject.SetActive(false);
            }
        }
    }
}