using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Artti.UI;

namespace Artti.Editor
{
    public static class ARFieldSceneBuilder
    {
        [MenuItem("Artti/Build ARFieldScene Hierarchy")]
        public static void Build()
        {
            // ── [AR] ──────────────────────────────────────────────
            var arRoot = new GameObject("[AR]");
            var arSession = new GameObject("AR Session");
            arSession.transform.SetParent(arRoot.transform);
            var arOrigin = new GameObject("AR Session Origin");
            arOrigin.transform.SetParent(arRoot.transform);
            var arCamera = new GameObject("AR Camera");
            arCamera.AddComponent<Camera>();
            arCamera.transform.SetParent(arOrigin.transform);

            // ── [Canvas] ──────────────────────────────────────────
            var canvasGo = new GameObject("[Canvas]");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            // CameraPanel
            var cameraPanel = CreatePanel("CameraPanel", canvasGo.transform);
            CreateRawImage("CameraRawImage", cameraPanel.transform);
            var guideText = CreateTMPText("GuideText", cameraPanel.transform, "가게 간판을 비춰주세요");

            // ResultConfirmPanel
            var resultPanel = CreatePanel("ResultConfirmPanel", canvasGo.transform);
            resultPanel.SetActive(false);
            var capturedImage = CreateRawImage("CapturedImage", resultPanel.transform);

            var resultCard = new GameObject("ResultCard");
            resultCard.transform.SetParent(resultPanel.transform, false);
            resultCard.AddComponent<RectTransform>();
            var aacIcon = CreateImage("AACIcon", resultCard.transform);
            var categoryLabel = CreateTMPText("CategoryLabel", resultCard.transform, "카테고리");
            var descriptionText = CreateTMPText("DescriptionText", resultCard.transform, "설명");

            var retryCard = new GameObject("RetryCard");
            retryCard.transform.SetParent(resultPanel.transform, false);
            retryCard.AddComponent<RectTransform>();
            var retryBtn = CreateButton("틀렸어요", retryCard.transform);

            // ResultConfirmUI 연결
            var resultConfirmUI = resultPanel.AddComponent<ResultConfirmUI>();
            var soResult = new SerializedObject(resultConfirmUI);
            soResult.FindProperty("capturedImage").objectReferenceValue = capturedImage;
            soResult.FindProperty("aacIcon").objectReferenceValue = aacIcon;
            soResult.FindProperty("categoryLabel").objectReferenceValue = categoryLabel;
            soResult.FindProperty("descriptionText").objectReferenceValue = descriptionText;
            soResult.FindProperty("retryButton").objectReferenceValue = retryBtn;
            soResult.ApplyModifiedProperties();

            // ManualSelectPanel
            var manualPanel = CreatePanel("ManualSelectPanel", canvasGo.transform);
            manualPanel.SetActive(false);
            var convBtn = CreateButton("편의점", manualPanel.transform);
            convBtn.gameObject.name = "ConvenienceBtn";
            var pharmBtn = CreateButton("약국", manualPanel.transform);
            pharmBtn.gameObject.name = "PharmacyBtn";
            var restBtn = CreateButton("음식점", manualPanel.transform);
            restBtn.gameObject.name = "RestaurantBtn";

            // CategoryPanel
            var categoryPanel = CreatePanel("CategoryPanel", canvasGo.transform);
            categoryPanel.SetActive(false);
            var catSlots = new AACCardUI[4];
            for (int i = 0; i < 4; i++)
                catSlots[i] = CreateCardSlot($"CardSlot_0{i + 1}", categoryPanel.transform);

            // SubCategoryPanel
            var subPanel = CreatePanel("SubCategoryPanel", canvasGo.transform);
            subPanel.SetActive(false);
            var subSlots = new AACCardUI[4];
            for (int i = 0; i < 4; i++)
                subSlots[i] = CreateCardSlot($"CardSlot_0{i + 1}", subPanel.transform);

            // ARFieldUIController 연결
            var uiController = canvasGo.AddComponent<ARFieldUIController>();
            var soUI = new SerializedObject(uiController);
            soUI.FindProperty("cameraPanel").objectReferenceValue = cameraPanel;
            soUI.FindProperty("resultConfirmPanel").objectReferenceValue = resultPanel;
            soUI.FindProperty("manualSelectPanel").objectReferenceValue = manualPanel;
            soUI.FindProperty("categoryPanel").objectReferenceValue = categoryPanel;
            soUI.FindProperty("subCategoryPanel").objectReferenceValue = subPanel;
            soUI.FindProperty("convenienceBtn").objectReferenceValue = convBtn;
            soUI.FindProperty("pharmacyBtn").objectReferenceValue = pharmBtn;
            soUI.FindProperty("restaurantBtn").objectReferenceValue = restBtn;

            var catSlotsProp = soUI.FindProperty("categorySlots");
            catSlotsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                catSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = catSlots[i];

            var subSlotsProp = soUI.FindProperty("subCategorySlots");
            subSlotsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                subSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = subSlots[i];

            soUI.ApplyModifiedProperties();

            // ── [Managers] ────────────────────────────────────────
            var managers = new GameObject("[Managers]");
            new GameObject("ARFieldManager").transform.SetParent(managers.transform);
            new GameObject("OCRManager").transform.SetParent(managers.transform);
            new GameObject("KeywordMatchManager").transform.SetParent(managers.transform);
            new GameObject("AACNavigator").transform.SetParent(managers.transform);
            new GameObject("ARTTSManager").transform.SetParent(managers.transform);

            Debug.Log("[ARFieldSceneBuilder] Hierarchy + Inspector 연결 완료!");
            Selection.activeGameObject = canvasGo;
        }

        // ── 헬퍼 ──────────────────────────────────────────────────

        static GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        static RawImage CreateRawImage(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<RawImage>();
        }

        static Image CreateImage(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<Image>();
        }

        static TMP_Text CreateTMPText(string name, Transform parent, string defaultText = "")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            return tmp;
        }

        static Button CreateButton(string label, Transform parent)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            CreateTMPText("Text", go.transform, label);
            return btn;
        }

        static AACCardUI CreateCardSlot(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var icon = CreateImage("Icon", go.transform);
            var label = CreateTMPText("Label", go.transform, "");
            go.AddComponent<Image>(); // 카드 배경
            var btn = go.AddComponent<Button>();

            var cardUI = go.AddComponent<AACCardUI>();
            var so = new SerializedObject(cardUI);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("labelText").objectReferenceValue = label;
            so.FindProperty("button").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            return cardUI;
        }
    }
}
