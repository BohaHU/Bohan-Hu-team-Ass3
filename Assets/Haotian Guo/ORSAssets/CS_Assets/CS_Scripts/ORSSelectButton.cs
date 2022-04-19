using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace OnRailsShooter
{
	/// <summary>
	/// Selects a certain button when this canvas/panel/object is enabled
	/// </summary>
	public class ORSSelectButton : MonoBehaviour 
	{
        [Tooltip("The button that will be selected when this object is activated")]
        public GameObject selectedButton;

		/// <summary>
		/// Runs when this object is enabled
		/// </summary>
		void OnEnable() 
		{
            if ( selectedButton )    
			{
				// Select the button
				if ( EventSystem.current )    EventSystem.current.SetSelectedGameObject(selectedButton);
			}	
		}
	}
}
