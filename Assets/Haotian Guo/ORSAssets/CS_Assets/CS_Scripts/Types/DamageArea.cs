using UnityEngine;
using System;

namespace OnRailsShooter.Types
{
	/// <summary>
	/// This script defines a damage area, which has a collider that can be hit, a damage multiplier, a damage effect, and a kill bonus multiplier.
	/// </summary>
	[Serializable]
	public class DamageArea
	{
        [Tooltip("The hit area collider that can be shot by the player")]
        public Collider hitArea;

        [Tooltip("The damage multiplier that the enemy recieves when being hit at this point. 0 means no damage is recieved, 1 means full damage, 2 means double damage, etc")]
        public float damageMultiplier = 1;

        [Tooltip("The effect created when this object is hit at this point")]
        public Transform damageEffect;

        [Tooltip("The bonus multiplier that killing this enemy gives. 0 means no bonus is recieved, 1 means full bonus, 2 means double bonus, etc")]
        public int bonusMultiplier = 1;

    }
}