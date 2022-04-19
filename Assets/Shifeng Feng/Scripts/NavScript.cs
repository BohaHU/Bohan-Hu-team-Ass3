using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NavScript : MonoBehaviour
{
 public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
 
}
