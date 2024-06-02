using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class demo1Controller : MonoBehaviour
{
    public MathExpression Equation;

    public GameObject EndPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Equation.RawContent == "1/2=tanA")
        {
            EndPanel.SetActive(true);
        }
    }

    public void ReLoad()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
