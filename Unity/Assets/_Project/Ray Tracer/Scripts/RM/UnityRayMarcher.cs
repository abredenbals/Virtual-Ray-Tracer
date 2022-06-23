using UnityEngine;
using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RM.RM_Sphere;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.Utility;

namespace _Project.Ray_Tracer.Scripts.RM
{
    public class UnityRayMarcher : UnityRayTracer
    {
        private const int RM_MAX_ITERATIONS = 250;
        public delegate void RayMarcherChanged();
        /// <summary>
        /// An event invoked whenever a property of this ray marcher is changed.
        /// </summary>
        public event RayMarcherChanged OnRayMarcherChanged;
        
        [SerializeField]
        private float epsilonRM = 0.001f;
        /// <summary>
        /// A small floating point value used to find hits with Ray Marching. Needs to be bigger than the Epsilon for shadow Acne.
        /// </summary>
        public float EpsilonRM
        {
            get { return epsilonRM; }
            set 
            {
                epsilonRM = value;
                OnRayMarcherChanged?.Invoke();
            }
        }



        private static UnityRayMarcher instance = null;
        public static UnityRayMarcher RMGet()
        {
            return instance;
        }
        public override List<TreeNode<RTRay>> Render()
        {
            return Render(out _);
        }
        
        public List<TreeNode<RTRay>> Render(out List<TreeNode<List<(float, Vector3)>>> collisionDistances)
        {
            collisionDistances = new List<TreeNode<List<(float, Vector3)>>>();
            List<TreeNode<RTRay>> rayTrees = new List<TreeNode<RTRay>>();
            scene = rtSceneManager.Scene;
            camera = scene.Camera;

            int width = camera.ScreenWidth;
            int height = camera.ScreenHeight;
            float aspectRatio = (float) width / height;
            float halfScreenHeight = camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            Vector3 origin = camera.transform.position;

            // Trace a ray for each pixel. 
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // Convert the pixel coordinates to camera space positions.
                    float pixelX = -halfScreenWidth + pixelWidth * (x + 0.5f);
                    float pixelY = -halfScreenHeight + pixelHeight * (y + 0.5f);

                    // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                    Vector3 pixel = new Vector3(pixelX, pixelY, camera.ScreenDistance);
                    pixel = camera.transform.rotation * pixel;

                    // This is the distance between the pixel on the screen and the origin. We need this to compensate
                    // for the length of the returned RTRay. Since we have this factor we also use it to normalize this
                    // vector to make the code more efficient.
                    float pixelDistance = pixel.magnitude;

                    // TODO: Compensate for the location of the screen so we don't render objects that are behind the screen. (origin + pixel removed)
                    var collisionDistance = new TreeNode<List<(float, Vector3)>>(new List<(float, Vector3)>());
                    TreeNode<RTRay> rayTree = Trace(origin , pixel,
                                                    pixel / pixelDistance, // Division by magnitude == .normalized.
                                                    MaxDepth, RTRay.RayType.Normal, out collisionDistance);
                    
                    // Fix the origin and the length so we visualize the right ray.
                    rayTree.Data.Origin = origin;
                    rayTree.Data.Length += pixelDistance; 
                    rayTrees.Add(rayTree);
                    collisionDistances.Add(collisionDistance);
                }
            }

