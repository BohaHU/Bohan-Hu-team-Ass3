using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines the attributes of the player, its health and hurt/dead status, and the current/default weapon
    /// </summary>
    public class ORSPlayer : MonoBehaviour
    {
        // Holds a reference to the gamecontroller so that we know when to go to game over screen 
        internal ORSGameController gameController;

        [Tooltip("The health of the player. If this reaches 0, the player dies")]
        public int health = 10;
        internal int healthMax;

        // The current score of this player
        internal int score = 0;

        // The score text object which displays the current score of the player
        internal Text scoreText;

        // Is the player dead?
        internal bool isDead = false;

        [Tooltip("The head of this player (Usually the camera inside the player). When assigned, enemies will look at the player head's position instead of looking at the position of the player object itself. You can use this to change the height of the player in the game")]
        public Transform playerHead;

        [Tooltip("Holds the player interface object, which has the crosshair, health, ammo, and more button UI. Can be assigned from the scene. If not assigned manually, it will be assigned automatically from the GameCanvas")]
        public RectTransform playerInterface;
        internal RectTransform crosshair;
        internal RectTransform healthGrid;
        internal RectTransform deadMessage;
        internal RectTransform ammoGrid;
        internal Button reloadObject;
        internal Button inventoryWeaponsObject;
        internal Button inventoryItemsObject;

        [Tooltip("How fast the crosshair moves when controlling it with a gamepad or keyboard")]
        public float crosshairSpeed = 1000;

        [Tooltip("The shoot button, click it or press it to shoot")]
        public string shootButton = "Fire1";

        [Tooltip("The reload button, click it or press it to reload")]
        public string reloadButton = "Fire2";

        [Tooltip("The hide button, hold it to hide behind cover if it exists in the waypoint")]
        public string hideButton = "Fire3";

        [Tooltip("The item button, click it to use the currently selected item")]
        public string useItemButton = "UseItem";

        [Tooltip("The next item button, roll it to change to the next/previous item in the invetory")]
        public string nextItemButton = "NextItem";

        [Tooltip("The next weapon button, roll it to change to the next/previous weapon in the invetory")]
        public string nextWeaponButton = "Mouse ScrollWheel";

        // Are we using the mouse now?
        internal bool usingMouse = false;

        // The position we are aiming at now
        internal Vector3 aimPosition;

        // Used to calcualte the direction the player is looking in when using free-look mode. Freelook is defined at each waypoint.
        internal Vector2 freeLookDirection;

        // The recoil value for the current weapon
        internal Vector3 currentRecoil;

        [Tooltip("The hurt time is used to make sure that the player doesn't lose too much health at once")]
        public float hurtTime = 1;
        internal float hurtTimeCount;

        [Tooltip("The sound that plays when the player loses health")]
        public AudioClip soundHurt;

        [Tooltip("The sound that plays when the player is hit, but doesn't lose health")]
        public AudioClip soundHit;

        [Tooltip("If set to true, the player will automatically hide if there is a hiding object at the waypoint the player reaches")]
        public bool autoHide = false;

        // The hittable object that the player is hiding behind. While hiding, the player will not be able to shoot or be shot
        internal ORSHittable hidingObject;

        [Tooltip("The weapons inventory of the player which can have a number of weapon slots. The first slot is the default weapon that never runs out of ammo. Weapons can also be picked up from the scene")]
        public ORSWeapon[] weapons;
        internal int weaponIndex = 0;

        [Tooltip("The point from which projectiles are shot")]
        public Transform weaponMuzzle;

        [Tooltip("The pickups inventory of the player which can have a number of pickup slots. Any item that is picked up from the scene is added to an empty slot in the inventory. If there is no empty slot, the item is used automatically")]
        public ORSPickup[] items;
        internal int itemIndex = -1;

        [Header("Deprecated fields (Don't use these)")]
        [Tooltip("(WARNING: This variable is deprectaed and will be removed in upcoming versions, use weapons variable instead, the first one being the default weapon). The main weapon used by the player. This is the default weapon that we return to after using all the ammo in a picked-up weapon")]
        public ORSWeapon currentWeapon;
        internal ORSWeapon defaultWeapon;

        //Used to calculate the accuracy of the player, measured by hit/miss ratio, used in game score stats
        internal float hitCount = 0;
        internal float shotCount = 0;

        internal int index = 0;
        internal int ammoIndex = 0;

        void Awake()
        {
            // Reset the hurt time of the player so that it doesn't lose health from the start of the game
            hurtTimeCount = hurtTime;
        }

        void Start()
        {
            // Assign the gamecontroller from the scene
            if (gameController == null) gameController = (ORSGameController)FindObjectOfType(typeof(ORSGameController));

            // If no player interface is assigned, Find it from the game canvas and assign it
            if (playerInterface == null && gameController.gameCanvas.Find("PlayerInterface")) playerInterface = gameController.gameCanvas.Find("PlayerInterface").GetComponent<RectTransform>();

            // Find the crosshair from the game canvas and assign it
            if ( crosshair == null && playerInterface.Find("Crosshair") ) crosshair = playerInterface.Find("Crosshair").GetComponent<RectTransform>();

            // Go through all the weapons and set their ammo counts
            for (weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                // If the weapon exists in the list, set its ammo count
                if ((System.Object)weapons[weaponIndex] != null)
                {
                    weapons[weaponIndex].ammoCount = weapons[weaponIndex].ammo;
                }
            }

            // If no player interface is assigned, Find it from the game canvas and assign it
            //if (playerInterface == null && gameController.gameCanvas.Find("PlayerInterface")) playerInterface = gameController.gameCanvas.Find("PlayerInterface").GetComponent<RectTransform>();

            // If no reload button is assigned, Find it from the game canvas and assign it
            if (reloadObject == null && playerInterface.Find("ButtonReload")) reloadObject = playerInterface.Find("ButtonReload").GetComponent<Button>();

            // Listen for a click on the reload button to reload the current weapon
            reloadObject.onClick.AddListener(delegate () { Reload(true); });

            // If no ammo grid is assigned, Find it from the game canvas and assign it
            if (ammoGrid == null && playerInterface.Find("AmmoGrid")) ammoGrid = playerInterface.Find("AmmoGrid").GetComponent<RectTransform>();

            // Set the first weapon in the weapons list as the default one
            if (weapons.Length > 0 && weapons[0])
            {
                weaponIndex = 0;
                
                // Set the default weapon
                SetWeapon(weaponIndex);

                // Reload the current weapon
                Reload(true);
            }
            else Debug.LogWarning("No default weapon assigned. Please assign at least one weapon in the weapons list or you won't be able to shoot");

            // Find the first item in the items invetory, if we have one
            NextItem();

            // Limit the maximum health of the player
            healthMax = health;

            // If no health grid is assigned, Find it from the game canvas and assign it
            if (healthGrid == null && playerInterface.Find("HealthGrid")) healthGrid = playerInterface.Find("HealthGrid").GetComponent<RectTransform>();

            // Set the health value and display its icons
            if (healthGrid) SetHealth();

            // If there is a dead message UI, assign it for later use ( player 2 dead message )
            if (deadMessage == null && playerInterface.Find("DeadMessage"))
            {
                deadMessage = playerInterface.Find("DeadMessage").GetComponent<RectTransform>();

                deadMessage.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            // Count down the hurt time. After this reaches 0, the player can be hurt again
            if (hurtTimeCount > 0) hurtTimeCount -= Time.deltaTime;
        }

        /// <summary>
        /// Sets the health of the player and displays the correct UI
        /// </summary>
        public void SetHealth()
        {
            // Go through each health icon and display it
            for (int healthIndex = 1; healthIndex < healthMax; healthIndex++)
            {
                // Create a new health icon
                RectTransform newHealth = Instantiate(healthGrid.Find("Health")) as RectTransform;

                // Put it inside the health grid
                newHealth.transform.SetParent(healthGrid);

                // Set the position to the default health's position
                newHealth.position = healthGrid.Find("Health").position;

                // Set the scale to the default health's scale
                newHealth.localScale = healthGrid.Find("Health").localScale;
            }
        }

        /// <summary>
        /// Goes to the next available weapon in the inventory, as long as it has ammo or it is the default weapon ( first one in weapons list ).
        /// </summary>
        public void NextWeapon()
        {
            // Hold the original weapon index so we can check if we actually changed to a different weapon or not
            int previousIndex = weaponIndex;

            // Used to check if we found a viable weapon
            bool weaponFound = false;

            // Go to the next weapon in the list
            weaponIndex++;

            // If we reached the end of the list, start from the first weapon index
            if (weaponIndex >= weapons.Length)    weaponIndex = 0;

            // Find the next weapon in the weapons list, as long as it has ammo
            while (weaponIndex < weapons.Length)
            {
                // If the weapon exists in the list, and we have ammo for it, set it as the current weapon
                if ((System.Object)weapons[weaponIndex] != null)
                {
                    // If this weapon has ammo ( or it is our default weapon that can be reloaded infinitely ), set it as our current weapon
                    if (weapons[weaponIndex].ammoCount > 0 || weaponIndex == 0 )
                    {
                        // We found a weapon, stop looking
                        weaponFound = true;
                        
                        break;
                    }
                }

                // As long as we didn't find a weapon, go to the next weapon index
                weaponIndex++;
            }

            // If we haven't found a viable weapon after reaching the end of the weapons list, just revert to the default weapon ( first in the list )
            if (weaponFound == false)    weaponIndex = 0;

            if (previousIndex != weaponIndex)
            {
                // Set the weapon we chose
                SetWeapon(weaponIndex);

                // Reload the current weapon
                Reload(false);
            }
        }

        /// <summary>
        /// Set the weapon, and displays the proper UI info
        /// </summary>
        /// <param name="weapon"></param>
        public void SetWeapon(int setValue)
        {
            // Set the current weapon
            weaponIndex = setValue;

            // If this weapon is not yet in the scene (not a clone), instantiate it
            if (!weapons[weaponIndex].name.Contains("(Clone)"))
            {
                weapons[weaponIndex] = Instantiate(weapons[weaponIndex]);

                // Set the ammo count to full based on the current weapon
                weapons[weaponIndex].ammoCount = weapons[weaponIndex].ammo;
            }

            // Reset the fire rate for this equipped weapon
            weapons[weaponIndex].fireRateCount = 0;

            // Set the ammo count to full based on the current weapon
            //weapons[weaponIndex].ammoCount = weapons[weaponIndex].ammo;
            
            // Go through all the ammo in the weapon and hide it
            for (ammoIndex = 0; ammoIndex < ammoGrid.transform.childCount; ammoIndex++)
            {
                // Hide the ammo object
                ammoGrid.transform.GetChild(ammoIndex).gameObject.SetActive(false);
            }

            // Set the size of each ammo icon in the grid based on the actual size of the texture
            ammoGrid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(weapons[weaponIndex].ammoIcon.texture.width, weapons[weaponIndex].ammoIcon.texture.height);

            // Change the texture of the crosshair based on the weapon we have
            if (crosshair.GetComponent<Image>()) crosshair.GetComponent<Image>().sprite = weapons[weaponIndex].crosshair;

            // Go through each ammo and display the correct ammo UI for it
            for (ammoIndex = 0; ammoIndex < weapons[weaponIndex].ammo; ammoIndex++)
            {
                // If there is an available ammo object, assign the ammo to it
                if (ammoIndex < ammoGrid.childCount)
                {
                    // Activate the ammo object
                    ammoGrid.transform.GetChild(ammoIndex).gameObject.SetActive(true);

                    // Set the correct ammo icon based on the current weapon
                    ammoGrid.transform.GetChild(ammoIndex).Find("AmmoIcon").GetComponent<Image>().sprite = weapons[weaponIndex].ammoIcon;
                }
                else // Otherwise if we don't have an ammo object, create a new one for the next ammo
                {
                    // Create a new ammo
                    RectTransform newAmmo = Instantiate(ammoGrid.Find("Ammo")) as RectTransform;

                    // Put it inside the ammos grid
                    newAmmo.transform.SetParent(ammoGrid);

                    // Set the position to the default ammo's position
                    newAmmo.position = ammoGrid.Find("Ammo").position;

                    // Set the scale to the default ammo's scale
                    newAmmo.localScale = ammoGrid.Find("Ammo").localScale;
                }
            }

            // If we have a weapons inventory, set the icon of the weapon in it
            if (playerInterface.Find("InventoryWeapons/Icon")) playerInterface.Find("InventoryWeapons/Icon").GetComponent<Image>().sprite = weapons[weaponIndex].ammoIcon;
            if (playerInterface.Find("Player2InventoryWeapons/Icon")) playerInterface.Find("Player2InventoryWeapons/Icon").GetComponent<Image>().sprite = weapons[weaponIndex].ammoIcon;
        }

        /// <summary>
        /// Reloads a weapon and returns to the default weapon if we run out of ammo
        /// </summary>
        /// <param name="weapon"></param>
        public void Reload(bool refillAmmo)
        {
            // If this weapon doesn't have infinite ammo (first weapon in list is default with infinite ammo), go to the next weapon
            if ( weaponIndex != 0 && weapons[weaponIndex].ammoCount <= 0)
            {
                NextWeapon();

                return;
            }

            // If we have a special weapon that has run out of ammo, revert to the default weapon
            //if (playerObject && weapons[weaponIndex].ammoCount <= 0 && weapons[weaponIndex] != defaultWeapon)
            // {
            // Set the default weapon
            //  weapons[weaponIndex] = defaultWeapon;

            // Set the weapon and show the correct UI
            // SetWeapon(weapons[weaponIndex]);
            // }

            // Set the ammo count to full
            if (refillAmmo == true) weapons[weaponIndex].ammoCount = weapons[weaponIndex].ammo;

            // If this weapon has infinite ammo, and we switch to it when it has no more ammo left, show the reload button so we can reload it. Otherwise, hide the reload button
            if ( weaponIndex == 0 && weapons[weaponIndex].ammoCount <= 0 && gameController.gameCanvas) reloadObject.gameObject.SetActive(true);
            else reloadObject.gameObject.SetActive(false);

            // Go through each element of available ammo and activate it
            for (int ammoIndex = 0; ammoIndex < weapons[weaponIndex].ammo; ammoIndex++)
            {
                // Deactivate the ammo object
                if (ammoIndex >= weapons[weaponIndex].ammoCount)
                {
                    ammoGrid.transform.GetChild(ammoIndex).gameObject.SetActive(false);
                }
                else if (weapons[weaponIndex].ammoReloadAnimation)
                {
                    ammoGrid.transform.GetChild(ammoIndex).gameObject.SetActive(true);

                    ammoGrid.GetChild(ammoIndex).Find("AmmoIcon").GetComponent<Animation>().Play(weapons[weaponIndex].ammoReloadAnimation.name);
                }
            }

            // If there is a source and a sound, play it from the source
            if (gameController.soundSource && weapons[weaponIndex].soundReload)
            {
                gameController.soundSource.pitch = Time.timeScale;

                gameController.soundSource.PlayOneShot(weapons[weaponIndex].soundReload);
            }
        }

        /// <summary>
        /// Picks up a weapon, adding it to any available slot, or refilling an empty weapon ammo
        /// </summary>
        public void PickupWeapon( ORSWeapon weapon )
        {
            // Go through all the weapons, and try to find a similar weapon to refill with ammo
            for ( index = 0; index < weapons.Length; index++ )
            {
                // If the weapon exists in the list, continue
                if ( (System.Object)weapons[index] != null )
                {
                    // Check if this is the same as the weapon we picked up ( Compare ammo icon )
                    if ( weapons[index].ammoIcon == weapon.ammoIcon)
                    {
                        // Refill the ammo of the weapon
                        weapons[index].ammoCount = weapons[index].ammo;

                        // Set the weapon we chose
                        SetWeapon(index);

                        // Reload the current weapon
                        Reload(true);

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Picks up a weapon, adding it to any available slot, or refilling an empty weapon ammo
        /// </summary>
        public bool PickupItem(ORSPickup pickup)
        {
            // Go through all the item slots, and try to find an empty one
            for (itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                // If the item slot is empty, assign the picked up item to it
                if ((System.Object)items[itemIndex] == null)
                {
                    // Assign the picked up item to the empty slot
                    items[itemIndex] = pickup;

                    // Step back in the items list and then go to the next item, which basically updates the invetory to the current item we just picked up
                    itemIndex--;
                    NextItem();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Goes to the next available pickup item in the inventory.
        /// </summary>
        public void NextItem()
        {
            // Hold the original item index so we can revert to it if we don't find an item
            int previousIndex = itemIndex;

            // Go to the next item in the list
            itemIndex++;

            // If we reached the end of the list, start from the first item index
            if ( itemIndex >= items.Length ) itemIndex = 0;

            // Find the next available item slot in the items list
            while (itemIndex < items.Length)
            {
                // If the slot has an item, set it as the current item
                if ((System.Object)items[itemIndex] != null)
                {
                    // If we have an items inventory, set the icon of the item in it
                    if (playerInterface.Find("InventoryItems/Icon"))
                    {
                        playerInterface.Find("InventoryItems/Icon").gameObject.SetActive(true);
                        playerInterface.Find("InventoryItems/Icon").GetComponent<Image>().sprite = items[itemIndex].itemIcon;

                        playerInterface.Find("InventoryItems/ButtonNext/Text").GetComponent<Text>().text = itemIndex.ToString();
                    }

                    return;
                }

                // As long as we didn't find an item, go to the next item index
                itemIndex++;
            }

            // Since we didn't find any items, revert to the previous item index
            if (itemIndex == items.Length && previousIndex < items.Length)
            {
                itemIndex = -1;

                if ((System.Object)items[0] != null) NextItem();
            }
            else itemIndex = previousIndex;
        }

        /// <summary>
        /// Uses the currently selected item, as if we just picked it up
        /// </summary>
        public void UseItem()
        {
            // Remove the item icon from the inventroy
            if (playerInterface.Find("InventoryItems/Icon")) playerInterface.Find("InventoryItems/Icon").gameObject.SetActive(false);

            if ( itemIndex >= 0 && itemIndex < items.Length && (System.Object)items[itemIndex] != null)
            {
                // Instantiate the item we are going to use
                ORSPickup tempItem = Instantiate(items[itemIndex]);
                
                // Create a throwable object at the position and rotation of the player
                if (tempItem.throwablePickup) Instantiate(tempItem.throwablePickup, transform.position, transform.rotation);

                // If we have a health pickup value, add it to the player's health
                if (tempItem.healthPickup != 0) ChangeHealth(tempItem.healthPickup);
                
                // Clear the item from the inventory
                items[itemIndex] = null;

                // Go to the next item in the inventory
                NextItem();
            }
        }

        /// <summary>
        /// Changes the health of the player, and checks if it should die
        /// </summary>
        /// <param name="changeValue"></param>
        public void ChangeHealth(int changeValue)
        {
            // If the health increases, and it hasn't reached the maximum yet. Update the health grid with a gain-health effect
            if (changeValue > 0 && health < healthMax)
            {
                // Animate the health gain based on the health increase we get
                for ( index = health; index < health + changeValue; index++)
                {
                    if (index >= healthMax) break;

                    healthGrid.GetChild(index).Find("HealthIcon").GetComponent<Animation>().Play("HealthGain");
                }
            }

            // Change health value, limited between 0 and max health value
            health = Mathf.Clamp(health + changeValue, 0, healthMax);

            // If the health decreases, hurt or hit the player
            if ( changeValue < 0 )
            {
                // If the hurt time reached 0, hurt the player again
                if ( hurtTimeCount <= 0 )
                {
                    hurtTimeCount = hurtTime;

                    // Animate the health loss
                    healthGrid.GetChild(health).Find("HealthIcon").GetComponent<Animation>().Play("HealthLose");
                }
            }

            // If health reaches 0, the player should die
            if (health <= 0) Die();
        }
        
        /// <summary>
        /// Kills the object and gives it a random animation from a list of death animations
        /// </summary>
        public void Die()
        {
            // The player can only die once
            if (isDead == false)
            {
                // The player is now dead. It can't move.
                isDead = true;

                // If there is a second player, disable its controls but don't end the game
                if ( gameController.player2Object && this == gameController.player2Object)
                {
                    // Show the dead message for player 2, and take it out of the interface hierarchy so we can hide the rest of the interface
                    if (deadMessage)
                    {
                        deadMessage.gameObject.SetActive(true);

                        deadMessage.SetParent(playerInterface.parent);
                    }

                    // Hide the interface object
                    playerInterface.gameObject.SetActive(false);
                }
                else // If player 1 is dead, end the game
                {
                    if (GetComponent<Rigidbody>())
                    {
                        // The player can now be affected by physics and gravity, so that it can fall down on the ground
                        GetComponent<Rigidbody>().isKinematic = false;

                        // Add a physics explosion to push the player away
                        GetComponent<Rigidbody>().AddExplosionForce(100, transform.position + Vector3.forward, 1, 1);
                    }

                    // Activate the player collider if it exists
                    if (GetComponent<Collider>()) GetComponent<Collider>().enabled = true;

                    // Stop the animation of the player
                    if (GetComponent<Animation>()) GetComponent<Animation>().Stop();

                    // Remove the game UI
                    gameController.gameCanvas.gameObject.SetActive(false);

                    //Start the game over event with a delay of 2 seconds
                    gameController.SendMessage("GameOver", 2);
                }
            }
        }

        /// <summary>
        /// Change the score and updates it for this player
        /// </summary>
        /// <param name="changeValue">Change value</param>
        public void ChangeScore(int changeValue)
        {
            score += changeValue;

            //Update the score text
            if (scoreText)
            {
                scoreText.text = score.ToString();

                // Play the score pop animation
                if (scoreText.GetComponent<Animation>())
                {
                    scoreText.GetComponent<Animation>().Stop();
                    scoreText.GetComponent<Animation>().Play();
                }
            }
        }
    }
}