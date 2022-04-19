using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OnRailsShooter.Types;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines a projectile which is released from an enemy's muzzle when shooting at the player
    /// </summary>
    public class ORSProjectile : MonoBehaviour
    {
        // These variables hold the player object that can be hit with a projectile, the gamecontroller which checks if the player should be hit or hurt, and the transform of the projectile
        static ORSPlayer targetPlayer;
        internal Transform currentTarget = null;
        static ORSGameController gameController;
        internal Transform thisTransform;
        internal Vector3 previousPosition;

        [Tooltip("The movement speed of this projectile, in the forward direction of the muzzle it comes out of. This is chosen randomly within the minimum and maximum range we set")]
        public Vector2 speedRange = new Vector2(100,100);

        [Tooltip("Give the projectile an initial upwards speed. For example this is used to make the Thumper shot curved")]
        public float throwUpwards = 0;

        [Tooltip("Make the projectile fall down based on the gravity value in the scene (0 is no gravity, 1 is full gravity). For example this is used to make the Thumper shot curved")]
        public float gravityModifier = 0;

        [Tooltip("The range at which this projectile can hit the player")]
        public float hitArea = 0.5f;

        [Tooltip("The damage this projectile causes when hitting the player")]
        public int damage = 1;

        [Tooltip("The hurt effect that appears on the player screen when this projectile hits us (Only used for projectiles shot by the enemy at the player)")]
        public Sprite hurtEffect;

        [Tooltip("The hit effect created at the point this projectile hits")]
        public Transform hitEffect;

        [Header("Deprecated fields (Don't use these)")]
        [Tooltip("(WARNING: This variable is deprectaed and will be removed in upcoming versions, use speedRange variable instead)")]
        public float speed = 10;

        // Holds a reference to the player that shot this projectile ( used only when the player shoots, and not when the enemy shoots )
        internal ORSPlayer shotByPlayer;

        void Start()
        {
            thisTransform = transform;

            // Record the initial position of the projectile
            previousPosition = thisTransform.position;

            // Assign the player object from the scene
            if (targetPlayer == null) targetPlayer = (ORSPlayer)FindObjectOfType(typeof(ORSPlayer));

            // Assign the gamecontroller object from the scene
            if ( gameController == null ) gameController = (ORSGameController)FindObjectOfType(typeof(ORSGameController));

            // Set a random movement speed within range for the projectile
            speed = Random.Range(speedRange.x, speedRange.y);
        }

        void Update()
        {
            // Record the previous position of the projectile
            previousPosition = thisTransform.position;

            // Move this projectile forward at a constant speed
            thisTransform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

            // Move this projectile forward at a constant speed
            thisTransform.Translate(Vector3.up * throwUpwards * Time.deltaTime, Space.Self);

            throwUpwards += Physics.gravity.y * gravityModifier * Time.deltaTime;

            // If the current target is the player, hit it when it gets in range. ( This means the projectile was shot by an enemy, and is targeting the player, so it uses a different system for hitting )
            if (currentTarget == targetPlayer.transform)
            {
                // If the projectile reaches the hit range of the player ( or the head of the player ), hit it!
                if ( Vector3.Distance(thisTransform.position, targetPlayer.transform.position) < hitArea || (targetPlayer.playerHead && Vector3.Distance(thisTransform.position, targetPlayer.playerHead.position) < hitArea) )
                {
                    // If the player is hiding in cover, hit the cover
                    if (targetPlayer.hidingObject)
                    {
                        targetPlayer.hidingObject.HitObject(this);

                        if (targetPlayer.hidingObject.GetComponent<ORSDestroyable>()) targetPlayer.hidingObject.GetComponent<ORSDestroyable>().ChangeHealth(-damage);
                    }
                    else // Otherwise, hit the player
                    {
                        // If the player's hurt time is off, it means that the player can be hurt again
                        if (targetPlayer.hurtTimeCount <= 0)
                        {
                            // Cause damage to the player, or to player 2 if it exists
                            if ( gameController.player2Object && gameController.player2Object.health >= targetPlayer.health ) gameController.player2Object.ChangeHealth(-damage);
                            else targetPlayer.ChangeHealth(-damage);

                            // Play the hurt effect on the player, which is shaking and a bullet hole effect
                            gameController.HurtEffect(hurtEffect);
                        }
                        else // Otherwise, it means we can't get hurt again, so just make a hit effect
                        {
                            // Play the hit effect on the player, which is shaking the camera with a special sound, without a bullet hole
                            gameController.HurtEffect(hurtEffect);
                        }
                    }

                    // If there is a hit effect, create it at the position of this projectile
                    if (hitEffect) Instantiate(hitEffect, thisTransform.position, thisTransform.rotation);

                    // Remove the projectile
                    Destroy(gameObject);

                }
            }
            else
            {
                // used to check if we hit a destroyable object ( so we can count how many enemies we hit, for the end-game accuracy statistic )
                bool destroyableHit = false;

                RaycastHit hit;

                // This is used for projectiles shot from player weapons, and they check collider hits on enemies and destroyable objects
                // Create a raycast between the current and previous positions of the projectile, and check if we hit anything
                // If we hit something, create a hit effect at the position and apply damage to the object if it can be destroyed
                if (Physics.Linecast(previousPosition, thisTransform.position, out hit))
                {
                    // We hit an hittable object, which creates a hit effect where the projectile touches
                    if (hit.collider.GetComponentInParent<ORSHittable>())
                    {
                        // Hit the target object
                        hit.collider.SendMessageUpwards("HitObject", hit, SendMessageOptions.DontRequireReceiver);
                    }

                    // We hit a destroyable object, which has health and can be destroyed
                    if (hit.collider.GetComponentInParent<ORSDestroyable>())
                    {
                        // Check if we hit one of the viable hit areas
                        foreach (DamageArea damageArea in hit.collider.GetComponentInParent<ORSDestroyable>().damageAreas)
                        {
                            // If this damage area has a collider, we can hit it
                            if (damageArea.hitArea == hit.collider)
                            {
                                destroyableHit = true;

                                // Set the bonus multiplier for this hit area
                                hit.collider.SendMessageUpwards("SetBonusMultiplier", damageArea.bonusMultiplier, SendMessageOptions.DontRequireReceiver);

                                // Cause damage to the target object
                                hit.collider.SendMessageUpwards("ChangeHealth", -shotByPlayer.weapons[shotByPlayer.weaponIndex].damage * damageArea.damageMultiplier, SendMessageOptions.DontRequireReceiver);

                                // Set a reference to the player that hit this object, used to keep track of which player shot and hit which object
                                hit.collider.SendMessageUpwards("SetHitter", shotByPlayer, SendMessageOptions.DontRequireReceiver);

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

                    // If there is a hit effect, create it at the position of this projectile
                    if (hitEffect) Instantiate(hitEffect, thisTransform.position, thisTransform.rotation);

                    // Remove the projectile
                    Destroy(gameObject);
                }

                // If we hit a destroyable object, add to the hit count. We do this check here and not in the loop above, because if we have a weapon with multiple pellets ( ex: shotgun ) we would add to the hit count for each pellet.
                if (destroyableHit == true) shotByPlayer.hitCount++;
            }
        }
    }
}