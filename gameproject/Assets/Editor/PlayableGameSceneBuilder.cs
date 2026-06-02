using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PlayableGameSceneBuilder
{
    private const string SourceScenePath = "Assets/Scenes/GameScene.unity";
    private const string PlayableScenePath = "Assets/Scenes/PlayableGameScene.unity";
    private const string PlayableStagePath = "Assets/StageData/PlayableStage_01.asset";
    private const string StonePrefabPath = "Assets/Prefabs/Stone.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemy_001.prefab";

    [MenuItem("Tools/GameProject/Build Playable Game Scene")]
    public static void Build()
    {
        EnsureCopiedScene();
        Scene scene = EditorSceneManager.OpenScene(PlayableScenePath, OpenSceneMode.Single);

        GameObject stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StonePrefabPath);
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        StageData stageData = EnsurePlayableStageData(enemyPrefab);

        Camera camera = EnsureCamera();
        EnsureEventSystem();
        Canvas canvas = EnsureCanvas();
        GameObject managerObject = EnsureObject("Manager");
        Transform spawnRoot = EnsureObject("SpawnRoot").transform;
        Transform stoneRoot = EnsureChild(spawnRoot, "Stones");
        Transform enemyRoot = EnsureChild(spawnRoot, "Enemies");
        Transform wallRoot = EnsureChild(spawnRoot, "Walls");

        CreateBoardPreview();
        ConfigureSlingshotControllers();

        DestroyIfExists("StageNameText");
        DestroyIfExists("TargetText");
        DestroyIfExists("StartPanel");
        DestroyIfExists("ClearPanel");
        DestroyIfExists("FailPanel");

        Text stageNameText = CreateText(canvas.transform, "StageNameText", "Playable Stage 1", 30, new Vector2(28f, -22f), new Vector2(360f, 44f), new Vector2(0f, 1f), TextAnchor.MiddleLeft);
        Text targetText = CreateText(canvas.transform, "TargetText", "목표: 3돌 만들기", 24, new Vector2(28f, -66f), new Vector2(360f, 40f), new Vector2(0f, 1f), TextAnchor.MiddleLeft);
        GameObject startPanel = CreateStartPanel(canvas.transform);
        GameObject clearPanel = CreateResultPanel(canvas.transform, "ClearPanel", "CLEAR");
        GameObject failPanel = CreateResultPanel(canvas.transform, "FailPanel", "FAIL");

        GameManager gameManager = EnsureComponent<GameManager>(managerObject);
        StageManager stageManager = EnsureComponent<StageManager>(managerObject);
        MergeManager mergeManager = EnsureComponent<MergeManager>(managerObject);
        TurnManager turnManager = EnsureComponent<TurnManager>(managerObject);

        SerializedObject gameSerialized = new SerializedObject(gameManager);
        gameSerialized.FindProperty("startOnAwake").boolValue = false;
        gameSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject stageSerialized = new SerializedObject(stageManager);
        stageSerialized.FindProperty("currentStageData").objectReferenceValue = stageData;
        stageSerialized.FindProperty("stonePrefab").objectReferenceValue = stonePrefab;
        stageSerialized.FindProperty("stoneParent").objectReferenceValue = stoneRoot;
        stageSerialized.FindProperty("enemyParent").objectReferenceValue = enemyRoot;
        stageSerialized.FindProperty("wallParent").objectReferenceValue = wallRoot;
        stageSerialized.FindProperty("targetText").objectReferenceValue = targetText;
        stageSerialized.FindProperty("stageNameText").objectReferenceValue = stageNameText;
        stageSerialized.FindProperty("clearPanel").objectReferenceValue = clearPanel;
        stageSerialized.FindProperty("failPanel").objectReferenceValue = failPanel;
        stageSerialized.FindProperty("stoneLinearDamping").floatValue = 1.25f;
        stageSerialized.FindProperty("stoneAngularDamping").floatValue = 1.0f;
        stageSerialized.FindProperty("enemyLinearDamping").floatValue = 1.6f;
        stageSerialized.FindProperty("enemyAngularDamping").floatValue = 1.2f;
        stageSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject mergeSerialized = new SerializedObject(mergeManager);
        mergeSerialized.FindProperty("stonePrefab").objectReferenceValue = stonePrefab;
        mergeSerialized.FindProperty("mergedStoneDefaultHp").intValue = 50;
        mergeSerialized.ApplyModifiedPropertiesWithoutUndo();

        Button startButton = startPanel.GetComponentInChildren<Button>();
        if (startButton != null)
        {
            StartGameButton startGameButton = EnsureComponent<StartGameButton>(startButton.gameObject);
            SerializedObject startButtonSerialized = new SerializedObject(startGameButton);
            startButtonSerialized.FindProperty("startPanel").objectReferenceValue = startPanel;
            startButtonSerialized.ApplyModifiedPropertiesWithoutUndo();

            startButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(startButton.onClick, startGameButton.StartGame);
            EditorUtility.SetDirty(startButton);
        }

        Selection.activeObject = managerObject;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureSlingshotControllers()
    {
        PlayerSlingshotController[] controllers = Object.FindObjectsByType<PlayerSlingshotController>(FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            SerializedObject serialized = new SerializedObject(controllers[i]);
            serialized.FindProperty("powerMultiplier").floatValue = 7.0f;
            serialized.FindProperty("maxDragDistance").floatValue = 2.6f;
            serialized.FindProperty("maxMoveTime").floatValue = 2.2f;
            serialized.FindProperty("stopSmoothPower").floatValue = 4.6f;
            serialized.FindProperty("stopVelocityThreshold").floatValue = 0.08f;
            serialized.FindProperty("arrowLengthMultiplier").floatValue = 3.2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Rigidbody2D body = controllers[i].GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.gravityScale = 0f;
                body.linearDamping = 1.25f;
                body.angularDamping = 1.0f;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
    }

    private static void EnsureCopiedScene()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(PlayableScenePath))
        {
            AssetDatabase.CopyAsset(SourceScenePath, PlayableScenePath);
            AssetDatabase.ImportAsset(PlayableScenePath);
        }
    }

    private static void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }

    private static StageData EnsurePlayableStageData(GameObject enemyPrefab)
    {
        StageData data = AssetDatabase.LoadAssetAtPath<StageData>(PlayableStagePath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<StageData>();
            AssetDatabase.CreateAsset(data, PlayableStagePath);
        }

        data.stageName = "Playable Stage 1";
        data.targetStoneLevel = 3;

        data.initialStones.Clear();
        data.initialStones.Add(new StageStoneData { level = 1, hp = 50, position = new Vector2(-4.2f, -1.2f) });
        data.initialStones.Add(new StageStoneData { level = 1, hp = 50, position = new Vector2(-1.7f, 1.7f) });
        data.initialStones.Add(new StageStoneData { level = 1, hp = 50, position = new Vector2(1.3f, 1.15f) });
        data.initialStones.Add(new StageStoneData { level = 1, hp = 50, position = new Vector2(3.8f, -1.3f) });

        data.initialEnemies.Clear();
        data.initialEnemies.Add(new StageEnemyData { enemyPrefab = enemyPrefab, hp = 3, position = new Vector2(0.1f, 0f) });

        data.stageWalls.Clear();
        data.stageWalls.Add(new StageWallData { position = new Vector2(0f, 4.45f), size = new Vector2(13.8f, 0.28f) });
        data.stageWalls.Add(new StageWallData { position = new Vector2(0f, -4.45f), size = new Vector2(13.8f, 0.28f) });
        data.stageWalls.Add(new StageWallData { position = new Vector2(-6.85f, 0f), size = new Vector2(0.28f, 8.9f) });
        data.stageWalls.Add(new StageWallData { position = new Vector2(6.85f, 0f), size = new Vector2(0.28f, 8.9f) });

        EditorUtility.SetDirty(data);
        return data;
    }

    private static Camera EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.88f, 0.93f, 0.86f);
        return camera;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        StandaloneInputModule oldInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldInputModule != null)
        {
            Object.DestroyImmediate(oldInputModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static Canvas EnsureCanvas()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = new GameObject("Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateBoardPreview()
    {
        GameObject oldBoard = GameObject.Find("PlayableBoardPreview");
        if (oldBoard != null)
        {
            Object.DestroyImmediate(oldBoard);
        }

        GameObject board = new GameObject("PlayableBoardPreview");
        CreateVisualBox(board.transform, "BoardSurface", Vector2.zero, new Vector2(13.8f, 8.9f), new Color(0.74f, 0.86f, 0.72f), -10);
    }

    private static void CreateVisualBox(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject box = new GameObject(name);
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private static GameObject CreateStartPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "StartPanel", new Color(0f, 0f, 0f, 0.55f));
        CreateText(panel.transform, "Title", "STONE MERGE", 54, new Vector2(0f, 80f), new Vector2(600f, 80f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
        Button button = CreateButton(panel.transform, "StartButton", "START", new Vector2(0f, -40f), new Vector2(220f, 70f));
        button.gameObject.SetActive(true);
        return panel;
    }

    private static GameObject CreateResultPanel(Transform parent, string name, string message)
    {
        GameObject panel = CreatePanel(parent, name, new Color(0f, 0f, 0f, 0.62f));
        CreateText(panel.transform, name + "Text", message, 58, Vector2.zero, new Vector2(520f, 100f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.94f, 0.87f, 0.54f);
        Button button = buttonObject.AddComponent<Button>();
        CreateText(buttonObject.transform, "Label", label, 34, Vector2.zero, size, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
        return button;
    }

    private static Text CreateText(Transform parent, string name, string value, int size, Vector2 position, Vector2 dimensions, Vector2 anchor, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static GameObject EnsureObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj != null ? obj : new GameObject(name);
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component != null ? component : obj.AddComponent<T>();
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
    }
}
