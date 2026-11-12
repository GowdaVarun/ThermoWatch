using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DataCenterGuard.EditorTools
{
    /// <summary>
    /// Builds the DataCenterGuard base scene: a clean data hall with server
    /// rack rows, CRAC cooling stations, power infrastructure (UPS, battery,
    /// PDU, switchgear), raised floor, cable trays, fire suppression and
    /// pipework. Uses primitives only - no imported models, no thermal visuals.
    ///
    /// Menu: DataCenterGuard -> Rebuild Data Center Scene
    /// </summary>
    public static class DataCenterSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/DataCenterGuard.unity";
        private const string MaterialFolder = "Assets/Materials";

        // -------------------------------------------------------------
        // Layout constants
        // -------------------------------------------------------------
        private const float TileSize = 0.6f;              // 600 mm raised floor tiles
        private const int RacksPerRow = 8;
        private const float RackSpacing = 1.0f;           // 0.8 m rack + 0.2 m gap
        private static readonly float[] RowZ = { -5.4f, -3.0f, -0.6f, 1.8f };
        private static readonly bool[] RowFacesPlusZ = { true, false, true, false };
        private static readonly string[] RowNames = { "A", "B", "C", "D" };
        private static readonly float[] ColdAisleZ = { -4.2f, 0.6f };

        private static readonly Color TextColor = new Color(0.18f, 0.20f, 0.24f);

        [MenuItem("DataCenterGuard/Rebuild Data Center Scene")]
        public static void BuildSceneMenu()
        {
            BuildScene();
        }

        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play Mode, then run DataCenterGuard -> Rebuild Data Center Scene again.");
                return;
            }

            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---------------------------------------------------------
            // Materials (white / gray palette, blue + red accents)
            // ---------------------------------------------------------
            Material floorMaterial = MakeMaterial("FloorTile", new Color(0.80f, 0.81f, 0.83f), 0.05f, 0.30f);
            Material gridMaterial = MakeMaterial("FloorGrid", new Color(0.70f, 0.71f, 0.74f), 0.05f, 0.35f);
            Material perforatedMaterial = MakeMaterial("PerforatedTile", new Color(0.66f, 0.68f, 0.71f), 0.05f, 0.35f);
            Material perforatedBarMaterial = MakeMaterial("PerforatedBar", new Color(0.44f, 0.46f, 0.50f), 0.05f, 0.35f);
            Material curbMaterial = MakeMaterial("SlabCurb", new Color(0.88f, 0.89f, 0.90f), 0.02f, 0.30f);
            Material wallMaterial = MakeMaterial("Wall", new Color(0.87f, 0.88f, 0.91f), 0.0f, 0.25f);

            Material rackBodyMaterial = MakeMaterial("RackBody", new Color(0.60f, 0.61f, 0.63f), 0.35f, 0.50f);
            Material rackPanelMaterial = MakeMaterial("RackPanel", new Color(0.46f, 0.47f, 0.49f), 0.30f, 0.45f);
            Material rackSlatMaterial = MakeMaterial("RackSlat", new Color(0.30f, 0.31f, 0.33f), 0.35f, 0.45f);
            Material rackTopMaterial = MakeMaterial("RackTop", new Color(0.55f, 0.56f, 0.58f), 0.35f, 0.50f);

            Material cracBodyMaterial = MakeMaterial("CracBody", new Color(0.13f, 0.14f, 0.15f), 0.40f, 0.40f);
            Material cracTopMaterial = MakeMaterial("CracTop", new Color(0.18f, 0.19f, 0.21f), 0.40f, 0.40f);
            Material cracBlueMaterial = MakeMaterial("CracBlue", new Color(0.25f, 0.47f, 0.86f), 0.20f, 0.55f);
            Material cracGridMaterial = MakeMaterial("CracGrid", new Color(0.09f, 0.10f, 0.12f), 0.30f, 0.40f);

            Material upsBodyMaterial = MakeMaterial("UpsBody", new Color(0.085f, 0.085f, 0.095f), 0.35f, 0.40f);
            Material upsPanelMaterial = MakeMaterial("UpsPanel", new Color(0.14f, 0.14f, 0.155f), 0.30f, 0.40f);
            Material indicatorMaterial = MakeMaterial("UpsIndicator", new Color(0.75f, 0.76f, 0.78f), 0.10f, 0.40f);

            Material pduBodyMaterial = MakeMaterial("PduBody", new Color(0.55f, 0.56f, 0.58f), 0.30f, 0.45f);
            Material pduStripeMaterial = MakeMaterial("PduStripe", new Color(0.30f, 0.50f, 0.90f), 0.20f, 0.50f);

            Material bluePipeMaterial = MakeMaterial("BluePipe", new Color(0.28f, 0.50f, 0.90f), 0.30f, 0.55f);
            Material redPipeMaterial = MakeMaterial("RedPipe", new Color(0.82f, 0.28f, 0.24f), 0.30f, 0.55f);
            Material tankMaterial = MakeMaterial("FireTank", new Color(0.78f, 0.26f, 0.22f), 0.25f, 0.45f);
            Material railingMaterial = MakeMaterial("Railing", new Color(0.88f, 0.89f, 0.91f), 0.30f, 0.45f);
            Material trayMaterial = MakeMaterial("CableTray", new Color(0.72f, 0.73f, 0.75f), 0.40f, 0.40f);
            Material lightStripMaterial = MakeMaterial("LightStrip", new Color(1f, 1f, 1f), 0f, 0.30f,
                new Color(1f, 1f, 1f), 0.55f);

            // ---------------------------------------------------------
            // Lighting - bright, neutral, white studio look
            // ---------------------------------------------------------
            GameObject lights = new GameObject("Lighting");

            GameObject keyLight = new GameObject("Key Light");
            keyLight.transform.SetParent(lights.transform, false);
            keyLight.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light key = keyLight.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.99f, 0.97f);
            key.intensity = 1.15f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.35f;

            GameObject fillLight = new GameObject("Fill Light");
            fillLight.transform.SetParent(lights.transform, false);
            fillLight.transform.rotation = Quaternion.Euler(28f, 152f, 0f);
            Light fill = fillLight.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.95f, 0.97f, 1f);
            fill.intensity = 0.45f;
            fill.shadows = LightShadows.None;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.80f, 0.81f, 0.84f);
            RenderSettings.fog = false;

            // ---------------------------------------------------------
            // Data hall: raised floor slab, curb and tile grid
            // ---------------------------------------------------------
            GameObject hall = new GameObject("Data Hall");

            CreateBox(hall.transform, "Floor Slab", new Vector3(0f, -0.125f, -1.8f),
                new Vector3(13.4f, 0.25f, 12.4f), floorMaterial);

            // White curb around the slab edge.
            CreateBox(hall.transform, "Curb Front", new Vector3(0f, 0.03f, -7.94f),
                new Vector3(13.4f, 0.10f, 0.14f), curbMaterial);
            CreateBox(hall.transform, "Curb Back", new Vector3(0f, 0.03f, 4.34f),
                new Vector3(13.4f, 0.10f, 0.14f), curbMaterial);
            CreateBox(hall.transform, "Curb Left", new Vector3(-6.63f, 0.03f, -1.8f),
                new Vector3(0.14f, 0.10f, 12.4f), curbMaterial);
            CreateBox(hall.transform, "Curb Right", new Vector3(6.63f, 0.03f, -1.8f),
                new Vector3(0.14f, 0.10f, 12.4f), curbMaterial);

            // Raised floor tile grid (600 mm).
            for (float x = -6.0f; x <= 6.01f; x += TileSize)
            {
                CreateBox(hall.transform, "Grid X", new Vector3(x, 0.006f, -1.8f),
                    new Vector3(0.018f, 0.008f, 12.0f), gridMaterial);
            }
            for (float z = -7.8f; z <= 4.21f; z += TileSize)
            {
                CreateBox(hall.transform, "Grid Z", new Vector3(0f, 0.006f, z),
                    new Vector3(12.0f, 0.008f, 0.018f), gridMaterial);
            }

            // Perforated supply tiles in the cold aisles.
            foreach (float aisleZ in ColdAisleZ)
            {
                for (int i = 0; i < 7; i++)
                {
                    float x = -3.6f + i * 1.2f;
                    CreateBox(hall.transform, "Perforated Tile", new Vector3(x, 0.014f, aisleZ),
                        new Vector3(0.56f, 0.012f, 0.56f), perforatedMaterial);
                    for (int b = -1; b <= 1; b++)
                    {
                        CreateBox(hall.transform, "Perf Bar", new Vector3(x, 0.021f, aisleZ + b * 0.17f),
                            new Vector3(0.50f, 0.010f, 0.030f), perforatedBarMaterial);
                        CreateBox(hall.transform, "Perf Bar", new Vector3(x + b * 0.17f, 0.021f, aisleZ),
                            new Vector3(0.030f, 0.010f, 0.50f), perforatedBarMaterial);
                    }
                }
            }

            // Perimeter walls (front stays open toward the camera).
            CreateBox(hall.transform, "Back Wall", new Vector3(0f, 1.6f, 4.4f),
                new Vector3(13.4f, 3.2f, 0.12f), wallMaterial);
            CreateBox(hall.transform, "Left Wall", new Vector3(-6.7f, 1.6f, -1.8f),
                new Vector3(0.12f, 3.2f, 12.4f), wallMaterial);

            CreateLabel(hall.transform, "DATA HALL", new Vector3(0f, 3.0f, 4.26f), TextColor, 0.075f, 0f);

            // ---------------------------------------------------------
            // Server rack rows (4 rows x 8 racks)
            // ---------------------------------------------------------
            GameObject racksRoot = new GameObject("Server Racks");
            for (int r = 0; r < 4; r++)
            {
                float yaw = RowFacesPlusZ[r] ? 0f : 180f;
                for (int k = 0; k < RacksPerRow; k++)
                {
                    float x = (k - (RacksPerRow - 1) * 0.5f) * RackSpacing;
                    CreateRack(racksRoot.transform, new Vector3(x, 0f, RowZ[r]), yaw,
                        rackBodyMaterial, rackPanelMaterial, rackSlatMaterial, rackTopMaterial);
                }

                // Row marker at the left end of each row.
                CreateLabel(hall.transform, "ROW " + RowNames[r], new Vector3(-4.55f, 2.35f, RowZ[r]),
                    TextColor, 0.045f, 0f);
            }

            // Cold aisle markers.
            CreateLabel(hall.transform, "COLD AISLE 1", new Vector3(0f, 2.7f, ColdAisleZ[0]), TextColor, 0.045f, 0f);
            CreateLabel(hall.transform, "COLD AISLE 2", new Vector3(0f, 2.7f, ColdAisleZ[1]), TextColor, 0.045f, 0f);

            // ---------------------------------------------------------
            // Row-end power and network cabinets
            // ---------------------------------------------------------
            GameObject rowEndRoot = new GameObject("Row End Cabinets");
            CreatePdu(rowEndRoot.transform, "PDU-1", new Vector3(4.5f, 0f, RowZ[0]), 90f,
                pduBodyMaterial, rackPanelMaterial, pduStripeMaterial);
            CreatePdu(rowEndRoot.transform, "PDU-2", new Vector3(4.5f, 0f, RowZ[3]), 90f,
                pduBodyMaterial, rackPanelMaterial, pduStripeMaterial);
            CreateNetworkCabinet(rowEndRoot.transform, "NET-1", new Vector3(-4.5f, 0f, RowZ[0]), -90f,
                pduBodyMaterial, rackPanelMaterial);
            CreateNetworkCabinet(rowEndRoot.transform, "NET-2", new Vector3(-4.5f, 0f, RowZ[3]), -90f,
                pduBodyMaterial, rackPanelMaterial);

            // ---------------------------------------------------------
            // CRAC cooling stations
            // ---------------------------------------------------------
            GameObject coolingRoot = new GameObject("Cooling Stations");
            CreateCrac(coolingRoot.transform, "CRAC-1", new Vector3(-5.8f, 0f, RowZ[0]), 90f,
                cracBodyMaterial, cracTopMaterial, cracBlueMaterial, cracGridMaterial);
            CreateCrac(coolingRoot.transform, "CRAC-2", new Vector3(-5.8f, 0f, ColdAisleZ[1]), 90f,
                cracBodyMaterial, cracTopMaterial, cracBlueMaterial, cracGridMaterial);
            CreateCrac(coolingRoot.transform, "CRAC-3", new Vector3(5.8f, 0f, ColdAisleZ[1]), -90f,
                cracBodyMaterial, cracTopMaterial, cracBlueMaterial, cracGridMaterial);
            CreateCrac(coolingRoot.transform, "CRAC-4", new Vector3(0f, 0f, 3.3f), 180f,
                cracBodyMaterial, cracTopMaterial, cracBlueMaterial, cracGridMaterial);

            // ---------------------------------------------------------
            // Cable trays, drops and light strips
            // ---------------------------------------------------------
            GameObject infrastructure = new GameObject("Infrastructure");

            // Trays above the two cold aisles.
            CreateBox(infrastructure.transform, "Tray Aisle 1", new Vector3(0f, 3.3f, ColdAisleZ[0]),
                new Vector3(0.55f, 0.08f, 9.0f), trayMaterial);
            CreateBox(infrastructure.transform, "Tray Aisle 2", new Vector3(0f, 3.3f, ColdAisleZ[1]),
                new Vector3(0.55f, 0.08f, 9.0f), trayMaterial);

            // Main feeder tray from the power room along the right edge.
            CreateBox(infrastructure.transform, "Main Tray", new Vector3(6.2f, 3.3f, -1.0f),
                new Vector3(0.55f, 0.08f, 8.8f), trayMaterial);
            CreateBox(infrastructure.transform, "Branch Tray 1", new Vector3(5.35f, 3.3f, RowZ[0]),
                new Vector3(1.7f, 0.08f, 0.55f), trayMaterial);
            CreateBox(infrastructure.transform, "Branch Tray 2", new Vector3(5.35f, 3.3f, RowZ[3]),
                new Vector3(1.7f, 0.08f, 0.55f), trayMaterial);
            CreateBox(infrastructure.transform, "PDU Drop 1", new Vector3(4.5f, 2.63f, RowZ[0]),
                new Vector3(0.06f, 1.26f, 0.06f), trayMaterial);
            CreateBox(infrastructure.transform, "PDU Drop 2", new Vector3(4.5f, 2.63f, RowZ[3]),
                new Vector3(0.06f, 1.26f, 0.06f), trayMaterial);

            // Overhead light strips above each aisle.
            float[] stripZ = { -4.2f, -1.8f, 0.6f, 3.0f };
            for (int i = 0; i < stripZ.Length; i++)
            {
                CreateBox(infrastructure.transform, "Light Strip " + i, new Vector3(0f, 4.25f, stripZ[i]),
                    new Vector3(0.22f, 0.05f, 9.0f), lightStripMaterial);
            }
            CreateBox(infrastructure.transform, "Light Strip Power", new Vector3(9.45f, 4.0f, -2.6f),
                new Vector3(0.22f, 0.05f, 5.0f), lightStripMaterial);

            // ---------------------------------------------------------
            // Catwalk railing and process pipework (blue / red)
            // ---------------------------------------------------------
            GameObject services = new GameObject("Services");

            // Front edge railing.
            for (int i = 0; i < 8; i++)
            {
                CreateBox(services.transform, "Railing Post", new Vector3(-6.4f + i * 1.84f, 0.475f, -7.90f),
                    new Vector3(0.05f, 0.95f, 0.05f), railingMaterial);
            }
            CreateBox(services.transform, "Rail Top", new Vector3(0f, 0.95f, -7.90f),
                new Vector3(13.0f, 0.04f, 0.04f), railingMaterial);
            CreateBox(services.transform, "Rail Mid", new Vector3(0f, 0.55f, -7.90f),
                new Vector3(13.0f, 0.04f, 0.04f), railingMaterial);

            // Left edge railing.
            for (int i = 0; i < 7; i++)
            {
                CreateBox(services.transform, "Railing Post", new Vector3(-6.60f, 0.475f, -7.9f + i * 2.033f),
                    new Vector3(0.05f, 0.95f, 0.05f), railingMaterial);
            }
            CreateBox(services.transform, "Rail Top", new Vector3(-6.60f, 0.95f, -1.8f),
                new Vector3(0.04f, 0.04f, 12.2f), railingMaterial);
            CreateBox(services.transform, "Rail Mid", new Vector3(-6.60f, 0.55f, -1.8f),
                new Vector3(0.04f, 0.04f, 12.2f), railingMaterial);

            // Chilled water (blue) and fire suppression (red) pipework.
            CreateCylinder(services.transform, "Pipe Blue Front", new Vector3(0f, 1.05f, -7.82f),
                0.09f, 13.0f, bluePipeMaterial, "X");
            CreateCylinder(services.transform, "Pipe Red Front", new Vector3(0f, 0.72f, -7.72f),
                0.09f, 13.0f, redPipeMaterial, "X");
            CreateCylinder(services.transform, "Pipe Blue Right", new Vector3(6.55f, 1.05f, -1.8f),
                0.09f, 12.2f, bluePipeMaterial, "Z");
            CreateCylinder(services.transform, "Pipe Red Right", new Vector3(6.45f, 0.72f, -1.8f),
                0.09f, 12.2f, redPipeMaterial, "Z");

            // ---------------------------------------------------------
            // Power room (UPS, battery, switchgear, fire suppression)
            // ---------------------------------------------------------
            GameObject powerRoom = new GameObject("Power Room");

            CreateBox(powerRoom.transform, "Floor Slab", new Vector3(9.45f, -0.125f, -2.6f),
                new Vector3(5.5f, 0.25f, 6.0f), floorMaterial);
            CreateBox(powerRoom.transform, "Back Wall", new Vector3(9.45f, 1.6f, 0.4f),
                new Vector3(5.5f, 3.2f, 0.12f), wallMaterial);
            CreateBox(powerRoom.transform, "Right Wall", new Vector3(12.2f, 1.6f, -2.6f),
                new Vector3(0.12f, 3.2f, 6.0f), wallMaterial);

            for (float x = 7.0f; x <= 12.01f; x += TileSize)
            {
                CreateBox(powerRoom.transform, "Grid X", new Vector3(x, 0.006f, -2.6f),
                    new Vector3(0.018f, 0.008f, 5.7f), gridMaterial);
            }
            for (float z = -5.4f; z <= 0.21f; z += TileSize)
            {
                CreateBox(powerRoom.transform, "Grid Z", new Vector3(9.45f, 0.006f, z),
                    new Vector3(5.2f, 0.008f, 0.018f), gridMaterial);
            }

            CreateUps(powerRoom.transform, "UPS-A", new Vector3(8.5f, 0f, -1.2f),
                upsBodyMaterial, upsPanelMaterial, indicatorMaterial);
            CreateUps(powerRoom.transform, "UPS-B", new Vector3(9.7f, 0f, -1.2f),
                upsBodyMaterial, upsPanelMaterial, indicatorMaterial);
            CreateBatteryCabinet(powerRoom.transform, "BATT-A", new Vector3(10.9f, 0f, -1.2f),
                upsBodyMaterial, upsPanelMaterial);

            // Wall-mounted low voltage switchgear.
            for (int i = 0; i < 3; i++)
            {
                CreateBox(powerRoom.transform, "LV Panel " + i, new Vector3(7.4f + i * 0.8f, 1.5f, 0.24f),
                    new Vector3(0.5f, 1.1f, 0.18f), upsPanelMaterial);
            }
            CreateLabel(powerRoom.transform, "LV SWITCHGEAR", new Vector3(8.2f, 2.45f, 0.2f), TextColor, 0.05f, 0f);

            // FM-200 fire suppression cylinders.
            CreateCylinder(powerRoom.transform, "FM-200 A", new Vector3(11.7f, 0.8f, -4.9f),
                0.5f, 1.6f, tankMaterial, "Y");
            CreateCylinder(powerRoom.transform, "FM-200 B", new Vector3(11.7f, 0.8f, -4.1f),
                0.5f, 1.6f, tankMaterial, "Y");
            CreateLabel(powerRoom.transform, "FM-200", new Vector3(11.7f, 2.0f, -4.5f), TextColor, 0.05f, 0f);

            CreateLabel(powerRoom.transform, "POWER ROOM", new Vector3(9.45f, 3.0f, 0.26f), TextColor, 0.06f, 0f);

            // ---------------------------------------------------------
            // Camera
            // ---------------------------------------------------------
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.95f, 0.95f, 0.96f);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.allowHDR = true;
            camera.allowMSAA = true;

            cameraObject.AddComponent<AudioListener>();

            CameraOrbit orbit = cameraObject.AddComponent<CameraOrbit>();
            orbit.target = new Vector3(2.0f, 1.0f, -2.0f);
            orbit.distance = 24f;
            orbit.yaw = 32f;
            orbit.pitch = 27f;
            orbit.minDistance = 8f;
            orbit.maxDistance = 45f;
            orbit.ApplyNow();

            // ---------------------------------------------------------
            // Label toggle UI (button at the top of the screen)
            // ---------------------------------------------------------
            GameObject labelUi = new GameObject("Label UI");
            labelUi.AddComponent<LabelToggleUI>();

            // Face every billboard label toward the camera for the saved view.
            Billboard[] labels = Object.FindObjectsByType<Billboard>();
            for (int i = 0; i < labels.Length; i++)
                labels[i].FaceCameraNow();

            PlayerSettings.productName = "DataCenterGuard 3D";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("DataCenterGuard base scene rebuilt at " + ScenePath
                + " (32 racks, 4 CRACs, UPS + battery + PDU + switchgear, raised floor, trays, pipework).");
        }

        // -------------------------------------------------------------
        // Equipment builders
        // -------------------------------------------------------------

        private static void CreateRack(
            Transform parent,
            Vector3 position,
            float yaw,
            Material bodyMaterial,
            Material panelMaterial,
            Material slatMaterial,
            Material topMaterial)
        {
            GameObject root = new GameObject("Rack");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(root.transform, "Body", new Vector3(0f, 1.0f, 0f),
                new Vector3(0.8f, 2.0f, 1.2f), bodyMaterial);
            CreateBox(root.transform, "Top", new Vector3(0f, 2.01f, 0f),
                new Vector3(0.82f, 0.03f, 1.22f), topMaterial);

            CreateBox(root.transform, "Front Panel", new Vector3(0f, 1.0f, 0.612f),
                new Vector3(0.68f, 1.84f, 0.03f), panelMaterial);
            for (int i = 0; i < 4; i++)
            {
                CreateBox(root.transform, "Slat", new Vector3(0f, 0.45f + i * 0.4f, 0.632f),
                    new Vector3(0.60f, 0.09f, 0.015f), slatMaterial);
            }

            CreateBox(root.transform, "Rear Panel", new Vector3(0f, 1.0f, -0.612f),
                new Vector3(0.68f, 1.84f, 0.03f), panelMaterial);
            for (int i = 0; i < 2; i++)
            {
                CreateBox(root.transform, "Rear Slat", new Vector3(0f, 0.7f + i * 0.7f, -0.632f),
                    new Vector3(0.60f, 0.09f, 0.015f), slatMaterial);
            }
        }

        private static void CreateCrac(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            Material bodyMaterial,
            Material topMaterial,
            Material blueMaterial,
            Material gridMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(root.transform, "Body", new Vector3(0f, 1.1f, 0f),
                new Vector3(1.6f, 2.2f, 1.6f), bodyMaterial);
            CreateBox(root.transform, "Top Plate", new Vector3(0f, 2.22f, 0f),
                new Vector3(1.55f, 0.05f, 1.55f), topMaterial);

            // Blue lattice discharge top, like the reference units.
            CreateBox(root.transform, "Lattice Plate", new Vector3(0f, 2.26f, 0f),
                new Vector3(1.3f, 0.02f, 1.3f), blueMaterial);
            float[] offsets = { -0.50f, -0.17f, 0.17f, 0.50f };
            for (int i = 0; i < offsets.Length; i++)
            {
                CreateBox(root.transform, "Lattice Bar", new Vector3(0f, 2.28f, offsets[i]),
                    new Vector3(1.3f, 0.045f, 0.035f), gridMaterial);
                CreateBox(root.transform, "Lattice Bar", new Vector3(offsets[i], 2.28f, 0f),
                    new Vector3(0.035f, 0.045f, 1.3f), gridMaterial);
            }

            CreateBox(root.transform, "Vent", new Vector3(0f, 1.35f, 0.81f),
                new Vector3(1.2f, 1.5f, 0.03f), gridMaterial);
            for (int i = 0; i < 5; i++)
            {
                CreateBox(root.transform, "Vent Slat", new Vector3(0f, 0.72f + i * 0.32f, 0.832f),
                    new Vector3(1.1f, 0.06f, 0.015f), topMaterial);
            }

            CreateLabel(root.transform, name, new Vector3(0f, 2.55f, 0f), TextColor, 0.05f, 0f);
        }

        private static void CreatePdu(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            Material bodyMaterial,
            Material panelMaterial,
            Material stripeMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(root.transform, "Body", new Vector3(0f, 1.0f, 0f),
                new Vector3(0.8f, 2.0f, 1.0f), bodyMaterial);
            CreateBox(root.transform, "Front Panel", new Vector3(0f, 1.0f, 0.515f),
                new Vector3(0.68f, 1.84f, 0.03f), panelMaterial);
            CreateBox(root.transform, "Power Stripe", new Vector3(-0.30f, 1.0f, 0.53f),
                new Vector3(0.06f, 1.9f, 0.02f), stripeMaterial);

            CreateLabel(root.transform, name, new Vector3(0f, 2.3f, 0f), TextColor, 0.045f, 0f);
        }

        private static void CreateNetworkCabinet(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            Material bodyMaterial,
            Material panelMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(root.transform, "Body", new Vector3(0f, 1.0f, 0f),
                new Vector3(0.8f, 2.0f, 1.0f), bodyMaterial);
            CreateBox(root.transform, "Front Panel", new Vector3(0f, 1.0f, 0.515f),
                new Vector3(0.68f, 1.84f, 0.03f), panelMaterial);
            for (int i = 0; i < 3; i++)
            {
                CreateBox(root.transform, "Patch Slat", new Vector3(0f, 1.3f + i * 0.18f, 0.532f),
                    new Vector3(0.6f, 0.05f, 0.015f), bodyMaterial);
            }

            CreateLabel(root.transform, name, new Vector3(0f, 2.3f, 0f), TextColor, 0.045f, 0f);
        }

        private static void CreateUps(
            Transform parent,
            string name,
            Vector3 position,
            Material bodyMaterial,
            Material panelMaterial,
            Material indicatorMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            CreateBox(root.transform, "Body", new Vector3(0f, 1.0f, 0f),
                new Vector3(0.9f, 2.0f, 0.85f), bodyMaterial);
            CreateBox(root.transform, "Front Panel", new Vector3(0f, 1.0f, -0.44f),
                new Vector3(0.76f, 1.84f, 0.03f), panelMaterial);
            CreateBox(root.transform, "Indicator", new Vector3(0f, 1.72f, -0.462f),
                new Vector3(0.5f, 0.06f, 0.02f), indicatorMaterial);

            CreateLabel(root.transform, name, new Vector3(0f, 2.3f, 0f), TextColor, 0.05f, 0f);
        }

        private static void CreateBatteryCabinet(
            Transform parent,
            string name,
            Vector3 position,
            Material bodyMaterial,
            Material panelMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            CreateBox(root.transform, "Body", new Vector3(0f, 1.0f, 0f),
                new Vector3(0.9f, 2.0f, 0.85f), bodyMaterial);
            CreateBox(root.transform, "Front Panel", new Vector3(0f, 1.0f, -0.44f),
                new Vector3(0.76f, 1.84f, 0.03f), panelMaterial);
            for (int i = 0; i < 3; i++)
            {
                CreateBox(root.transform, "Battery Tray", new Vector3(0f, 0.55f + i * 0.5f, -0.462f),
                    new Vector3(0.68f, 0.12f, 0.02f), bodyMaterial);
            }

            CreateLabel(root.transform, name, new Vector3(0f, 2.3f, 0f), TextColor, 0.05f, 0f);
        }

        // -------------------------------------------------------------
        // Primitives
        // -------------------------------------------------------------

        private static GameObject CreateBox(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject CreateCylinder(
            Transform parent,
            string name,
            Vector3 localPosition,
            float diameter,
            float length,
            Material material,
            string axis)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

            switch (axis)
            {
                case "X":
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    break;
                case "Z":
                    go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                default:
                    go.transform.localRotation = Quaternion.identity;
                    break;
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void CreateLabel(
            Transform parent,
            string text,
            Vector3 localPosition,
            Color color,
            float characterSize,
            float yaw)
        {
            GameObject labelObject = new GameObject("Label " + text);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;

            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                // Fall through to an OS font below.
            }

            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(
                    new[] { "DejaVu Sans", "Liberation Sans", "Arial" }, 64);

            if (font != null)
            {
                textMesh.font = font;
                MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sharedMaterial = font.material;
            }

            labelObject.AddComponent<Billboard>();
        }

        private static Material MakeMaterial(
            string name,
            Color baseColor,
            float metallic,
            float smoothness,
            Color? emission = null,
            float emissionIntensity = 1f)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = name;
            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);

            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value * emissionIntensity);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path);
            string leaf = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
