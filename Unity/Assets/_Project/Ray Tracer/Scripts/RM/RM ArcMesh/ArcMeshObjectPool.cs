using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_ArcMesh
{
    /// <summary>
    /// Class thats purpose is holding a number of arcMeshes for visualisation ready to use.
    /// </summary>
    public class ArcMeshObjectPool
    {
        private List<ArcMeshObject> arcMeshObjects;
        private ArcMeshObject arcMeshPrefab; //change these to Mesh?
        private int currentlyActive;
        private int currentlyUsed;

        /// <summary>
        /// Construct a new pool of <see cref="ArcMeshObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="arcMeshPrefab"> The <see cref="ArcMeshObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="ArcMeshObject"/>s to instantiate. </param>
        public ArcMeshObjectPool(ArcMeshObject arcMeshPrefab, int initialAmount)
        {
            this.arcMeshPrefab = arcMeshPrefab;

            arcMeshObjects = new List<ArcMeshObject>(initialAmount);

            for (int i = 0; i < initialAmount; ++i)
            {
                var arcMesh = Object.Instantiate(arcMeshPrefab);

                arcMesh.gameObject.SetActive(false);
                arcMeshObjects.Add(arcMesh);
            }

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all <see cref="ArcMeshObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            for (int i = 0; i < arcMeshObjects.Count; ++i)
                arcMeshObjects[i].gameObject.SetActive(false);

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all unused <see cref="ArcMeshObject"/>s in this pool.
        /// </summary>
        public void DeactivateUnused()
        {
            for (int i = currentlyUsed; i < currentlyActive; ++i)
                arcMeshObjects[i].gameObject.SetActive(false);

            currentlyActive = currentlyUsed;
        }

        /// <summary>
        /// Mark all <see cref="ArcMeshObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetArcMeshObject"/>. The intended usage is to first call this function, then get
        /// the objects you need using <see cref="GetArcMeshObject"/> and finally deactivate all unused objects left active
        /// by calling <see cref="DeactivateUnused"/>.
        /// </summary>
        public void SetAllUnused()
        {
            currentlyUsed = 0;
        }

        /// <summary>
        /// Get an unused <see cref="ArcMeshObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="ArcMeshObject"/> from the pool. </returns>
        public ArcMeshObject GetArcMeshObject()
        {

            // First try to get unused but already active ArcMeshObjects from the pool.
            if (currentlyUsed < currentlyActive)
                return arcMeshObjects[currentlyUsed++];

            // Otherwise we get the first unused ArcMeshObject and activate it.
            if (currentlyUsed < arcMeshObjects.Count)
            {
                arcMeshObjects[currentlyUsed].gameObject.SetActive(true);
                ++currentlyActive;
                return arcMeshObjects[currentlyUsed++];
            }

            // If all ArcMeshObjects are already in use we create a new one.
            var arcMesh = Object.Instantiate(arcMeshPrefab);

            arcMeshObjects.Add(arcMesh);
            ++currentlyActive;
            ++currentlyUsed;
            return arcMesh;
        }
    }
}