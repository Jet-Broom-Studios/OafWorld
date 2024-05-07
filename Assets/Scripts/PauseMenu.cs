using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    // add a pause check around mechanics you want paused
    public static bool isPaused;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        //opens and closes pause menu
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(isPaused){
                ResumeGame();
            }
            else{
                PauseGame();
            }
        }
    }

    public void PauseGame(){
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    //continues game from same scene
    public void ResumeGame(){
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    //sends player to select level scene
    public void SelectLevel(){
        isPaused = false;
        Time.timeScale = 1f;
        MusicManager.instance.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("LevelSelectScene");
        ResumeGame();
    }
    //sends player back to title scene
    public void QuitGame(){
        isPaused = false;
        Time.timeScale = 1f;
        MusicManager.instance.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("TitleScene");
        ResumeGame();
    }
}
