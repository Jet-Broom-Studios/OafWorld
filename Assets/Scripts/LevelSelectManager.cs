using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // make sure that the button has the matching parameter for the next scene
    public void selectLevel(string level)
    {
        DialogueSelectManager.currLevel = level;
        SceneManager.LoadScene("DialogueScene");
    }
}
