using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSelect : MonoBehaviour
{
    [SerializeField] private string[] scenes;
    [SerializeField] private Transform panel;
    [SerializeField] private Button button;
    
    private void Start()
    {
        foreach (var scene in scenes)
        {
            var instance = Instantiate(button, panel, false);
            instance.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(scene, LoadSceneMode.Single);
            });
            instance.GetComponentInChildren<Text>().text = scene;
        }
        Destroy(button.gameObject);
    }
}
