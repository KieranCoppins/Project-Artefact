///
/// Code by Kieran Coppins
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HoloLensMenuManager : MonoBehaviour
{
    private int level = 0;

    [SerializeField]
    private string[] LevelNames;

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private GameObject continueButton;

    /// <summary>
    /// Sets the level to be launched into
    /// </summary>
    /// <param name="lvl"></param>
    public void SelectLevel(int lvl)
    {
        level = lvl;
        continueButton.SetActive(true);
        levelText.text = "Continue to " + LevelNames[lvl];
    }

    /// <summary>
    /// Launches into the scene num with the level currently selected
    /// </summary>
    /// <param name="sceneNum"></param>
    public void LaunchLevel(int sceneNum)
    {
        //Store our selected level
        PlayerPrefs.SetInt("Level", level);
        SceneManager.LoadScene(sceneNum);
    }
}
