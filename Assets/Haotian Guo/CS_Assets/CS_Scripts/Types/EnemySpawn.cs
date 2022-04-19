using UnityEngine;
using System;

namespace OnRailsShooter.Types
{
	/// <summary>
	/// This script defines an enemy spawn which has a delay
	/// </summary>
	[Serializable]
	public class EnemySpawn
	{
        [Tooltip("The enemy object that will be spawned")]
        public ORSEnemy enemy;

        [Tooltip("The time delay before spawning this enemy")]
        public float spawnDelay = 0;
    }
}