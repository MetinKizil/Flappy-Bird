using System.IO;
using TMPro;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Oyun sahnesini, sprite'ları, PipePair prefab'ını ve UI'yi tek tıkla kurar.
/// Menü: Tools → Flappy Bird → Sahneyi Kur
/// Komut satırı: Unity.exe -batchmode -projectPath . -executeMethod FlappySceneBuilder.Build -quit
/// </summary>
public static class FlappySceneBuilder
{
    private const string SpritesDir = "Assets/Sprites";
    private const string PrefabsDir = "Assets/Prefabs";
    private const string TemplateScene = "Assets/Scenes/SampleScene.unity";
    private const string GameScene = "Assets/Scenes/Game.unity";
    private const string TmpEssentials =
        "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

    private static readonly Color32 SkyColor = new Color32(78, 192, 202, 255);

    [MenuItem("Tools/Flappy Bird/TMP Essentials Yükle")]
    public static void ImportTmpEssentials()
    {
        if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro")) return;
        AssetDatabase.ImportPackage(Path.GetFullPath(TmpEssentials), false);
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Flappy Bird/VS Code'u Script Editörü Yap")]
    public static void SetupVSCode()
    {
        string[] candidates =
        {
            Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe"),
            @"C:\Program Files\Microsoft VS Code\Code.exe",
            @"D:\vscode\Microsoft VS Code\Code.exe",
        };
        string code = System.Array.Find(candidates, File.Exists);
        if (code == null)
        {
            Debug.LogWarning("VS Code bulunamadı.");
            return;
        }
        CodeEditor.SetExternalScriptEditor(code);
        CodeEditor.CurrentEditor.SyncAll(); // .sln / .csproj dosyalarını üret
        Debug.Log("External Script Editor: " + code);
    }

    [MenuItem("Tools/Flappy Bird/Sahneyi Kur")]
    public static void Build()
    {
        Directory.CreateDirectory(SpritesDir);
        Directory.CreateDirectory(PrefabsDir);

        // Kamera ve Global Light 2D'yi hazır almak için URP 2D şablon sahnesinin kopyası.
        // Sahne açılışı kullanılmayan asset'leri boşalttığı için asset'ler bundan SONRA üretilir.
        if (File.Exists(GameScene)) AssetDatabase.DeleteAsset(GameScene);
        AssetDatabase.CopyAsset(TemplateScene, GameScene);
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        Sprite birdSprite = CreateSprite("Bird", 64, 128, DrawBird, false);
        Sprite pipeSprite = CreateSprite("Pipe", 32, 32, DrawPipe, false);
        Sprite groundSprite = CreateSprite("Ground", 32, 32, DrawGround, true);
        Sprite whiteSprite = CreateSprite("White", 4, 4, (x, y, s) => Color.white, false);

        string pipePrefabPath = BuildPipePrefab(pipeSprite);
        var pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pipePrefabPath).GetComponent<PipePair>();

        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SkyColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        BuildBird(birdSprite);
        BuildGround(groundSprite);

        var spawnerGo = new GameObject("PipeSpawner");
        spawnerGo.transform.position = new Vector3(3.8f, 0f, 0f);
        var spawner = spawnerGo.AddComponent<PipeSpawner>();
        SetRef(spawner, "pipePrefab", pipePrefab);

        BuildUI(whiteSprite);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(GameScene, true) };
        PlayerSettings.productName = "Flappy Bird";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.runInBackground = true; // Masaüstü testinde pencere odağı kaybolunca durmasın

