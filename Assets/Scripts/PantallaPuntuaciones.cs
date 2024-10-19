using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PantallaPuntuaciones : MonoBehaviour
{
    [SerializeField] TextAsset scoreFile;
    Text text;
    void Start()
    {
        text = GetComponent<Text>();
        text.text = scoreFile.text;
    }
}
