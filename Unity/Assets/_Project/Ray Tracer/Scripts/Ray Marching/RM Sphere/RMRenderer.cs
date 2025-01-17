﻿using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Sphere
{
    /// <summary>
    /// Renders a sphere used for different visualisations.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RMRenderer:MonoBehaviour
    {
        private Vector3 center;
        private float radius;

        /// <summary>
        /// The center of the sphere.
        /// </summary>
        public Vector3 Center
        {
            get => center;
            set
            {
                // Because we often reset the origin to the same value this check improves performance.
                if (center == value)
                    return;

                center = value;
                transform.position = center;
            }
        }

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float Radius
        {
            get => radius;
            set
            {
                // Because we often reset the origin to the same value this check improves performance.
                if (radius == value)
                    return;

                radius = value;
                transform.localScale = new Vector3(radius,radius,radius);
            }
        }
        
        
        private Material material;
        /// <summary>
        /// The material used to draw the sphere.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set
            {
                // Because we often reset the material to the same value this check improves performance.
                if (material == value)
                    return;

                material = value;
                meshRenderer.material = material;
            }
        }

        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}