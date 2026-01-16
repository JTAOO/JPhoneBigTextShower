using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class BigTextDisplay : MonoBehaviour
{
    [Header("字体设置")]
    public Font customFont; // 公开的字体输入（UGUI字体）

    [Header("默认设置")]
    [Tooltip("默认显示文字")]
    public string defaultText = "超大文字";
    [Tooltip("默认字体颜色")]
    public Color defaultTextColor = Color.white;
    [Tooltip("默认相对字体大小（0-1，基于屏幕高度）")]
    [Range(0.1f, 0.9f)] public float defaultFontSizeRatio = 0.5f;

    // 核心UI元素
    private Text bigText;          // 主显示文字
    private Button settingButton;  // 设置按钮
    private GameObject settingPanel; // 设置面板
    private bool isSettingPanelOpen = false; // 设置面板状态
    private bool isMirrorMode = false; // 镜像模式状态

    // 设置面板控件
    private InputField textInput;
    private Button colorButton;
    private Slider fontSizeSlider;
    private Toggle mirrorToggle;
    private Button confirmButton;
    private Button cancelButton;

    private Color currentTextColor;
    private float currentFontSizeRatio;
    private string currentDisplayText;

    void Awake()
    {
        // 初始化Canvas（自动创建UI根节点）
        InitCanvas();

        // 初始化默认值
        currentDisplayText = defaultText;
        currentTextColor = defaultTextColor;
        currentFontSizeRatio = defaultFontSizeRatio;

        // 创建主显示文字
        CreateBigText();

        // 创建右下角设置按钮
        CreateSettingButton();

        // 创建设置面板（初始隐藏）
        CreateSettingPanel();

        // 适配屏幕尺寸
        UpdateTextSizeAndPosition();
    }

    void Update()
    {
        // 监听屏幕尺寸/方向变化，实时适配
        if (Input.deviceOrientation != DeviceOrientation.Unknown)
        {
            UpdateTextSizeAndPosition();
        }
    }

    #region UI创建相关方法
    /// <summary>
    /// 初始化Canvas设置
    /// </summary>
    private void InitCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        // 添加CanvasScaler确保适配不同分辨率
        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); // 参考分辨率（手机主流）
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 添加GraphicRaycaster用于UI交互
        if (!GetComponent<GraphicRaycaster>())
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    /// <summary>
    /// 创建主显示的超大文字
    /// </summary>
    private void CreateBigText()
    {
        GameObject textObj = new GameObject("BigText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        bigText = textObj.GetComponent<Text>();
        bigText.text = currentDisplayText;
        bigText.color = currentTextColor;
        bigText.alignment = TextAnchor.MiddleCenter;
        bigText.horizontalOverflow = HorizontalWrapMode.Overflow;
        bigText.verticalOverflow = VerticalWrapMode.Overflow;

        // 设置自定义字体（如果有）
        if (customFont != null)
        {
            bigText.font = customFont;
        }
    }

    /// <summary>
    /// 创建右下角50x50的白色设置按钮
    /// </summary>
    private void CreateSettingButton()
    {
        GameObject buttonObj = new GameObject("SettingButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(transform, false);

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(1, 0);
        buttonRect.sizeDelta = new Vector2(250, 250); // 50x50像素
        buttonRect.anchoredPosition = new Vector2(-20, 20); // 距离边缘20像素

        // 设置按钮背景为白色
        Image buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.color = Color.white;
        buttonImage.type = Image.Type.Simple;

        // 添加按钮点击事件
        settingButton = buttonObj.GetComponent<Button>();
        settingButton.onClick.AddListener(ToggleSettingPanel);

        // 添加按钮文字提示
        GameObject buttonTextObj = new GameObject("ButtonText", typeof(RectTransform), typeof(Text));
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = buttonTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text buttonText = buttonTextObj.GetComponent<Text>();
        buttonText.text = "⚙";
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;

        // 给按钮文字设置默认字体（防止无字体报错）
        if (buttonText.font == null)
        {
            buttonText.font = customFont;
        }
    }

    /// <summary>
    /// 创建设置面板
    /// </summary>
    private void CreateSettingPanel()
    {
        // 面板背景
        settingPanel = new GameObject("SettingPanel", typeof(RectTransform), typeof(Image));
        settingPanel.transform.SetParent(transform, false);
        settingPanel.SetActive(false);

        RectTransform panelRect = settingPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.2f);
        panelRect.anchorMax = new Vector2(0.8f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 面板半透明背景
        Image panelImage = settingPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        panelImage.type = Image.Type.Simple;

        // 创建面板内容容器
        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(settingPanel.transform, false);

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.1f);
        contentRect.anchorMax = new Vector2(0.9f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = contentObj.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // 1. 文字输入框
        CreateTextInputField(contentObj.transform);

        // 2. 颜色选择按钮
        CreateColorButton(contentObj.transform);

        // 3. 字体大小滑块
        CreateFontSizeSlider(contentObj.transform);

        // 4. 镜像模式开关
        CreateMirrorToggle(contentObj.transform);

        // 5. 确认和取消按钮
        CreateControlButtons(contentObj.transform);
    }

    /// <summary>
    /// 创建文字输入框
    /// </summary>
    private void CreateTextInputField(Transform parent)
    {
        GameObject inputObj = new GameObject("TextInput", typeof(RectTransform), typeof(InputField));
        inputObj.transform.SetParent(parent, false);

        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(400, 60);

        InputField inputField = inputObj.GetComponent<InputField>();
        inputField.text = currentDisplayText;

        // 创建占位文字
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        placeholderObj.transform.SetParent(inputObj.transform, false);

        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);

        Text placeholderText = placeholderObj.GetComponent<Text>();
        placeholderText.text = "请输入显示文字";
        placeholderText.fontSize = 24;
        placeholderText.color = new Color(1, 1, 1, 0.5f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        placeholderText.font = customFont;
        inputField.placeholder = placeholderText;

        // 输入框文字设置
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(inputObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        Text inputText = textObj.GetComponent<Text>();
        inputText.text = currentDisplayText;
        inputText.fontSize = 24;
        inputText.color = Color.white;
        inputText.alignment = TextAnchor.MiddleLeft;
        inputText.font = customFont;
        inputField.textComponent = inputText;

        // 输入框背景
        Image inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        inputField.image = inputImage;

        // 输入值变化监听
        inputField.onValueChanged.AddListener((text) => currentDisplayText = text);
        textInput = inputField;
    }

    /// <summary>
    /// 创建颜色选择按钮
    /// </summary>
    private void CreateColorButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("ColorButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 60);

        Image buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 按钮文字
        GameObject textObj = new GameObject("ButtonText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text buttonText = textObj.GetComponent<Text>();
        buttonText.text = "点击选择文字颜色";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = customFont;

        // 颜色预览小方块
        GameObject colorPreviewObj = new GameObject("ColorPreview", typeof(RectTransform), typeof(Image));
        colorPreviewObj.transform.SetParent(buttonObj.transform, false);

        RectTransform previewRect = colorPreviewObj.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.9f, 0.1f);
        previewRect.anchorMax = new Vector2(0.98f, 0.9f);
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;

        Image previewImage = colorPreviewObj.GetComponent<Image>();
        previewImage.color = currentTextColor;

        // 按钮点击事件（随机颜色，可替换为颜色选择器）
        colorButton = buttonObj.GetComponent<Button>();
        colorButton.onClick.AddListener(() =>
        {
            currentTextColor = new Color(Random.value, Random.value, Random.value);
            previewImage.color = currentTextColor;
        });
    }

    /// <summary>
    /// 创建字体大小滑块
    /// </summary>
    private void CreateFontSizeSlider(Transform parent)
    {
        GameObject sliderObj = new GameObject("FontSizeSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(400, 60);

        // 滑块背景
        Image sliderBG = sliderObj.AddComponent<Image>();
        sliderBG.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 添加Slider组件
        Slider slider = sliderObj.AddComponent<Slider>();

        // 滑动条背景
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.1f, 0.4f);
        bgRect.anchorMax = new Vector2(0.9f, 0.6f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f);
        slider.targetGraphic = bgImage;

        // 滑动条填充
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(sliderObj.transform, false);

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.1f, 0.4f);
        fillRect.anchorMax = new Vector2(0.1f, 0.6f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObj.GetComponent<Image>();
        fillImage.color = Color.cyan;
        slider.fillRect = fillRect;

        // 滑动条手柄
        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(sliderObj.transform, false);

        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);

        Image handleImage = handleObj.GetComponent<Image>();
        handleImage.color = Color.white;
        slider.handleRect = handleRect;

        // 设置滑块参数
        slider.minValue = 0.1f;
        slider.maxValue = 0.9f;
        slider.value = currentFontSizeRatio;
        slider.onValueChanged.AddListener((value) => currentFontSizeRatio = value);

        // 滑块文字提示
        GameObject textObj = new GameObject("SliderText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(sliderObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text sliderText = textObj.GetComponent<Text>();
        sliderText.text = $"字体大小: {Mathf.Round(currentFontSizeRatio * 100)}%";
        sliderText.fontSize = 20;
        sliderText.color = Color.white;
        sliderText.alignment = TextAnchor.MiddleCenter;
        sliderText.font = customFont;

        // 滑块值变化时更新文字
        slider.onValueChanged.AddListener((value) =>
        {
            sliderText.text = $"字体大小: {Mathf.Round(value * 100)}%";
        });

        fontSizeSlider = slider;
    }

    /// <summary>
    /// 创建镜像模式开关
    /// </summary>
    private void CreateMirrorToggle(Transform parent)
    {
        GameObject toggleObj = new GameObject("MirrorToggle", typeof(RectTransform));
        toggleObj.transform.SetParent(parent, false);

        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(400, 60);

        // 背景
        Image toggleBG = toggleObj.AddComponent<Image>();
        toggleBG.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 添加Toggle组件
        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // 开关背景
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(toggleObj.transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.7f, 0.2f);
        bgRect.anchorMax = new Vector2(0.85f, 0.8f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f);
        toggle.targetGraphic = bgImage;

        // 开关勾选标记
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkObj.transform.SetParent(toggleObj.transform, false);

        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.72f, 0.25f);
        checkRect.anchorMax = new Vector2(0.83f, 0.75f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        Image checkImage = checkObj.GetComponent<Image>();
        checkImage.color = Color.green;
        toggle.graphic = checkImage;

        // 设置开关参数
        toggle.isOn = isMirrorMode;
        toggle.onValueChanged.AddListener((value) => isMirrorMode = value);

        // 开关文字
        GameObject textObj = new GameObject("ToggleText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(toggleObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0);
        textRect.anchorMax = new Vector2(0.7f, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text toggleText = textObj.GetComponent<Text>();
        toggleText.text = "镜像显示模式";
        toggleText.fontSize = 24;
        toggleText.color = Color.white;
        toggleText.alignment = TextAnchor.MiddleLeft;
        toggleText.font = customFont;

        mirrorToggle = toggle;
    }

    /// <summary>
    /// 创建确认和取消按钮
    /// </summary>
    private void CreateControlButtons(Transform parent)
    {
        GameObject buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonGroup.transform.SetParent(parent, false);

        RectTransform groupRect = buttonGroup.GetComponent<RectTransform>();
        groupRect.sizeDelta = new Vector2(400, 60);

        HorizontalLayoutGroup groupLayout = buttonGroup.GetComponent<HorizontalLayoutGroup>();
        groupLayout.spacing = 20;
        groupLayout.childControlWidth = true;
        groupLayout.childControlHeight = true;
        groupLayout.childAlignment = TextAnchor.MiddleCenter;

        // 确认按钮
        GameObject confirmObj = CreateButton(buttonGroup.transform, "确认", new Color(0.1f, 0.6f, 0.1f));
        confirmButton = confirmObj.GetComponent<Button>();
        confirmButton.onClick.AddListener(ApplySettings);

        // 取消按钮
        GameObject cancelObj = CreateButton(buttonGroup.transform, "取消", new Color(0.6f, 0.1f, 0.1f));
        cancelButton = cancelObj.GetComponent<Button>();
        cancelButton.onClick.AddListener(HideSettingPanel);
    }

    /// <summary>
    /// 辅助方法：创建按钮
    /// </summary>
    private GameObject CreateButton(Transform parent, string text, Color color)
    {
        GameObject buttonObj = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(180, 60);

        Image buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.color = color;

        // 按钮文字
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text buttonText = textObj.GetComponent<Text>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = customFont;

        return buttonObj;
    }
    #endregion

    #region 功能逻辑方法
    /// <summary>
    /// 切换设置面板显示/隐藏
    /// </summary>
    private void ToggleSettingPanel()
    {
        isSettingPanelOpen = !isSettingPanelOpen;
        settingPanel.SetActive(isSettingPanelOpen);
    }

    /// <summary>
    /// 隐藏设置面板
    /// </summary>
    private void HideSettingPanel()
    {
        isSettingPanelOpen = false;
        settingPanel.SetActive(false);
    }

    /// <summary>
    /// 应用设置
    /// </summary>
    private void ApplySettings()
    {
        // 更新文字内容
        bigText.text = currentDisplayText;
        // 更新文字颜色
        bigText.color = currentTextColor;
        // 更新字体大小
        UpdateTextSizeAndPosition();
        // 更新镜像模式
        UpdateMirrorMode();
        // 隐藏设置面板
        HideSettingPanel();
    }

    /// <summary>
    /// 更新文字大小和位置（自适应屏幕）
    /// </summary>
    private void UpdateTextSizeAndPosition()
    {
        if (bigText == null) return;

        // 根据屏幕高度设置字体大小
        float fontSize = Screen.height * currentFontSizeRatio;
        bigText.fontSize = Mathf.RoundToInt(fontSize);

        // 确保文字居中显示
        bigText.rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 更新镜像模式
    /// </summary>
    private void UpdateMirrorMode()
    {
        if (bigText == null) return;

        if (isMirrorMode)
        {
            // 设置水平镜像
            bigText.rectTransform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // 恢复正常
            bigText.rectTransform.localScale = new Vector3(1, 1, 1);
        }
    }
    #endregion

    // 可选：在编辑器模式下预览
    void OnValidate()
    {
        if (bigText != null)
        {
            if (customFont != null)
            {
                bigText.font = customFont;
            }
            UpdateTextSizeAndPosition();
        }
    }
}