using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_Sphere
{
    /// <summary>
    /// Class thats purpose is holding a number of spheres for collision indication ready to use.
    /// </summary>
    public class SphereObjectPool
    {
        private List<SphereObject> sphereObjects;
        private SphereObject spherePrefab;
        private Transform parent;
        private int currentlyActive;
        private int currentlyUsed;

        /// <summary>
        /// Construct a new pool of <see cref="SphereObjects"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="spherePrefab"> The <see cref="SphereObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="SphereObject"/>s to instantiate. </param>
        public SphereObjectPool(SphereObject spherePrefab, int initialAmount) : this(spherePrefab, initialAmount, null)
        {
        }

        /// <summary>
        /// Construct a new pool of <see cref="SphereObjects"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="spherePrefab"> The <see cref="SphereObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="SphereObject"/>s to instantiate. </param>
        /// <param name="parent"> The parent object of all <see cref="RayObject"/>s instantiated by this pool. </param>
        public SphereObjectPool(SphereObject spherePrefab, int initialAmount, Transform parent)
        {
            this.spherePrefab = spherePrefab;
            this.parent = parent;

            sphereObjects = new List<SphereObject>(initialAmount);

            for (int i = 0; i < initialAmount; ++i)
            {
                SphereObject sphere;
                if (parent != null)
                    sphere = Object.Instantiate(spherePrefab, parent);
                else
                    sphere = Object.Instantiate(spherePrefab);

                sphere.gameObject.SetActive(false);
                sphereObjects.Add(sphere);
            }

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all <see cref="RayObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            for (int i = 0; i < sphereObjects.Count; ++i)
                sphereObjects[i].gameObject.SetActive(false);

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all unused <see cref="RayObject"/>s in this pool.
        /// </summary>
        public void DeactivateUnused()
        {
            for (int i = currentlyUsed; i < currentlyActive; ++i)
                sphereObjects[i].gameObject.SetActive(false);

            currentlyActive = currentlyUsed;
        }

        /// <summary>
        /// Mark all <see cref="RayObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetSphereObject"/>. The intended usage is to first call this function, then get
        /// the objects you need using <see cref="GetSphereObject"/> and finally deactivate all unused objects left active
        /// by calling <see cref="DeactivateUnused"/>.
        /// </summary>
        public void SetAllUnused()
        {
            currentlyUsed = 0;
        }

        /// <summary>
        /// Get an unused <see cref="RayObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="RayObject"/> from the pool. </returns>
        public SphereObject GetSphereObject()
        {

            // First try to get unused but already active ray objects from the pool.
            if (currentlyUsed < currentlyActive)
                return sphereObjects[currentlyUsed++];

            // Otherwise we get the first unused ray object and activate it.
            if (currentlyUsed < sphereObjects.Count)
            {
                sphereObjects[currentlyUsed].gameObject.SetActive(true);
                ++currentlyActive;
                return sphereObjects[currentlyUsed++];
            }

            // If all ray object are already in use we create a new one.
            SphereObject ray;
            if (parent != null)
                ray = Object.Instantiate(spherePrefab, parent);
            else
                ray = Object.Instantiate(spherePrefab);

            sphereObjects.Add(ray);
            ++currentlyActive;
            ++currentlyUsed;
            return ray;
        }
    }
}