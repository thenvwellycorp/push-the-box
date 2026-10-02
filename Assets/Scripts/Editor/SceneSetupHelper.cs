using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using PushTheBox.Core;
using PushTheBox.Gameplay;
using PushTheBox.Level;
using PushTheBox.Save;
using PushTheBox.Audio;
using PushTheBox.InputSystem;
using PushTheBox.UI;

namespace PushTheBox.EditorTools
{
    public static class SceneSetupHelper
    {
        [MenuItem("PushTheBox/Setup Complete Game Scene")]
        public static void SetupScene()
        {
            // Ensure assets exist first
            AssetGenerator.GenerateAll();

            // Setup Prefabs
            SetupPrefabs();

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Setup Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 1f); // Deep sleek dark blue
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(0f, 0.4f, -10f);

            // 2. Setup Core Managers
            GameObject managersRoot = new GameObject("--- MANAGERS ---");

            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(managersRoot.transform);
            gmObj.AddComponent<GameManager>();

            GameObject gsmObj = new GameObject("GameStateManager");
            gsmObj.transform.SetParent(managersRoot.transform);
            gsmObj.AddComponent<GameStateManager>();

            GameObject saveObj = new GameObject("SaveManager");
            saveObj.transform.SetParent(managersRoot.transform);
            saveObj.AddComponent<SaveManager>();

            GameObject audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(managersRoot.transform);
            audioObj.AddComponent<AudioManager>();

            GameObject gridObj = new GameObject("GridManager");
            gridObj.transform.SetParent(managersRoot.transform);
            gridObj.AddComponent<GridManager>();

            GameObject undoObj = new GameObject("UndoManager");
            undoObj.transform.SetParent(managersRoot.transform);
            undoObj.AddComponent<UndoManager>();

            GameObject deadlockObj = new GameObject("DeadlockDetector");
            deadlockObj.transform.SetParent(managersRoot.transform);
            deadlockObj.AddComponent<DeadlockDetector>();

            GameObject inputObj = new GameObject("InputController");
            inputObj.transform.SetParent(managersRoot.transform);
            inputObj.AddComponent<InputController>();

            // Setup LevelManager & LevelLoader
            GameObject levelMgrObj = new GameObject("LevelManager");
            levelMgrObj.transform.SetParent(managersRoot.transform);
            LevelManager levelMgr = levelMgrObj.AddComponent<LevelManager>();
            LevelLoader levelLoader = levelMgrObj.AddComponent<LevelLoader>();

            // Wire Prefabs into LevelLoader
            PlayerController playerPrefab = AssetDatabase.LoadAssetAtPath<PlayerController>("Assets/Prefabs/Player.prefab");
            BoxController boxPrefab = AssetDatabase.LoadAssetAtPath<BoxController>("Assets/Prefabs/Box.prefab");
            WallController wallPrefab = AssetDatabase.LoadAssetAtPath<WallController>("Assets/Prefabs/Wall.prefab");
            TargetController targetPrefab = AssetDatabase.LoadAssetAtPath<TargetController>("Assets/Prefabs/Target.prefab");
            GameObject floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Floor.prefab");

            SerializedObject soLoader = new SerializedObject(levelLoader);
            if (playerPrefab != null) soLoader.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            if (boxPrefab != null) soLoader.FindProperty("boxPrefab").objectReferenceValue = boxPrefab;
            if (wallPrefab != null) soLoader.FindProperty("wallPrefab").objectReferenceValue = wallPrefab;
            if (targetPrefab != null) soLoader.FindProperty("targetPrefab").objectReferenceValue = targetPrefab;
            if (floorPrefab != null) soLoader.FindProperty("floorTilePrefab").objectReferenceValue = floorPrefab;
            soLoader.ApplyModifiedProperties();

            // Load Level Assets into LevelManager
            List<LevelData> levelList = new List<LevelData>();
            for (int i = 1; i <= 5; i++)
            {
                LevelData ld = AssetDatabase.LoadAssetAtPath<LevelData>($"Assets/ScriptableObjects/Levels/Level_{i}.asset");
                if (ld != null)
                {
                    levelList.Add(ld);
                }
            }
            levelMgr.SetLevelsList(levelList);

