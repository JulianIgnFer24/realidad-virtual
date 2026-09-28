using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateScene()
    {
        var root = new GameObject("Root");
        var controller = root.AddComponent<RotatingController>();

        // Create materials
        var sphereMat = new Material(Shader.Find("Standard"))
        {
            name = "SphereMaterial",
            color = new Color(0.2f, 0.6f, 1f, 1f)
        };

        var plane1Mat = new Material(Shader.Find("Standard"))
        {
            name = "Plane1Material",
            color = new Color(1f, 0.3f, 0.3f, 1f)
        };

        var plane2Mat = new Material(Shader.Find("Standard"))
        {
            name = "Plane2Material",
            color = new Color(0.3f, 1f, 0.3f, 1f)
        };

        // Sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.transform.SetParent(root.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * 2f;
        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material = sphereMat;
        Destroy(sphere.GetComponent<SphereCollider>());

        // Plane XY (horizontal) - 4x4 to match sphere diameter (radius 2 = diameter 4)
        var planeXY = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeXY.name = "PlaneXY";
        planeXY.transform.SetParent(root.transform);
        planeXY.transform.localPosition = Vector3.zero;
        planeXY.transform.localScale = new Vector3(0.4f, 1f, 0.4f); // Plane is 10x10, scale 0.4 = 4x4
        planeXY.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Rotate to XY plane
        var planeXYRenderer = planeXY.GetComponent<Renderer>();
        planeXYRenderer.material = plane1Mat;
        Destroy(planeXY.GetComponent<MeshCollider>());

        // Plane XZ (vertical) - 4x4 to match sphere diameter
        var planeXZ = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeXZ.name = "PlaneXZ";
        planeXZ.transform.SetParent(root.transform);
        planeXZ.transform.localPosition = Vector3.zero;
        planeXZ.transform.localScale = new Vector3(0.4f, 1f, 0.4f); // Plane is 10x10, scale 0.4 = 4x4
        planeXZ.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // XZ plane (default)
        var planeXZRenderer = planeXZ.GetComponent<Renderer>();
        planeXZRenderer.material = plane2Mat;
        Destroy(planeXZ.GetComponent<MeshCollider>());

        // Camera
        var camObj = new GameObject("MainCamera");
        var cam = camObj.AddComponent<Camera>();
        camObj.transform.position = new Vector3(0f, 2f, 7f);
        camObj.transform.LookAt(Vector3.zero);
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = Color.black;

        // Light
        var lightObj = new GameObject("DirectionalLight");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 2f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
}