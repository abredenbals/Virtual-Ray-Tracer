using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace _Project.Ray_Tracer.Scripts.RT_Scene
{


    /// <summary>
    /// Represents a mesh in the ray tracer scene. Requires that the attached game object has a mesh and a material
    /// based on the RayTracerShader. Should be considered something like a tag to indicate to the scene manager that
    /// this mesh should be sent to the ray tracer. Almost all actual information for the ray tracer is stored in the
    /// transform, mesh and material components.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)), RequireComponent(typeof(Outline))]
    public class RTMesh : MonoBehaviour
    {

        private static Shader StandardShader = null;
        private static Shader TransparentShader = null;

        private static readonly int ambient = Shader.PropertyToID("_Ambient");
        private static readonly int diffuse = Shader.PropertyToID("_Diffuse");
        private static readonly int specular = Shader.PropertyToID("_Specular");
        private static readonly int shininess = Shader.PropertyToID("_Shininess");
        private static readonly int refractiveIndex = Shader.PropertyToID("_RefractiveIndex");

        private MeshCollider meshCollider;

        [Serializable]
        public class MeshChanged : UnityEvent { }
        /// <summary>
        /// An event invoked whenever a property of this mesh is changed.
        /// </summary>
        public MeshChanged OnMeshChanged, OnMeshColorChanged, OnAmbientChanged, OnDiffuseChanged, 
            OnSpecularChanged, OnShininessChanged, OnRefractiveIndexChanged, OnMaterialTypeChanged;

        /// <summary>
        /// An event invoked whenever a mesh is selected.
        /// </summary>
        public MeshChanged OnMeshSelected;

        /// <summary>
        /// The underlying <see cref="UnityEngine.Material"/> used by the mesh. Its shader should be either
        /// RayTracerShader or RayTracerShaderTransparent.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// The outline used to highlight the mesh when it is selected.
        /// </summary>
        public Outline Outline { get; private set; }

        /// <summary>
        /// The position of the mesh.
        /// </summary>
        public Vector3 Position
        {
            get => transform.position;
            set
            {
                if (value == transform.position) return;
                transform.position = value;
            }
        }

        /// <summary>
        /// The rotation of the mesh.
        /// </summary>
        public Vector3 Rotation
        {
            get => transform.eulerAngles;
            set
            {
                if (value == transform.eulerAngles) return;
                transform.eulerAngles = value;
            }
        }

        /// <summary>
        /// The scale of the mesh.
        /// </summary>
        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                if (value == transform.localScale) return;
                transform.localScale = value;
            }
        }

        /// <summary>
        /// The color of the mesh.
        /// </summary>
        public Color Color
        {
            get => Material.color;
            set
            {
                if (value == Material.color) return;
                Material.color = value;
                OnMeshChanged?.Invoke();
                OnMeshColorChanged?.Invoke();
            }
        }

        /// <summary>
        /// The ambient component of the mesh's material.
        /// </summary>
        public float Ambient
        {
            get => Material.GetFloat(ambient);
            set
            {
                if (value == Material.GetFloat(ambient)) return;
                Material.SetFloat(ambient, value);
                OnMeshChanged?.Invoke();
                OnAmbientChanged?.Invoke();
            }
        }

        /// <summary>
        /// The diffuse component of the mesh's material.
        /// </summary>
        public float Diffuse
        {
            get => Material.GetFloat(diffuse);
            set
            {
                if (value == Material.GetFloat(diffuse)) return;
                Material.SetFloat(diffuse, value);
                OnMeshChanged?.Invoke();
                OnDiffuseChanged?.Invoke();
            }
        }

        /// <summary>
        /// The specular component of the mesh's material.
        /// </summary>
        public float Specular
        {
            get => Material.GetFloat(specular);
            set
            {
                if (value == Material.GetFloat(specular)) return;
                Material.SetFloat(specular, value);
                OnMeshChanged?.Invoke();
                OnSpecularChanged?.Invoke();
            }
        }

        /// <summary>
        /// The shininess of the mesh's material.
        /// </summary>
        public float Shininess
        {
            get => Material.GetFloat(shininess);
            set
            {
                if (value == Material.GetFloat(shininess)) return;
                Material.SetFloat(shininess, value);
                OnMeshChanged?.Invoke();
                OnShininessChanged?.Invoke();
            }
        }

        /// <summary>
        /// The refractive index of the mesh's material.
        /// </summary>
        public float RefractiveIndex
        {
            get => Material.GetFloat(refractiveIndex);
            set
            {
                if (value == Material.GetFloat(refractiveIndex)) return;
                Material.SetFloat(refractiveIndex, value);
                OnMeshChanged?.Invoke();
                OnRefractiveIndexChanged?.Invoke();
            }
        }

        public enum MeshType
        {
            Sphere,
            Cube,
            Capsule,
            Cylinder,
            Goat,
            Prism,
            Other
        }
        public enum ObjectType
        {
            Opaque,
            Transparent,
            Mirror
        }

        private MeshType shape = MeshType.Other;
        /// <summary>
        /// The shape of the mesh. i.e. sphere, cube, etc. decides the SDF of the object.
        /// </summary>
        public MeshType Shape
        {
            get => Shape;
            private set
            {
                Shape = value;
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// Whether the mesh is transparent a mirror or just opaque.
        /// </summary>
        [SerializeField] 
        public ObjectType Type;

        [SerializeField]
        private bool shadeSmooth = true;
        /// <summary>
        /// Whether the ray tracer smooths the normals of the mesh. Does not affect the visuals of the mesh in the
        /// Unity scene.
        /// </summary>
        public bool ShadeSmooth
        {
            get => shadeSmooth;
            private set 
            { 
                if (value == shadeSmooth) return;
                shadeSmooth = value;
                OnMeshChanged?.Invoke();
            }
        }

        public void ChangeObjectType(ObjectType type)
        {
            if (Type == type) return;

            switch (type)
            {
                case ObjectType.Transparent:
                    Material.shader = TransparentShader;
                    Material.SetFloat(ambient, 0);
                    Material.SetFloat(diffuse, 0);
                    Material.SetFloat(specular, 0);
                    Material.SetFloat(shininess, 128);
                    Material.SetFloat(refractiveIndex, 1.5f);
                    Color color = Material.color;
                    color.a = 120 / 256f;
                    Material.color = color;
                    break;
                case ObjectType.Opaque:
                    Material.shader = StandardShader;
                    Material.SetFloat(ambient, 0.2f);
                    Material.SetFloat(diffuse, 1f);
                    Material.SetFloat(specular, 0);
                    Material.SetFloat(shininess, 1);
                    break;
                case ObjectType.Mirror:
                    Material.shader = StandardShader;
                    Material.SetFloat(ambient, 0);
                    Material.SetFloat(diffuse, 0);
                    Material.SetFloat(specular, 1);
                    Material.SetFloat(shininess, 128);
                    break;
            }

            Type = type;
            OnMeshChanged?.Invoke();
            OnMaterialTypeChanged?.Invoke();
        }

        private void FixedUpdate()
        {
            if (transform.hasChanged) OnMeshChanged?.Invoke();
        }

        private void Update()
        {
            transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }

        public Vector3 normalRM(ref Vector3 point, ref Vector3 collision)
        {
            RaycastHit hit;
            if (shape == MeshType.Sphere)
            {
                return Vector3.Normalize(point - Position); //smooth normal
            }
            
            Physics.Raycast(point, collision - point, out hit);
            // should probably put the computation of the normal into another function that is not called every time to save computation time.
            return Vector3.Normalize(hit.normal);
        }
        
        public float DistanceToPoint(ref Vector3 point, out Vector3 collision)
        {
            switch (shape)
            {
                    
                case MeshType.Sphere:
                    // doesnt work for ellipsoids
                    if (Scale.x != Scale.y || Scale.y != Scale.z)
                    {
                        break;
                    }
                    Vector3 insideObject = (point - Position).normalized * Scale.x/2.0f;
                    collision = Position + insideObject;
                    return Vector3.Distance(point, collision);
                
                
                case MeshType.Cube:
                    // Allows for the function call below. TODO: make a RMMesh class, and put this into initialize to only be called once.
                    if (!meshCollider.convex)
                    {
                        meshCollider.convex = true;
                    }
                    collision = meshCollider.ClosestPoint(point); //use ClosestPoint instead if only convex
                    if (Rotation != Vector3.zero)
                    {
                        break;
                    }
                    // the rotation invariant part unfortunately doesnt work, thus it uses the standard case for that.
                    /*Quaternion rot = new Quaternion();
                    rot.SetEulerAngles(Rotation);
                    Vector3 rotatedPoint = rot * (point - Position) + Position;*/
                    
                    // Taken and corrected from https://www.alanzucconi.com/2016/07/01/signed-distance-functions/#part3
                    float x = Mathf.Max(Math.Abs(point.x - Position.x) - Scale.x / 2.0f, 0.0f);
                    float y = Mathf.Max(Math.Abs(point.y - Position.y) - Scale.y / 2.0f, 0.0f);
                    float z = Mathf.Max(Math.Abs(point.z - Position.z) - Scale.z / 2.0f, 0.0f);
                    return Vector3.Magnitude(new Vector3(x, y, z));
            }
            // Allows for the function call below. TODO: make a RMMesh class, and put this into initialize to only be called once.
            if (!meshCollider.convex)
            {
                meshCollider.convex = true;
            }
            collision = meshCollider.ClosestPoint(point); //use ClosestPoint instead if only convex
            return Vector3.Distance(point, collision);
        }
        

        private void Awake()
        {
            if (StandardShader == null)
                StandardShader = Shader.Find("Custom/RayTracerShader");
            if (TransparentShader == null)
                TransparentShader = Shader.Find("Custom/RayTracerShaderTransparent");

            Initialize();
        }

        /// <summary>
        /// Initialize this ray tracer mesh based on its attached components. What material settings are used is
        /// determined by inspecting the attached <see cref="MeshRenderer"/>. If this component is missing or if
        /// something else goes wrong an error is printed.
        /// </summary>
        private void Initialize()
        {
            meshCollider = GetComponent<MeshCollider>();
            if (Enum.TryParse<MeshType>(meshCollider.sharedMesh.name, out shape))
            {
            }
            else
            {
                shape = MeshType.Other;
                Debug.LogError("Unrecognized Mesh");
            }
            GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            // Find the material used by this object and verify that it uses the correct shader.
            Material = GetComponent<MeshRenderer>().material;
            
            Type = Material.name.Replace("(Instance)","").Trim()  switch
            {
                "Glass" => ObjectType.Transparent,
                "Mirror" => ObjectType.Mirror,
                _ => ObjectType.Opaque
            };
            
            if (Material == null)
                Debug.LogError("Could not find material of " + gameObject.name + "!");
            if (Type == ObjectType.Transparent && Material.shader != TransparentShader)
                Debug.LogError("Material of " + gameObject.name + " uses a non transparent shader or a shader not" +
                    " supported by the ray tracer!");
            if (Type != ObjectType.Transparent && Material.shader != StandardShader)
                Debug.LogError("Material of " + gameObject.name + " uses a transparent shader or a shader not" +
                    " supported by the ray tracer!");

            // Find the outline component attached to this object.
            Outline = GetComponent<Outline>();
            if (Outline == null)
                Debug.LogWarning("Could not find outline of " + gameObject.name + "!");
            else
                Outline.enabled = false; // Outline should be disabled by default.
        }
    }
}
