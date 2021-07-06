using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Volumes.Factories;

public class FillItUp : MonoBehaviour
{
    [SerializeField] private Button buttonPrefabInScene;
    
    void Start()
    {
        foreach (var methodInfo in typeof(VolumeFactories).GetMethods(BindingFlags.Static| BindingFlags.Public))
        {
            var button = Instantiate(buttonPrefabInScene, transform, false);
            button.GetComponentInChildren<Text>().text = methodInfo.Name;
            button.onClick.AddListener(() =>
            {
                methodInfo.Invoke(null, null);
            });
        }
        Destroy(buttonPrefabInScene.gameObject);
    }
}
