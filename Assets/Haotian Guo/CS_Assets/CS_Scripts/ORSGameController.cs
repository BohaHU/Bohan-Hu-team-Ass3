using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OnRailsShooter.Types;

namespace OnRailsShooter
{           
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over or victory.
	/// </summary>
	public class ORSGameController : MonoBehaviour 
	{
        [Tooltip("The player object which moves and shoot. Must be assigned from the scene")]
        public ORSPlayer playerObject;
        internal Animation playerAnimation;

        [Tooltip("The second player object, which has its own health, weapons, and items. Movement, looking, and hiding are controlled by player 1")]
        public ORSPlayer player2Object;

        // Checks if the player is moving now
        internal bool playerMoving = false;
        
        [Tooltip("The first waypoint in the game. Assign this from the scene. This is a useful way to test the game by jumping into a waypoint without having to go through all the ones before it")]
        public ORSWaypoint currentWaypoint;

        [Tooltip("The waypoint arrow object that appears at a crossroads that leads to several paths")]
        public ORSWaypointArrow waypointArrow;

        // The number of enemies left to be killed before we can move to the next waypoint
        internal int enemiesLeft = 0;

        [Tooltip("How long to wait before starting the game.")]
        public float startDelay = 1;

        // The highscore of the player ( not used anymore after player stats update )
        internal int highScore = 0;
        
        // Check if we are on a mobile device and applies different behaviour such as hiding the cursor when not in use
        internal bool isMobile = false;
        
        // Various canvases for the UI
        public Transform gameCanvas;
        public Transform pauseCanvas;
		public Transform gameOverCanvas;
		public Transform victoryCanvas;

		// Is the game over?
		internal bool  isGameOver = false;
		
		// The level of the main menu that can be loaded after the game ends
		public string mainMenuLevelName = "StartMenu";

        // The button that pauses the game. Clicking on the pause button in the UI also pauses the game
        public string pauseButton = "Cancel";
        internal bool isPaused = false;

        // Various sounds and their source
        public string soundSourceTag = "Sound";
		internal AudioSource soundSource;

        // The button that will restart the game after game over
        public string confirmButton = "Submit";
		
		// A general use index
		internal int index = 0;

        [Tooltip("The bonus we get based on our accuracy at the end of the game. Accuracy is measured by hit/miss ratio")]
        public float accuracyBonus = 10000;

        [Tooltip("The bonus we get based on our health at the end of the game. Health is measured by health/healthMax ratio")]
        public float healthBonus = 5000;

        [Tooltip("The bonus we get based on our how far in the level we got. Completion is measured by currentWaypoint/totalWaypoints ratio")]
        public float completionBonus = 10000;

        [Tooltip("The ranks we get based on our score at the end of the game. Each rank has a unique icon")]
        public Rank[] gameEndRanks;

		void Awake()
		{
            // Activate the pause canvas early on, so it can detect info about sound volume state
            if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
        }

        void OnValidate()
        {
            // Set the position and rotation of the player to the initial waypoint
            if ( currentWaypoint )
            {
                // Set position and rotation for player 1
                if (playerObject )
                {
                    playerObject.transform.position = currentWaypoint.transform.position;
                    playerObject.transform.rotation = currentWaypoint.transform.rotation;
                }

                // Set position and rotation for player 2
                if (player2Object )
                {
                    player2Object.transform.position = currentWaypoint.transform.position;
                    player2Object.transform.rotation = currentWaypoint.transform.rotation;
                }
            }
        }

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			// Make sure the time scale is reset to 1 when starting the game
			Time.timeScale = 1;

            // Disable multitouch so that we don't tap two answers at the same time ( prevents multi-answer cheating, thanks to Miguel Paolino for catching this bug )
            // If there are two players, enable multi touch
            if (player2Object) Input.multiTouchEnabled = false;
            
            //Hide the cavases
            if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( victoryCanvas )    victoryCanvas.gameObject.SetActive(false);
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);

