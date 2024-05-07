using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    public static bool gameWin;
    public static bool gameOver;
    public GameObject w;
    public GameObject l;

    void Start()
    {
        w = transform.Find("Win").gameObject as GameObject;
        w.SetActive(false);
        l = transform.Find("GameOver").gameObject as GameObject;
        l.SetActive(false);
    }

    void Update()
    {
        if(gameWin)
        {
            w.SetActive(true);
        }
        else
        {
            w.SetActive(false);
        }
        if(gameOver)
        {
            l.SetActive(true);
        }
        else
        {
            l.SetActive(false);
        }
    }
    public void setWin()
    {
        w.SetActive(true);
    }
}
