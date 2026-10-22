using UnityEngine;

namespace BoxRoom
{
    [DisallowMultipleComponent]
    public sealed class BoxRoomBootstrap : MonoBehaviour
    {
        [Header("Room")]
        public float size = 20f;
        public float wallThickness = 0.2f;

        [Header("Colors")]
        public Color floorColor = new Color(0.30f, 0.32f, 0.36f);
        public Color wallColor = new Color(0.72f, 0.75f, 0.80f);
        public Color ceilingColor = new Color(0.62f, 0.65f, 0.70f);
        public Color crateColor = new Color(0.55f, 0.36f, 0.18f);

        private void Awake()
        {
            if (transform.Find("Room") == null) BuildRoom();
            if (transform.Find("Sun") == null) BuildLights();
            if (transform.Find("Player") == null) BuildPlayer();
            TryPlaceDecoration();
        }

        public void BuildRoom()
        {
            GameObject roomObject = new GameObject("Room");
            roomObject.transform.SetParent(transform, false);

            float s = size;
            float t = wallThickness;

            CreateCube("Floor", new Vector3(s + t * 2f, t, s + t * 2f), new Vector3(0f, -t / 2f, 0f), floorColor, roomObject.transform);
            CreateCube("Ceiling", new Vector3(s + t * 2f, t, s + t * 2f), new Vector3(0f, s + t / 2f, 0f), ceilingColor, roomObject.transform);
            CreateCube("Wall Back", new Vector3(s + t * 2f, s + t, t), new Vector3(0f, s / 2f, -s / 2f - t / 2f), wallColor, roomObject.transform);
            CreateCube("Wall Front", new Vector3(s + t * 2f, s + t, t), new Vector3(0f, s / 2f, s / 2f + t / 2f), wallColor, roomObject.transform);
            CreateCube("Wall Left", new Vector3(t, s + t, s + t * 2f), new Vector3(-s / 2f - t / 2f, s / 2f, 0f), wallColor, roomObject.transform);
            CreateCube("Wall Right", new Vector3(t, s + t, s + t * 2f), new Vector3(s / 2f + t / 2f, s / 2f, 0f), wallColor, roomObject.transform);
        }

        public void BuildLights()
        {
            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(transform, false);
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        public void BuildPlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(transform, false);
            player.transform.localPosition = Vector3.zero;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.3f;

            GameObject cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 75f;
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();

            player.AddComponent<FirstPersonController>();
        }

        public void TryPlaceDecoration()
        {
            Transform decor = transform.Find("Decor");
            if (decor == null)
            {
                decor = new GameObject("Decor").transform;
                decor.SetParent(transform, false);
            }

            if (decor.childCount > 0 && decor.GetComponentInChildren<Renderer>() == null)
            {
                for (int i = decor.childCount - 1; i >= 0; i--)
                {
                    Destroy(decor.GetChild(i).gameObject);
                }
            }

            if (decor.childCount == 0)
            {
                CreateFallbackCrate(decor);
            }

            decor.localPosition = new Vector3(4f, 0f, 4f);
        }

        public void CreateFallbackCrate(Transform parent)
        {
            CreateCube("CrateFallback", Vector3.one * 0.9f, new Vector3(0f, 0.45f, 0f), crateColor, parent);
        }

        private GameObject CreateCube(string name, Vector3 scale, Vector3 localPosition, Color color, Transform parent)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = scale;

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateLitMaterial(color, cube.name);
            return cube;
        }

        private static Material CreateLitMaterial(Color color, string materialName)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

            Material material = new Material(shader);
            material.name = materialName;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }
    }
}