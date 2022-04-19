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
    public class ORSAreaOfEffect : MonoBehaviour
    {
        // These variables hold the player object that can be hit with a projectile, the gamecontroller which checks if the player should be hit or hurt, and the transform of the projectile
        static ORSPlayer playerObject;
        static ORSGameController gameController;
        
        [Tooltip("The damage this projectile causes when hitting the player")]
        public int damage = 10;

        [Tooltip("The hurt effect that appears on the player screen when this projectile hits us")]
        public Transform hitEffect;
        
        void Start()
        {
            // Assign the player object from the scene
            if (playerObject == null) playerObject = (ORSPlayer)FindObjectOfType(typeof(ORSPlayer));

            // Assign the gamecontroller object from the scene
            if ( gameController == null ) gameController = (ORSGameController)FindObjectOfType(typeof(ORSGameController));
        }
        
       
        public void OnTriggerEnter(Collider other)
        {
            // If this is a destroyable object, change its health
            if (other.transform.GetComponent<ORSDestroyable>())
            {
                other.transform.GetComponent<ORSDestroyable>().ChangeHealth(-damage);
            }
        }




    }
}