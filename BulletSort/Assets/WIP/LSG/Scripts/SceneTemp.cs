using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTemp : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    
    private void SceneLoadByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SceneLoad()
    {
        SceneLoadByName(_sceneName);
    }
}
