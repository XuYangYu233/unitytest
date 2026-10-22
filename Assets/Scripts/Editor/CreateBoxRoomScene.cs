using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace BoxRoom.EditorTools
{
    public static class CreateBoxRoomScene
    {
        private const string ScenePath = "Assets/Scenes/BoxRoom.unity";
        private const string CrateModelPath = "Assets/Art/Kenney/PlatformerKit/Models/FBX format/crate.fbx";

        [MenuItem("Tools/BoxRoom/Create Scene")]
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Game");
            BoxRoomBootstrap bootstrap = root.AddComponent<BoxRoomBootstrap>();
            bootstrap.BuildRoom();
            bootstrap.BuildLights();

            GameObject decor = new GameObject("Decor");
            decor.transform.SetParent(root.transform, false);

            if (TryCreateCrate(decor.transform))
            {
                decor.transform.localPosition = new Vector3(4f, 0f, 4f);
            }
            else
            {
                bootstrap.CreateFallbackCrate(decor.transform);
                decor.transform.localPosition = new Vector3(4f, 0f, 4f);
            }

            EditorSceneManager.MarkSceneDirty(scene);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError("[BoxRoom] Failed to save scene: " + ScenePath);
                return;
            }

            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[BoxRoom] BoxRoom scene created successfully: " + ScenePath);
        }

        private static bool TryCreateCrate(Transform parent)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(CrateModelPath);
            if (model == null)
            {
                Debug.LogWarning("[BoxRoom] Crate model not found, using fallback: " + CrateModelPath);
                return false;
            }

            GameObject crate = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            crate.name = "Crate";
            crate.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            if (crate.GetComponent<Collider>() == null)
            {
                Bounds bounds = CalculateBounds(crate);
                BoxCollider box = crate.AddComponent<BoxCollider>();
                box.center = crate.transform.InverseTransformPoint(bounds.center);
                box.size = crate.transform.InverseTransformVector(bounds.size);
            }

            return true;
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool exists = false;
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ScenePath)
                {
                    exists = true;
                    scenes[i] = new EditorBuildSettingsScene(ScenePath, true);
                    break;
                }
            }

            if (!exists)
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}