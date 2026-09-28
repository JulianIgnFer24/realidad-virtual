using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    const float SPHERE_RADIUS = 1.1f;
    const float PLANE_SIZE = 3.4f;
    const int CHECKER_RESOLUTION = 256;
    const int CHECKER_COUNT = 4;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateScene()
    {
        // Root = the figure that rotates (at origin)
        // Processing initial model rotation: rotateX(-0.42) then rotateY(-0.55)
        // = R_y(-0.55) * R_x(-0.42)
        var root = new GameObject("Root");
        root.transform.position = Vector3.zero;
        // Initial rotation to match Processing's fixed view
        root.transform.rotation = Quaternion.Euler(0, -0.55f * Mathf.Rad2Deg, 0) * Quaternion.Euler(-0.42f * Mathf.Rad2Deg, 0, 0);

        // Create checkerboard textures
        var sphereTexture = CreateCheckerboardTexture(Color.white, new Color(0.24f, 0.24f, 0.24f), "SphereTexture");
        var plane1Texture = CreateCheckerboardTexture(new Color(0.35f, 0.63f, 0.94f), new Color(0.1f, 0.2f, 0.37f), "Plane1Texture");
        var plane2Texture = CreateCheckerboardTexture(new Color(0.96f, 0.59f, 0.27f), new Color(0.39f, 0.18f, 0.08f), "Plane2Texture");

        var standardShader = Shader.Find("Standard");

        var sphereMat = new Material(standardShader)
        {
            name = "SphereMaterial",
            mainTexture = sphereTexture
        };

        var plane1Mat = new Material(standardShader)
        {
            name = "Plane1Material",
            mainTexture = plane1Texture
        };

        var plane2Mat = new Material(standardShader)
        {
            name = "Plane2Material",
            mainTexture = plane2Texture
        };

        // Sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.transform.SetParent(root.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * SPHERE_RADIUS * 2f;
        sphere.GetComponent<Renderer>().material = sphereMat;
        Destroy(sphere.GetComponent<SphereCollider>());

        // Plane XY (horizontal) - orientation 0 on Y axis (normal along Z)
        // Unity Plane is XZ, rotateX(90) -> XY
        var planeXY = CreateDoubleSidedPlane("PlaneXY", root.transform, plane1Mat);
        planeXY.transform.localPosition = Vector3.zero;
        planeXY.transform.localScale = new Vector3(PLANE_SIZE / 10f, 1f, PLANE_SIZE / 10f);
        planeXY.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Plane YZ (vertical) - orientation 90° on Y axis (normal along -X)
        // Unity Plane is XZ, rotateZ(90) -> YZ (normal -X)
        var planeYZ = CreateDoubleSidedPlane("PlaneYZ", root.transform, plane2Mat);
        planeYZ.transform.localPosition = Vector3.zero;
        planeYZ.transform.localScale = new Vector3(PLANE_SIZE / 10f, 1f, PLANE_SIZE / 10f);
        planeYZ.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        // Camera - match Processing default camera
        // Processing: camera at (0,0,606) looking at (0,0,0), user translates to center
        // Scale 100x: camera at (0,0,6.06)
        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        camObj.transform.position = new Vector3(0f, 0f, -6.06f);
        camObj.transform.rotation = Quaternion.identity; // Root has the rotation
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(18f/255f, 20f/255f, 30f/255f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Lights - match Processing
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(60f/255f, 60f/255f, 70f/255f);

        var light1Obj = new GameObject("DirectionalLight1");
        var light1 = light1Obj.AddComponent<Light>();
        light1.type = LightType.Directional;
        light1.color = new Color(220f/255f, 215f/255f, 200f/255f);
        light1.intensity = 1.5f;
        light1Obj.transform.rotation = Quaternion.LookRotation(new Vector3(-0.5f, 0.7f, -0.6f));

        var light2Obj = new GameObject("DirectionalLight2");
        var light2 = light2Obj.AddComponent<Light>();
        light2.type = LightType.Directional;
        light2.color = new Color(90f/255f, 90f/255f, 110f/255f);
        light2.intensity = 0.5f;
        light2Obj.transform.rotation = Quaternion.LookRotation(new Vector3(0.5f, -0.7f, 0.6f));

        var pointLightObj = new GameObject("PointLight");
        var pointLight = pointLightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 230f/255f, 200f/255f);
        pointLight.intensity = 2f;
        pointLight.range = 20f;
        pointLightObj.transform.position = new Vector3(7.2f, 1.75f, 4.2f);

        // RotationController on Root
        var controller = root.AddComponent<RotationController>();
        controller.figureRoot = root.transform;

        // HUD Camera (overlay)
        var hudCamObj = new GameObject("HUDCamera");
        var hudCam = hudCamObj.AddComponent<Camera>();
        hudCam.clearFlags = CameraClearFlags.Depth;
        hudCam.cullingMask = 0;
        hudCam.depth = 1;
        var hud = hudCamObj.AddComponent<HUDDisplay>();
        hud.controller = controller;
    }

    static Texture2D CreateCheckerboardTexture(Color light, Color dark, string name)
    {
        var tex = new Texture2D(CHECKER_RESOLUTION, CHECKER_RESOLUTION, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        int cellSize = CHECKER_RESOLUTION / CHECKER_COUNT;
        Color[] pixels = new Color[CHECKER_RESOLUTION * CHECKER_RESOLUTION];

        for (int y = 0; y < CHECKER_RESOLUTION; y++)
        {
            for (int x = 0; x < CHECKER_RESOLUTION; x++)
            {
                int cx = x / cellSize;
                int cy = y / cellSize;
                pixels[y * CHECKER_RESOLUTION + x] = ((cx + cy) % 2 == 0) ? light : dark;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static GameObject CreateDoubleSidedPlane(string name, Transform parent, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);

        // Front face (normal +Y in local space)
        var front = GameObject.CreatePrimitive(PrimitiveType.Plane);
        front.name = "Front";
        front.transform.SetParent(go.transform);
        front.transform.localPosition = new Vector3(0f, 0.001f, 0f); // Slight offset to avoid z-fighting
        front.transform.localRotation = Quaternion.identity;
        front.transform.localScale = Vector3.one;
        front.GetComponent<Renderer>().material = material;
        Destroy(front.GetComponent<MeshCollider>());

        // Back face (normal -Y in local space) - rotate 180° on X
        var back = GameObject.CreatePrimitive(PrimitiveType.Plane);
        back.name = "Back";
        back.transform.SetParent(go.transform);
        back.transform.localPosition = new Vector3(0f, -0.001f, 0f);
        back.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        back.transform.localScale = Vector3.one;
        back.GetComponent<Renderer>().material = material;
        Destroy(back.GetComponent<MeshCollider>());

        return go;
    }
}

public class HUDDisplay : MonoBehaviour
{
    public RotationController controller;

    void OnGUI()
    {
        GUIStyle style = new GUIStyle
        {
            fontSize = 14,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };

        GUIStyle statusStyle = new GUIStyle(style)
        {
            normal = { textColor = new Color(120f/255f, 220f/255f, 1f) }
        };

        GUI.Label(new Rect(18, 16, 400, 20), "Flechas IZQ/DER : giro sobre eje VERTICAL", style);
        GUI.Label(new Rect(18, 34, 400, 20), "Flechas ARR/ABA : giro sobre eje HORIZONTAL", style);
        GUI.Label(new Rect(18, 52, 400, 20), "D : detener     R : reiniciar posicion", style);
        GUI.Label(new Rect(18, 78, 600, 20), controller?.GetStatus() ?? "", statusStyle);
    }
}