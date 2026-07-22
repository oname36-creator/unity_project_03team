using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CreditView : MonoBehaviour
{
    #region Serialized Fields
    [Header("Credit Entry")]
    [SerializeField] private List<CreditEntry> _entries = new List<CreditEntry>();

    [Header("Setup")]
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _content;
    [SerializeField] private CreditLineView _linePrefab;
    [SerializeField] private TMP_FontAsset _partFont;
    [SerializeField] private TMP_FontAsset _nameFont;
    [SerializeField] private float _partLineHeight = 50f;
    [SerializeField] private float _nameLineHeight = 40f;
    [SerializeField] private float _lineSpacing = 5f;
    [SerializeField] private float _groupSpacing = 30f;

    [Header("Auto Scroll")]
    [SerializeField] private bool _autoScroll = true;
    [SerializeField] private float _scrollSpeed = 40f;

    [Header("Event")]
    [SerializeField] private UnityEvent _onDisableEvent;

    [Header("Ending")]
    [SerializeField] private bool ending = false;

    #endregion

    #region Private Fields
    private float _contentHeight;
    private float _scrolledY;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        BuildLines();
    }

    private void OnEnable()
    {
        _scrolledY = 0f;

        if (_content != null && _viewport != null)
        {
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, -_viewport.rect.height);
        }

    }

    private void OnDisable()
    {
        _onDisableEvent?.Invoke();
    }



    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame && !ending)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!_autoScroll || _content == null || _viewport == null) return;

        float viewportHeight = _viewport.rect.height;

        float maxScrolledY = _contentHeight + viewportHeight;

        _scrolledY = Mathf.Min(maxScrolledY, _scrolledY + _scrollSpeed * Time.deltaTime);
        _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, -viewportHeight + _scrolledY);

        if (_scrolledY >= maxScrolledY)
        {
            gameObject.SetActive(false);
        }
    }
    #endregion

    #region Private Methods
    private void BuildLines()
    {
        float y = 0f;

        foreach (CreditEntry entry in _entries)
        {
            y += AddLine(entry.Part, _partFont, _partLineHeight, y);

            foreach (string name in entry.Names)
            {
                y += AddLine(name, _nameFont, _nameLineHeight, y);
            }

            y += _groupSpacing;
        }

        _contentHeight = y;

        if (_content != null)
        {
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, _contentHeight);
        }
    }

    /// <summary>
    /// 크레딧 Content 에 들어갈 실제 Text 를 추가
    /// </summary>
    /// <param name="text">표시 텍스트</param>
    /// <param name="font">텍스트 폰트</param>
    /// <param name="height">라인의 높이</param>
    /// <param name="y">text가 생성될 높이</param>
    /// <returns>height + y, 다음 컨텐츠가 생성될 y값</returns>
    private float AddLine(string text, TMP_FontAsset font, float height, float y)
    {
        CreditLineView line = Instantiate(_linePrefab, _content);
        line.Text = text;
        line.SetFont(font);

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, height);
        rect.anchoredPosition = new Vector2(0f, -y);

        return height + _lineSpacing;
    }
    #endregion





}
