using UnityEngine;
using System.Collections;

namespace OnRailsShooter
{
	/// <summary>
	/// This script removes the object after some time
	/// </summary>
	public class ORSRemoveAfterTime : MonoBehaviour 
	{
        [Tooltip("How many seconds to wait before removing this object")]
        public float removeAfterTime = 0.1f;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start() 
		{
			// Remove this object after a delay
			Destroy( gameObject, removeAfterTime);
		}
        
	}
}
