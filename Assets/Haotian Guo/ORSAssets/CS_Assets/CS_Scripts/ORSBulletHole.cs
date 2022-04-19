using UnityEngine;
using System.Collections;

namespace OnRailsShooter
{
	/// <summary>
	/// Randomizes the scale and rotation of a bullet hole
	/// </summary>
	public class ORSBulletHole : MonoBehaviour
	{
        internal Transform thisTransform;

		[Tooltip("The range of the rotation for the bullet hole")]
		public Vector2 rotationRange = new Vector2(0, 360);

		[Tooltip("The scale of the rotation for each axis in the bullet hole")]
		public Vector2 scaleRange = new Vector2(0.9f, 1.1f);

        [Tooltip("How long to wait before scaling out of the game. The bullet hole gets smaller until it's no longer visible")]
        public float scaleOutDelay = 8;

        [Tooltip("How fast the bullet scales out")]
        public float scaleOutSpeed = 5;
	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
            thisTransform = this.transform;

            // Set a random rotation for the object
            thisTransform.localEulerAngles = Vector3.forward * Random.Range(rotationRange.x, rotationRange.y);

            // Set a random scale for the object
            thisTransform.localScale *= Random.Range(scaleRange.x, scaleRange.y);

            // Start the scaling out routine, which has a delay before it happens
            StartCoroutine(ScaleOut());
        }

        /// <summary>
        /// Scales out an object until it is to small to be seen
        /// </summary>
        /// <returns></returns>
        public IEnumerator ScaleOut()
        {
            // Wait for the scale out delay
            yield return new WaitForSeconds(scaleOutDelay);

            // While the object scale is larger than 0, keep scaling it down
            while ( thisTransform.localScale.x > 0 )
            {
                // Set the scale based on scale-out speed
                thisTransform.localScale = Vector3.Slerp(thisTransform.localScale, Vector3.zero, scaleOutSpeed * Time.deltaTime);

                // Wait a little to animate the effect
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
	}
}