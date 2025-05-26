using TMPro;
using UnityEngine;


public class TextToProgress : MonoBehaviour
{
    [Header("Configurações Principais")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private string numberFormat = "0";

    [Header("Formatação de Texto")]
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";
    [SerializeField] private string separator = " / ";
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    private Billboard billboard;
    private float _currentValue;
    private RectTransform _rectTransform;

    private void Awake()
    {
        if (progressText == null)
            Debug.LogError("NAO ENCONTREI O TEXTO");

        _rectTransform = progressText.rectTransform;
        billboard = GetComponentInChildren<Billboard>();
        UpdateVisuals();
    }

    public void Setup(float max, float current = 0)
    {
        maxValue = Mathf.Max(0.1f, max);
        _currentValue = Mathf.Clamp(current, 0, maxValue);
        UpdateVisuals();
    }

    public void UpdateProgress(float current)
    {
        _currentValue = Mathf.Clamp(current, 0, maxValue);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (progressText == null) return;

        float progress = _currentValue / maxValue;
        Color currentColor = GetProgressColor(progress);

        progressText.text = $"{prefix}{_currentValue.ToString(numberFormat)}{separator}{maxValue.ToString(numberFormat)}{suffix}";
        progressText.color = currentColor;
    }

    private Color GetProgressColor(float progress)
    {
        return progress switch
        {
            >= 0.7f => fullColor,
            >= 0.3f => Color.Lerp(midColor, fullColor, (progress - 0.3f) / 0.4f),
            _ => Color.Lerp(lowColor, midColor, progress / 0.3f)
        };
    }

    public void SetFormat(string newPrefix, string newSuffix, string newSeparator)
    {
        prefix = newPrefix;
        suffix = newSuffix;
        separator = newSeparator;
        UpdateVisuals();
    }

    public RectTransform GetRectTransform() => _rectTransform;

    // Métodos adicionais úteis
    public void Increment(float amount)
    {
        UpdateProgress(_currentValue + amount);
    }

    public void Decrement(float amount)
    {
        UpdateProgress(_currentValue - amount);
    }

    public void ChangeBillboard(Vector3 _positionOffset, Vector3 _rotationOffset)
    {
        if (billboard == null) return;

        billboard.positionOffset = _positionOffset;
        billboard.rotationOffset = _rotationOffset;
    }

    public float GetProgress01() => _currentValue / maxValue;
}