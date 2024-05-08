using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject[] levelSelectButtons;
    // Start is called before the first frame update
    void Start()
    {
        StopAllCoroutines();
        levelSelectButtons[1].SetActive(false);
        levelSelectButtons[2].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.l1Complete)
        {
            levelSelectButtons[1].SetActive(true);
        }
        if (GameManager.l2Complete)
        {
            levelSelectButtons[2].SetActive(true);
        }
    }

    // make sure that the button has the matching parameter for the next scene
    public void selectLevel(string level)
    {
        DialogueSelectManager.currLevel = level;
        MusicManager.instance.GetComponent<AudioSource>().Stop();
        SceneManager.LoadScene("DialogueScene");
    }
}
