using UnityEngine;

namespace Project.Core
{
    public class StabilizedRay
    {
        /// <summary>
        /// Half life used for position decay calculations.
        /// </summary>
        public float HalfLifePosition { get; } = 0.1f;

        /// <summary>
        /// Half life used for velocity decay calculations.
        /// </summary>
        public float HalfLifeDirection { get; } = 0.1f;

        /// <summary>
        /// Computed Stabilized position.
        /// </summary>
        public Vector3 StabilizedPosition { get; private set; }

        /// <summary>
        /// Computed stabilized direction.
        /// </summary>
        public Vector3 StabilizedDirection { get; private set; }

        private bool isInitialized = false;

        /// <summary>
        /// HalfLife closer to zero means lerp closer to one.
        /// </summary>
        public StabilizedRay(float halfLife)
        {
            HalfLifePosition = halfLife;
            HalfLifeDirection = halfLife;
        }

        /// <summary>
        /// StabilizedRay with distinct position and direction half life values.
        /// HalfLife closer to zero means lerp closer to one.
        /// </summary>
        /// <param name="positionHalfLife">The half life used for position decay calculations.</param>
        /// <param name="directionHalfLife">The half life used for direction decay calculations.</param>
        public StabilizedRay(float positionHalfLife, float directionHalfLife)
        {
            HalfLifePosition = positionHalfLife;
            HalfLifeDirection = directionHalfLife;
        }

        /// <summary>
        /// Add sample to ray stabilizer.
        /// </summary>
        /// <param name="ray">New Sample used to update stabilized ray.</param>
        public void AddSample(Ray ray)
        {
            if (!isInitialized)
            {
                StabilizedPosition = ray.origin;
                StabilizedDirection = ray.direction.normalized;
                isInitialized = true;
            }
            else
            {
                StabilizedPosition = DynamicExpDecay(StabilizedPosition, ray.origin, HalfLifePosition);
                StabilizedDirection = DynamicExpDecay(StabilizedDirection, ray.direction.normalized, HalfLifeDirection);
            }
        }

        /// <summary>
        /// Compute dynamic exponential coefficient.
        /// </summary>
        /// <param name="hLife">Half life</param>
        /// <param name="delta">Distance delta</param>
        /// <returns>The dynamic exponential coefficient.</returns>
        public static float DynamicExpCoefficient(float hLife, float delta)
        {
            if (hLife == 0)
            {
                return 1;
            }

            return 1.0f - Mathf.Pow(0.5f, delta / hLife);
        }

        /// <summary>
        /// Compute stabilized vector3 given a previously stabilized value, and a new sample, given a half life.
        /// </summary>
        /// <param name="from">Previous stabilized Vector3.</param>
        /// <param name="to">New Vector3 sample.</param>
        /// <param name="hLife">Half life used for stabilization.</param>
        /// <returns>Stabilized Vector 3.</returns>
        public static Vector3 DynamicExpDecay(Vector3 from, Vector3 to, float hLife)
        {
            return Vector3.Lerp(from, to, DynamicExpCoefficient(hLife, Vector3.Distance(to, from)));
        }
    }
}