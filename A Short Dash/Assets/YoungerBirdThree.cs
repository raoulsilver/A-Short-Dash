using UnityEngine;

public class YoungerBirdThree : TextWindowLoader
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.SetInt("currentlyReading",0);
    }

    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "youngerBird3GiveQuest")
        {
            PlayerPrefs.SetInt("bird3QuestGiven",1);
        }
        if(lineIdToDisplay == "youngerBird3OneBook")
        {
            PlayerPrefs.SetInt("bookCount",PlayerPrefs.GetInt("bookCount")-1);
            PlayerPrefs.SetInt("booksGiven",1);
            PlayerPrefs.SetInt("currentlyReading",1);
        }
        if(lineIdToDisplay == "youngerBird3BookTwo")
        {
            PlayerPrefs.SetInt("bookCount",PlayerPrefs.GetInt("bookCount")-1);
            PlayerPrefs.SetInt("booksGiven",2);
            PlayerPrefs.SetInt("currentlyReading",1);
        }
        if(lineIdToDisplay == "youngerBird3FinishedReadingBookTwo")
        {
            PlayerPrefs.SetInt("givenFlowerCrown",1);
            PlayerPrefs.SetInt("hasFlowerCrown",1);
        }
    }

    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("bird3QuestGiven") == 0)
        {
            lineIdToDisplay = "youngerBird3GiveQuest";
            return;
        }
        if (PlayerPrefs.GetInt("currentlyReading") == 1)
        {
            if (PlayerPrefs.GetInt("booksGiven") == 1)
            {
                lineIdToDisplay= "youngerBird3ReadingBookOne";
                return;
            }
            if (PlayerPrefs.GetInt("booksGiven") == 2)
            {
                lineIdToDisplay= "youngerBird3ReadingBookTwo";
                return;
            }
        }
        if (PlayerPrefs.GetInt("booksGiven")<2 && PlayerPrefs.GetInt("currentlyReading")==0 && PlayerPrefs.GetInt("bird3QuestGiven") == 1)
        {
            
            if (PlayerPrefs.GetInt("bookCount") == 0)
            {
                lineIdToDisplay = "youngerBird3QuestIdle";
                return;
            }
            if (PlayerPrefs.GetInt("bookCount") > 0 && PlayerPrefs.GetInt("booksGiven")==0)
            {
                Debug.Log("got through");
                lineIdToDisplay = "youngerBird3OneBook";
                return;
            }
            if (PlayerPrefs.GetInt("bookCount") > 0 && PlayerPrefs.GetInt("booksGiven")==1)
            {
                lineIdToDisplay = "youngerBird3BookTwo";
                return;
            }
        }
        if (PlayerPrefs.GetInt("booksGiven") == 2 && PlayerPrefs.GetInt("currentlyReading") == 0)
        {
            if (PlayerPrefs.GetInt("givenFlowerCrown") == 0)
            {
                lineIdToDisplay = "youngerBird3FinishedReadingBookTwo";
                return;
            }
            if (PlayerPrefs.GetInt("givenFlowerCrown") == 1)
            {
                lineIdToDisplay = "youngerBird3FinishedIdle";
                return;
            }
        }
    }


}
