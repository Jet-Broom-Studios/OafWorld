using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    public static bool gameWin;
    public static bool gameOver;
    public static GameObject win;
    public static GameObject loss;
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
        win = GameObject.FindGameObjectWithTag("Win");
        loss = GameObject.FindGameObjectWithTag("GameOver");
        win.SetActive(false);
        loss.SetActive(false);
    }
}
