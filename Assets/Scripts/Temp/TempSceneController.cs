using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TempSceneController : MonoBehaviour
{
    public GameObject GamePanel;
    public GameObject DialogPanel;
    public Text DialogText;
    public Button DialogButton;
    private GameObject[] OriginalQ;
    GameObject[] RefinedQ;

    [SerializeField] private int DialogIndex;
    private GameObject ToolButton;
    private GameObject ToolPanel;
    private int PreviousFormula;
    private bool combineFormulaUnlock;

    // Start is called before the first frame update
    void Start()
    {
        PreviousFormula = 0;
        combineFormulaUnlock = false;
        GamePanel = GameObject.Find("GamePanel");
        DialogPanel = GameObject.Find("DialogPanel");
        DialogText = DialogPanel.transform.GetChild(1).GetComponent<Text>();
        DialogButton = DialogPanel.transform.GetChild(2).GetComponent<Button>();
        ToolPanel = GameObject.Find("ToolPanel");
        DialogPanel.SetActive(false);
        OriginalQ = new GameObject[4];
        RefinedQ = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            OriginalQ[i] = GameObject.Find("OriginalQuestion").transform.GetChild(i).gameObject;
            RefinedQ[i] = GameObject.Find("RefinedQuestion").transform.GetChild(i).gameObject;
        }
        RefinedQ[1].transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(() => Clicked(0));
        RefinedQ[1].transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(() => Clicked(1));
        RefinedQ[1].transform.GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(() => Clicked(0));
        RefinedQ[1].transform.GetChild(2).GetChild(0).GetComponent<Button>().onClick.AddListener(() => Clicked(1));
        for (int i = 0; i < 4; i++)
        {
            RefinedQ[1].transform.GetChild(i).gameObject.SetActive(false);
        }
        RefinedQ[1].transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            RefinedQ[i].gameObject.SetActive(false);
        }
        OriginalQ[0].GetComponent<Button>().onClick.AddListener(() => OriginQuesOnClick(0));
        OriginalQ[1].GetComponent<Button>().onClick.AddListener(() => OriginQuesOnClick(1));
        DialogIndex = 0;
        ToolButton = GameObject.Find("ToolButton");
        ToolButton.SetActive(false);
        GamePanel.SetActive(false);
        ToolPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        Debug.Log("GameStart");
        GameObject.Find("LanguagePanel").SetActive(false);
        GamePanel.SetActive(true);
        NextDialog();
    }

    public void NextDialog()
    {
        DisplayDialog(DialogIndex);
        DialogIndex++;
    }

    void DisplayDialog(int index)
    {
        switch (index)
        {
            case 0:
                DialogPanel.SetActive(true);
                DialogText.text = "Hi, welcome to this magical math game!\n(Click to Continue)";
                break;
            case 1:
                DialogText.text =
                    "In this game, I will teach you how to solve math questions logically\nWithout anything hard to understand";
                break;
            case 2:
                DialogText.text = "Now I am going to introduce you the basic controls of this game";
                break;
            case 3:
                DialogText.text =
                    "In the very first step of solving math question, you are supposed to look at the question first, right?\nTry to click on words in the question which you think is important.";
                DialogButton.gameObject.SetActive(false);
                break;
            case 4:
                DialogText.text = "It is a common triangle,right?";
                break;
            case 5:
                DialogText.text = "This equation seems important";
                break;
            case 6:
                DialogText.text = "Great! You have found all the useful information in the question!";
                break;
            case 7:
                OriginalQ[3].SetActive(false);
                DialogText.text =
                    "Take a look question (a) first. Although we have a new condition, the value of C. It seems that we are not able to directly get the value of angle B, right?";
                break;
            case 8:
                DialogText.text =
                    "Maybe we can get some information from the complex equation. Many useful formula about triangular functions are about multiplying some triangular functions.";
                break;
            case 9:
                DialogText.text =
                    "We can try to convert it from division to multiplication. Try to drag the denominators to the other side.";
                DisplayHint(0);
                DialogButton.gameObject.SetActive(false);
                break;
            case 10:
                DialogText.text =
                    "Good! You make it! But the expression now is not telling you the answer.\nClick the equation to expand it!";
                DialogButton.gameObject.SetActive(false);
                break;
            case 11:
                DialogText.text =
                    "There seems to be something that you are familiar with. Click Tools to see what you have learned before and how can they help you!";
                ToolButton.SetActive(true);
                DialogButton.gameObject.SetActive(false);
                StartCoroutine(WaitForToolPanel());
                break;
            case 12:
                DialogText.text =
                    "There should be a tool that you can use on the equation. Try to move some terms in the equation to make it in the correct form and drag the correct formula to the equation!";
                break;
            case 13:
                DialogText.text =
                    "Nice Work! You are halfway there! Now you know that you can move terms, and apply formula by dragging!";
                break;
            case 14:
                DialogText.text =
                    "Next, you can try dragging a formula to another, and see if they can form any new formula by combination";
                combineFormulaUnlock = true;
                DialogButton.gameObject.SetActive(false);
                break;
            case 15:
                DialogText.text =
                    "Great! You find a useful advanced conclusion! Can you find out how to use it?";
                DialogButton.gameObject.SetActive(false);
                break;
            case 16:
                DialogText.text =
                    "Impressive, and you're one step away from the final answer!";
                break;
            default:
                break;
        }
        Debug.Log("index = " + index);
    }

    void OriginQuesOnClick(int index)
    {
        OriginalQ[index].SetActive(false);
        RefinedQ[index].SetActive(true);
        DisplayDialog(index + 4);
        if (RefinedQ[0].activeSelf && RefinedQ[1].activeSelf)
        {
            DialogButton.gameObject.SetActive(true);
            DialogIndex = 6;
        }
    }

    void DisplayHint(int index)
    {
        
    }

    void Clicked(int index)
    {
        if (index == 0)
        {
            if (RefinedQ[1].transform.GetChild(0).gameObject.activeSelf)
            {
                RefinedQ[1].transform.GetChild(0).gameObject.SetActive(false);
                RefinedQ[1].transform.GetChild(2).gameObject.SetActive(true);
            }
            else
            {
                RefinedQ[1].transform.GetChild(1).gameObject.SetActive(false);
                RefinedQ[1].transform.GetChild(3).gameObject.SetActive(true);
            }
        }
        else
        {
            if (RefinedQ[1].transform.GetChild(0).gameObject.activeSelf)
            {
                RefinedQ[1].transform.GetChild(0).gameObject.SetActive(false);
                RefinedQ[1].transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                RefinedQ[1].transform.GetChild(2).gameObject.SetActive(false);
                RefinedQ[1].transform.GetChild(3).gameObject.SetActive(true);
            }
        }

        if (RefinedQ[1].transform.GetChild(3).gameObject.activeSelf)
        {
            DialogButton.gameObject.SetActive(true);
        }
    }

    public void ExpandEquation()
    {
        RefinedQ[1].gameObject.SetActive(false);
        RefinedQ[2].gameObject.SetActive(true);
        NextDialog();
    }
    public void MoveTermFormula1()
    {
        DialogText.text =
            "You move the terms to the same side to apply formula! good!";
    }
    IEnumerator WaitForToolPanel()
    {
        while (!ToolPanel.activeSelf)
        {
            yield return null;
        }

        NextDialog();
    }
    public void ApplyFormula(int index)
    {
        switch (index)
        {
            case 1:
                if (RefinedQ[2].gameObject.activeSelf)
                {
                    if (RefinedQ[2].transform.GetChild(1).gameObject.activeSelf)
                        {
                            RefinedQ[2].transform.GetChild(1).gameObject.SetActive(false);
                            RefinedQ[2].transform.GetChild(2).gameObject.SetActive(true);
                            DialogText.text =
                                "That is exactly what you need to solve the problem! Good job!";
                            DialogButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            if (RefinedQ[2].transform.GetChild(0).gameObject.activeSelf)
                            {
                                DialogText.text =
                                    "It seems to work. But maybe you should try to move the terms so that there is a term to which the formula can be applied.";
                            }
                            else{DialogText.text =
                                "Seems not useful here";}
                        }
                }
                else
                {
                    DialogText.text =
                        "Really? Is there anything that you can apply this formula to?";
                }
                break;
            case 2:
                if (RefinedQ[2].transform.GetChild(1).gameObject.activeSelf)
                {
                    DialogText.text =
                        "Although it looks similar, but the is there really a plus sign between cosXcosY and sinXsinY?";
                }
                else
                {
                    DialogText.text =
                        "How to use this formula....";
                }
                break;
            case 3:
                if (RefinedQ[2].transform.GetChild(2).gameObject.activeSelf)
                {
                    DialogText.text =
                        "Great, you find that sin2B is actually sin(B+B), so sin2B=2sinBcosB";
                    RefinedQ[2].transform.GetChild(2).gameObject.SetActive(false);
                    RefinedQ[2].transform.GetChild(3).gameObject.SetActive(true);
                }
                else
                {
                    if (RefinedQ[2].transform.GetChild(4).gameObject.activeSelf)
                    {
                        DialogText.text =
                            "Great, you find that sin2B is actually sin(B+B), so sin2B=2sinBcosB";
                        RefinedQ[2].transform.GetChild(4).gameObject.SetActive(false);
                        RefinedQ[2].transform.GetChild(5).gameObject.SetActive(true);
                    }
                    else
                    {
                        DialogText.text = "Seems not useful here";
                    }
                    
                }
                break;
            case 4:
                if (RefinedQ[2].transform.GetChild(2).gameObject.activeSelf)
                {
                    DialogText.text =
                        "Yes! Let X = A+B, Y=B, cos(A+2B) will be cos(X+Y), cosA will be cos(X-Y).\ncos(A+2B)+cosA = cos((A+B)+B)+cos((A+B)-B)=2cos(A+B)cosB";
                    RefinedQ[2].transform.GetChild(2).gameObject.SetActive(false);
                    RefinedQ[2].transform.GetChild(4).gameObject.SetActive(true);
                }
                else
                {
                    if (RefinedQ[2].transform.GetChild(3).gameObject.activeSelf)
                    {
                        DialogText.text =
                            "Yes! Let X = A+B, Y=B, cos(A+2B) will be cos(X+Y), cosA will be cos(X-Y).\ncos(A+2B)+cosA = cos((A+B)+B)+cos((A+B)-B)=2cos(A+B)cosB";
                        RefinedQ[2].transform.GetChild(3).gameObject.SetActive(false);
                        RefinedQ[2].transform.GetChild(5).gameObject.SetActive(true);
                    }
                    else
                    {
                        DialogText.text = "Seems not useful here";
                    }
                    
                }
                break;
            default:
                break;
        }

        if (combineFormulaUnlock)
        {
            if ((index == 1 && PreviousFormula == 2) || (index == 2 && PreviousFormula == 1))
            { 
                ToolPanel.transform.GetChild(4).gameObject.SetActive(true);
                NextDialog();
                DialogButton.gameObject.SetActive(true);
            }
        
        }

        if (RefinedQ[2].transform.GetChild(5).gameObject.activeSelf)
        {
            DialogButton.gameObject.SetActive(true);
        }
        PreviousFormula = index;
    }
}
