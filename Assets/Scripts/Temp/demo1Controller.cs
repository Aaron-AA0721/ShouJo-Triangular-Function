using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class demo1Controller : MonoBehaviour
{
    public MathExpression Equation;

    public GameObject EndPanel;

    public static bool GameEnd = false;
    // Start is called before the first frame update
    void Start()
    {
        GameEnd = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameEnd)
        {
            EndPanel.SetActive(true);
            GameEnd = false;
        }
    }

    public void ReLoad()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
