
using UnityEngine;
using UnityEngine.SceneManagement;
public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    public void LoadMod1Scene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }


}