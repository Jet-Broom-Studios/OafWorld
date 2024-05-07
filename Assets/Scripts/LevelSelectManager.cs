using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    private GameObject dsm;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        dsm = GameObject.FindGameObjectWithTag("dsm");
    }

    // make sure that the button has the matching parameter for the next scene
    public void selectLevel(string level)
    {
        dsm.GetComponent<DialogueSelectManager>().currLevel = level;
        SceneManager.LoadScene("DialogueScene");
    }
}
