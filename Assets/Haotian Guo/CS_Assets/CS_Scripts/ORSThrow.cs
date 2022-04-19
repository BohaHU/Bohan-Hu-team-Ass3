using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OnRailsShooter.Types;

namespace OnRailsShooter
{
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// This script defines a projectile which is released from an enemy's muzzle when shooting at the player
    /// </summary>
    public class ORSThrow : MonoBehaviour
    {
        public Vector3 throwPower = new Vector3(0,10,0);

        public bool throwAtStart = true;

        void Start()
        {
            // Throw the object once it appears in the game
            if (throwAtStart == true) Throw();
        }
        
        /// <summary>
        /// Throws an objects in a direction set by the player
        /// </summary>
        public void Throw()
        {
            GetComponent<Rigidbody>().AddForce(throwPower);
        }
        



    }
}