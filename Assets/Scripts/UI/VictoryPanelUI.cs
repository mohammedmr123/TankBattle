using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VictoryPanelUI : MonoBehaviour
{
    private const string PanelObjectName = "VictoryPanel";

    private void Start()
    {
        LevelManager lm = LevelManager.Instance;
        if (lm == null)
        {
            Debug.LogWarning("[VictoryPanelUI] No LevelManager found in this scene. " +
                             "Add this component only to gameplay levels that have a LevelManager.");
            return;
        }

        if (lm.victoryPanel != null)
            return;

        GameObject existing = GameObject.Find(PanelObjectName);
        if (existing != null)
        {
            lm.victoryPanel = existing;
            existing.SetActive(false);
            return;
        }

        GameObject panel = BuildVictoryPanel();
        panel.SetActive(false);
        lm.victoryPanel = panel;
    }

    private GameObject BuildVictoryPanel()
    {

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("VictoryCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        GameObject panel = new GameObject(PanelObjectName, typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform prt = (RectTransform)panel.transform;
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);

        CreateText("VictoryTitle", panel.transform, "YOU  WIN!",
            fontSize: 96, color: new Color(1f, 0.85f, 0.2f),
            anchor: new Vector2(0.5f, 0.5f), position: new Vector2(0f, 130f),
            size: new Vector2(1400, 200));

        CreateButton("NextLevelButton", panel.transform, "NEXT LEVEL",
            anchor: new Vector2(0.5f, 0.5f), position: new Vector2(-200f, -50f),
            size: new Vector2(340, 110), onClick: OnNextLevelClicked);

        CreateButton("MainMenuButton", panel.transform, "MAIN MENU",
            anchor: new Vector2(0.5f, 0.5f), position: new Vector2(200f, -50f),
            size: new Vector2(340, 110), onClick: OnMainMenuClicked);

        EnsureEventSystem();

        panel.transform.SetAsLastSibling();

        return panel;
    }

    private void OnNextLevelClicked()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnNextLevelButton();
    }

    private void OnMainMenuClicked()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnMainMenuButton();
    }

    private Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    private GameObject CreateText(string name, Transform parent, string content, int fontSize,
        Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = GetBuiltinFont();
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return go;
    }

    private void CreateButton(string name, Transform parent, string label,
        Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        btn.colors = colors;
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lrt = (RectTransform)labelGo.transform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text lt = labelGo.AddComponent<Text>();
        lt.text = label;
        lt.font = GetBuiltinFont();
        lt.fontSize = 40;
        lt.alignment = TextAnchor.MiddleCenter;
        lt.color = new Color(0.1f, 0.1f, 0.1f);
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;

        _ = new GameObject("EventSystem",
            typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
