using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Util.Ui
{
    public class RenderUiDocumentToCanvas : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
    
        private void Start()
        {
            var rt = new RenderTexture(Screen.width, Screen.height, 32);
            var canvas = new GameObject().AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var rawImage = new GameObject().AddComponent<RawImage>();
            var rawImageRt = rawImage.GetComponent<RectTransform>();
            rawImageRt.anchorMin = Vector2.zero;
            rawImageRt.anchorMax = Vector2.one;
            rawImageRt.offsetMin = Vector2.zero;
            rawImageRt.offsetMax = Vector2.zero;
            rawImage.transform.SetParent(canvas.transform,false);
            rawImage.texture = rt;

            var uiDocumentPanelSettings = Instantiate(uiDocument.panelSettings);
            uiDocumentPanelSettings.targetTexture = rt;
            uiDocument.panelSettings = uiDocumentPanelSettings;
        }
    }
}