        AssetDatabase.SaveAssets();
        Debug.Log("[FlappySceneBuilder] Sahne kuruldu: " + GameScene);
    }

    // ---------------------------------------------------------------- Oyun objeleri

    private static void BuildBird(Sprite sprite)
    {
        var go = new GameObject("Bird");
        go.tag = "Player";
        go.transform.position = new Vector3(-1f, 0.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.2f; // Sprite (0.25) biraz küçük: daha adil çarpışma

        go.AddComponent<Bird>();
    }

    private static void BuildGround(Sprite sprite)
    {
        var go = new GameObject("Ground");
        go.transform.position = new Vector3(0f, -4.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(30f, 1f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(30f, 1f);

        go.AddComponent<GroundScroller>();
    }

    private static string BuildPipePrefab(Sprite pipeSprite)
    {
        var root = new GameObject("PipePair");
        root.AddComponent<PipePair>();

        CreatePipe(root.transform, "TopPipe", 6.5f, pipeSprite, true);
        CreatePipe(root.transform, "BottomPipe", -6.5f, pipeSprite, false);

        var zone = new GameObject("ScoreZone");
        zone.transform.SetParent(root.transform, false);
        zone.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        var trigger = zone.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.2f, 3f);
        zone.AddComponent<ScoreZone>();

        string path = PrefabsDir + "/PipePair.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return path;
    }

    private static void CreatePipe(Transform parent, string name, float y, Sprite sprite, bool flip)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, y, 0f);
        go.transform.localScale = new Vector3(1f, 10f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipY = flip;
        sr.sortingOrder = 3;

        go.AddComponent<BoxCollider2D>(); // Sprite boyutuna (1x1) otomatik oturur
    }

    // ---------------------------------------------------------------- UI

    private static void BuildUI(Sprite white)
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>(); // Proje yeni Input System kullanıyor

        TMP_Text scoreText = CreateText(canvasGo.transform, "ScoreText", "0", 180, new Vector2(0f, -250f),
            new Vector2(600f, 250f), new Vector2(0.5f, 1f));

        GameObject startPanel = CreatePanel(canvasGo.transform, "StartPanel", white, new Color(0f, 0f, 0f, 0.25f));
        CreateText(startPanel.transform, "TitleText", "FLAPPY BIRD", 130, new Vector2(0f, 400f), new Vector2(1000f, 200f));
        CreateText(startPanel.transform, "TapText", "Başlamak için dokun", 80, new Vector2(0f, -350f), new Vector2(1000f, 150f));

        GameObject overPanel = CreatePanel(canvasGo.transform, "GameOverPanel", white, new Color(0f, 0f, 0f, 0.6f));
        var title = CreateText(overPanel.transform, "TitleText", "GAME OVER", 140, new Vector2(0f, 400f), new Vector2(1000f, 200f));
        title.color = new Color32(255, 200, 60, 255);
        TMP_Text finalText = CreateText(overPanel.transform, "FinalScoreText", "Skor: 0", 90, new Vector2(0f, 150f), new Vector2(900f, 130f));
        TMP_Text bestText = CreateText(overPanel.transform, "BestScoreText", "En İyi: 0", 90, new Vector2(0f, 20f), new Vector2(900f, 130f));
        Button restart = CreateButton(overPanel.transform, "RestartButton", "Tekrar Oyna", white, new Vector2(0f, -250f));

        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        SetRef(gm, "scoreText", scoreText);
        SetRef(gm, "startPanel", startPanel);
        SetRef(gm, "gameOverPanel", overPanel);
        SetRef(gm, "finalScoreText", finalText);
        SetRef(gm, "bestScoreText", bestText);

        UnityEventTools.AddPersistentListener(restart.onClick, gm.Restart);

        overPanel.SetActive(false);
    }

    private static GameObject CreatePanel(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float size,
        Vector2 pos, Vector2 box, Vector2? anchor = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = a;
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label, Sprite sprite, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600f, 180f);

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = new Color32(240, 120, 40, 255);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var text = CreateText(go.transform, "Text", label, 80, Vector2.zero, Vector2.zero);
        var trt = (RectTransform)text.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        return btn;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------------------------------------------------------- Sprite üretimi

    private delegate Color PixelFunc(int x, int y, int size);

    private static Sprite CreateSprite(string name, int size, int ppu, PixelFunc pixel, bool tiled)
    {
        string path = $"{SpritesDir}/{name}.png";
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, pixel(x, y, size));
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        if (tiled)
        {
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // Tiled çizim için gerekli
            ti.SetTextureSettings(settings);
        }
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Color DrawBird(int x, int y, int s)
    {
        Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
        Vector2 c = new Vector2(s * 0.45f, s * 0.5f);
        float r = s * 0.38f;

        // Gaga
        if (x > s * 0.72f && x < s * 0.97f && Mathf.Abs(y - s * 0.45f) < (s * 0.97f - x) * 0.45f)
            return new Color32(245, 120, 30, 255);
        // Göz
        Vector2 eye = new Vector2(s * 0.62f, s * 0.64f);
        if (Vector2.Distance(p, eye) < s * 0.05f) return Color.black;
        if (Vector2.Distance(p, eye) < s * 0.12f) return Color.white;
        // Kanat
        if (Vector2.Distance(p, new Vector2(s * 0.3f, s * 0.42f)) < s * 0.14f)
            return new Color32(250, 240, 200, 255);

        float d = Vector2.Distance(p, c);
        if (d < r - 2f) return new Color32(250, 205, 40, 255);
        if (d < r) return new Color32(120, 70, 20, 255); // Kontur
        return Color.clear;
    }

    private static Color DrawPipe(int x, int y, int s)
    {
        if (x < 2 || x >= s - 2) return new Color32(40, 90, 20, 255);          // Kenar
        if (x < s * 0.35f) return new Color32(140, 220, 70, 255);              // Parlama
        return new Color32(100, 180, 50, 255);
    }

    private static Color DrawGround(int x, int y, int s)
    {
        if (y >= s - 2) return new Color32(60, 110, 30, 255);
        if (y >= s - 7) return ((x / 4 + y) % 2 == 0) ? new Color32(120, 200, 60, 255) : new Color32(100, 180, 50, 255);
        if (y >= s - 9) return new Color32(180, 140, 70, 255);
        return ((x + y / 2) % 16 < 2) ? new Color32(200, 170, 100, 255) : new Color32(222, 216, 149, 255);
    }
}