            // If there is a second player, set the aiming reticles side by side in the center of the screen
            if ( player2Object )
            {
                // Set the aiming positions for both players to the center of the screen
                playerObject.aimPosition = new Vector3(Screen.width * 0.3f, Screen.height * 0.5f, playerObject.aimPosition.z);
                player2Object.aimPosition = new Vector3(Screen.width * 0.7f, Screen.height * 0.5f, player2Object.aimPosition.z);
            }
            else
            {
                // Set the aiming position to the center of the screen
                playerObject.aimPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, playerObject.aimPosition.z);
            }


            // Hide the reload button, if it exists inside the game canvas
            if ( gameCanvas )
            {
                // Assign the score text object so we can update it
                if (gameCanvas.Find("TextScore"))
                {
                    playerObject.scoreText = gameCanvas.Find("TextScore").GetComponent<Text>();

                    // Set the score at the start of the game
                    playerObject.ChangeScore(0);
                }

                // Assign the score text object for player 2 so we can update it
                if (player2Object && gameCanvas.Find("TextScorePlayer2"))
                {
                    player2Object.scoreText = gameCanvas.Find("TextScorePlayer2").GetComponent<Text>();

                    // Set the score at the start of the game
                    player2Object.ChangeScore(0);
                }

                // Check if we are running on a mobile device. If so, remove the crosshair as we don't need it for taps
                if (Application.isMobilePlatform)
                {
                    isMobile = true;

                    // If we have a player assigned
                    if (playerObject)
                    {
                        // If a crosshair is assigned, hide it
                        if (playerObject.crosshair) playerObject.crosshair.GetComponent<Image>().enabled = false;

                        //crosshair = null; // If is is uncommented it will cause aiming not to run at all on mobile
                    }

                    // If we have a player assigned
                    if (player2Object)
                    {
                        // If a crosshair is assigned, hide it
                        if (player2Object.crosshair) player2Object.crosshair.GetComponent<Image>().enabled = false;

                        //crosshair = null; // If is is uncommented it will cause aiming not to run at all on mobile
                    }
                }

                // Hide the reload button
                if (gameCanvas.Find("ButtonReload")) gameCanvas.Find("ButtonReload").gameObject.SetActive(false);
                if (gameCanvas.Find("Player2ButtonReload")) gameCanvas.Find("Player2ButtonReload").gameObject.SetActive(false);

                // Deactivate the hurt effect at the start of the game
                if (gameCanvas.Find("HurtEffect")) gameCanvas.Find("HurtEffect").gameObject.SetActive(false);
                if (gameCanvas.Find("Player2HurtEffect")) gameCanvas.Find("Player2HurtEffect").gameObject.SetActive(false);
            }

            //Get the highscore for the player
            highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HighScore", 0);

            //Assign the sound source for easier access
            if (GameObject.FindGameObjectWithTag(soundSourceTag)) soundSource = GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>();

            // If we have a player and an initial waypoint assigned
            if ( playerObject && currentWaypoint )
            {
                // Start the animation of the player
                if (playerObject.GetComponent<Animation>()) playerAnimation = playerObject.GetComponent<Animation>();

                // Set the position and rotation of the player to the initial waypoint
                playerObject.transform.position = currentWaypoint.transform.position;
                playerObject.transform.rotation = currentWaypoint.transform.rotation;

                // We reached a waypoint, which may have pickups, enemies, or multiple paths to choose from
                WaypointReached();
            }
        }

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
            // Delay the start of the game
            if ( startDelay > 0 )
			{
				startDelay -= Time.deltaTime;
            }
			else
			{
				//If the game is over, listen for the Restart and MainMenu buttons
				if ( isGameOver == true )
				{
					//The jump button restarts the game
					if ( Input.GetButtonDown(confirmButton) )
					{
						Restart();
					}
					
					//The pause button goes to the main menu
					if ( Input.GetButtonDown(pauseButton) )
					{
						MainMenu();
					}
				}
				else
				{
                    // If player is not moving and there are enemies at the waypoint and the player is not hiding, keep them at the center of the camera frame
                    if ( currentWaypoint.enemies.Length > 0 && currentWaypoint.lookAtEnemies == true && playerMoving == false && playerObject.hidingObject == null )
                    {
                        // Set initial camera center
                        Vector3 cameraCenter = Vector3.zero;
                        int objectsInFocus = 0;

                        // Go through all the enemies and add them to the center calculation
                        for (index = 0; index < currentWaypoint.enemies.Length; index++)
                        {
                            cameraCenter += currentWaypoint.enemies[index].enemy.transform.position;

                            objectsInFocus++;
                        }

                        // Go through all the pickups and add them to the center calculation
                        for (index = 0; index < currentWaypoint.pickups.Length; index++)
                        {
                            cameraCenter += currentWaypoint.pickups[index].transform.position;

                            objectsInFocus++;
                        }
                        
                        // If there is a player head assigned, make it smoothly rotate to look at the center between all enemies
                        if (playerObject.playerHead) playerObject.playerHead.rotation = Quaternion.Slerp(playerObject.playerHead.rotation, Quaternion.LookRotation((cameraCenter / objectsInFocus) - playerObject.playerHead.position), Time.deltaTime * 3);
                        else playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation, Quaternion.LookRotation((cameraCenter / objectsInFocus) - playerObject.transform.position), Time.deltaTime * 3);

                        // Smoothly rotate to look at the center between all enemies
                        //playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation, Quaternion.LookRotation((cameraCenter / objectsInFocus) - playerObject.transform.position), Time.deltaTime * 3);
                    }

                    // Calculate aiming for both players
                    if ( playerObject ) Aiming(playerObject);
                    if ( player2Object ) Aiming(player2Object);

                    // If player is not moving and not hiding, allow the player to look freely within the limits of the freeLook angle
                    if (playerMoving == false && playerObject.hidingObject == null && currentWaypoint.lookAtEnemies == false )
                    {
                        // If there is a player head assigned, make it rotate instead of the body
                        if (playerObject.playerHead) playerObject.playerHead.eulerAngles = currentWaypoint.transform.eulerAngles + new Vector3(-playerObject.freeLookDirection.y, playerObject.freeLookDirection.x, 0);
                        else playerObject.transform.eulerAngles = currentWaypoint.transform.eulerAngles + new Vector3(-playerObject.freeLookDirection.y, playerObject.freeLookDirection.x, 0);

                        //playerObject.transform.eulerAngles = currentWaypoint.transform.eulerAngles + new Vector3(-playerObject.freeLookDirection.y, playerObject.freeLookDirection.x, 0);
                    }

                    // Count the rate of fire for player 1
                    //print(playerObject.weaponIndex);

                    if ( playerObject.weapons[playerObject.weaponIndex].fireRateCount < playerObject.weapons[playerObject.weaponIndex].fireRate ) playerObject.weapons[playerObject.weaponIndex].fireRateCount += Time.deltaTime;
                    else if ( isMobile == true && playerObject.crosshair.GetComponent<Image>().enabled == true ) playerObject.crosshair.GetComponent<Image>().enabled = false; // If a crosshair is assigned on a mobile device, hide it when not shooting

                    // Count the rate of fire for player 2
                    if ( player2Object )
                    {
                        if (player2Object.weapons[player2Object.weaponIndex].fireRateCount < player2Object.weapons[player2Object.weaponIndex].fireRate) player2Object.weapons[player2Object.weaponIndex].fireRateCount += Time.deltaTime;
                        else if (isMobile == true && player2Object.crosshair.GetComponent<Image>().enabled == true) player2Object.crosshair.GetComponent<Image>().enabled = false; // If a crosshair is assigned on a mobile device, hide it when not shooting
                    }

                    // If we press the shoot button, SHOOT!
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        // If the player is not hiding behind cover, it can shoot, reload, and pick up items
                        if (playerObject.hidingObject == null)
                        {
                            // If we are on a mobile device and we have a second player, check if we are in the player 2 side of the screen
                            if (isMobile == true && player2Object && player2Object.isDead == false)
                            {
                                if ( Input.mousePosition.x > Screen.width * 0.5f || player2Object.isDead == true )
                                {
                                    // Check if we have an automatic weapon, a single shot weapon, or if we ran out of ammo
                                    if (player2Object.weapons[player2Object.weaponIndex].autoFire == false && Input.GetButtonDown(playerObject.shootButton)) Shoot(player2Object);
                                    else if (player2Object.weapons[player2Object.weaponIndex].autoFire == true && Input.GetButton(playerObject.shootButton)) Shoot(player2Object);
                                    else if (player2Object.weapons[player2Object.weaponIndex].ammoCount <= 0 && Input.GetButtonDown(playerObject.reloadButton)) player2Object.Reload(true);

                                    // Check if we picked up an item
                                    if (Input.GetButton(playerObject.shootButton)) PickUpItem(player2Object);
                                }
                                else
                                {
                                    // Check if we have an automatic weapon, a single shot weapon, or if we ran out of ammo
                                    if (playerObject.weapons[playerObject.weaponIndex].autoFire == false && Input.GetButtonDown(playerObject.shootButton)) Shoot(playerObject);
                                    else if (playerObject.weapons[playerObject.weaponIndex].autoFire == true && Input.GetButton(playerObject.shootButton)) Shoot(playerObject);
                                    else if (playerObject.weapons[playerObject.weaponIndex].ammoCount <= 0 && Input.GetButtonDown(playerObject.reloadButton)) playerObject.Reload(true);

                                    // Check if we picked up an item
                                    if (Input.GetButton(playerObject.shootButton)) PickUpItem(playerObject);
                                }
                            }
                            else
                            {
                                // Check if we have an automatic weapon, a single shot weapon, or if we ran out of ammo
                                if (playerObject.weapons[playerObject.weaponIndex].autoFire == false && Input.GetButtonDown(playerObject.shootButton)) Shoot(playerObject);
                                else if (playerObject.weapons[playerObject.weaponIndex].autoFire == true && Input.GetButton(playerObject.shootButton)) Shoot(playerObject);
                                else if (playerObject.weapons[playerObject.weaponIndex].ammoCount <= 0 && Input.GetButtonDown(playerObject.reloadButton)) playerObject.Reload(true);

                                // Check if we picked up an item
                                if (Input.GetButton(playerObject.shootButton)) PickUpItem(playerObject);
                            }

                            if (isMobile == false && player2Object)
                            {
                                // Check if we have an automatic weapon, a single shot weapon, or if we ran out of ammo
                                if (player2Object.weapons[player2Object.weaponIndex].autoFire == false && Input.GetButtonDown(player2Object.shootButton)) Shoot(player2Object);
                                else if (player2Object.weapons[player2Object.weaponIndex].autoFire == true && Input.GetButton(player2Object.shootButton)) Shoot(player2Object);
                                else if (player2Object.weapons[player2Object.weaponIndex].ammoCount <= 0 && Input.GetButtonDown(player2Object.reloadButton)) player2Object.Reload(true);

                                // Check if we picked up an item
                                if (Input.GetButton(player2Object.shootButton)) PickUpItem(player2Object);
                            }
                        }

                        // Check if we are pressing the hide button
                        if (currentWaypoint.hidingObject && playerMoving == false)
                        {
                            if (playerObject.autoHide == true)
                            {
                                // We can get out of hiding only if we press the hide button when it exists, or when the hiding object is destroyed. We hide again when we release the hide button.
                                if (Input.GetButtonUp(playerObject.hideButton) && currentWaypoint.hidingObject.gameObject.activeSelf == true) StartCoroutine("StartHiding");
                                else if (Input.GetButtonDown(playerObject.hideButton) || currentWaypoint.hidingObject.gameObject.activeSelf == false) StartCoroutine("StopHiding");
                            }
                            else
                            {
                                // We can hide only if we press the hide button when it exists. We stop hiding when we release the hide button, or when the hiding object is destroyed.
                                if (Input.GetButtonDown(playerObject.hideButton) && currentWaypoint.hidingObject.gameObject.activeSelf == true) StartCoroutine("StartHiding");
                                else if (Input.GetButtonUp(playerObject.hideButton) || currentWaypoint.hidingObject.gameObject.activeSelf == false) StartCoroutine("StopHiding");
                            }
                        }

                        // Try to use the current item in the items inventory
                        if (Input.GetButtonDown(playerObject.useItemButton)) playerObject.UseItem();
                        if (player2Object && Input.GetButtonDown(player2Object.useItemButton)) player2Object.UseItem();

                        // Go to the next item in the inventory
                        if (Input.GetButtonDown(playerObject.nextItemButton)) playerObject.NextItem();
                        if (player2Object && Input.GetButtonDown(player2Object.nextItemButton)) player2Object.NextItem();

                        // Go to the next weapon in the inventory
                        if (Input.GetButtonDown(playerObject.nextWeaponButton) ) playerObject.NextWeapon();
                        else if (Input.GetAxisRaw("Mouse ScrollWheel") != 0) playerObject.NextWeapon(); //(mouse controls Axis Button case for player 1 only)

                        // Go to the next weapon in the inventory player 2
                        if ( player2Object && Input.GetButtonDown(player2Object.nextWeaponButton) ) player2Object.NextWeapon();
                        
                    }

                    // Check if we killed all enemies at a waypoint, and move to the next
                    if (currentWaypoint)
                    {
                        if (currentWaypoint.enemies.Length > 0)
                        {
                            // If there are no more enemies left, move to the next waypoint
                            if ((currentWaypoint.waitForEnemies == false || enemiesLeft <= 0) && playerMoving == false)
                            {
                                StartCoroutine(MoveToWaypoint(currentWaypoint.nextWaypoint[0]));

                                if (currentWaypoint.enemies.Length > 0 && currentWaypoint.removeEnemiesDelay > 0 )
                                {
                                    // Go through all the enemies and deactivate them
                                    for (index = 0; index < currentWaypoint.enemies.Length; index++)
                                    {
                                        if (currentWaypoint.enemies[index].enemy)
                                        {
                                            // Deactivate the enemy after a delay
                                            StartCoroutine(DespawnEnemy(currentWaypoint.enemies[index].enemy.gameObject, currentWaypoint.removeEnemiesDelay));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Toggle pause/unpause in the game
                    if ( Input.GetButtonDown(pauseButton) )
                    {
                    	if ( isPaused == true )    Unpause();
                    	else    Pause(true);
                    }
                }
            }
		}

        public void Aiming( ORSPlayer player )
        {
            // Keyboard and Gamepad controls
            if (player.crosshair)
            {
                // Only the first player can use the mouse
                if (player != player2Object || isMobile == true )
                {
                    // If we move the mouse in any direction, then mouse controls take effect ( Disabled for player 2 )
                    if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetMouseButtonDown(0) || Input.touchCount > 0) player.usingMouse = true;

                    // We are using the mouse, hide the crosshair
                    if (player.usingMouse == true)
                    {
                        // Calculate the mouse/tap position
                        player.aimPosition = Input.mousePosition + player.currentRecoil;
                    }
                }

                // if we have two players in the game, only the second player can use keyboard
                if ( player == playerObject )
                {
                    // If we press gamepad or keyboard arrows, then mouse controls are turned off
                    if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                    {
                        player.usingMouse = false;

                        // Move the crosshair based on gamepad/keyboard directions
                        player.aimPosition += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), player.aimPosition.z) * player.crosshairSpeed * Time.deltaTime;
                    }
                }
                // If we have a second player using the gamepad, use "HorizontalPlayer2" and "VerticalPlayer2" as the axis for aiming
                else if ( player == player2Object )
                {
                    // If we press gamepad or keyboard arrows, then mouse controls are turned off
                    if (Input.GetAxisRaw("HorizontalPlayer2") != 0 || Input.GetAxisRaw("VerticalPlayer2") != 0)
                    {
                        player.usingMouse = false;

                        // Move the crosshair based on gamepad/keyboard directions
                        player.aimPosition += new Vector3(Input.GetAxis("HorizontalPlayer2"), Input.GetAxis("VerticalPlayer2"), player.aimPosition.z) * player.crosshairSpeed * Time.deltaTime;
                    }
                }

                // Limit the position of the crosshair to the edges of the screen
                // Limit to the left screen edge
                if (player.aimPosition.x < 0) player.aimPosition = new Vector3(0, player.aimPosition.y, player.aimPosition.z);

                // Limit to the right screen edge
                if (player.aimPosition.x > Screen.width) player.aimPosition = new Vector3(Screen.width, player.aimPosition.y, player.aimPosition.z);

                // Limit to the bottom screen edge
                if (player.aimPosition.y < 0) player.aimPosition = new Vector3(player.aimPosition.x, 0, player.aimPosition.z);

                // Limit to the top screen edge
                if (player.aimPosition.y > Screen.height) player.aimPosition = new Vector3(player.aimPosition.x, Screen.height, player.aimPosition.z);

                // Place the crosshair at the position of the mouse/tap, with an added offset
                player.crosshair.position = player.aimPosition;

                // Calculate the free look direction value based on the crosshair position relative to the center of the screen
                player.freeLookDirection = new Vector2((player.aimPosition.x - Screen.width * 0.5f) / Screen.width, (player.aimPosition.y - Screen.height * 0.5f) / Screen.height) * 2 * currentWaypoint.freeLook;

                // If we have two players, disable the freelook feature
                if (player2Object ) player.freeLookDirection = Vector2.zero;

                // Gradually reset the recoil value
                player.currentRecoil = Vector3.Slerp(player.currentRecoil, Vector3.zero, Time.deltaTime * 3);
            }
        }

        /// <summary>
        /// Moves the player object to a target waypoint at a set speed and rotation
        /// </summary>
        /// <param name="targetWaypoint"></param>
        /// <returns></returns>
        IEnumerator MoveToWaypoint( ORSWaypoint targetWaypoint )
        {
            // Deactivate the hide button while we move
            gameCanvas.Find("ButtonHide").gameObject.SetActive(false);
            
            if (playerObject && targetWaypoint )
            {
                // The player is moving
                playerMoving = true;

                // Stop hiding
                if (currentWaypoint.hidingObject )    StartCoroutine("StopHiding");
                
                // Delay the movement to the next waypoint a little
                yield return new WaitForSeconds(currentWaypoint.moveDelay);

                // Play the start animation for this waypoint. This is the animation that plays when we start moving.
                if ( currentWaypoint.startAnimation )
                {
                    // Stop the animation so we can switch to it quickly
                    playerAnimation.Stop();

                    // Play the animation
                    playerAnimation.Play(currentWaypoint.startAnimation.name);
                }

                // As long as the player hasn't reached the target waypoint, keep moving towards it
                while ( Vector3.Distance(playerObject.transform.position, targetWaypoint.transform.position) > 0.5f || (Quaternion.Angle(playerObject.transform.rotation, targetWaypoint.transform.rotation) > 1 && currentWaypoint.lookAtWaypoint == false) )
                {
                    // If the game is over, don't move at all
                    if (isGameOver == true)
                    {
                        playerAnimation.Stop();

                        break;
                    }

                    // Wait a frame
                    yield return new WaitForSeconds(Time.deltaTime);

                    // Move the player towards the target waypoint
                    playerObject.transform.position = Vector3.MoveTowards(playerObject.transform.position, targetWaypoint.transform.position, Time.deltaTime * currentWaypoint.moveSpeed);

                    // If there is a second player, set its position to be the same as player 1
                    if (player2Object) player2Object.transform.position = playerObject.transform.position;

                    // Increase the speed using acceleration
                    currentWaypoint.moveSpeed += currentWaypoint.moveAcceleration;

                    // If we haven't reached the target waypoint yet, rotate the player towards the target rotation ( based on the waypoint's rotation ) at a constant speed
                    if ( Vector3.Distance(playerObject.transform.position, targetWaypoint.transform.position) > 0.5f )
                    {
                        // Smoothly rotate to look at the next waypoint position, otherwise rotate to match the rotation of the camera at the next waypoint
                        if ( currentWaypoint.lookAtWaypoint == true )    playerObject.transform.rotation = Quaternion.RotateTowards(playerObject.transform.rotation, Quaternion.LookRotation(targetWaypoint.transform.position - playerObject.transform.position), Time.deltaTime * currentWaypoint.turnSpeed);
                        else    playerObject.transform.rotation = Quaternion.RotateTowards(playerObject.transform.rotation, targetWaypoint.transform.rotation, Time.deltaTime * currentWaypoint.turnSpeed);

                        // Smoothly rotate to match the rotation of the camera at the next waypoint
                        //playerObject.transform.rotation = Quaternion.RotateTowards(playerObject.transform.rotation, targetWaypoint.transform.rotation, Time.deltaTime * currentWaypoint.turnSpeed);
                    }
                    else // Otherwise, if we reached the target, rotate quickly to align with the target waypoint rotation
                    {
                        // Stop the player animation
                        playerAnimation.Stop();
                        
                        // Rotate quickly to align with the target waypoint rotation
                        playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation, targetWaypoint.transform.rotation, Time.deltaTime * currentWaypoint.turnSpeed * 0.1f);

                        // If there is a second player, set its rotation to be the same as player 1
                        if (player2Object) player2Object.transform.rotation = playerObject.transform.rotation;

                    }
                }

                if ( isGameOver == false )
                {
                    // If we reached the target waypoint, play the end animation
                    if (currentWaypoint.endAnimation)
                    {
                        playerAnimation.Stop();

                        playerAnimation.Play(currentWaypoint.endAnimation.name);
                    }

                    // Set the target waypoint as the one we just reached
                    currentWaypoint = targetWaypoint;

                    // We reached a waypoint, which may have pickups, enemies, or multiple paths to choose from
                    WaypointReached();
                }
            }
        }

        /// <summary>
        /// We reached a waypoint, which may have pickups, enemies, or multiple paths to choose from
        /// </summary>
        public void WaypointReached()
        {
            // Display a message for this waypoint, if it exists
            if (currentWaypoint.messageScreen) Instantiate(currentWaypoint.messageScreen);

            // Go through all the targeted objects and run the special functions on them
            currentWaypoint.specialFunctions.Invoke();
             
            // If there are pickups at this waypoint, activate them
            if (currentWaypoint.pickups.Length > 0)
            {
                // Go through all the pickups and activate them
                for (index = 0; index < currentWaypoint.pickups.Length; index++)
                {
                    // Activate the pickup
                    if (currentWaypoint.pickups[index]) currentWaypoint.pickups[index].gameObject.SetActive(true);
                }

                // The player is not moving anymore
                playerMoving = false;
            }

            // If there are enemies at this waypoint, activate them
            if (currentWaypoint.enemies.Length > 0)
            {
                // Set the number of enemies left at this waypoint. You must kill all enemies at a waypoint before moving on to the next one
                enemiesLeft = currentWaypoint.enemies.Length;

                // Go through all the enemies and activate them
                for (index = 0; index < currentWaypoint.enemies.Length; index++)
                {
                    if (currentWaypoint.enemies[index].enemy)
                    {
                        // Activate the enemy after a delay
                        StartCoroutine(SpawnEnemy(currentWaypoint.enemies[index].enemy, currentWaypoint.enemies[index].spawnDelay));
                    }
                }

                // If we need to kill all enemies at the waypoint before moving on, stop.
                playerMoving = false;

                // If we have an object we can hide behind on this waypoint, show the 'HIDE' button and activate it
                if ( currentWaypoint.hidingObject != null )
                {
                    // Show the hide button
                    gameCanvas.Find("ButtonHide").gameObject.SetActive(true);

                    // If Auto-Hide is set to true, immediately hide and set the button to get out of hiding
                    if ( playerObject.autoHide == true )
                    {
                        // Immediately hide when reaching this waypoint
                        StartCoroutine("StartHiding");

                        // Listen for a click down on the hide button to hide behind any available cover
                        gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine("StopHiding"); });

                    }
                    else // Otherwise, just listen for a click to hide
                    {
                        // Listen for a click down on the hide button to hide behind any available cover
                        gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine("StartHiding"); });
                    }
                }
            }
            else
            {
                // If we have more than one waypoint, display arrows to choose a path
                if (currentWaypoint.nextWaypoint.Length > 1 && waypointArrow)
                {
                    // Show arrows that lead to all the waypoint from this point
                    for (index = 0; index < currentWaypoint.nextWaypoint.Length; index++)
                    {
                        // Create a waypoint arrow at the position of this point
                        ORSWaypointArrow newArrow = Instantiate(waypointArrow, currentWaypoint.transform.position, Quaternion.identity) as ORSWaypointArrow;

                        // Make the arrow look at the next waypoint where it will lead to
                        newArrow.transform.LookAt(currentWaypoint.nextWaypoint[index].transform.position);

                        // Set the target waypoint for this waypoint arrow, so that when we click it we go to the correct waypoint
                        newArrow.GetComponent<ORSWaypointArrow>().targetWaypoint = currentWaypoint.nextWaypoint[index];
                    }
                }
                else if ( currentWaypoint.nextWaypoint.Length <= 0 )
                {
                    // If there is no next waypoint, then we reached the end of the path and we win
                    StartCoroutine(Victory(0.5f));
                }
                else
                {
                    // If there is a next waypoint, go to it
                    StartCoroutine(MoveToWaypoint(currentWaypoint.nextWaypoint[0]));
                }
            }
        }

        /// <summary>
        /// Activates an object after a delay
        /// </summary>
        /// <param name="activatedObject"></param>
        /// <param name="activateDelay"></param>
        /// <returns></returns>
        IEnumerator SpawnEnemy( ORSEnemy spawnedEnemy, float activateDelay)
        {
            // For some time
            yield return new WaitForSeconds(activateDelay);

            // Activate the object if it ist not already activated
            if ( spawnedEnemy.isSpawned == false )    spawnedEnemy.gameObject.SetActive(true);

            // Play the "Spawn" animation
            if (spawnedEnemy.GetComponent<Animator>()) spawnedEnemy.GetComponent<Animator>().Play("Spawn");

            // Create a spawn effect where this enemy is activated
            if (spawnedEnemy.spawnEffect) Instantiate(spawnedEnemy.spawnEffect, spawnedEnemy.transform.position, spawnedEnemy.transform.rotation);

            // If this enemy was killed before, reset its values for a respawn
            if ( spawnedEnemy.isDead == true )
            {
                // Enemy is not dead anymore
                spawnedEnemy.isDead = false;

                // If there is already a death effect spawned on this enemy, hide it
                if (spawnedEnemy.currentDeathEffect) spawnedEnemy.currentDeathEffect.gameObject.SetActive(false);

                // Return to the original object color
                if (spawnedEnemy.flashObject) spawnedEnemy.flashObject.material.SetColor("_EmissionColor", spawnedEnemy.defaultColor);

                // Set the health of the enemy
                spawnedEnemy.health = spawnedEnemy.healthMax;

                // Update the health bar
                spawnedEnemy.ChangeHealth(0);
            }
        }

        /// <summary>
        /// Deactivates an object after a delay
        /// </summary>
        /// <param name="deactivatedObject"></param>
        /// <param name="deactivateDelay"></param>
        /// <returns></returns>
        IEnumerator DespawnEnemy(GameObject deactivatedObject, float deactivateDelay)
        {
            // For some time
            yield return new WaitForSeconds(deactivateDelay);

            // Activate the object
            deactivatedObject.SetActive(false);
        }

        /// <summary>
        /// Creates a player hurt effect on the screen with some camera shake
        /// </summary>
        public void HurtEffect( Sprite hurtEffect)
        {
            // Choose a random position in the screen to display the hurt effect
            Vector2 hurtEffectPosition = new Vector2(Random.Range(-300, 300), Random.Range(-200, 200));

            // Shake the camera based on the hurt position
            Camera.main.GetComponent<ORSCameraShake>().cameraTurn = new Vector3(hurtEffectPosition.x, hurtEffectPosition.y, hurtEffectPosition.x * 0.3f) * 0.1f;

            if ( playerObject )
            {
                // If we have a hurt effect, display it
                if ( gameCanvas && gameCanvas.Find("HurtEffect") )
                {
                    // Activate the hurt effect
                    gameCanvas.Find("HurtEffect").gameObject.SetActive(true);

                    // Give it a random rotation
                    gameCanvas.Find("HurtEffect").GetComponent<RectTransform>().rotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

                    // Set its position based on the random value we chose 
                    gameCanvas.Find("HurtEffect").GetComponent<RectTransform>().anchoredPosition = hurtEffectPosition;

                    // Assign the hurt effect sprite
                    gameCanvas.Find("HurtEffect").GetComponent<Image>().sprite = hurtEffect;

                    // Play the hurt animation, and reset it if it's already playing
                    gameCanvas.Find("HurtEffect").GetComponent<Animation>().Stop();
                    gameCanvas.Find("HurtEffect").GetComponent<Animation>().Play();
                }
            
                //If there is a source and a sound, play it from the source
                if ( soundSource && playerObject.soundHurt)
                {
                    soundSource.pitch = Time.timeScale;

                    soundSource.PlayOneShot(playerObject.soundHurt);
                }
            }
        }

        /// <summary>
        /// Hits the player without showing a hurt effect
        /// </summary>
        public void HitPlayer()
        {
            // Choose a random position in the screen to shake the camera at
            Vector2 hurtEffectPosition = new Vector2(Random.Range(-300, 300), Random.Range(-200, 200));
            
            // Shake the camera based on the random position we chose
            Camera.main.GetComponent<ORSCameraShake>().cameraTurn = new Vector3(hurtEffectPosition.x * 0.3f, hurtEffectPosition.y * 0.3f, hurtEffectPosition.x * 0.3f) * 0.1f;

            if (playerObject)
            {
                // If there is a source and a sound, play it from the source
                if (soundSource && playerObject.soundHit )
                {
                    soundSource.pitch = Time.timeScale;

                    soundSource.PlayOneShot(playerObject.soundHit);
                }
            }
        }

        /// <summary>
        /// Shoots from the player position to the crosshair position and checks if we hit anything
        /// </summary>
        /// <param name="playerObject"></param>
        public void Shoot(ORSPlayer playerObject)
        {
            // If the player is dead, we can't shoot (duh!)
            if (playerObject.isDead == true) return;

            // If we have a player and a weapon, shoot
            if ( playerObject && playerObject.weapons[playerObject.weaponIndex] )
            {
                // If we reached the firerate count, we can shoot
                if (playerObject.weapons[playerObject.weaponIndex].fireRateCount >= playerObject.weapons[playerObject.weaponIndex].fireRate)
                {
                    // Reset the firerate
                    playerObject.weapons[playerObject.weaponIndex].fireRateCount = 0;

                    // If we have ammo in the weapon, shoot!
                    if ( playerObject.weapons[playerObject.weaponIndex].ammoCount > 0 )
                    {
                        // Reduce the ammo count
                        playerObject.weapons[playerObject.weaponIndex].ammoCount--;

                        // Add to the shot count when shooting. This is used to calculate accuracy (hitCount/shotCount)
                        playerObject.shotCount++;

                        // Play the shoot animation of the ammo object. This is the animation of bullets flying off
                        if (gameCanvas)
                        {
                            if (playerObject.weapons[playerObject.weaponIndex].ammoShootAnimation) playerObject.ammoGrid.GetChild(playerObject.weapons[playerObject.weaponIndex].ammoCount).Find("AmmoIcon").GetComponent<Animation>().Play(playerObject.weapons[playerObject.weaponIndex].ammoShootAnimation.name);
                        }

                        // If we have a crosshair, animate it
                        if (playerObject.crosshair.GetComponent<Animation>())
                        {
                            if ( isMobile == true ) playerObject.crosshair.GetComponent<Image>().enabled = true;

                            playerObject.crosshair.GetComponent<Animation>().Stop();
                            playerObject.crosshair.GetComponent<Animation>().Play();
                        }

                        // used to check if we hit a destroyable object
                        bool destroyableHit = false;

                        // Repeat the hit check based on the number of pellets in the shot ( example: a shotgun releases several pellets in a shot, while a pistol releases just one pellet )
                        for (int pelletIndex = 0; pelletIndex < playerObject.weapons[playerObject.weaponIndex].pelletsPerShot; pelletIndex++)
                        {
                            // Shoot a ray at the position to see if we hit something
                            Ray ray = Camera.main.ScreenPointToRay(playerObject.aimPosition);// + new Vector3(Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread), Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread), playerObject.aimPosition.z));
                            ray = new Ray(ray.origin, ray.direction + new Vector3( 0, Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread), Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread)));// + new Vector3(Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread), Random.Range(-playerObject.weapons[playerObject.weaponIndex].shotSpread, playerObject.weapons[playerObject.weaponIndex].shotSpread), ray.direction.z));

                            RaycastHit hit;

                            float hitRange = playerObject.weapons[playerObject.weaponIndex].hitRange;

                            // If we have a projectile, ignore hitRange so that the bullet can travel through space as long as it should
                            if (playerObject.weapons[playerObject.weaponIndex].projectile) hitRange = 10000;
                            else hitRange = playerObject.weapons[playerObject.weaponIndex].hitRange;

                            // If we hit something, create a hit effect at the position and apply damage to the object if it can be destroyed
                            if (Physics.Raycast(ray, out hit, hitRange))
                            {
                                // If we have a projectile assigned to this weapon, shoot it at the target instead of instantly hitting the target
                                if (playerObject.weapons[playerObject.weaponIndex].projectile)
                                {
                                    // Create a projectile on the player muzzle
                                    ORSProjectile newProjectile = Instantiate(playerObject.weapons[playerObject.weaponIndex].projectile, ray.origin, Quaternion.identity);

                                    // If the player weapon has a muzzle, shoot the projectile from it
                                    if (playerObject.weaponMuzzle) newProjectile.transform.position = playerObject.weaponMuzzle.position;

                                    // Apply the attributes of the projectile attack to the projectile object we created
                                    newProjectile.damage = Mathf.RoundToInt(playerObject.weapons[playerObject.weaponIndex].damage);
                                    newProjectile.hitArea = playerObject.weapons[playerObject.weaponIndex].hitRange;
                                    //newProjectile.speed = attackTypes[currentAttack].attackSpeed;
                                    //newProjectile.hurtEffect = attackTypes[currentAttack].hurtEffect;

                                    newProjectile.currentTarget = hit.collider.transform;

                                    newProjectile.transform.LookAt(hit.point);

                                    // Set a reference to the shooter of this projectile, used to keep track of which player shot and hit which enemy
                                    newProjectile.shotByPlayer = playerObject;
                                }
                                else // Otherwise hit instantly using Raycast to the hit point
                                {
                                    // If we have a projectile assigned to this weapon, shoot it at the target instead of instantly hitting the target
                                    if (playerObject.weapons[playerObject.weaponIndex].projectile)
                                    {
                                        // Create a projectile on the player muzzle
                                        ORSProjectile newProjectile = Instantiate(playerObject.weapons[playerObject.weaponIndex].projectile, ray.origin, Quaternion.identity);

                                        if (playerObject.weaponMuzzle) newProjectile.transform.position = playerObject.weaponMuzzle.position;

                                        // Apply the attributes of the projectile attack to the projectile object we created
                                        newProjectile.damage = Mathf.RoundToInt(playerObject.weapons[playerObject.weaponIndex].damage);
                                        newProjectile.hitArea = playerObject.weapons[playerObject.weaponIndex].hitRange;
                                        //newProjectile.speed = attackTypes[currentAttack].attackSpeed;
                                        //newProjectile.hurtEffect = attackTypes[currentAttack].hurtEffect;

                                        newProjectile.currentTarget = hit.collider.transform;

                                        newProjectile.transform.LookAt(hit.point);

                                        // Set a reference to the shooter of this projectile, used to keep track of which player shot and hit which object
                                        newProjectile.shotByPlayer = playerObject;
                                    }
                                    else
                                    {
                                        if (hit.collider.GetComponentInParent<ORSHittable>())
                                        {
                                            // Hit the target object
                                            hit.collider.SendMessageUpwards("HitObject", hit, SendMessageOptions.DontRequireReceiver);

                                            // Set a reference to the player that hit this object, used to keep track of which player shot and hit which object
                                            hit.collider.SendMessageUpwards("SetHitter", playerObject, SendMessageOptions.DontRequireReceiver);
                                        }

                                        if (hit.collider.GetComponentInParent<ORSDestroyable>())
                                        {
                                            // Check if we hit one of the viable hit areas
                                            foreach (DamageArea damageArea in hit.collider.GetComponentInParent<ORSDestroyable>().damageAreas)
                                            {
                                                if (damageArea.hitArea == hit.collider)
                                                {
                                                    destroyableHit = true;

                                                    // Set the bonus multiplier for this hit area
                                                    hit.collider.SendMessageUpwards("SetBonusMultiplier", damageArea.bonusMultiplier, SendMessageOptions.DontRequireReceiver);

                                                    // Cause damage to the target object
                                                    hit.collider.SendMessageUpwards("ChangeHealth", -playerObject.weapons[playerObject.weaponIndex].damage * damageArea.damageMultiplier, SendMessageOptions.DontRequireReceiver);

                                                    // If there is a hit effect, create it at the position of the object being hit
                                                    if (damageArea.damageEffect)
                                                    {
                                                        // Create the hit effect
                                                        Transform newHitEffect = Instantiate(damageArea.damageEffect) as Transform;

                                                        // Set the position of the hit effect
                                                        newHitEffect.position = hit.point;

                                                        // Make the effect look away from the impact point ( the way a bullet hit flies away from the wall it hits )
                                                        newHitEffect.rotation = Quaternion.LookRotation(hit.normal);

                                                        // Set the hit effect as the child of the hittable object, so that they move together and disappear when the parent is destroyed
                                                        //newHitEffect.SetParent(hit.transform);
                                                    }

                                                    // Add to the hit count when hitting a destroyable object
                                                    //if (hit.collider.GetComponentInParent<ORSDestroyable>()) destroyableHit = true;
                                                }
                                                else
                                                {
                                                    // Hit the target object
                                                    //hit.collider.SendMessageUpwards("MissObject", hit, SendMessageOptions.DontRequireReceiver);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // If we hit a destroyable object, add to the hit count. We do this check here and not in the loop above, because if we have a weapon with multiple pellets ( ex: shotgun ) we would add to the hit count for each pellet.
                        if (destroyableHit == true) playerObject.hitCount++;

                        // Apply a recoil effect to the weapon, takes into consideration the scale of the game canvas so that it is correct for all screen sizes
                        playerObject.currentRecoil += new Vector3( Random.Range(-playerObject.weapons[playerObject.weaponIndex].recoil.x * gameCanvas.localScale.x, playerObject.weapons[playerObject.weaponIndex].recoil.x), playerObject.weapons[playerObject.weaponIndex].recoil.y * gameCanvas.localScale.y, 0);

                        // If there is a source and a sound, play it from the source
                        if (soundSource && playerObject.weapons[playerObject.weaponIndex].soundShoot)
                        {
                            soundSource.pitch = Time.timeScale;

                            soundSource.PlayOneShot(playerObject.weapons[playerObject.weaponIndex].soundShoot);
                        }
                    }

                    // If we don't have ammo, show the "RELOAD!" button, and shoot on empty
                    if (playerObject.weapons[playerObject.weaponIndex].ammoCount <= 0)
                    {
                        // Show the reload button, so that we can reload
                        if (gameCanvas) playerObject.reloadObject.gameObject.SetActive(true);

                        // If there is a source and a sound, play it from the source
                        if (soundSource && playerObject.weapons[playerObject.weaponIndex].soundEmpty)
                        {
                            soundSource.pitch = Time.timeScale;

                            soundSource.PlayOneShot(playerObject.weapons[playerObject.weaponIndex].soundEmpty);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Moves to the hiding object point and starts hiding
        /// </summary>
        IEnumerator StartHiding()
        {
            // If you were mid-motion, stop that motion
            StopCoroutine("StopHiding");

            // Remove any previous button listeners
            gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.RemoveAllListeners();

            // for UI buttons listen for a click down on the hide button to stop hiding
            gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine("StopHiding"); });

            // If there is a hiding object
            if (currentWaypoint.hidingObject)
            {
                // Stop the idle animation
                playerAnimation.Stop();

                // Set the hiding motion time
                float timeout = 0.5f;

                // Animate the hiding motion
                while (timeout > 0 && Vector3.Distance(playerObject.transform.position, currentWaypoint.hidingObject.transform.position) > 0.5f )
                {
                    // Moves towards the hiding point position
                    playerObject.transform.position = Vector3.Slerp(playerObject.transform.position, currentWaypoint.hidingObject.transform.position, Time.deltaTime * 20);

                    // Rotate towards the hiding point rotation
                    playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation, currentWaypoint.hidingObject.transform.rotation, Time.deltaTime * 20);

                    // Count the timeout
                    timeout -= Time.deltaTime;

                    // Wait a litte so the motion is animated
                    yield return new WaitForSeconds(Time.deltaTime);
                }

                // Set the player position to the hiding point
                playerObject.transform.position = currentWaypoint.hidingObject.transform.position;

                // Set the player rotation to the hiding point
                playerObject.transform.eulerAngles = currentWaypoint.hidingObject.transform.eulerAngles;

                // Rotate the hide button icon
                gameCanvas.Find("ButtonHide").transform.localScale = new Vector3(1, -1, 1);

                // Set the hiding object for this player
                playerObject.hidingObject = currentWaypoint.hidingObject;

            }
        }

        /// <summary>
        /// Moves to the last position the player was at before hiding, and stop hiding
        /// </summary>
        IEnumerator StopHiding()
        {
            // If you were mid-motion, stop that motion
            StopCoroutine("StartHiding");

            // Remove any previous button listeners
            gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.RemoveAllListeners();

            // Stop the idle animation
            playerAnimation.Play(currentWaypoint.endAnimation.name);

            // Set the hiding motion time
            float timeout = 0.5f;

            // Animate the hiding motion
            while (timeout > 0 && Vector3.Distance(playerObject.transform.position,currentWaypoint.transform.position) > 0.5f )
            {
                // Moves towards the hiding point position
                playerObject.transform.position = Vector3.Slerp(playerObject.transform.position, currentWaypoint.transform.position, Time.deltaTime * 20);

                // Rotate towards the hiding point rotation
                playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation, currentWaypoint.transform.rotation, Time.deltaTime * 20);

                // Count the timeout
                timeout -= Time.deltaTime;

                // Wait a litte so the motion is animated
                yield return new WaitForSeconds(Time.deltaTime);
            }

            // Set the player position to the hiding point
            playerObject.transform.position = currentWaypoint.transform.position;

            // Set the player rotation to the hiding point
            playerObject.transform.eulerAngles = currentWaypoint.transform.eulerAngles;

            // If the hiding object has been destroyed, deactivate the hide button
            if (currentWaypoint.hidingObject.gameObject.activeSelf == false)
            {
                gameCanvas.Find("ButtonHide").gameObject.SetActive(false);
            }
            else // Otherwise, listen for a click to hide
            {
                // Rotate the hide button icon
                gameCanvas.Find("ButtonHide").transform.localScale = new Vector3(1, 1, 1);

                // for UI buttons listen for a click down on the hide button to start hiding
                gameCanvas.Find("ButtonHide").GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine("StartHiding"); });
            }

            // Remove the hiding object from this player
            playerObject.hidingObject = null;
        }


        /// <summary>
        /// Picks up an item and gives it to the player
        /// </summary>
        /// <param name="hitPosition"></param>
        public void PickUpItem( ORSPlayer player )
        {
            // Shoot a ray at the position to see if we hit something
            Ray ray = Camera.main.ScreenPointToRay(player.aimPosition);
            ray = new Ray(ray.origin, ray.direction);

            RaycastHit hit;

            float hitRange = 1000;

            // If we hit a pickup object, pick it up!
            if (Physics.Raycast(ray, out hit, hitRange))
            {
                if (hit.collider.GetComponent<ORSPickup>() || hit.collider.GetComponent<ORSWaypointArrow>())
                {
                    //hit.collider.SendMessageUpwards("Pickup", gameObject, SendMessageOptions.DontRequireReceiver);

                    hit.collider.SendMessageUpwards("Pickup", player, SendMessageOptions.DontRequireReceiver);

                }
            }
        }
        
        /// <summary>
        /// Pause the game, and shows the pause menu
        /// </summary>
        /// <param name="showMenu">If set to <c>true</c> show menu.</param>
        public void Pause(bool showMenu)
        {
            isPaused = true;

            //Set timescale to 0, preventing anything from moving
            Time.timeScale = 0;

            //Show the pause screen and hide the game screen
            if (showMenu == true)
            {
                if (pauseCanvas) pauseCanvas.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void Unpause()
        {
            isPaused = false;

            //Set timescale back to the current game speed
            Time.timeScale = 1;

            //Hide the pause screen and show the game screen
            if (pauseCanvas) pauseCanvas.gameObject.SetActive(false);
        }

        /// <summary>
        /// Runs the game over event and shows the game over screen
        /// </summary>
        IEnumerator GameOver(float delay)
		{
			isGameOver = true;

			yield return new WaitForSeconds(delay);
			
			//Remove the pause and game screens
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
            if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);

            //Show the game over screen
            if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);

                // Show the game stats and final score
                FinalScore(gameOverCanvas);
			}
		}

		/// <summary>
		/// Runs the victory event and shows the victory screen
		/// </summary>
		IEnumerator Victory(float delay)
		{
			isGameOver = true;
			
			yield return new WaitForSeconds(delay);
			
			//Remove the pause and game screens
			if ( pauseCanvas )    Destroy(pauseCanvas.gameObject);
			if ( gameCanvas )    Destroy(gameCanvas.gameObject);
			
			//Show the game over screen
			if ( victoryCanvas )    
			{
				//Show the game over screen
				victoryCanvas.gameObject.SetActive(true);
                
                // Show the game stats and final score
                FinalScore(victoryCanvas);
			}
		}

        /// <summary>
        /// Shows game stats and the final score for each player
        /// </summary>
        /// <param name="scoreCanvas"></param>
        public void FinalScore( Transform scoreCanvas )
        {
            // Prevent a NaN by setting the accuracy to a minimum of 0
            if (playerObject.shotCount == 0) playerObject.shotCount = 1;

            // If there is a second player, display the stats for both players
            if (player2Object)
            {
                // Prevent a NaN by setting the accuracy to a minimum of 0
                if (player2Object.shotCount == 0) player2Object.shotCount = 1;

                // Display stats for accuracy, health left, and level completion.
                scoreCanvas.Find("Base/Stats/TextAccuracy").GetComponent<Text>().text += "  P1: " + Mathf.Round((playerObject.hitCount / playerObject.shotCount) * 100).ToString() + "% /   P2: " + Mathf.Round((player2Object.hitCount / player2Object.shotCount) * 100).ToString() + "%";
                scoreCanvas.Find("Base/Stats/TextHealth").GetComponent<Text>().text += "  P1: " + Mathf.Round((1.0f * playerObject.health / playerObject.healthMax) * 100).ToString() + "% /   P2: " + Mathf.Round((1.0f * player2Object.health / player2Object.healthMax) * 100).ToString() + "%";
                scoreCanvas.Find("Base/Stats/TextCompletion").GetComponent<Text>().text += "  P1: " + Mathf.Round((1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * 100).ToString() + "% /   P2: " + Mathf.Round((1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * 100).ToString() + "%";

                // Calculate the final score which also includes bonuses for accuracy, health left, and level completion
                playerObject.score += Mathf.RoundToInt((playerObject.hitCount / playerObject.shotCount) * accuracyBonus + (1.0f * playerObject.health / playerObject.healthMax) * healthBonus + (1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * completionBonus);
                player2Object.score += Mathf.RoundToInt((player2Object.hitCount / player2Object.shotCount) * accuracyBonus + (1.0f * player2Object.health / player2Object.healthMax) * healthBonus + (1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * completionBonus);

                // Display the final score for both players
                victoryCanvas.Find("Base/Stats/TextScore").GetComponent<Text>().text += "  P1: " + playerObject.score.ToString() + " /   P2: " + player2Object.score.ToString();

                // Hide the rank icon if we have two players
                victoryCanvas.Find("Base/RankIcon").gameObject.SetActive(false);
            }
            else
            {
                // Display stats for accuracy, health left, and level completion.
                scoreCanvas.Find("Base/Stats/TextAccuracy").GetComponent<Text>().text += " " + Mathf.Round((playerObject.hitCount / playerObject.shotCount) * 100).ToString() + "%";
                scoreCanvas.Find("Base/Stats/TextHealth").GetComponent<Text>().text += " " + Mathf.Round((1.0f * playerObject.health / playerObject.healthMax) * 100).ToString() + "%";
                scoreCanvas.Find("Base/Stats/TextCompletion").GetComponent<Text>().text += " " + Mathf.Round((1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * 100).ToString() + "%";

                // Calculate the final score which also includes bonuses for accuracy, health left, and level completion
                playerObject.score += Mathf.RoundToInt((playerObject.hitCount / playerObject.shotCount) * accuracyBonus + (1.0f * playerObject.health / playerObject.healthMax) * healthBonus + (1.0f * currentWaypoint.transform.GetSiblingIndex() / (currentWaypoint.transform.parent.childCount - 1)) * completionBonus);

                // Display the final score
                victoryCanvas.Find("Base/Stats/TextScore").GetComponent<Text>().text += " " + playerObject.score.ToString();

                // Show the relevant rank icon based on our final score
                for (index = 0; index < gameEndRanks.Length; index++)
                {
                    if (playerObject.score >= gameEndRanks[index].rankScore)
                    {
                        victoryCanvas.Find("Base/RankIcon").GetComponent<Image>().sprite = gameEndRanks[index].rankIcon;
                    }
                }
            }
        }
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
			SceneManager.LoadScene(mainMenuLevelName);
		}

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            // Display the range of the weapon. Beyond this point we don't hit anything
            //if (playerObject && playerObject.weapons[playerObject.weaponIndex]) Gizmos.DrawRay(playerObject.transform.position, playerObject.transform.forward * playerObject.weapons[playerObject.weaponIndex].hitRange);
        }
    }
}