using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnRailsShooter
{
    /// <summary>
    /// This script defines a hittable object, which can be hit by the player's bullets and creates a hit effect at the location of impact
    /// This is used for both background objects such as rocks, and for the enemies themselves which can also be hit by the player
    /// </summary>
    public class ORSHittable : MonoBehaviour
    {
        [Tooltip("The effect created when this object is hit")]
        public Transform hitEffect;
        
        [Tooltip("Should the hit effect be attached to the parent object? This is good when you have bullet holes on a destroyable object, so that they get removed when it is destroyed. Or if you have a moving object that you want the holes to move with it.")]
        public bool attachHitEffect = true;

        [Tooltip("The color this object flashes when hit")]
        public Color hitFlashColor = new Color(0.5f,0.5f,0.5f,0.5f);
        internal Color defaultColor;

        [Tooltip("The part of the model (mesh) that will flash when hit")]
        public Renderer flashObject;

        // Holds a reference to the player that hit this object
        internal ORSPlayer hitByPlayer;

        /// <summary>
        /// Hits the object at a point, and creates an effect at the point of impact
        /// </summary>
        /// <param name="hit"></param>
        public void HitObject(RaycastHit hit)
        {
            // If there is a hit effect, create it at the position of the object being hit
            if (hitEffect)
            {
                // Create the hit effect
                Transform newHitEffect = Instantiate(hitEffect) as Transform;

                // Set the position of the hit effect
                newHitEffect.position = hit.point;

                // Make the effect look away from the impact point ( the way a bullet hit flies away from the wall it hits )
                newHitEffect.rotation = Quaternion.LookRotation(hit.normal);
                
                // Set the hit effect as the child of the hittable object, so that they move together and disappear when the parent is destroyed
                if ( attachHitEffect == true ) newHitEffect.SetParent(gameObject.transform);
            }

            // Flash the object that was hit
            StartCoroutine("FlashObject");
        }

        /// <summary>
        /// Hits the object at a point, and creates an effect at the point of impact
        /// </summary>
        /// <param name="hit"></param>
        public void HitObject(ORSProjectile projectile)
        {
            // If there is a hit effect, create it at the position of the object being hit
            if (hitEffect)
            {
                // Create the hit effect
                Transform newHitEffect = Instantiate(hitEffect) as Transform;

                // Set the position of the hit effect
                newHitEffect.position = projectile.transform.position;

                // Make the effect look away from the impact point ( the way a bullet hit flies away from the wall it hits )
                newHitEffect.rotation = projectile.transform.rotation;

                // Set the hit effect as the child of the hittable object, so that they move together and disappear when the parent is destroyed
                if (attachHitEffect == true) newHitEffect.SetParent(gameObject.transform);
            }

            // Flash the object that was hit
            if ( gameObject.activeSelf == true )    StartCoroutine("FlashObject");
        }

        /// <summary>
        /// Flashes the object in a color, and then returns to the default object color
        /// </summary>
        /// <returns></returns>
        public IEnumerator FlashObject()
        {
            if (flashObject)
            {
                // Hold the default color of the object so we can return to it after flashing
                defaultColor = flashObject.material.GetColor("_EmissionColor");

                // Flash the color of the object
                flashObject.material.SetColor("_EmissionColor", hitFlashColor);
            }

            // Wait for a frame or so
            yield return new WaitForSeconds(Time.deltaTime);

            // Return to the original object color
            if (flashObject) flashObject.material.SetColor("_EmissionColor", defaultColor);
        }


        /// <summary>
        /// Sets the shooter of this projectile
        /// </summary>
        /// <param name="player"></param>
        public void SetHitter(ORSPlayer player)
        {
            hitByPlayer = player;
        }
    }
}