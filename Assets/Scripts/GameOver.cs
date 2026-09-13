using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (keyboard.eKey.isPressed) SceneManager.LoadScene();
    }
}
