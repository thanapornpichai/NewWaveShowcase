using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WebViewPopupController : MonoBehaviour
{
    [Header("URL")]
    public string startUrl = "https://www.apple.com";

    [Header("UI Popup")]
    public GameObject popupRoot;     // Panel ครอบ (background)
    public Button closeButton;       // X
    public Button backButton;        // <

    [Header("Margins (leave space for top buttons)")]
    public int LeftMargin = 0;
    public int RightMargin = 0;
    public int TopMargin = 140;      // เว้นด้านบนให้ปุ่มไม่โดนเว็บทับ
    public int BottomMargin = 0;

    [Header("WebView")]
    [SerializeField] private WebViewObject webViewObject;

    private Coroutine _loadCoroutine;
    private bool _inited;

    void Awake()
    {
        if (popupRoot != null) popupRoot.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackOrClose);
        }
    }

    void OnDisable()
    {
        if (_loadCoroutine != null) StopCoroutine(_loadCoroutine);
    }

    // ===== Public API =====
    public void Open()
    {
        Open(startUrl);
    }

    public void Open(string url)
    {
        if (string.IsNullOrEmpty(url)) return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // ใน Editor Windows ปลั๊กอินนี้ไม่รองรับ -> เปิด browser แทน
        Application.OpenURL(url);
        return;
#else
        if (popupRoot != null) popupRoot.SetActive(true);

        if (!_inited) InitWebView();

        // โหลดหน้า
        if (_loadCoroutine != null) StopCoroutine(_loadCoroutine);
        _loadCoroutine = StartCoroutine(LoadWebView(url));

        // แสดง
        webViewObject.SetVisibility(true);
#endif
    }

    public void Close()
    {
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
        if (webViewObject != null) webViewObject.SetVisibility(false);
#endif
        if (popupRoot != null) popupRoot.SetActive(false);
    }

    public void BackOrClose()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // Editor เปิด browser ไปแล้ว ก็ไม่มี back ให้คุม
        Close();
#else
        if (webViewObject == null)
        {
            Close();
            return;
        }

        // ถ้าปลั๊กอินรองรับ CanGoBack/GoBack
        // (ส่วนใหญ่ gree/unity-webview มี)
        try
        {
            if (webViewObject.CanGoBack())
            {
                webViewObject.GoBack();
            }
            else
            {
                Close();
            }
        }
        catch
        {
            // ถ้ารุ่นปลั๊กอินคุณไม่มี CanGoBack ก็ให้ปิดไปเลย
            Close();
        }
#endif
    }

    // ===== Internal =====
    private void InitWebView()
    {
        if (webViewObject == null)
        {
            webViewObject = gameObject.AddComponent<WebViewObject>();
        }

        webViewObject.Init(
            cb: (msg) => { },
            err: (msg) => { Debug.LogError("WebView Error: " + msg); },
            httpErr: (msg) => { Debug.LogError("WebView HttpError: " + msg); },
            started: (msg) => { },
            hooked: (msg) => { },
            cookies: (msg) => { },
            ld: (msg) => { }
        );

        webViewObject.SetMargins(LeftMargin, TopMargin, RightMargin, BottomMargin);
        webViewObject.SetVisibility(false);

        _inited = true;
    }

    private IEnumerator LoadWebView(string url)
    {
        if (!_inited) yield break;

        // โหลด URL ปกติ (http/https)
        if (url.StartsWith("http"))
        {
            webViewObject.LoadURL(url.Replace(" ", "%20"));
            yield break;
        }

        // ถ้าอยากเปิดไฟล์ใน StreamingAssets (ตามโค้ดเดิมของนายน้อย)
        // ตรงนี้คงไว้แบบสั้น ๆ
        var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
        byte[] result = null;

        if (src.Contains("://"))
        {
            var req = UnityWebRequest.Get(src);
            yield return req.SendWebRequest();
            result = req.downloadHandler.data;
        }
        else
        {
            result = System.IO.File.ReadAllBytes(src);
        }

        var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
        System.IO.File.WriteAllBytes(dst, result);
        webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
    }
}