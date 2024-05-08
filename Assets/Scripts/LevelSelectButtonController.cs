using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelSelectButtonController : MonoBehaviour, IPointerClickHandler
{
    public string levelName;

    public void OnPointerClick(PointerEventData eventData)
    {
        print("Clicked!");
        DialogueSelectManager.currLevel = levelName;
        MusicManager.instance.GetComponent<AudioSource>().Stop();
        SceneManager.LoadScene("DialogueScene");
    }
}
