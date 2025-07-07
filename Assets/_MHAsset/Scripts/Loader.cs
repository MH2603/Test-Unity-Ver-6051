using UnityEngine;
using UnityEngine.SceneManagement;
using Reflex.Core;

public class Loader : MonoBehaviour
{
    private void Start()
    {
        var scene = SceneManager.LoadScene("Greet", new LoadSceneParameters(LoadSceneMode.Single));
        ReflexSceneManager.PreInstallScene(scene, builder => builder.AddSingleton("Beautiful"));
    }
}