            return rayTrees;
        }
        
        /// <summary>
        /// Render the current <see cref="RTSceneManager"/>'s <see cref="RTScene"/> while building up a "high resolution"
        /// image.
        /// </summary>
        /// <returns> A high resolution render in the form of a <see cref="Texture2D"/>. </returns>
        public override Texture2D RenderImage()
        {
            scene = rtSceneManager.Scene;
            camera = scene.Camera;
            
            int width = camera.ScreenWidth;
            int height = camera.ScreenHeight;
            float aspectRatio = (float) width / height;
            
            // Scale width and height in such a way that the image has around a total of 160,000 pixels.
            int scaleFactor = Mathf.RoundToInt(Mathf.Sqrt(160000f / (width * height)));
            width = scaleFactor * width;
            height = scaleFactor * height;
            
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            // Calculate the other variables.
            float halfScreenHeight = camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            int superSamplingSquared = SuperSamplingFactor * SuperSamplingFactor;
            Vector3 origin = camera.transform.position;

            // Trace a ray for each pixel.
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Color color = Color.black;
                    float step = 1f / SuperSamplingFactor;

                    for (int supY = 0; supY < SuperSamplingFactor; supY++)
                    {
                        float difY = pixelHeight * (y + step * (0.5f + supY));

                        for (int supX = 0; supX < SuperSamplingFactor; supX++)
                        {
                            float difX = pixelWidth * (x + step * (0.5f + supX));

                            // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                            Vector3 pixel = new Vector3(-halfScreenWidth + difX, -halfScreenHeight + difY, camera.ScreenDistance); 
                            pixel = camera.transform.rotation * pixel;

                            // Compensate for the location of the screen so we don't render objects that are behind the screen.
                            color += TraceImage(origin + pixel, pixel.normalized, MaxDepth); //TODO: there was origin + pixel to exclude things behind the screen. find a workaround.
                        }
                    }

                    // Divide by supersamplingFactor squared and set alpha levels back to 1. It should always be 1!
                    color /= superSamplingSquared;
                    color.a = 1.0f;
                    image.SetPixel(x, y, ClampColor(color));
                }
            }

            image.Apply(); // Very important.
            return image;
        }
        
        //screen to check, if object is behind screen. needs to be added to the other functions as well. TODO: implement
        protected TreeNode<RTRay> Trace(Vector3 origin, Vector3 screen, Vector3 direction, int depth, RTRay.RayType type, out TreeNode<List<(float, Vector3)>> collisionDistances)
        {
            HitInfo hitInfo;
            float totalDist;
            bool intersected = RayMarch(origin, direction, out hitInfo, out totalDist,out collisionDistances, RM_MAX_ITERATIONS, 99.0f, Mathf.Infinity, camera.ScreenDistance);
            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());

            // If we did not hit anything we return a no hit ray whose result color is black.
            if (!intersected)
            {
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            }

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hitInfo.Point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                {
                    rayTree.AddChild(TraceLight(ref lightVector, light, in hitInfo, out TreeNode<List<(float, Vector3)>> distList));
                    collisionDistances.AddChild(distList);
                }
                    
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
            {
                var newRays = TraceReflectionAndRefraction(depth, hitInfo, out var ChildIterationList);
                for (int i = 0; i < newRays.Count; i++)
                {
                    rayTree.AddChild(newRays[i]);
                    collisionDistances.AddChild(ChildIterationList[i]);
                }
            }

            // Add the child ray colors to the parent ray.
            foreach (var child in rayTree.Children)
            {
                color += child.Data.Color;
            }
            rayTree.Data = new RTRay(origin, direction, totalDist, ClampColor(color), type);
            return rayTree;
        }
        
        protected RTRay TraceLight(ref Vector3 lightVector, RTLight light, in HitInfo hitInfo, out TreeNode<List<(float, Vector3)>> distList)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, light.transform.position - hitInfo.Point);

            distList = new TreeNode<List<(float, Vector3)>>(new List<(float, Vector3)>());
            // If we render shadows, check whether a shadow ray first meets the light or an object.
            Vector3 shadowOrigin = hitInfo.Point + Epsilon * hitInfo.Normal;
            if (RenderShadows)
            {
                float totalDist;
                
                
                // Trace a ray until we reach the light source. If we hit something return a shadow ray. TODO: do we show the rayMarch to the lightsource as well?
                if (RayMarch(shadowOrigin, lightVector, out _, out totalDist, out distList, RM_MAX_ITERATIONS, lightDistance, lightDistance, 0.0f))
                    return new RTRay(shadowOrigin, lightVector, totalDist, Color.black,
                        RTRay.RayType.Shadow);
            }

            
            // We either don't render shadows or nothing is between the object and the light source.
            
            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = light.Ambient * light.Color * hitInfo.Color;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                     light.Color * hitInfo.Color; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) * 
                     hitInfo.Specular * light.Specular * light.Color; // Is

            return new RTRay(shadowOrigin, lightVector, lightDistance, ClampColor(color), RTRay.RayType.Light);
        }
        
        protected override Color TraceImage(Vector3 origin, Vector3 direction, int depth)
        {
            HitInfo hitInfo;
            float totalDist;
            bool intersected = RayMarch(origin, direction, out hitInfo, out totalDist, out _, RM_MAX_ITERATIONS, 99.0f, Mathf.Infinity, camera.ScreenDistance);

            // If we did not hit anything we return the background color.
            if (!intersected) return BackgroundColor;

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hitInfo.Point).normalized;

                if (Vector3.Dot(hitInfo.Normal,lightVector) >= 0.0f) 
                    color += TraceLightImage(ref lightVector, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);
            
            return ClampColor(color);
        }
        
        protected override Color TraceLightImage(ref Vector3 lightVector, RTLight light, in HitInfo hitInfo)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, light.transform.position - hitInfo.Point);

            // If we render shadows, check whether a shadow ray first meets the light or an object.
            if (RenderShadows)
            {
                Vector3 shadowOrigin = hitInfo.Point + Epsilon * hitInfo.Normal;
                
                // Trace a ray until we reach the light source. If we hit something return a shadow ray.
                if (RayMarch(shadowOrigin, lightVector, out _, out _, out _, RM_MAX_ITERATIONS, lightDistance, lightDistance, 0.0f))
                    return Color.black;
            }

            // We either don't render shadows or nothing is between the object and the light source.
            
            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = light.Ambient * light.Color * hitInfo.Color;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                     light.Color * hitInfo.Color; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) *
                     hitInfo.Specular * light.Specular * light.Color; // Is

            return ClampColor(color);
        }
        
        protected List<TreeNode<RTRay>> TraceReflectionAndRefraction(int depth, in HitInfo hitInfo, out List<TreeNode<List<(float, Vector3)>>> collisionDistances)
        {
            List<TreeNode<RTRay>> rays = new List<TreeNode<RTRay>>();
            TreeNode<RTRay> node;
            TreeNode<List<(float, Vector3)>> collisionDistance;
            collisionDistances = new List<TreeNode<List<(float, Vector3)>>>();

            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                node = Trace(hitInfo.Point + hitInfo.Normal * Epsilon,Vector3.zero,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1, RTRay.RayType.Reflect, out collisionDistance);
                node.Data.Color *= kr;
                collisionDistances.Add(collisionDistance);
                rays.Add(node);

                // Refract.
                node = Trace(hitInfo.Point - hitInfo.Normal * Epsilon,Vector3.zero,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1, RTRay.RayType.Refract, out collisionDistance);
                node.Data.Color *= kt;
                rays.Add(node);
                collisionDistances.Add(collisionDistance);
                
                return rays;
            }

            // The object is not transparent, so we only reflect (provided it has a non zero specular component).
            if (hitInfo.Specular <= 0.0f) return rays;
            
            node = Trace(hitInfo.Point + hitInfo.Normal * Epsilon,Vector3.zero,
                Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                depth - 1, RTRay.RayType.Reflect, out collisionDistance);
            // node.Data.Color *= hitInfo.Specular;
            // rays.Add(node);
            // collisionDistances.Add(collisionDistance);
            
            return rays;
        }
        
        protected override Color TraceReflectionAndRefractionImage(int depth, in HitInfo hitInfo)
        {
            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                Color color;

                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                color = kr * TraceImage(hitInfo.Point + hitInfo.Normal * Epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);

                // Refract.
                color += kt * TraceImage(hitInfo.Point - hitInfo.Normal * Epsilon,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1);

                return ClampColor(color);
            }

            // The object is not transparent, so we only reflect (provided it has a non zero specular component).
            if (hitInfo.Specular > 0.0f)
                return hitInfo.Specular * TraceImage(hitInfo.Point + hitInfo.Normal * Epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);
            
            return Color.black;
        }
        
        private bool RayMarch(Vector3 origin, Vector3 direction, out HitInfo hit, out float totalDist, out TreeNode<List<(float, Vector3)>> distList, int maxIterations,
            float maxIterationDistance, float maxTotalDistance, float screenDistance)
        {
            // initialize some variables
            distList = new TreeNode<List<(float, Vector3)>>(new List<(float, Vector3)>());
            List<RTMesh> meshes = scene.Meshes;
            RTMesh nearestObject = meshes[0];
            Vector3 nearestCollision = Vector3.zero;
            Vector3 nearestNormal = Vector3.zero;
            totalDist = 0.0f;
            // Go through iterations until break conditions or max iterations are met.
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // find the nearest object
                float minDist = Mathf.Infinity;
                foreach (var mesh in meshes)
                {
                    Vector3 collision;
                    Vector3 normalPlaceholder;
                    float distance = mesh.DistanceToPoint(ref origin, out collision, out normalPlaceholder);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        nearestObject = mesh;
                        nearestCollision = collision;
                        nearestNormal = normalPlaceholder;
                    }
                }
                
                // Break conditions
                if (minDist < EpsilonRM)
                {
                    // a "hit", return the corresponding HitInfo
                    distList.Data.Add((totalDist , nearestCollision));
                    hit = new HitInfo(ref origin, ref nearestNormal, ref direction, ref nearestObject);
                    totalDist -= screenDistance;
                    return true;
                } else if (maxIterationDistance < minDist)
                {
                    break;
                } else if (maxTotalDistance < totalDist)
                {
                    break;
                }
                
                // preparations for the next iteration.
                distList.Data.Add((totalDist, nearestCollision));
                totalDist += minDist;
                origin += minDist * direction; // Move the origin of the ray march
            }
            // "no hit" break conditions terminate here.
            hit = new HitInfo(ref origin, ref nearestNormal, ref direction, ref nearestObject);
            return false;
        }
        
        protected void Awake()
        {
            instance = this;
            rayTracerLayer = LayerMask.GetMask("Ray Tracer Objects");
        }

    }
}