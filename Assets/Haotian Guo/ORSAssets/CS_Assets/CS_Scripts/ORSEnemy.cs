using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using OnRailsShooter.Types;
using System.Collections.Generic;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines an enemy, which looks and shoots at the player, and can be killed by it.
    /// </summary>
    public class ORSEnemy : ORSDestroyable
    {
        // These variables hold the gamecontroller, camera, and the pickup object, for easier access during gameplay
        static ORSGameController gameController;
        internal Transform thisTransform;
        static ORSPlayer playerObject;
        static Transform playerTransform;
        
        [Tooltip("The enemy body is the part of the enemy that looks at the player along the Y(Up) axis only")]
        public Transform enemyBody;

        [Tooltip("The enemy head is the part of the enemy that looks directly at the player")]
        public Transform enemyHead;

        [Tooltip("These are the points from which shots are released in the direction of the muzzle. If you have more than one muzzle, each shot is released from a random point")]
        public Transform[] muzzles;
        internal int muzzleIndex = 0;

        [Tooltip("If set to true, the enemy is spawned when the game starts. If false, the enemy will only spawn when we reach a waypoint it is assigned to")]
        public bool isSpawned = false;

        // Holds the last position the enemy was in before closing in to attack the player. The enemy returns to this positin after attacking the player
        internal Vector3 lastPosition;

        [Tooltip("The spawn effect that appears at the position of this object when it's activated")]
        public Transform spawnEffect;

        [Tooltip("This is the delay before an enemy attack the player, chosen randomly between two values")]
        public Vector2 attackDelayRange = new Vector2(2,4);
        internal float attackDelayCount;

        [Tooltip("A list of attack types that this enemy can use. These can be either projectile or melee attacks.")]
        public ORSAttackType[] attackTypes;
        internal int currentAttack = 0;
        internal bool isAttacking = false;

        [Tooltip("Should the enemy choose a random attack from the list, or use them in sequence")]
        public bool randomAttackType = false;

        internal int index;

        [Header("Deprecated fields (Don't use these)")]
        [Tooltip("(WARNING: This variable is deprectaed and will be removed in upcoming versions, use Attack Types variable instead).This is projectile that the enemy throws at the player. The damage and speed is decided by the projectile object")]
        public ORSProjectile projectile;

        void Awake()
        {
            // Deactivate the object at the start of the game, because it has not been spawned yet
            if (isSpawned == false) gameObject.SetActive(false);
        }

        void Start()
        {
            // Assign the gamecontroller from the scene
            if (gameController == null) gameController = (ORSGameController)FindObjectOfType(typeof(ORSGameController));

            // Assign this transfor for easier access
            thisTransform = transform;

            lastPosition = thisTransform.position;

            // Assign the player object from the scene
            if (playerObject == null)
            {
                playerObject = (ORSPlayer)FindObjectOfType(typeof(ORSPlayer));

                playerTransform = playerObject.transform;
            }

            // Create a spawn effect where this enemy is activated
            if (spawnEffect) Instantiate(spawnEffect, transform.position, transform.rotation);

            // Set a random value for the delay before attacking
            attackDelayCount = Random.Range(attackDelayRange.x, attackDelayRange.y);
        }

        void Update()
        {
            // If there is an enemy body, make it look at the player
            if ( enemyBody )
            {
                // Set the target we should look at
                Vector3 lookTarget = playerTransform.position - enemyBody.position;

                // Only rotate the body of the enemy along the ground
                lookTarget.y = 0;

                // Set the rotation of the body towards the player
                enemyBody.rotation = Quaternion.LookRotation(lookTarget);
            }

            // If there is an enemy head, make it look at the player
            if ( enemyHead )
            {
                // If there is a player camera assigned, look at it instead of the playerTransform
                if (playerObject.playerHead)    enemyHead.LookAt(playerObject.playerHead);
                else    enemyHead.LookAt(playerTransform);
            }

            // Count down the delay for the next attack
            attackDelayCount -= Time.deltaTime;

            // If the attack delay is up, and the player is still alive, attack the player!
            if ( attackDelayCount <= 0 && playerObject.isDead == false && isAttacking == false )
            {
                // Start attacking
                isAttacking = true;

                // If this is a projectile attack, play the attack animation ( which includes a call to ExecuteAttack() )
                if (attackTypes[currentAttack].projectile )
                {
                    // Play the attack animation. The attack animation contains a point at which the function "ExecuteAttack()" is called. This method is used to make sure the actual attack happens at the exact point in the animation when it should
                    if (GetComponentInChildren<Animator>()) GetComponentInChildren<Animator>().Play(attackTypes[currentAttack].attackAnimation);

                    // Set a random value for the delay for the next attack (not this one)
                    attackDelayCount = Random.Range(attackDelayRange.x, attackDelayRange.y);

                    // Stop attacking
                    isAttacking = false;
                }
                else // Otherwise, this is a melee attack, so the enemy must first move closer to the player before executing the attack
                {
                    // Get closer to the player
                    StartCoroutine(CloseDistance());
                }
            }
        }

        /// <summary>
        /// When using a melee attack, the enemy will get close to the player
        /// </summary>
        /// <returns></returns>
        IEnumerator CloseDistance()
        {
            // If this is a flying enemy, delay its regular movement so we can attack freely
            if (GetComponent<ORSFlier>()) GetComponent<ORSFlier>().moveDelayCount = 1000;

            // Remember the last position the enemy was at before using the melee attack
            lastPosition = thisTransform.position;

            // While we are far from the target location, keep moving towards it
            while (Vector3.Distance(thisTransform.position, playerObject.transform.position) > attackTypes[currentAttack].attackRange)
            {
                // Wait a little to animate the effect
                yield return new WaitForSeconds(Time.deltaTime);

                // Move close to the target location
                thisTransform.position = Vector3.Slerp(thisTransform.position, playerObject.transform.position, Time.deltaTime * attackTypes[currentAttack].attackSpeed);
            }

            // When we reach the attack range, play the attack animation
            // Play the attack animation. The attack animation contains a point at which the function "ExecuteAttack()" is called. This method is used to make sure the actual attack happens at the exact point in the animation when it should
            if (GetComponentInChildren<Animator>()) GetComponentInChildren<Animator>().Play(attackTypes[currentAttack].attackAnimation);

            // Set a random value for the delay for the next attack (not this one)
            attackDelayCount = Random.Range(attackDelayRange.x, attackDelayRange.y);

            // Stop attacking
            isAttacking = false;
        }

        /// <summary>
        /// Enemy is dead, reduce from the enemy count at current waypoint
        /// </summary>
        public void EnemyDead()
        {
            // Reduce from the count of enemies currently active ( and add to the kills stat ). This is used so we know when all enemies at a waypoint have been killed, and move on to the next one
            if (gameController)
            {
                gameController.enemiesLeft--;
            }
        }

        /// <summary>
        /// Executes an attack from a random muzzle of the enemy
        /// </summary>
        public void ExecuteAttack()
        {
            // If this is a projectile attack, shoot it at the player
            if (attackTypes[currentAttack].projectile )
            {
                // Choose a random muzzle from the list of the enemy muzzles
                int randomMuzzle = Mathf.FloorToInt(Random.Range(0, muzzles.Length));

                // Create a projectile at the position and rotation of the muzzle
                ORSProjectile newProjectile = Instantiate(attackTypes[currentAttack].projectile, muzzles[randomMuzzle].position, muzzles[randomMuzzle].rotation);

                // Apply the attributes of the projectile attack to the projectile object we created
                newProjectile.damage = attackTypes[currentAttack].attackDamage;
                newProjectile.hitArea = attackTypes[currentAttack].attackRange;
                newProjectile.speed = attackTypes[currentAttack].attackSpeed;
                newProjectile.hurtEffect = attackTypes[currentAttack].hurtEffect;

                // Assign the player as the target for this projectile, and look at it
                newProjectile.currentTarget = playerObject.transform;

                // If there is a player camera assigned, shoot at it instead of the playerTransform
                if ( playerObject.playerHead )    newProjectile.transform.LookAt(playerObject.playerHead.transform);
                else    newProjectile.transform.LookAt(playerTransform);
            }
            else //Oterwise, this is a melee attack so we apply the damage instantly
            {
                if (playerObject)
                {
                    // If the player's hurt time is off, it means that the player can be hurt again
                    if (playerObject.hurtTimeCount <= 0)
                    {
                        // Cause damage to the player
                        playerObject.ChangeHealth(-attackTypes[currentAttack].attackDamage);

                        // Play the hurt effect on the player, which is shaking and a bullet hole effect
                        gameController.HurtEffect(attackTypes[currentAttack].hurtEffect);
                    }
                    else // Otherwise, it means we can't get hurt again, so just make a hit effect
                    {
                        // Play the hit effect on the player, which is shaking the camera with a special sound, without a bullet hole
                        gameController.HurtEffect(attackTypes[currentAttack].hurtEffect);
                    }
                }

                // If this is a flying enemy, return it to its flying area
                if (GetComponent<ORSFlier>()) GetComponent<ORSFlier>().moveDelayCount = 1;
            }

            if (randomAttackType == true )
            {
                // Choose a random attack from the list of attack types
                currentAttack = Mathf.FloorToInt(Random.Range(0, attackTypes.Length));
            }
            else
            {
                // Go to the next attack type, loop from the start if we reach the end
                if (currentAttack < attackTypes.Length - 1) currentAttack++;
                else currentAttack = 0;
            }
        }
    }
}