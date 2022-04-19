using UnityEngine;
using System;

namespace OnRailsShooter.Types
{
	/// <summary>
	/// This script defines a rank score and icon that appears at the end of the game
	/// </summary>
	[Serializable]
	public class Rank
	{
        [Tooltip("The score needed to reach this rank")]
        public float rankScore;

        [Tooltip("The icon of the rank we reached")]
        public Sprite rankIcon;
    }
}