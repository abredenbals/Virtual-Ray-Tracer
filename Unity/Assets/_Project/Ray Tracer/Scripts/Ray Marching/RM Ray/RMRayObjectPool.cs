using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Ray
{
    /// <summary>
    /// A simple class used to pool <see cref="RMRayObject"/>s for drawing by the <see cref="RayManager"/>. For more
    /// information on object pooling in Unity see: https://learn.unity.com/tutorial/introduction-to-object-pooling.
    /// Note that our implementation differs a somewhat because we have optimized the pool for our specific use case.
    /// </summary>
    public class RMRayObjectPool
    {
        private List<RMRayObject> rayObjects;
        private RMRayObject rmRayPrefab;
        private Transform parent;
        private int currentlyActive;
        private int currentlyUsed;

        /// <summary>
        /// Construct a new pool of <see cref="RMRayObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="rmRayPrefab"> The <see cref="RMRayObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="RMRayObject"/>s to instantiate. </param>
        public RMRayObjectPool(RMRayObject rmRayPrefab, int initialAmount) : this(rmRayPrefab, initialAmount, null)
        {
        }

        /// <summary>
        /// Construct a new pool of <see cref="RMRayObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="rmRayPrefab"> The <see cref="RMRayObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="RMRayObject"/>s to instantiate. </param>
        /// <param name="parent"> The parent object of all <see cref="RMRayObject"/>s instantiated by this pool. </param>
        public RMRayObjectPool(RMRayObject rmRayPrefab, int initialAmount, Transform parent)
        {
            this.rmRayPrefab = rmRayPrefab;
            this.parent = parent;

            rayObjects = new List<RMRayObject>(initialAmount);
            RMRayObject rmRay;

            for (int i = 0; i < initialAmount; ++i)
            {
                if (parent != null)
                    rmRay = Object.Instantiate(rmRayPrefab, parent);
                else
                    rmRay = Object.Instantiate(rmRayPrefab);

                rmRay.gameObject.SetActive(false);
                rayObjects.Add(rmRay);
            }

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all <see cref="RMRayObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            for (int i = 0; i < rayObjects.Count; ++i)
                rayObjects[i].gameObject.SetActive(false);

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all unused <see cref="RMRayObject"/>s in this pool.
        /// </summary>
        public void DeactivateUnused()
        {
            for (int i = currentlyUsed; i < currentlyActive; ++i)
                rayObjects[i].gameObject.SetActive(false);

            currentlyActive = currentlyUsed;
        }

        /// <summary>
        /// Mark all <see cref="RMRayObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetRayObject"/>. The intended usage is to first call this function, then get
        /// the objects you need using <see cref="GetRayObject"/> and finally deactivate all unused objects left active
        /// by calling <see cref="DeactivateUnused"/>.
        /// </summary>
        public void SetAllUnused()
        {
            currentlyUsed = 0;
        }

        /// <summary>
        /// Get an unused <see cref="RMRayObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="RMRayObject"/> from the pool. </returns>
        public RMRayObject GetRayObject()
        {

            // First try to get unused but already active ray objects from the pool.
            if (currentlyUsed < currentlyActive)
                return rayObjects[currentlyUsed++];

            // Otherwise we get the first unused ray object and activate it.
            if (currentlyUsed < rayObjects.Count)
            {
                rayObjects[currentlyUsed].gameObject.SetActive(true);
                ++currentlyActive;
                return rayObjects[currentlyUsed++];
            }

            // If all ray object are already in use we create a new one.
            RMRayObject rmRay;
            if (parent != null)
                rmRay = Object.Instantiate(rmRayPrefab, parent);
            else
                rmRay = Object.Instantiate(rmRayPrefab);

            rayObjects.Add(rmRay);
            ++currentlyActive;
            ++currentlyUsed;
            return rmRay;
        }
    }
}