            // 3. Setup UI Hierarchy
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Event System
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // UI Manager
            UIManager uiManager = canvasObj.AddComponent<UIManager>();

            // ================== A. MAIN MENU VIEW ==================
            GameObject mainMenu = CreateUIElement("MainMenu_View", canvasObj.transform);
            SetStretch(mainMenu);
            MainMenuUI menuUI = mainMenu.AddComponent<MainMenuUI>();

            // Title
            GameObject titleObj = CreateUIElement("TitleText", mainMenu.transform);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "PUSH THE BOX\n<size=44><color=#60A5FA>MINI PUZZLE</color></size>";
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 72;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.supportRichText = true;
            titleTxt.resizeTextForBestFit = true;
            titleTxt.resizeTextMinSize = 30;
            titleTxt.resizeTextMaxSize = 72;
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.08f, 0.65f);
            titleRt.anchorMax = new Vector2(0.92f, 0.90f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            // Buttons Container
            GameObject menuBtns = CreateUIElement("ButtonsContainer", mainMenu.transform);
            RectTransform menuBtnsRt = menuBtns.GetComponent<RectTransform>();
            menuBtnsRt.anchorMin = new Vector2(0.16f, 0.20f);
            menuBtnsRt.anchorMax = new Vector2(0.84f, 0.58f);
            menuBtnsRt.offsetMin = Vector2.zero;
            menuBtnsRt.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = menuBtns.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            Button playBtn = CreateStyledButton("PlayButton", menuBtns.transform, "PLAY", defaultFont, new Color(0.2f, 0.72f, 0.4f, 1f), 120f);
            Button levelsBtn = CreateStyledButton("LevelsButton", menuBtns.transform, "LEVELS", defaultFont, new Color(0.25f, 0.52f, 0.9f, 1f), 120f);
            Button soundBtn = CreateStyledButton("SoundButton", menuBtns.transform, "SOUND: ON", defaultFont, new Color(0.32f, 0.38f, 0.48f, 1f), 120f);

            // Wire menu serialized fields via SerializedObject
            SerializedObject soMenu = new SerializedObject(menuUI);
            soMenu.FindProperty("playButton").objectReferenceValue = playBtn;
            soMenu.FindProperty("levelSelectButton").objectReferenceValue = levelsBtn;
            soMenu.FindProperty("soundToggleButton").objectReferenceValue = soundBtn;
            soMenu.FindProperty("soundToggleText").objectReferenceValue = soundBtn.GetComponentInChildren<Text>();
            soMenu.ApplyModifiedProperties();

            // ================== B. LEVEL SELECT VIEW ==================
            GameObject levelSelect = CreateUIElement("LevelSelect_View", canvasObj.transform);
            SetStretch(levelSelect);
            LevelSelectUI levelSelectUI = levelSelect.AddComponent<LevelSelectUI>();

            GameObject lsTitle = CreateUIElement("Title", levelSelect.transform);
            Text lsTitleTxt = lsTitle.AddComponent<Text>();
            lsTitleTxt.text = "SELECT LEVEL";
            lsTitleTxt.font = defaultFont;
            lsTitleTxt.fontSize = 56;
            lsTitleTxt.fontStyle = FontStyle.Bold;
            lsTitleTxt.alignment = TextAnchor.MiddleCenter;
            lsTitleTxt.color = Color.white;
            RectTransform lsTitleRt = lsTitle.GetComponent<RectTransform>();
            lsTitleRt.anchorMin = new Vector2(0.1f, 0.85f);
            lsTitleRt.anchorMax = new Vector2(0.9f, 0.95f);
            lsTitleRt.offsetMin = Vector2.zero;
            lsTitleRt.offsetMax = Vector2.zero;

            GameObject gridContainer = CreateUIElement("GridContainer", levelSelect.transform);
            RectTransform gridRt = gridContainer.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.1f, 0.22f);
            gridRt.anchorMax = new Vector2(0.9f, 0.82f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;
            GridLayoutGroup glg = gridContainer.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(240, 240);
            glg.spacing = new Vector2(40, 40);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.childAlignment = TextAnchor.UpperCenter;

            Button backBtn = CreateStyledButton("BackButton", levelSelect.transform, "◀ BACK", defaultFont, new Color(0.32f, 0.38f, 0.48f, 1f), 100f);
            RectTransform backRt = backBtn.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.25f, 0.08f);
            backRt.anchorMax = new Vector2(0.75f, 0.17f);
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;

