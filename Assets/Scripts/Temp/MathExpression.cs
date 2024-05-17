using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;

public class MathExpression : MonoBehaviour
{
    public static Dictionary<string, int> OperationPriority = new Dictionary<string, int>()
    {
        {"+",1},
        {"-",1},
        {"*",2},
        {"/",2},
    };
    // Start is called before the first frame update
    public bool isFormula { get; private set; } = false;
    public bool isDraggable { get; private set; } = false;
    public bool isParenthesized { get; private set; } = false;
    public string MathOperator { get; private set; } = "";
    public RectTransform[] OperatorObjects { get; private set; } = null;
    public string RawContent { get; private set; } = "";
    public MathExpression[] Terms { get; private set; }
    public MathExpression ParentTerm { get; private set; }
    public Image Render_Image { get; private set; }
    public Text ContentText { get; private set; }
    public RectTransform Rect { get; private set; }
    void Start()
    {
        if (gameObject.transform.parent.name == "Canvas")
        {
            StartCoroutine(Initialize("(a+b+c)*d=e*(e+d)/f"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator Initialize(string content, MathExpression parent = null)
    {
        Debug.Log("Start Initializing: " + content);
        RawContent = content;
        GameObject textObject = new GameObject($"TextOf{name}"); 
        textObject.transform.parent = transform;
        textObject.AddComponent<Text>();
        ContentText = textObject.GetComponent<Text>();
        
        gameObject.AddComponent<Image>();
        Render_Image = GetComponent<Image>();
        
        
        Rect = GetComponent<RectTransform>();
        ParentTerm = parent;
        while (content[0] == '(' && content.Last() == ')') {
            content = content.Substring(1, content.Length - 2);
            isParenthesized = true;
        }
        if (content.Contains("="))
        {
            Terms = new MathExpression[2];
            isDraggable = false;
            MathOperator = "=";
            yield return CreateChildTerm("LeftTerm", 0, content.Split("=")[0]);
            yield return CreateChildTerm("RightTerm", 1, content.Split("=")[1]);
        }
        else
        {
            int curr = 0;
            int parentheses = 0;
            List<int> tmpOPs = new List<int>();
            int minimumOPpriority = 1000;
            int OperatorCount = 0;
            while (curr < content.Length)
            {
                if (content[curr] == '(')
                {
                    // if (parentheses == 0)
                    // {
                    //     tempTerm = (curr,-1);
                    // }
                    parentheses++;
                }

                if (content[curr] == ')')
                {
                    parentheses--;
                    // if (parentheses == 0)
                    // {
                    //     tempTerm = (tempTerm.Item1,curr);
                    // }
                }

                if (OperationPriority.ContainsKey(content[curr].ToString()) && parentheses == 0)
                {
                    if (minimumOPpriority > OperationPriority[content[curr].ToString()])
                    {
                        minimumOPpriority = OperationPriority[content[curr].ToString()];
                        OperatorCount = 0;
                    }

                    if (minimumOPpriority == OperationPriority[content[curr].ToString()]) OperatorCount++;
                    tmpOPs.Add(curr);
                }
                curr++;
            }
            
            if (tmpOPs.Count != 0)
            {
                Terms = new MathExpression[OperatorCount + 1];
                string str = "";
                curr = 0;
                OperatorCount = 0;
                for (int i = 0; i < tmpOPs.Count; i++)
                {
                    if (tmpOPs[i] - curr > 0)
                    {
                        str += content.Substring(curr, tmpOPs[i] - curr);
                        if (OperationPriority[content[tmpOPs[i]].ToString()] == minimumOPpriority)
                        {
                            MathOperator += content[tmpOPs[i]];
                            yield return CreateChildTerm($"Term{OperatorCount}", OperatorCount, str);
                            OperatorCount++;
                            str = "";
                        }
                        else
                        {
                            str += content[tmpOPs[i]];
                        }
                        curr = tmpOPs[i] + 1;
                    }
                    else
                    {
                        Debug.LogError("No term between operators!");
                        continue;
                    }
                }

                if (content.Length == curr)
                {
                    Debug.LogError("No term after operators at the end!");
                }
                else
                {
                    str += content.Substring(curr, content.Length - curr);
                    yield return CreateChildTerm($"Term{OperatorCount}", OperatorCount, str);
                    OperatorCount++;
                }
            }
        }
        yield return RenderExpression();
        if (Terms != null)
        {
            string s = "";
            for (int i = 0 ;i < Terms.Length;i++)
            {
                if(Terms[i]!=null)s += Terms[i].RawContent+',';
            }
            Debug.Log(s+" | "+RawContent+" | "+Terms.Length+" | "+MathOperator);
        }
        
    }

    IEnumerator CreateChildTerm(string childName, int index, string content)
    {
        GameObject newTerm = new GameObject(childName);
        newTerm.AddComponent<MathExpression>();
        newTerm.transform.SetParent(transform);
        Terms[index] = newTerm.GetComponent<MathExpression>();
        yield return Terms[index].Initialize(content, this);
    }

    IEnumerator RenderExpression()
    {
        if (MathOperator.Length == 0)
        {
            ContentText.fontSize = 40;
            ContentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ContentText.text = RawContent;
            ContentText.color = Color.black;
            ContentText.gameObject.AddComponent<ContentSizeFitter>();
            ContentText.gameObject.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ContentText.gameObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            yield return null;
            Rect.sizeDelta = ContentText.GetComponent<RectTransform>().sizeDelta;
            yield return null;
        }
        else if (MathOperator == "=")
        {
            GameObject EqualObject = new GameObject($"Equal");
            EqualObject.transform.parent = transform;
            EqualObject.AddComponent<Image>();
            EqualObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Equal");
            EqualObject.GetComponent<RectTransform>().sizeDelta = new Vector2(40,13);
            OperatorObjects = new RectTransform[1];
            OperatorObjects[0] = EqualObject.GetComponent<RectTransform>();
            float totalWidth = Terms[0].Rect.rect.width + Terms[1].Rect.rect.width +
                                OperatorObjects[0].rect.width + 20;
            Terms[0].Rect.position = new Vector3 (Rect.position.x-totalWidth/2+Terms[0].Rect.rect.width/2,Rect.position.y,Rect.position.z);
            OperatorObjects[0].position= new Vector3 (Rect.position.x-totalWidth/2+Terms[0].Rect.rect.width+OperatorObjects[0].rect.width/2+10,Rect.position.y,Rect.position.z);
            Terms[1].Rect.position = new Vector3 (Rect.position.x+totalWidth/2-Terms[1].Rect.rect.width/2,Rect.position.y,Rect.position.z);
            Rect.sizeDelta = new Vector2(totalWidth, Mathf.Max(Terms[0].Rect.rect.height,Terms[1].Rect.rect.height)+20);
        }
        else if (OperationPriority[MathOperator[0].ToString()] == 1)
        {
            float totalWidth = 0;
            OperatorObjects = new RectTransform[MathOperator.Length];
            for (int i = 0; i < MathOperator.Length;i++)
            {
                if (MathOperator[i] == '+')
                {
                    GameObject PlusObject = new GameObject($"Plus");
                    PlusObject.transform.parent = transform;
                    PlusObject.AddComponent<Image>();
                    PlusObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Plus");
                    OperatorObjects[i] = PlusObject.GetComponent<RectTransform>();
                    OperatorObjects[i].sizeDelta = new Vector2(25, 25);
                }
                else
                {
                    GameObject MinusObject = new GameObject($"Minus");
                    MinusObject.transform.parent = transform;
                    MinusObject.AddComponent<Image>();
                    MinusObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Minus");
                    OperatorObjects[i] = MinusObject.GetComponent<RectTransform>();
                    OperatorObjects[i].sizeDelta = new Vector2(20, 6);
                }
                //Debug.Log(Terms[i]+"|"+i);
                totalWidth += OperatorObjects[i].rect.width + Terms[i].Rect.rect.width;
            }
            totalWidth += Terms.Last().Rect.rect.width + 20 * (MathOperator.Length+1);
            //Debug.Log(totalWidth);
            float prevPoint = Rect.position.x - totalWidth/2;
            float Xposition = prevPoint;
            float maxHeight = 0;
            for (int i = 0; i < MathOperator.Length;i++)
            {
                Xposition = prevPoint + 10 + Terms[i].Rect.rect.width / 2;
                Terms[i].Rect.position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
                if (Terms[i].Rect.rect.height > maxHeight) maxHeight = Terms[i].Rect.rect.height;
                prevPoint += 10 + Terms[i].Rect.rect.width;
                
                Xposition = prevPoint + 10 + OperatorObjects[i].GetComponent<RectTransform>().rect.width / 2;
                OperatorObjects[i].position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
                if (OperatorObjects[i].rect.height > maxHeight) maxHeight = OperatorObjects[i].rect.height;
                prevPoint += 10 + OperatorObjects[i].rect.width;
            }
            Xposition = prevPoint + 10 + Terms.Last().Rect.rect.width / 2;
            Terms.Last().Rect.position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
            Rect.sizeDelta = new Vector2(totalWidth, maxHeight+20);
        }
        else if (OperationPriority[MathOperator[0].ToString()] == 2)
        {
            OperatorObjects = new RectTransform[MathOperator.Length];
            if (MathOperator.Contains("/"))
            {
                List<int> Denominators = new List<int>();
                List<int> Numerators = new List<int>();
                float denominatorWidth = 0, numeratorWidth = 20 + Terms[0].Rect.rect.width, denominatorHeight = 0, numeratorHeight = Terms[0].Rect.rect.height;
                Numerators.Add(0);
                for (int i = 0; i < MathOperator.Length; i++)
                {
                    if (MathOperator[i] == '/')
                    {
                        Denominators.Add(i+1);
                        denominatorWidth += 10 + Terms[i + 1].Rect.rect.width;
                        if (denominatorHeight < Terms[i + 1].Rect.rect.height)
                            denominatorHeight = Terms[i + 1].Rect.rect.height;
                    }
                    else
                    {
                        Numerators.Add(i+1);
                        numeratorWidth += 10 + Terms[i + 1].Rect.rect.width;
                        if (numeratorHeight < Terms[i + 1].Rect.rect.height)
                            numeratorHeight = Terms[i + 1].Rect.rect.height;
                    }

                    if (i >= 1)
                    {
                        GameObject TimesObject = new GameObject($"Times");
                        TimesObject.transform.parent = transform;
                        TimesObject.AddComponent<Image>();
                        TimesObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Times");
                        OperatorObjects[i] = TimesObject.GetComponent<RectTransform>();
                        OperatorObjects[i].sizeDelta = new Vector2(25, 25);
                    }
                }
                int DenominatorCount = Denominators.Count;
                denominatorWidth += 40 * (DenominatorCount - 1);
                numeratorWidth += 40 * (Numerators.Count - 1);
                GameObject FLineObject = new GameObject($"FractionLine");
                FLineObject.transform.parent = transform;
                FLineObject.AddComponent<Image>();
                FLineObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Minus");
                OperatorObjects[0] = FLineObject.GetComponent<RectTransform>();
                OperatorObjects[0].sizeDelta = new Vector2(Mathf.Max(denominatorWidth,numeratorWidth), 6);
                OperatorObjects[0].position = Rect.position;
                float prevPoint = Rect.position.x - denominatorWidth/2;
                float Xposition = prevPoint;
                float Yposition = Rect.position.y;
                for (int i = 0; i < DenominatorCount; i++)
                {
                    Xposition = prevPoint + 10 + Terms[Denominators[i]].Rect.rect.width / 2;
                    Yposition = Rect.position.y - 10 - denominatorHeight / 2;
                    Terms[Denominators[i]].Rect.position= new Vector3 (Xposition, Yposition, Rect.position.z);
                    prevPoint += 10 + Terms[Denominators[i]].Rect.rect.width;

                    if (i < DenominatorCount-1)
                    {
                        Xposition = prevPoint + 10 + OperatorObjects[i+1].rect.width / 2;
                        OperatorObjects[i+1].position= new Vector3 (Xposition, Yposition, Rect.position.z);
                        prevPoint += 10 + OperatorObjects[i+1].rect.width;
                    }
                    
                }
                prevPoint = Rect.position.x - numeratorWidth/2;
                Xposition = prevPoint;
                Yposition = Rect.position.y;
                for (int i = 0; i < Numerators.Count; i++)
                {
                    Xposition = prevPoint + 10 + Terms[Numerators[i]].Rect.rect.width / 2;
                    Yposition = Rect.position.y + 10 + numeratorHeight / 2;
                    Terms[Numerators[i]].Rect.position= new Vector3 (Xposition, Yposition, Rect.position.z);
                    prevPoint += 10 + Terms[Numerators[i]].Rect.rect.width;

                    if (i < Numerators.Count - 1)
                    {
                        Xposition = prevPoint + 10 + OperatorObjects[i+DenominatorCount].rect.width / 2;
                        OperatorObjects[i+DenominatorCount].position= new Vector3 (Xposition, Yposition, Rect.position.z);
                        prevPoint += 10 + OperatorObjects[i+DenominatorCount].rect.width;
                    }
                }
                Rect.sizeDelta = new Vector2(Mathf.Max(denominatorWidth,numeratorWidth), 26 + denominatorHeight + numeratorHeight);
            }
            else
            {
                float totalWidth = Terms.Last().Rect.rect.width + 10;
                for (int i = 0; i < MathOperator.Length;i++)
                {
                    if (MathOperator[i] == '*')
                    {
                        GameObject TimesObject = new GameObject($"Times");
                        TimesObject.transform.parent = transform;
                        TimesObject.AddComponent<Image>();
                        TimesObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Times");
                        OperatorObjects[i] = TimesObject.GetComponent<RectTransform>();
                        OperatorObjects[i].sizeDelta = new Vector2(25, 25);
                    }
                    else
                    {
                        Debug.LogError("Invalid operator detected in multiplication!");
                    }
                    //Debug.Log(Terms[i]+"|"+i);
                    totalWidth += 20 + OperatorObjects[i].rect.width + Terms[i].Rect.rect.width;
                }
                float prevPoint = Rect.position.x - totalWidth/2;
                float Xposition = prevPoint;
                float maxHeight = 0;
                for (int i = 0; i < MathOperator.Length;i++)
                {
                    Xposition = prevPoint + 10 + Terms[i].Rect.rect.width / 2;
                    Terms[i].Rect.position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
                    if (Terms[i].Rect.rect.height > maxHeight) maxHeight = Terms[i].Rect.rect.height;
                    prevPoint += 10 + Terms[i].Rect.rect.width;
                
                    Xposition = prevPoint + 10 + OperatorObjects[i].GetComponent<RectTransform>().rect.width / 2;
                    OperatorObjects[i].position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
                    if (OperatorObjects[i].rect.height > maxHeight) maxHeight = OperatorObjects[i].rect.height;
                    prevPoint += 10 + OperatorObjects[i].rect.width;
                }
                Xposition = prevPoint + 10 + Terms.Last().Rect.rect.width / 2;
                Terms.Last().Rect.position= new Vector3 (Xposition, Rect.position.y, Rect.position.z);
                Rect.sizeDelta = new Vector2(totalWidth, maxHeight+20);
            }
        }

        if (isParenthesized)
        {
            
        }
    }
    
}
