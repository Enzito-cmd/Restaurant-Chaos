using System;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    [Serializable]
    public struct PatienceCarryOver
    {
        [Range(0f, 1f)]
        public float queueThreshold;

        [Range(0f, 1f)]
        public float seatedStartFill;
    }
}