            LevelButtonUI lbPrefab = AssetDatabase.LoadAssetAtPath<LevelButtonUI>("Assets/Prefabs/LevelButton.prefab");
            SerializedObject soLs = new SerializedObject(levelSelectUI);
            soLs.FindProperty("buttonsContainer").objectReferenceValue = gridContainer.transform;
            soLs.FindProperty("backButton").objectReferenceValue = backBtn;
            if (lbPrefab != null) soLs.FindProperty("levelButtonPrefab").objectReferenceValue = lbPrefab;
            soLs.ApplyModifiedProperties();

            // ================== C. GAMEPLAY HUD VIEW ==================
            GameObject gameHUD = CreateUIElement("GameHUD_View", canvasObj.transform);
            SetStretch(gameHUD);
            GameUI gameUI = gameHUD.AddComponent<GameUI>();

            // Top HUD Bar
            GameObject topBar = CreateUIElement("TopBar", gameHUD.transform);
            RectTransform topBarRt = topBar.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0.05f, 0.88f);
            topBarRt.anchorMax = new Vector2(0.95f, 0.98f);
            topBarRt.offsetMin = Vector2.zero;
            topBarRt.offsetMax = Vector2.zero;

            GameObject lvlNameObj = CreateUIElement("LevelNameText", topBar.transform);
            Text lvlNameTxt = lvlNameObj.AddComponent<Text>();
            lvlNameTxt.text = "Level 1";
            lvlNameTxt.font = defaultFont;
            lvlNameTxt.fontSize = 44;
            lvlNameTxt.fontStyle = FontStyle.Bold;
            lvlNameTxt.color = Color.white;
            lvlNameTxt.alignment = TextAnchor.MiddleLeft;
            RectTransform lvlRt = lvlNameObj.GetComponent<RectTransform>();
            lvlRt.anchorMin = new Vector2(0f, 0.45f);
            lvlRt.anchorMax = new Vector2(0.5f, 1f);
            lvlRt.offsetMin = Vector2.zero;
            lvlRt.offsetMax = Vector2.zero;

            GameObject movesObj = CreateUIElement("MovesText", topBar.transform);
            Text movesTxt = movesObj.AddComponent<Text>();
            movesTxt.text = "Moves: 0";
            movesTxt.font = defaultFont;
            movesTxt.fontSize = 44;
            movesTxt.fontStyle = FontStyle.Bold;
            movesTxt.color = new Color(0.95f, 0.8f, 0.2f, 1f);
            movesTxt.alignment = TextAnchor.MiddleRight;
            RectTransform movesRt = movesObj.GetComponent<RectTransform>();
            movesRt.anchorMin = new Vector2(0.5f, 0.45f);
            movesRt.anchorMax = new Vector2(1f, 1f);
            movesRt.offsetMin = Vector2.zero;
            movesRt.offsetMax = Vector2.zero;

            GameObject starGoalObj = CreateUIElement("StarGoalText", topBar.transform);
            Text starGoalTxt = starGoalObj.AddComponent<Text>();
            starGoalTxt.text = "★★★ ≤ 3 Moves";
            starGoalTxt.font = defaultFont;
            starGoalTxt.fontSize = 28;
            starGoalTxt.color = new Color(0.7f, 0.75f, 0.85f, 0.9f);
            starGoalTxt.alignment = TextAnchor.MiddleLeft;
            RectTransform starGoalRt = starGoalObj.GetComponent<RectTransform>();
            starGoalRt.anchorMin = new Vector2(0f, 0f);
            starGoalRt.anchorMax = new Vector2(0.7f, 0.45f);
            starGoalRt.offsetMin = Vector2.zero;
            starGoalRt.offsetMax = Vector2.zero;

            // Deadlock Banner
            GameObject deadlockBanner = CreateUIElement("DeadlockBanner", gameHUD.transform);
            RectTransform dlRt = deadlockBanner.GetComponent<RectTransform>();
            dlRt.anchorMin = new Vector2(0.15f, 0.82f);
            dlRt.anchorMax = new Vector2(0.85f, 0.87f);
            dlRt.offsetMin = Vector2.zero;
            dlRt.offsetMax = Vector2.zero;
            Image dlBg = deadlockBanner.AddComponent<Image>();
            dlBg.color = new Color(0.85f, 0.2f, 0.2f, 0.85f);
            GameObject dlTxtObj = CreateUIElement("Text", deadlockBanner.transform);
            SetStretch(dlTxtObj);
            Text dlTxt = dlTxtObj.AddComponent<Text>();
            dlTxt.text = "⚠ Corner Deadlock! Press Undo or Restart";
            dlTxt.font = defaultFont;
            dlTxt.fontSize = 30;
            dlTxt.fontStyle = FontStyle.Bold;
            dlTxt.alignment = TextAnchor.MiddleCenter;
            dlTxt.color = Color.white;
            deadlockBanner.SetActive(false);

            // Bottom Action Controls (Undo, Restart, Menu)
            GameObject actionBtns = CreateUIElement("ActionButtons", gameHUD.transform);
            RectTransform actRt = actionBtns.GetComponent<RectTransform>();
            actRt.anchorMin = new Vector2(0.06f, 0.25f);
            actRt.anchorMax = new Vector2(0.94f, 0.33f);
            actRt.offsetMin = Vector2.zero;
            actRt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup hlg = actionBtns.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            Button undoBtn = CreateStyledButton("UndoButton", actionBtns.transform, "↶ UNDO", defaultFont, new Color(0.25f, 0.45f, 0.75f, 1f), 90f);
            Button restartBtn = CreateStyledButton("RestartButton", actionBtns.transform, "↻ RESTART", defaultFont, new Color(0.75f, 0.35f, 0.25f, 1f), 90f);
            Button menuHUDButton = CreateStyledButton("MenuButton", actionBtns.transform, "☰ MENU", defaultFont, new Color(0.35f, 0.4f, 0.5f, 1f), 90f);

            // Virtual D-pad
            GameObject dpadRoot = CreateUIElement("VirtualDPad", gameHUD.transform);
            RectTransform dpadRt = dpadRoot.GetComponent<RectTransform>();
            dpadRt.anchorMin = new Vector2(0.2f, 0.03f);
            dpadRt.anchorMax = new Vector2(0.8f, 0.24f);
            dpadRt.offsetMin = Vector2.zero;
            dpadRt.offsetMax = Vector2.zero;

            Button dpadUp = CreateDpadButton("DpadUp", dpadRoot.transform, "▲", defaultFont, new Vector2(0.35f, 0.65f), new Vector2(0.65f, 1f));
            Button dpadDown = CreateDpadButton("DpadDown", dpadRoot.transform, "▼", defaultFont, new Vector2(0.35f, 0f), new Vector2(0.65f, 0.35f));
            Button dpadLeft = CreateDpadButton("DpadLeft", dpadRoot.transform, "◀", defaultFont, new Vector2(0.02f, 0.32f), new Vector2(0.32f, 0.68f));
            Button dpadRight = CreateDpadButton("DpadRight", dpadRoot.transform, "▶", defaultFont, new Vector2(0.68f, 0.32f), new Vector2(0.98f, 0.68f));

            SerializedObject soGame = new SerializedObject(gameUI);
            soGame.FindProperty("levelTitleText").objectReferenceValue = lvlNameTxt;
            soGame.FindProperty("moveCountText").objectReferenceValue = movesTxt;
            soGame.FindProperty("targetMovesText").objectReferenceValue = starGoalTxt;
            soGame.FindProperty("undoButton").objectReferenceValue = undoBtn;
            soGame.FindProperty("restartButton").objectReferenceValue = restartBtn;
            soGame.FindProperty("menuButton").objectReferenceValue = menuHUDButton;
            soGame.FindProperty("dpadUpButton").objectReferenceValue = dpadUp;
            soGame.FindProperty("dpadDownButton").objectReferenceValue = dpadDown;
            soGame.FindProperty("dpadLeftButton").objectReferenceValue = dpadLeft;
            soGame.FindProperty("dpadRightButton").objectReferenceValue = dpadRight;
            soGame.FindProperty("deadlockBanner").objectReferenceValue = deadlockBanner;
            soGame.FindProperty("deadlockText").objectReferenceValue = dlTxt;
            soGame.ApplyModifiedProperties();

            // ================== D. LEVEL COMPLETE MODAL ==================
            GameObject completeModal = CreateUIElement("LevelComplete_Modal", canvasObj.transform);
            SetStretch(completeModal);
            Image modalDarkBg = completeModal.AddComponent<Image>();
            modalDarkBg.color = new Color(0.04f, 0.06f, 0.1f, 0.88f);
            LevelCompleteUI completeUI = completeModal.AddComponent<LevelCompleteUI>();

            // Modal Card Panel
            GameObject cardPanel = CreateUIElement("CardPanel", completeModal.transform);
            Image cardBg = cardPanel.AddComponent<Image>();
            cardBg.color = new Color(0.14f, 0.18f, 0.25f, 1f);
            RectTransform cardRt = cardPanel.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.1f, 0.30f);
            cardRt.anchorMax = new Vector2(0.9f, 0.70f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            GameObject winTitle = CreateUIElement("WinTitle", cardPanel.transform);
            Text winTitleTxt = winTitle.AddComponent<Text>();
            winTitleTxt.text = "LEVEL COMPLETE!";
            winTitleTxt.font = defaultFont;
            winTitleTxt.fontSize = 52;
            winTitleTxt.fontStyle = FontStyle.Bold;
            winTitleTxt.alignment = TextAnchor.MiddleCenter;
            winTitleTxt.color = new Color(0.35f, 0.95f, 0.45f, 1f);
            RectTransform winTitleRt = winTitle.GetComponent<RectTransform>();
            winTitleRt.anchorMin = new Vector2(0.05f, 0.80f);
            winTitleRt.anchorMax = new Vector2(0.95f, 0.95f);
            winTitleRt.offsetMin = Vector2.zero;
            winTitleRt.offsetMax = Vector2.zero;

            // Stars Row
            GameObject starsRow = CreateUIElement("StarsRow", cardPanel.transform);
            RectTransform starsRowRt = starsRow.GetComponent<RectTransform>();
            starsRowRt.anchorMin = new Vector2(0.15f, 0.55f);
            starsRowRt.anchorMax = new Vector2(0.85f, 0.75f);
            starsRowRt.offsetMin = Vector2.zero;
            starsRowRt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup starsHlg = starsRow.AddComponent<HorizontalLayoutGroup>();
            starsHlg.spacing = 30f;
            starsHlg.childAlignment = TextAnchor.MiddleCenter;
            starsHlg.childControlWidth = false;
            starsHlg.childControlHeight = false;

            Image[] starImgs = new Image[3];
            Sprite starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Star.png");
            for (int s = 0; s < 3; s++)
            {
                GameObject starObj = CreateUIElement($"Star_{s + 1}", starsRow.transform);
                RectTransform sRt = starObj.GetComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(90, 90);
                Image sImg = starObj.AddComponent<Image>();
                if (starSprite != null) sImg.sprite = starSprite;
                sImg.color = new Color(0.3f, 0.35f, 0.4f, 0.5f);
                starImgs[s] = sImg;
            }

            // Moves summary text
            GameObject winMovesObj = CreateUIElement("WinMovesText", cardPanel.transform);
            Text winMovesTxt = winMovesObj.AddComponent<Text>();
            winMovesTxt.text = "Total Moves: 0";
            winMovesTxt.font = defaultFont;
            winMovesTxt.fontSize = 38;
            winMovesTxt.alignment = TextAnchor.MiddleCenter;
            winMovesTxt.color = Color.white;
            RectTransform winMovesRt = winMovesObj.GetComponent<RectTransform>();
            winMovesRt.anchorMin = new Vector2(0.1f, 0.38f);
            winMovesRt.anchorMax = new Vector2(0.9f, 0.50f);
            winMovesRt.offsetMin = Vector2.zero;
            winMovesRt.offsetMax = Vector2.zero;

            // Card Action Buttons
            GameObject winBtns = CreateUIElement("WinButtons", cardPanel.transform);
            RectTransform winBtnsRt = winBtns.GetComponent<RectTransform>();
            winBtnsRt.anchorMin = new Vector2(0.06f, 0.08f);
            winBtnsRt.anchorMax = new Vector2(0.94f, 0.30f);
            winBtnsRt.offsetMin = Vector2.zero;
            winBtnsRt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup winHlg = winBtns.AddComponent<HorizontalLayoutGroup>();
            winHlg.spacing = 16f;
            winHlg.childControlWidth = true;
            winHlg.childControlHeight = true;
            winHlg.childForceExpandWidth = true;
            winHlg.childForceExpandHeight = true;
            winHlg.childAlignment = TextAnchor.MiddleCenter;

            Button replayBtn = CreateStyledButton("ReplayButton", winBtns.transform, "↻ Replay", defaultFont, new Color(0.35f, 0.4f, 0.5f, 1f), 85f);
            Button nextBtn = CreateStyledButton("NextButton", winBtns.transform, "Next ▶", defaultFont, new Color(0.2f, 0.72f, 0.4f, 1f), 85f);
            Button lsBtn = CreateStyledButton("LevelsButton", winBtns.transform, "Levels", defaultFont, new Color(0.25f, 0.45f, 0.75f, 1f), 85f);

            SerializedObject soWin = new SerializedObject(completeUI);
            soWin.FindProperty("modalRoot").objectReferenceValue = completeModal;
            soWin.FindProperty("dialogContent").objectReferenceValue = cardPanel.transform;
            soWin.FindProperty("titleText").objectReferenceValue = winTitleTxt;
            soWin.FindProperty("movesText").objectReferenceValue = winMovesTxt;
            soWin.FindProperty("replayButton").objectReferenceValue = replayBtn;
            soWin.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            soWin.FindProperty("levelSelectButton").objectReferenceValue = lsBtn;

            SerializedProperty starProp = soWin.FindProperty("starImages");
            starProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                starProp.GetArrayElementAtIndex(i).objectReferenceValue = starImgs[i];
            }
            soWin.ApplyModifiedProperties();
            completeModal.SetActive(false);

            // Wire UIManager
            SerializedObject soUiMgr = new SerializedObject(uiManager);
            soUiMgr.FindProperty("mainMenuView").objectReferenceValue = mainMenu;
            soUiMgr.FindProperty("levelSelectView").objectReferenceValue = levelSelect;
            soUiMgr.FindProperty("gameView").objectReferenceValue = gameHUD;
            soUiMgr.FindProperty("levelCompleteModal").objectReferenceValue = completeModal;
            soUiMgr.ApplyModifiedProperties();

            // Set Initial view visibility
            mainMenu.SetActive(true);
            levelSelect.SetActive(false);
            gameHUD.SetActive(false);

            // 4. Save Scene
            string scenePath = "Assets/Scenes/Game.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Add to Build Settings
            var buildScenes = new List<EditorBuildSettingsScene>();
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Debug.Log($"[SceneSetupHelper] Game scene successfully created and configured at '{scenePath}'!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            return go;
        }

        private static void SetStretch(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Button CreateStyledButton(string name, Transform parent, string label, Font font, Color bgColor, float prefHeight = 110f)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ButtonBg.png");
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
            }

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.15f;
            cb.pressedColor = bgColor * 0.85f;
            cb.disabledColor = new Color(0.2f, 0.22f, 0.26f, 0.5f);
            btn.colors = cb;

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 70f;
            le.preferredHeight = prefHeight;
            le.flexibleHeight = 1f;
            le.minWidth = 140f;
            le.preferredWidth = 280f;
            le.flexibleWidth = 1f;

            GameObject textObj = CreateUIElement("Label", btnObj.transform);
            SetStretch(textObj);
            Text txt = textObj.AddComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = 38;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 20;
            txt.resizeTextMaxSize = 44;

            return btn;
        }

        private static Button CreateDpadButton(string name, Transform parent, string arrow, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.22f, 0.28f, 0.38f, 0.95f);

            Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ButtonBg.png");
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
            }

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.35f, 0.45f, 0.65f, 1f);
            cb.pressedColor = new Color(0.15f, 0.18f, 0.25f, 1f);
            btn.colors = cb;

            GameObject textObj = CreateUIElement("Arrow", btnObj.transform);
            SetStretch(textObj);
            Text txt = textObj.AddComponent<Text>();
            txt.text = arrow;
            txt.font = font;
            txt.fontSize = 52;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 28;
            txt.resizeTextMaxSize = 60;

            return btn;
        }

        private static void SetupPrefabs()
        {
            string prefabsDir = "Assets/Prefabs";
            if (!System.IO.Directory.Exists(prefabsDir))
            {
                System.IO.Directory.CreateDirectory(prefabsDir);
            }

            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Player.png");
            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Box.png");
            Sprite boxOnTargetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/BoxOnTarget.png");
            Sprite targetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Target.png");
            Sprite wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Wall.png");
            Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Floor.png");

            // 1. Player Prefab
            GameObject playerGo = new GameObject("Player");
            PlayerController pc = playerGo.AddComponent<PlayerController>();
            GameObject visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(playerGo.transform, false);
            SpriteRenderer srP = visualGo.AddComponent<SpriteRenderer>();
            srP.sprite = playerSprite;
            srP.sortingOrder = 15;
            SerializedObject soP = new SerializedObject(pc);
            soP.FindProperty("spriteRenderer").objectReferenceValue = srP;
            soP.FindProperty("visualTransform").objectReferenceValue = visualGo.transform;
            soP.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(playerGo, $"{prefabsDir}/Player.prefab");
            Object.DestroyImmediate(playerGo);

            // 2. Box Prefab
            GameObject boxGo = new GameObject("Box");
            BoxController bc = boxGo.AddComponent<BoxController>();
            SpriteRenderer srB = boxGo.AddComponent<SpriteRenderer>();
            srB.sprite = boxSprite;
            srB.sortingOrder = 10;
            SerializedObject soB = new SerializedObject(bc);
            soB.FindProperty("spriteRenderer").objectReferenceValue = srB;
            soB.FindProperty("normalSprite").objectReferenceValue = boxSprite;
            soB.FindProperty("onTargetSprite").objectReferenceValue = boxOnTargetSprite;
            soB.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(boxGo, $"{prefabsDir}/Box.prefab");
            Object.DestroyImmediate(boxGo);

            // 3. Target Prefab
            GameObject targetGo = new GameObject("Target");
            TargetController tc = targetGo.AddComponent<TargetController>();
            SpriteRenderer srT = targetGo.AddComponent<SpriteRenderer>();
            srT.sprite = targetSprite;
            srT.sortingOrder = -5;
            SerializedObject soT = new SerializedObject(tc);
            soT.FindProperty("spriteRenderer").objectReferenceValue = srT;
            soT.FindProperty("pulseTransform").objectReferenceValue = targetGo.transform;
            soT.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(targetGo, $"{prefabsDir}/Target.prefab");
            Object.DestroyImmediate(targetGo);

            // 4. Wall Prefab
            GameObject wallGo = new GameObject("Wall");
            wallGo.AddComponent<WallController>();
            SpriteRenderer srW = wallGo.AddComponent<SpriteRenderer>();
            srW.sprite = wallSprite;
            srW.sortingOrder = 5;
            PrefabUtility.SaveAsPrefabAsset(wallGo, $"{prefabsDir}/Wall.prefab");
            Object.DestroyImmediate(wallGo);

            // 5. Floor Prefab
            GameObject floorGo = new GameObject("Floor");
            SpriteRenderer srF = floorGo.AddComponent<SpriteRenderer>();
            srF.sprite = floorSprite;
            srF.sortingOrder = -10;
            PrefabUtility.SaveAsPrefabAsset(floorGo, $"{prefabsDir}/Floor.prefab");
            Object.DestroyImmediate(floorGo);

            // 6. LevelButton Prefab
            GameObject btnGo = new GameObject("LevelButton", typeof(RectTransform));
            RectTransform btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(240, 240);
            Image btnImg = btnGo.AddComponent<Image>();
            Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ButtonBg.png");
            if (btnSprite != null)
            {
                btnImg.sprite = btnSprite;
                btnImg.type = Image.Type.Sliced;
            }
            btnImg.color = new Color(0.24f, 0.28f, 0.36f, 1f);

            Button btnCmp = btnGo.AddComponent<Button>();
            ColorBlock cbL = btnCmp.colors;
            cbL.highlightedColor = new Color(0.35f, 0.45f, 0.6f, 1f);
            cbL.pressedColor = new Color(0.18f, 0.22f, 0.3f, 1f);
            cbL.disabledColor = new Color(0.15f, 0.17f, 0.2f, 0.5f);
            btnCmp.colors = cbL;

            GameObject numObj = new GameObject("NumberText", typeof(RectTransform));
            numObj.transform.SetParent(btnGo.transform, false);
            RectTransform numRt = numObj.GetComponent<RectTransform>();
            numRt.anchorMin = new Vector2(0.1f, 0.35f);
            numRt.anchorMax = new Vector2(0.9f, 0.95f);
            numRt.offsetMin = Vector2.zero;
            numRt.offsetMax = Vector2.zero;
            Text numTxt = numObj.AddComponent<Text>();
            numTxt.text = "1";
            numTxt.alignment = TextAnchor.MiddleCenter;
            numTxt.color = Color.white;
            numTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (numTxt.font == null) numTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            numTxt.fontSize = 54;
            numTxt.fontStyle = FontStyle.Bold;
            numTxt.resizeTextForBestFit = true;

            GameObject starObj = new GameObject("StarsText", typeof(RectTransform));
            starObj.transform.SetParent(btnGo.transform, false);
            RectTransform starRt = starObj.GetComponent<RectTransform>();
            starRt.anchorMin = new Vector2(0.1f, 0.05f);
            starRt.anchorMax = new Vector2(0.9f, 0.40f);
            starRt.offsetMin = Vector2.zero;
            starRt.offsetMax = Vector2.zero;
            Text starTxt = starObj.AddComponent<Text>();
            starTxt.text = "★★★";
            starTxt.alignment = TextAnchor.MiddleCenter;
            starTxt.color = new Color(1f, 0.85f, 0.2f, 1f);
            starTxt.font = numTxt.font;
            starTxt.fontSize = 28;
            starTxt.resizeTextForBestFit = true;

            GameObject lockObj = new GameObject("LockOverlay", typeof(RectTransform));
            lockObj.transform.SetParent(btnGo.transform, false);
            SetStretch(lockObj);
            Image lockImg = lockObj.AddComponent<Image>();
            lockImg.color = new Color(0.08f, 0.1f, 0.14f, 0.85f);
            GameObject lockTextObj = new GameObject("LockIcon", typeof(RectTransform));
            lockTextObj.transform.SetParent(lockObj.transform, false);
            SetStretch(lockTextObj);
            Text lockTxt = lockTextObj.AddComponent<Text>();
            lockTxt.text = "🔒";
            lockTxt.alignment = TextAnchor.MiddleCenter;
            lockTxt.font = numTxt.font;
            lockTxt.fontSize = 44;
            lockObj.SetActive(false);

            LevelButtonUI lbUI = btnGo.AddComponent<LevelButtonUI>();
            SerializedObject soBtn = new SerializedObject(lbUI);
            soBtn.FindProperty("button").objectReferenceValue = btnCmp;
            soBtn.FindProperty("levelNumberText").objectReferenceValue = numTxt;
            soBtn.FindProperty("starsText").objectReferenceValue = starTxt;
            soBtn.FindProperty("lockOverlay").objectReferenceValue = lockObj;
            soBtn.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(btnGo, $"{prefabsDir}/LevelButton.prefab");
            Object.DestroyImmediate(btnGo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
