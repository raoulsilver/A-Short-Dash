using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class VariableManager : MonoBehaviour
{
    public Dictionary<string,int> changes=new Dictionary<string, int>();
    public static VariableManager instance;

    void Awake(){
        if(instance == null){
            instance = this;
        } else if(instance != this){
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }

    public int CheckVariable(string varToCheck){
        if (changes.ContainsKey(varToCheck))
        {
            return changes[varToCheck];
        }
        else
        {
            changes.Add(varToCheck,0);
            return changes[varToCheck];
        }
    }

    public void UpdateVariable(string varToCheck,int value)
    {
        if (changes.ContainsKey(varToCheck))
        {
            changes[varToCheck] = value;
        }
        else
        {
            changes.Add(varToCheck,0);
            changes[varToCheck] = value;
        }
    }

    /*public string SaveVariable(string name,int value)
    {
        return $"{name}}\t{value}\n";

    }*/
    /*public void SaveData()
    {
        string dataString="";
        foreach(var pair in changes)
        {
         dataString+=$"{pair.Key}}\t{pair.Value}\n";   

        }
        System.IO.File.WriteAllText(Path.Combine(Application.persistentDataPath,"data.txt"),dataString);

    }
    void LoadChanges()
    {
        string loadData=System.IO.File.ReadAllText(Path.Combine(Application.persistentDataPath,"data.txt"));
        if(loadData!=null)
        {
                changes.Clear();
            StringReader sr=new StringReader(loadData);  
            while(true)
            {
                string line=sr.ReadLine();
                if(line==null)break;
                string[] pair=line.Split("\t");
                changes.Add(pair[0],int.Parse(pair[1]));

            }

        }
    }*/

}
