using UnityEngine;
using UnityEngine.UIElements;

public class HideShowUi : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        var all = root.Q<VisualElement>("all");
        var hideShow = root.Q<Button>("hideShow");
        hideShow.clicked += () =>
        {
            all.visible = !all.visible;
        };
    }
}
