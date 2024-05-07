using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Name : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string[] names;

    public GameObject dialogueBox;

    private Dialogue dialogueText;

    private int index;
    // Start is called before the first frame update
    void Start()
    {
        dialogueText = dialogueBox.GetComponent<Dialogue>();
        text.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = names[dialogueText.speaker];
        getColor();
        if (dialogueBox.activeSelf == false)
        {
            gameObject.SetActive(false);
        }
    }

    void StartDialogue()
    {
        text.text = names[dialogueText.speaker];
    }

    void getColor()
    {
            if(dialogueText.speaker == 0)
            {
                text.color = Color.blue;    
            }
            else if(dialogueText.speaker == 1)
            {
                text.color = Color.red;    

            }
            else if(dialogueText.speaker == 2)
            {
                text.color = new Color32(150, 75, 0, 255);

            }
            else if(dialogueText.speaker == 3)
            {
                text.color = new Color32(135, 206, 235, 255);    

            }
            else
            {
                text.color= new Color32(0, 0, 0, 255);
            }
    }
}
