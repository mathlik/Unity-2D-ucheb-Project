using System.IO;
using MetroPlatform;
using MetroPlatform.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Меню: Tools > Build Lesson 1 Scene
// Собирает прототип уровня из урока 1: Ground, Wall, Player (префабы), Background.
public static class BuildLesson1Scene
{
    private const string SpritePath = "Assets/Sprites/Square.png";

    [MenuItem("Tools/Build Lesson 1 Scene")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Sprites");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Level");
        Directory.CreateDirectory("Assets/Scenes");

        Sprite square = CreateSquareSprite();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0f, 2f, -10f);
        }

        // Пустой родитель по нулевым координатам.
        GameObject level = new GameObject("Level");
        level.transform.position = Vector3.zero;

        // Фон (только SpriteRenderer, без физики).
        GameObject bg = CreateSquare("Background", square, new Color(0.25f, 0.3f, 0.45f), -10, false);
        bg.transform.SetParent(level.transform);
        bg.transform.position = new Vector3(0f, 2f, 0f);
        bg.transform.localScale = new Vector3(30f, 14f, 1f);

        // Пол.
        GameObject ground = CreateSquare("Ground", square, new Color(0.35f, 0.25f, 0.15f), 0, true);
        ground.transform.localScale = new Vector3(10f, 1f, 1f);
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        GameObject groundPrefab = SavePrefab(ground, "Assets/Level/Ground.prefab");
        groundPrefab.transform.SetParent(level.transform);

        // Стена.
        GameObject wall = CreateSquare("Wall", square, new Color(0.5f, 0.5f, 0.5f), 0, true);
        wall.transform.localScale = new Vector3(1f, 2f, 1f);
        wall.transform.position = new Vector3(4.5f, 1f, 0f);
        GameObject wallPrefab = SavePrefab(wall, "Assets/Level/Wall.prefab");
        wallPrefab.transform.SetParent(level.transform);

        // Игрок: SpriteRenderer + BoxCollider2D + Rigidbody2D + PlayerMovement.
        GameObject player = CreateSquare("Player", square, Color.red, 1, true);
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        player.AddComponent<Rigidbody2D>().gravityScale = 1f;
        player.AddComponent<PlayerMovement>();
        GameObject playerPrefab = SavePrefab(player, "Assets/Prefabs/Player.prefab");
        // Ссылку _rigidbody можно назначить вручную (перетащить компонент в слот).
        SerializedObject so = new SerializedObject(playerPrefab.GetComponent<PlayerMovement>());
        so.FindProperty("_rigidbody").objectReferenceValue = playerPrefab.GetComponent<Rigidbody2D>();
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("Lesson 1 scene built: Assets/Scenes/Main.unity");
    }

    private static GameObject CreateSquare(string name, Sprite sprite, Color color, int order, bool collider)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        if (collider) go.AddComponent<BoxCollider2D>();
        return go;
    }

    // Сохраняет объект как префаб и оставляет в сцене связанный экземпляр.
    private static GameObject SavePrefab(GameObject go, string path)
    {
        return PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
    }

    private static Sprite CreateSquareSprite()
    {
        if (!File.Exists(SpritePath))
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] px = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SpritePath);
        }

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 32;
        imp.filterMode = FilterMode.Point;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }
}
