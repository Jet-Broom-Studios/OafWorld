using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI text;
    public List<string> line;
    public float speed;
    public int speaker;
    public Canvas background;

    public GameObject[] wizards;
    public GameObject[] backgroundMusic;

    private int index;
    // Start is called before the first frame update
    void Start()
    {
        getScript();
        text.text = string.Empty;
        StartDialogue();
        chooseAudio();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(text.text == line[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                text.text = line[index];
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        getBG();
        index++;

        if(line[index].ToUpper() == "A")
            {
                speaker = 0;
                index++;
            }
            else if(line[index].ToUpper() == "V")
            {
                speaker = 1;
                index++;
            }
            else if(line[index].ToUpper() == "R")
            {
                speaker = 2;
                index++;
            }
            else if(line[index].ToUpper() == "Z")
            {
                speaker = 3;
                index++;
            }
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        if(line[index] != "bg")
        {
            foreach (char c in line[index].ToCharArray())
            {
                text.text += c;
                yield return new WaitForSeconds(speed);
            }
        }
    }

    void NextLine()
    {
        if (index < line.Count - 1)
        {
            index++;
            if(line[index].ToUpper() == "A")
            {
                speaker = 0;
                index++;
            }
            else if(line[index].ToUpper() == "V")
            {
                speaker = 1;
                index++;
            }
            else if(line[index].ToUpper() == "R")
            {
                speaker = 2;
                index++;
            }
            else if(line[index].ToUpper() == "Z")
            {
                speaker = 3;
                index++;
            }

            text.text = string.Empty;
            if(line[index] != "bg")
            {
                StartCoroutine(TypeLine());
            }
            else
            {
                getBG();
                NextLine();
            }
        }
        else
        {
            gameObject.SetActive(false);
            background.enabled = false;
            wizards[0].SetActive(false);
            wizards[1].SetActive(false);
            wizards[2].SetActive(false);
            wizards[3].SetActive(false);
        }
    }

    void getScript()
    {
        string scriptLine;
        try
        {
            StreamReader sr = new StreamReader("Script");
            scriptLine = sr.ReadLine();
            while(scriptLine != null)
            {
                line.Add(scriptLine);
                scriptLine = sr.ReadLine();
            }
            sr.Close();
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    void chooseAudio()
    {
        int choice = 0;
        Instantiate(backgroundMusic[choice],new Vector3(0,0,0), Quaternion.identity);

    }

    void getBG()
    {
        if (line[index] == "bg")
        {
            index++;
            background.GetComponent<Backgrounds>().bg.GetComponent<Image>().material = background.GetComponent<Backgrounds>().backgrounds[Int32.Parse(line[index])];
        }
    }
}
