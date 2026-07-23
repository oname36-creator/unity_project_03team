using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class CreditEntry
{
    [SerializeField] private string _part;
    [SerializeField] private List<string> _names = new List<string>();


    public string Part 
    {  
        get { return _part; }
        set {  _part = value; }
    }

    public List<string> Names
    {
        get { return _names; }
        set { _names = value; }
    }
}
