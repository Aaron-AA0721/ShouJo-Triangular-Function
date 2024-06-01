using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MathExpression : DragAndDrop
{
    public static Dictionary<string, int> OperationPriority = new Dictionary<string, int>()
    {
        {"+",1},
        {"-",1},
        {"*",2},
        {"/",2},
    };
    [field: Header("MathExpression Setting")]
    [field: SerializeField] public string Enclosed { get; private set; }= "None";
    [field: SerializeField] public string MathOperator{ get; private set; } = "";
    [field: SerializeField] public RectTransform[] EncloseSymbols { get; private set; } = null;
    [field: SerializeField] public RectTransform[] NegativeSymbol { get; private set; } = null;
    [field: SerializeField] public RectTransform[] OperatorObjects { get; private set; } = null;
    [field: SerializeField] public string RawContent{ get; private set; } = "";
    [field: SerializeField]public bool isNegative { get; private set; }= false;
    public string InitialRawContent = "";
    [field: SerializeField] public MathExpression[] Terms { get; private set; } = null;
    [field: SerializeField] public MathExpression ParentTerm { get; private set; } = null;
    [field: SerializeField] public int ParentIndex { get; private set; } = -1;
    [field: SerializeField] public Text ContentText { get; private set; } = null;
    void Start()
    {
        if (InitialRawContent != "")
        {
            InitializebyString(InitialRawContent);
            StartCoroutine(RenderExpression());
        }
    }

    public void StartRenderExpression()
    {
        StartCoroutine(RenderExpression());
    }
    // Update is called once per frame
    void Update()
    {
        if (ParentTerm != null)
        {
            if (ParentTerm.Terms == null) 
                Destroy(this.gameObject);
            else if(ParentIndex >= ParentTerm.Terms.Length || ParentTerm.Terms[ParentIndex] != this)
                    Destroy(this.gameObject);
        }
    }

    public void CheckComponents()
    {
        if (ContentText == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                //Debug.Log(name+"|"+transform.GetChild(i).gameObject.name+"|"+transform.GetChild(i).gameObject.GetComponent<Text>());
                if(transform.GetChild(i).gameObject.GetComponent<Text>()!=null)ContentText = transform.GetChild(i).gameObject.GetComponent<Text>(); 
            }
            if (ContentText == null)
            { 
                GameObject textObject = new GameObject($"TextOf{name}");
                textObject.transform.parent = transform;
                textObject.transform.position = transform.position;
                textObject.AddComponent<Text>();
                ContentText = textObject.GetComponent<Text>(); 
            }
           
        }
        if(GetComponent<Image>()==null)gameObject.AddComponent<Image>();
        Render_Image = GetComponent<Image>();
        Rect = GetComponent<RectTransform>();
    }

    public void UpdateRawContent()
    {
        if (Terms == null) return;
        RawContent = "";
        for (int i = 0; i < MathOperator.Length; i++)
        {
            if (Terms[i].isNegative) RawContent += "(-";
            switch (Terms[i].Enclosed)
            {
                case "None":
                    RawContent += Terms[i].RawContent;
                    break;
                case "Parenthesized":
                    RawContent += '(' + Terms[i].RawContent + ')' ;
                    break;
            }

            if (Terms[i].isNegative) RawContent += ")";
                RawContent += MathOperator[i];
        }
        if (Terms.Last().isNegative) RawContent += "(-";
        switch (Terms.Last().Enclosed)
        {
            case "None":
                RawContent += Terms.Last().RawContent;
                break;
            case "Parenthesized":
                RawContent += '(' + Terms.Last().RawContent +')';
                break;
        }
        if (Terms.Last().isNegative) RawContent += ")";
        //if(ParentTerm!=null)ParentTerm.UpdateRawContent();
    }
    public void InitializebyValues(string OperatorValue, MathExpression[] TermsValue, MathExpression parent = null, int parentIndex = -1)
    {
        MathOperator = OperatorValue;
        // if (Terms != null)
        // {
        //     for (int i = 0; i < Terms.Length; i++)
        //     {
        //         Destroy(Terms[i].gameObject);
        //     }
        // }
        //Debug.Log(OperatorValue + "|" + name+".."+isNegative);
        Terms = new MathExpression[TermsValue.Length];
        TermsValue.CopyTo(Terms,0);
        for (int i = 0; i < MathOperator.Length; i++)
        {
            Terms[i].ParentTerm = this;
            Terms[i].ParentIndex = i;
            Terms[i].transform.SetParent(transform);
            //Debug.Log(Terms[i].name+Terms[i].RawContent+Terms[i].ParentTerm.name);
        }
        Terms.Last().ParentTerm = this;
        Terms.Last().ParentIndex = Terms.Length - 1;
        Terms.Last().transform.SetParent(transform);
        //Debug.Log(Terms.Last().name+Terms.Last().RawContent+Terms.Last().ParentTerm.name);
        ParentTerm = parent;
        ParentIndex = parentIndex;
        UpdateRawContent();
    }
    public void InitializebyString(string content, MathExpression parent = null, int parentIndex = -1)
    {
        //Debug.Log("Start Initializing: " + content);
        RawContent = content;
        CheckComponents();
        ParentTerm = parent;
        ParentIndex = parentIndex;
        MathOperator = "";
        Terms = null;
        // if (Terms != null)
        // {
        //     for (int i = 0; i < Terms.Length; i++)
        //     {
        //         if(Terms[i]!=null)Destroy(Terms[i].gameObject);
        //     }
        // }
        if (content.Length == 0)
        {
            Debug.LogError("empty content in initialization!!!"+name);
        }

        if (content.Contains("="))
        {
            Terms = new MathExpression[2];
            MathOperator = "=";
            CreateChildTerm("LeftTerm", 0, content.Split("=")[0]);
            CreateChildTerm("RightTerm", 1, content.Split("=")[1]);
        }
        else{
            bool firstNegative = content[0] == '-';
            if(firstNegative)content = content.Substring(1);
            //Debug.Log($"parathesized = {Enclosed}, negative = {firstNegative},content = {content}");
            while (content[0] == '(' && content.Last() == ')')
            {
                int ParentheseNum = 0;
                bool Parenthesized = true;
                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i] == '(') ParentheseNum++;
                    if (content[i] == ')') ParentheseNum--;
                    if (ParentheseNum == 0 && i != content.Length - 1) {Parenthesized = false;break;}
                }

                if (Parenthesized)
                {
                    content = content.Substring(1, content.Length - 2);
                    Enclosed = "Parenthesized";
                    if (firstNegative)
                    {
                        isNegative = true;
                        firstNegative = false;
                    }
                }
                else break;
            }

            if (content[0] == '-')
            {
                firstNegative = true;
                Enclosed = "None";
                content = content.Substring(1);
            }
            //Debug.Log($"parathesized = {Enclosed}, negative = {firstNegative},content = {content}");
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
            curr = 0;
            
            if (tmpOPs.Count != 0)
            {
                Terms = new MathExpression[OperatorCount + 1 ];
                //MathOperator = "";
                string str = "";
                OperatorCount = 0;
                
                for (int i = 0; i < tmpOPs.Count; i++)
                {
                    if (tmpOPs[i] - curr > 0)
                    {
                        str += content.Substring(curr, tmpOPs[i] - curr);
                        if (OperationPriority[content[tmpOPs[i]].ToString()] == minimumOPpriority)
                        {
                            MathOperator += content[tmpOPs[i]];
                            //Debug.Log(MathOperator+"|"+name);
                            
                            if (OperatorCount == 0 && firstNegative)
                            {
                                str = '-' + str;
                            }
                            CreateChildTerm($"Term{OperatorCount}", OperatorCount, str);
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
                        Debug.LogError("No term between operators!" +curr +"|"+tmpOPs[i]);
                        curr++;
                        continue;
                    }
                }
                //Debug.Log(MathOperator);
                if (content.Length == curr)
                {
                    Debug.LogError("No term after operators at the end!");
                }
                else
                {
                    str += content.Substring(curr, content.Length - curr);
                    CreateChildTerm($"Term{OperatorCount}", OperatorCount, str);
                    OperatorCount++;
                }
            }
            else
            {
                Terms = null;
                RawContent = content;
                if (firstNegative) isNegative = true;
                //Debug.Log(name+"| no tmpOPs");
                //if(ParentTerm == null)StartCoroutine(RenderExpression());
            }
        }
        // if (Terms != null)
        // {
        //     string s = "";
        //     for (int i = 0 ;i < Terms.Length;i++)
        //     {
        //         if(Terms[i]!=null)s += Terms[i].RawContent+',';
        //     }
        //     Debug.Log(s+" | "+RawContent+" | "+Terms.Length+" | "+MathOperator);
        // }
        
    }
    IEnumerator RenderExpression()
    {
        if (Terms == null)
        {
            if(ContentText == null)Debug.Log(name);
            ContentText.fontSize = 40;
            ContentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            //ContentText.fontStyle = FontStyle.Italic;
            ContentText.text = RawContent;
            ContentText.color = Color.black;
            ContentText.gameObject.AddComponent<ContentSizeFitter>();
            ContentText.gameObject.GetComponent<ContentSizeFitter>().horizontalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            ContentText.gameObject.GetComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            yield return null;
            if (ContentText != null)
            {
                Rect.sizeDelta = ContentText.GetComponent<RectTransform>().sizeDelta;
                ContentText.GetComponent<RectTransform>().position = Rect.position;
            }
            yield return null;
        }
        else
        {
            
            if (Terms != null)
            {
                for (int i = 0; i < Terms.Length; i++)
                {
                    if(Terms[i] != null) yield return Terms[i].RenderExpression();
                    //else Debug.Log("empty term | "+name);
                }
            }
            if (OperatorObjects != null)
            {
                for (int i = 0; i < OperatorObjects.Length; i++)
                    Destroy(OperatorObjects[i].gameObject);
            }
            if (MathOperator == "=")
            {
                
                OperatorObjects = new RectTransform[1];
                GameObject EqualObject = new GameObject($"Equal");
                EqualObject.transform.parent = transform;
                EqualObject.AddComponent<Image>();
                EqualObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Equal");
                EqualObject.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 13);
                OperatorObjects[0] = EqualObject.GetComponent<RectTransform>();
                
                
                float totalWidth = Terms[0].Rect.rect.width + Terms[1].Rect.rect.width +
                                   OperatorObjects[0].rect.width + 20;
                Terms[0].Rect.localPosition = new Vector3( - totalWidth / 2 + Terms[0].Rect.rect.width / 2,
                    0, 0);
                OperatorObjects[0].localPosition =
                    new Vector3(
                        - totalWidth / 2 + Terms[0].Rect.rect.width +
                        OperatorObjects[0].rect.width / 2 + 10, 0, 0);
                Terms[1].Rect.localPosition = new Vector3(totalWidth / 2 - Terms[1].Rect.rect.width / 2,
                    0, 0);
                Rect.sizeDelta = new Vector2(totalWidth + 40,
                    Mathf.Max(Terms[0].Rect.rect.height, Terms[1].Rect.rect.height) + 20);
            }
            else if (OperationPriority[MathOperator[0].ToString()] == 1)
            {
                float totalWidth = 10;
                OperatorObjects = new RectTransform[MathOperator.Length];
                for (int i = 0; i < MathOperator.Length; i++)
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
                        OperatorObjects[i].sizeDelta = new Vector2(20, 5);
                    }
                    totalWidth += OperatorObjects[i].rect.width + Terms[i].Rect.rect.width;
                }

                totalWidth += Terms.Last().Rect.rect.width + 20 * (MathOperator.Length + 1) - 10;
                //Debug.Log(totalWidth);
                float prevPoint = - totalWidth / 2 ;
                float Xposition = prevPoint;
                float maxHeight = 0;
                for (int i = 0; i < MathOperator.Length; i++)
                {
                    Xposition = prevPoint + 10 + Terms[i].Rect.rect.width / 2;
                    Terms[i].Rect.localPosition = new Vector3(Xposition, 0, 0);
                    if (Terms[i].Rect.rect.height > maxHeight) maxHeight = Terms[i].Rect.rect.height;
                    prevPoint += 10 + Terms[i].Rect.rect.width;

                    Xposition = prevPoint + 10 + OperatorObjects[i].GetComponent<RectTransform>().rect.width / 2;
                    OperatorObjects[i].localPosition = new Vector3(Xposition, 0, 0);
                    if (OperatorObjects[i].rect.height > maxHeight) maxHeight = OperatorObjects[i].rect.height;
                    prevPoint += 10 + OperatorObjects[i].rect.width;
                }

                Xposition = prevPoint + 10 + Terms.Last().Rect.rect.width / 2;
                Terms.Last().Rect.localPosition = new Vector3(Xposition, 0,0);
                if (Terms.Last().Rect.rect.height > maxHeight) maxHeight = Terms.Last().Rect.rect.height;
                Rect.sizeDelta = new Vector2(totalWidth, maxHeight + 20);
            }
            else if (OperationPriority[MathOperator[0].ToString()] == 2)
            {
                if (OperatorObjects != null)
                {
                    for (int i = 0; i < OperatorObjects.Length; i++)
                        Destroy(OperatorObjects[i].gameObject);
                }
                OperatorObjects = new RectTransform[MathOperator.Length];
                if (MathOperator.Contains("/"))
                {
                    List<int> Denominators = new List<int>();
                    List<int> Numerators = new List<int>();
                    float denominatorWidth = 10,
                        numeratorWidth = 20 + Terms[0].Rect.rect.width,
                        denominatorHeight = 0,
                        numeratorHeight = Terms[0].Rect.rect.height;
                    Numerators.Add(0);
                    for (int i = 0; i < MathOperator.Length; i++)
                    {
                        if (MathOperator[i] == '/')
                        {
                            Denominators.Add(i + 1);
                            denominatorWidth += 10 + Terms[i + 1].Rect.rect.width;
                            if (denominatorHeight < Terms[i + 1].Rect.rect.height)
                                denominatorHeight = Terms[i + 1].Rect.rect.height;
                        }
                        else
                        {
                            Numerators.Add(i + 1);
                            //Debug.Log(name+"|"+MathOperator+"|"+Terms.Length);
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
                    denominatorWidth += 35 * (DenominatorCount - 1);
                    numeratorWidth += 35 * (Numerators.Count - 1);
                    GameObject FLineObject = new GameObject($"FractionLine");
                    FLineObject.transform.parent = transform;
                    FLineObject.AddComponent<Image>();
                    FLineObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Minus");
                    OperatorObjects[0] = FLineObject.GetComponent<RectTransform>();
                    OperatorObjects[0].sizeDelta = new Vector2(Mathf.Max(denominatorWidth, numeratorWidth), 6);
                    OperatorObjects[0].localPosition = new Vector3(0,(denominatorHeight-numeratorHeight)/2,0);
                    denominatorWidth += 20;
                    numeratorWidth += 20;
                    float prevPoint = 10- denominatorWidth / 2;
                    float Xposition;
                    float Yposition = OperatorObjects[0].localPosition.y  - 5 - denominatorHeight / 2;
                    for (int i = 0; i < DenominatorCount; i++)
                    {
                        Xposition = prevPoint + 10 + Terms[Denominators[i]].Rect.rect.width / 2;
                        Terms[Denominators[i]].Rect.localPosition = new Vector3(Xposition, Yposition, 0);
                        prevPoint += 10 + Terms[Denominators[i]].Rect.rect.width;

                        if (i < DenominatorCount - 1)
                        {
                            Xposition = prevPoint + 10 + OperatorObjects[i + 1].rect.width / 2;
                            OperatorObjects[i + 1].localPosition = new Vector3(Xposition, Yposition, 0);
                            prevPoint += 10 + OperatorObjects[i + 1].rect.width;
                        }

                    }

                    prevPoint = 10 - numeratorWidth / 2 ;
                    Yposition = OperatorObjects[0].localPosition.y + 5 + numeratorHeight / 2;
                    for (int i = 0; i < Numerators.Count; i++)
                    {
                        Xposition = prevPoint + 10 + Terms[Numerators[i]].Rect.rect.width / 2;
                        Terms[Numerators[i]].Rect.localPosition = new Vector3(Xposition, Yposition, 0);
                        prevPoint += 10 + Terms[Numerators[i]].Rect.rect.width;

                        if (i < Numerators.Count - 1)
                        {
                            Xposition = prevPoint + 10 + OperatorObjects[i + DenominatorCount].rect.width / 2;
                            OperatorObjects[i + DenominatorCount].localPosition =
                                new Vector3(Xposition, Yposition, 0);
                            prevPoint += 10 + OperatorObjects[i + DenominatorCount].rect.width;
                        }
                    }

                    Rect.sizeDelta = new Vector2(Mathf.Max(denominatorWidth, numeratorWidth),denominatorHeight + numeratorHeight + 16);
                }
                else
                {
                    float totalWidth = Terms.Last().Rect.rect.width + 10;
                    for (int i = 0; i < MathOperator.Length; i++)
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

                    float prevPoint = Rect.position.x - totalWidth / 2;
                    float Xposition = prevPoint;
                    float maxHeight = 0;
                    for (int i = 0; i < MathOperator.Length; i++)
                    {
                        Xposition = prevPoint + 10 + Terms[i].Rect.rect.width / 2;
                        Terms[i].Rect.position = new Vector3(Xposition, Rect.position.y, Rect.position.z);
                        if (Terms[i].Rect.rect.height > maxHeight) maxHeight = Terms[i].Rect.rect.height;
                        prevPoint += 10 + Terms[i].Rect.rect.width;

                        Xposition = prevPoint + 10 + OperatorObjects[i].GetComponent<RectTransform>().rect.width / 2;
                        OperatorObjects[i].position = new Vector3(Xposition, Rect.position.y, Rect.position.z);
                        if (OperatorObjects[i].rect.height > maxHeight) maxHeight = OperatorObjects[i].rect.height;
                        prevPoint += 10 + OperatorObjects[i].rect.width;
                    }

                    Xposition = prevPoint + 10 + Terms.Last().Rect.rect.width / 2;
                    Terms.Last().Rect.position = new Vector3(Xposition, Rect.position.y, Rect.position.z);
                    Rect.sizeDelta = new Vector2(totalWidth, maxHeight + 20);
                }
            }
        }
        if (ParentTerm != null && MathOperator.Length>0 && OperationPriority.ContainsKey(ParentTerm.MathOperator[0].ToString()) && OperationPriority.ContainsKey(MathOperator[0].ToString()))
        {
            if (OperationPriority[ParentTerm.MathOperator[0].ToString()] >
                OperationPriority[MathOperator[0].ToString()])
            {
                Enclosed = "Parenthesized";
            }
            else
            {
                Enclosed = "None";
            }
        }

        if (ParentTerm == null || ParentTerm.MathOperator == "=")
        {
            Enclosed = "None";
        }
        if (isNegative && MathOperator.Length > 0)
        {
            Enclosed = "Parenthesized";
        }
        if (EncloseSymbols != null)
        {
            for(int i=0;i<EncloseSymbols.Length;i++)
                if (EncloseSymbols[i] != null)
                {
                    Destroy(EncloseSymbols[i].gameObject);
                }
        }
        if (NegativeSymbol != null)
        {
            for(int i=0;i<NegativeSymbol.Length;i++)
                if(NegativeSymbol[i]!=null)
                    Destroy(NegativeSymbol[i].gameObject);
        }
        switch (Enclosed)
            {
                case "Parenthesized":
                    if (MathOperator.Length == 0) break;
                    EncloseSymbols = new RectTransform[2];
                    GameObject LeftParenthese = new GameObject($"LeftParenthese");
                    LeftParenthese.transform.parent = transform;
                    LeftParenthese.AddComponent<Image>();
                    LeftParenthese.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/LeftParenthese");
                    EncloseSymbols[0] = LeftParenthese.GetComponent<RectTransform>();
                    EncloseSymbols[0].sizeDelta = new Vector2(20, Rect.rect.height);
                    EncloseSymbols[0].position =
                        new Vector3(Rect.position.x - Rect.rect.width / 2 - 10, Rect.position.y, Rect.position.z);
                    GameObject RightParenthese = new GameObject($"RightParenthese");
                    RightParenthese.transform.parent = transform;
                    RightParenthese.AddComponent<Image>();
                    RightParenthese.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/RightParenthese");
                    EncloseSymbols[1] = RightParenthese.GetComponent<RectTransform>();
                    EncloseSymbols[1].sizeDelta = new Vector2(20, Rect.rect.height);
                    EncloseSymbols[1].position =
                        new Vector3(Rect.position.x + Rect.rect.width / 2 + 10, Rect.position.y, Rect.position.z);
                    Rect.sizeDelta = new Vector2(Rect.rect.width + 40, Rect.rect.height);
                    break;
                default:
                    break;
            }
        if (isNegative)
        {
            if (ParentTerm && ParentTerm.MathOperator!="=" && ParentIndex!=0)
            {
                NegativeSymbol = new RectTransform[3];
            }
            else NegativeSymbol = new RectTransform[1];
            GameObject MinusObject = new GameObject($"Minus");
            MinusObject.transform.parent = transform;
            MinusObject.AddComponent<Image>();
            MinusObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/Minus");
            NegativeSymbol[0] = MinusObject.GetComponent<RectTransform>();
            NegativeSymbol[0].sizeDelta = new Vector2(25, 5);
            NegativeSymbol[0].localPosition = 
                new Vector3(- Rect.rect.width / 2 - 10, 0, 0);
            Rect.sizeDelta = new Vector2(Rect.rect.width + 40, Rect.rect.height);
            ContentText.GetComponent<RectTransform>().position += new Vector3(15, 0, 0);
            if (OperatorObjects != null)
            {
                foreach (var opObj in OperatorObjects)
                {
                    opObj.position += new Vector3(15, 0, 0);
                }

                foreach (var t in Terms)
                {
                    t.Rect.position += new Vector3(15, 0, 0);
                }
            }

            if (EncloseSymbols != null)
            {
                foreach (var Obj in EncloseSymbols)
                {
                    Obj.position += new Vector3(15, 0, 0);
                }
            }
            if (NegativeSymbol.Length == 3)
            {
                GameObject LeftParenthese = new GameObject($"LeftParenthese");
                LeftParenthese.transform.parent = transform;
                LeftParenthese.AddComponent<Image>();
                LeftParenthese.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/LeftParenthese");
                NegativeSymbol[1] = LeftParenthese.GetComponent<RectTransform>();
                NegativeSymbol[1].sizeDelta = new Vector2(20, Rect.rect.height);
                NegativeSymbol[1].position =
                    new Vector3(Rect.position.x - Rect.rect.width / 2 - 10, Rect.position.y, Rect.position.z);
                GameObject RightParenthese = new GameObject($"RightParenthese");
                RightParenthese.transform.parent = transform;
                RightParenthese.AddComponent<Image>();
                RightParenthese.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Operators/RightParenthese");
                NegativeSymbol[2] = RightParenthese.GetComponent<RectTransform>();
                NegativeSymbol[2].sizeDelta = new Vector2(20, Rect.rect.height);
                NegativeSymbol[2].position =
                    new Vector3(Rect.position.x + Rect.rect.width / 2 + 10, Rect.position.y, Rect.position.z);
                Rect.sizeDelta = new Vector2(Rect.rect.width + 40, Rect.rect.height);
            }
        }
        UpdateRawContent();
    }

    public void PlusOperation(MathExpression newTerm)
    {
        addRegularOperation(newTerm, '+');
    }
    public void MinusOperation(MathExpression newTerm)
    {
        addRegularOperation(newTerm, '-');
    }
    public void MultiplyOperation(MathExpression newTerm)
    {
        addRegularOperation(newTerm, '*');
    }
    public void DivideOperation(MathExpression newTerm)
    {
        addRegularOperation(newTerm, '/');
    }
    public void addRegularOperation(MathExpression newTerm, char operatorChar, bool notCancel = false)
    {
        if (MathOperator == "=")
        {
            MathExpression Add = CreateCopy(newTerm);
            Terms[0].addRegularOperation(CreateCopy(newTerm),operatorChar);
            Terms[1].addRegularOperation(Add,operatorChar);
            StartCoroutine(RenderExpression());
        }
        else 
        {
            Debug.Log(ParentIndex+" "+operatorChar +" regular:, " +newTerm.RawContent +", "+newTerm.isNegative);
            GameObject newTermNode = new GameObject(name+operatorChar+newTerm.name);
            newTermNode.AddComponent<MathExpression>();
            newTermNode.transform.SetParent(transform.parent);
            MathExpression NewNodeMath = newTermNode.GetComponent<MathExpression>();
            NewNodeMath.CheckComponents();
            MathExpression[] tmpTerms = new MathExpression[2];
            tmpTerms[0] = this;
            tmpTerms[1] = newTerm; 
            if(ParentTerm != null) ParentTerm.Terms[ParentIndex] = NewNodeMath;
            NewNodeMath.InitializebyValues(operatorChar.ToString(),tmpTerms, ParentTerm, ParentIndex);
            NewNodeMath.MergeSameOperationInTerms();
            if(!notCancel)NewNodeMath.CancelAllSameTerms();
        }
    }
    
    public void MergeSameOperationInTerms()
    {
        if (MathOperator.Length == 0) return;
        if (MathOperator == "=")
        {
            Terms[0].MergeSameOperationInTerms();
            Terms[1].MergeSameOperationInTerms();
            return;
        }
        List<MathExpression> MergeTerms = new List<MathExpression>();
        int totalTermNum = Terms.Length;
        int OpPriority = OperationPriority[MathOperator[0].ToString()];
        string mergeOperators = (OpPriority == 1 ? '+':'*')+MathOperator;
        bool inverse = false;
        
        for (int i = 0; i < Terms.Length; i++)
        {
            if (Terms[i].MathOperator.Length > 0 && OperationPriority[Terms[i].MathOperator[0].ToString()] ==
                OpPriority)
            {
                MergeTerms.Add(Terms[i]);
                totalTermNum += Terms[i].Terms.Length - 1;
            }
        }
        int curr = 0;
        MathExpression[] newTerms = new MathExpression[totalTermNum];
        bool endNegative = false;
        for (int i = 0; i<Terms.Length;i++)
        {
            if (MergeTerms.Contains(Terms[i]))
            {
                endNegative ^= Terms[i].isNegative;
                inverse = false;
                if(curr > 0)inverse = MathOperator[i-1] == '-' || MathOperator[i-1] == '/';
                // if (Terms[i].isNegative && OpPriority == 1)
                // {
                //     inverse = !inverse;
                // }
                mergeOperators = mergeOperators.Insert(curr+1, InverseOperationString(Terms[i].MathOperator,inverse));
                inverse = Terms[i].isNegative && OpPriority == 1;
                mergeOperators = mergeOperators.Substring(0, curr) +
                                 InverseOperationString(
                                     mergeOperators.Substring(curr, Terms[i].MathOperator.Length + 1), inverse) +
                                 mergeOperators.Substring(curr + Terms[i].MathOperator.Length + 1);
                for (int j = 0; j < Terms[i].Terms.Length; j++)
                {
                    newTerms[curr] = Terms[i].Terms[j];
                    curr++;
                }
            }
            else
            {
                newTerms[curr] = Terms[i];
                curr++;
            }
        }
        InitializebyValues(mergeOperators.Substring(1), newTerms, ParentTerm, ParentIndex);
        if(OpPriority == 2)isNegative ^= endNegative;
        if (mergeOperators[0] == '-') Terms[0].isNegative = !Terms[0].isNegative;
        
        for (int i = 0; i < MergeTerms.Count;i++)
        {
            Destroy(MergeTerms[i].gameObject);
            //MergeTerms.RemoveAt(i);
        }
        //StartCoroutine(RenderExpression());
    }

    
    public void CancelAllSameTerms()
    {
        if (MathOperator.Length == 0) return;
        if (MathOperator == "=")
        {
            Terms[0].CancelAllSameTerms();
            Terms[1].CancelAllSameTerms();
            StartCoroutine(RenderExpression());
            return;
        }

        char PositiveOperator = OperationPriority[MathOperator[0].ToString()] == 1 ? '+' : '*';
        List<int> CancelTerms = new List<int>();
        List<int> ExistTerms = new List<int>();
        string tmpOpString = PositiveOperator + MathOperator;
        bool cancelAllTerms = false;
        bool EndNegative = false;
        bool EqualFound = false;
        for (int i = 0; i < Terms.Length; i++)
        {
            if (Terms[i].RawContent == "0" && PositiveOperator == '*')
            {
                cancelAllTerms = true;
                break;
            }
            if ( (Terms[i].RawContent == "1" && PositiveOperator == '*') || (Terms[i].RawContent == "0" && PositiveOperator == '+'))
            {  
                CancelTerms.Add(i);
            }
            else
            {
                EqualFound = false;
                foreach (var t in ExistTerms)
                {
                    if (PositiveOperator == '*')
                    {
                        if (tmpOpString[i] != tmpOpString[t])
                        {
                            if (Terms[t].IsTermEqualTo(Terms[i],false))
                            {
                                CancelTerms.Add(t);
                                CancelTerms.Add(i);
                                ExistTerms.Remove(t);
                                EqualFound = true;
                                break;
                            }
                            if (Terms[t].IsTermEqualTo(Terms[i],true))
                            {
                                CancelTerms.Add(t);
                                CancelTerms.Add(i);
                                ExistTerms.Remove(t);
                                EqualFound = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (Terms[t].IsTermEqualTo(Terms[i], tmpOpString[i] == tmpOpString[t]))
                        {
                            CancelTerms.Add(t);
                            CancelTerms.Add(i);
                            ExistTerms.Remove(t);
                            EqualFound = true;
                            break;
                        }
                    }
                    
                }
                if (!EqualFound)
                {
                    ExistTerms.Add(i);
                }
            }
        }

        
        if (cancelAllTerms)
        {
            InitializebyString("0",ParentTerm,ParentIndex);
            return;
        }
        
        MathExpression[] newTerms = new MathExpression[Terms.Length-CancelTerms.Count];
        //Debug.Log(tmpOpString + "|" + RawContent);
        int curr = 0;
        for (int i = 0; i < Terms.Length; i++)
        {
            if (CancelTerms.Contains(i))
            {
                EndNegative ^= Terms[i].isNegative;
                tmpOpString = tmpOpString.Remove(curr,1);
                continue;
            }
            newTerms[curr] = Terms[i];
            curr++;
        }
        if (tmpOpString.Length > 0)
        {
            if (tmpOpString[0] == '-')
            {
                if (tmpOpString.Contains('+'))
                {
                    int switchTerm = tmpOpString.IndexOf('+');
                    tmpOpString = '+' + tmpOpString.Substring(1, switchTerm-1)+'-'+ tmpOpString.Substring(switchTerm+1);
                    MathExpression tmp = newTerms[0];
                    newTerms[0] = newTerms[switchTerm];
                    newTerms[switchTerm] = tmp;
                }
                else
                {
                    tmpOpString = '+' + tmpOpString.Substring(1);
                    newTerms[0].isNegative = !newTerms[0].isNegative;
                }
            }
            if (tmpOpString[0] == '/')
            {
                if (tmpOpString.Contains('*'))
                {
                    int switchTerm = tmpOpString.IndexOf('*');
                    tmpOpString = '*' + tmpOpString.Substring(1, switchTerm-1)+'/'+ tmpOpString.Substring(switchTerm+1);
                    MathExpression tmp = newTerms[0];
                    newTerms[0] = newTerms[switchTerm];
                    newTerms[switchTerm] = tmp;
                }
                else
                {
                    tmpOpString = '*' + tmpOpString;
                    GameObject newTerm = new GameObject("1");
                    newTerm.AddComponent<MathExpression>();
                    newTerm.GetComponent<MathExpression>().InitializebyString("1");
                    MathExpression[] tmpTerms = new MathExpression[newTerms.Length];
                    newTerms.CopyTo(tmpTerms,0);
                    newTerms = new MathExpression[tmpTerms.Length+1];
                    newTerms[0]=newTerm.GetComponent<MathExpression>();
                    tmpTerms.CopyTo(newTerms,1);
                }
            }
        }
        if (newTerms.Length == 0)
        {
            InitializebyString(OperationPriority[PositiveOperator.ToString()] == 1 ? "0" : "1",ParentTerm,ParentIndex);
        }
        else if (newTerms.Length == 1)
        {
            if (ParentTerm != null)
            {
                ParentTerm.Terms[ParentIndex] = newTerms[0];
                newTerms[0].ParentTerm = ParentTerm;
                newTerms[0].ParentIndex = ParentIndex;
            }
            newTerms[0].transform.SetParent(transform.parent);
            if(PositiveOperator == '*')newTerms[0].isNegative = isNegative ^ EndNegative;
        }
        else
        {
            InitializebyValues(tmpOpString.Substring(1),newTerms,ParentTerm,ParentIndex);
        }
        if (PositiveOperator == '*') isNegative ^= EndNegative;
    }
    
    public void ExpandRegularOperationTerm(int AddTerm, int MultiplyTerm)
    {
        if (Terms.Length > AddTerm && Terms.Length > MultiplyTerm && AddTerm != MultiplyTerm)
        {
            if (OperationPriority[MathOperator[0].ToString()] == 1)
            {
                Debug.Log("plus, cannot expand");
                return;
            }
            
            if (!Terms[AddTerm].MathOperator.Contains('+') && !Terms[AddTerm].MathOperator.Contains('-'))
            {
                Debug.Log("AddTerm is not plus, cannot expand" + Terms[AddTerm].MathOperator);
                return;
            }
            string tmpOP = '*' + MathOperator;
            foreach (var childTerm in Terms[AddTerm].Terms)
            {
                childTerm.addRegularOperation(CreateCopy(Terms[MultiplyTerm]),tmpOP[AddTerm] == tmpOP[MultiplyTerm]?'*':'/');
            }
            addRegularOperation(CreateCopy(Terms[MultiplyTerm]),InverseOperationChar(tmpOP[MultiplyTerm],true));
            MathExpression root = this;
            while (root.ParentTerm != null)
            {
                root = root.ParentTerm;
            }
            root.StartRenderExpression();
        }
    }
    public void CombineLikeTerms(MathExpression LikeTerm){
        
    }

    public void EquationSubstitude(MathExpression newEQ)
    {
        if (newEQ.MathOperator != "=")
        {
            Debug.LogError("Not a equation. Cannot Substitude");
            return;
        }
        MathExpression origin = this;
        int ContainFlag = origin.IsTermContaining(newEQ.Terms[0]);
        while (ContainFlag==0 && origin.ParentTerm!=null)
        {
            origin = origin.ParentTerm;
            ContainFlag = origin.IsTermContaining(newEQ.Terms[0]);
        }
        if(ContainFlag == 0)
        {
            Debug.Log("Not Containing, Equation Substitude failed");
            return;
        }
        bool PlusFlag = origin.MathOperator.Length == 0 || OperationPriority[origin.MathOperator[0].ToString()] == 1;
        MathExpression OP = origin.ParentTerm;
        int OPIndex = origin.ParentIndex;
        if (OP)
        {
            Debug.Log("start operation1, contailFlag = "+ContainFlag + ", origin = "+OP.Terms[OPIndex].RawContent+"|"+OP.Terms[OPIndex].name);
            OP.Terms[OPIndex].addRegularOperation(CreateCopy(newEQ.Terms[0]),InverseOperationChar(PlusFlag?'+':'*',ContainFlag == 1),true);
            Debug.Log("start operation2, contailFlag = "+ContainFlag + ", origin = "+OP.Terms[OPIndex].RawContent+"|"+OP.Terms[OPIndex].name);
            OP.Terms[OPIndex].addRegularOperation(CreateCopy(newEQ.Terms[1]),InverseOperationChar(PlusFlag?'+':'*',ContainFlag == -1),true);
            OP.Terms[OPIndex].CancelAllSameTerms();
        }
        else
        {
            
        }
        // if (origin.ParentTerm != null)
        // {
        //     origin.ParentTerm.MergeSameOperationInTerms();
        //     origin.ParentTerm.CancelAllSameTerms();
        // }
        // origin.CancelAllSameTerms();
        
        while (origin.ParentTerm!=null)
        {
            origin = origin.ParentTerm;
        }
        origin.StartRenderExpression();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);
        //Debug.Log(eventData.pointerDrag.name);
        if (eventData.pointerDrag != null)
        {
            GameObject dragObject = eventData.pointerDrag;
            if (dragObject.GetComponent<MathExpression>() != null)
            {
                MathExpression termToBeOperated = dragObject.GetComponent<MathExpression>();
                MathExpression termDropOn = this;
                if (termDropOn == termToBeOperated) return;
                bool PlusFlag;
                if (termToBeOperated.MathOperator == "=")
                {
                    termDropOn.EquationSubstitude(termToBeOperated);
                }
                else
                {
                    PlusFlag = MathOperator.Length == 0 || OperationPriority[MathOperator[0].ToString()] == 1;
                    while (termDropOn.ParentTerm != null)
                    {
                        if (termDropOn.ParentTerm.MathOperator == "=") break;
                        if (termDropOn.ParentTerm == termToBeOperated.ParentTerm || termToBeOperated.ParentTerm == termDropOn) break;
                        termDropOn = termDropOn.ParentTerm;
                    }
                    if (termDropOn.ParentTerm == null) return;
                    if (termDropOn == termToBeOperated || termDropOn == termToBeOperated.ParentTerm) return;
                    if (termDropOn.ParentTerm == termToBeOperated.ParentTerm && termDropOn.ParentTerm.MathOperator != "=")
                    {
                        termDropOn.ParentTerm.ExpandRegularOperationTerm(termDropOn.ParentIndex,termToBeOperated.ParentIndex);
                        return;
                    }
                    if (termToBeOperated.ParentTerm.MathOperator != "=")
                    {
                        if (termToBeOperated.ParentTerm.ParentTerm != null &&
                            termToBeOperated.ParentTerm.ParentTerm.MathOperator == "=")
                        {
                            if (termDropOn.ParentTerm.MathOperator == "=")
                            {
                                MathExpression Eq = termToBeOperated.ParentTerm;
                                bool inverse = true;
                                if(OperationPriority[termToBeOperated.ParentTerm.MathOperator[0].ToString()] == 1) 
                                    if(termToBeOperated.ParentTerm.isNegative) inverse = false;
                                
                                if (termToBeOperated.ParentIndex == 0)
                                {
                                    termDropOn.ParentTerm.addRegularOperation(termToBeOperated,
                                        InverseOperationChar(OperationPriority[termToBeOperated.ParentTerm.MathOperator[0].ToString()] == 1 ? '-' : '/',
                                            !inverse));
                                }
                                else
                                {
                                    termDropOn.ParentTerm.addRegularOperation(termToBeOperated,
                                        InverseOperationChar(termToBeOperated.ParentTerm.MathOperator[termToBeOperated.ParentIndex - 1],
                                            inverse));
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        MathExpression Eq = termToBeOperated.ParentTerm;
                        Eq.addRegularOperation(termToBeOperated, Random.Range(0,1f)>0.5f?'-':'/');
                        
                    }
                }
            }
        }
    }
    void CreateChildTerm(string childName, int index, string content)
    {
        GameObject newTerm = new GameObject(childName);
        newTerm.AddComponent<MathExpression>();
        newTerm.transform.SetParent(transform);
        Terms[index] = newTerm.GetComponent<MathExpression>();
        Terms[index].InitializebyString(content, this, index);
    }
    public MathExpression CreateCopy(MathExpression newTerm)
    {
        GameObject newTermCopy = new GameObject(newTerm.RawContent+"(clone)");
        //GameObject newTermCopy = Instantiate(newTerm.gameObject);
        newTermCopy.AddComponent<MathExpression>();
        newTermCopy.GetComponent<MathExpression>().InitializebyString(newTerm.RawContent);
        newTermCopy.GetComponent<MathExpression>().isNegative = newTerm.isNegative;
        return newTermCopy.GetComponent<MathExpression>();
    }
    string InverseOperationString(string OperatorString, bool inverse = false)
    {
        if (!inverse) return OperatorString;
        string InverseString = OperatorString;
        for (int i = 0; i < OperatorString.Length; i++)
        {
            switch (OperatorString[i])
            {
                case '+':
                    InverseString = InverseString.Insert(i+1,"-");
                    InverseString = InverseString.Remove(i,1);
                    break;
                case '-':
                    InverseString = InverseString.Insert(i+1,"+");
                    InverseString = InverseString.Remove(i,1);
                    break;
                case '*':
                    InverseString = InverseString.Insert(i+1,"/");
                    InverseString = InverseString.Remove(i,1);
                    break;
                case '/':
                    InverseString = InverseString.Insert(i+1,"*");
                    InverseString = InverseString.Remove(i,1);
                    break;
            }
        }
        //Debug.Log(OperatorString+"->"+InverseString);
        return InverseString;
    }
    char InverseOperationChar(char OperatorChar, bool inverse = false)
    {
        if (!inverse) return OperatorChar;

        switch (OperatorChar)
        {
            case '+':
                return '-';
            case '-':
                return '+';
            case '*':
                return '/';
            case '/':
                return '*';
        }
        return '+';
    }
    bool IsTermEqualTo(MathExpression termToBeCompared, bool ParentNegative = false, bool inverseOP = false)
    {
        //Debug.Log("Comparing: "+(termToBeCompared.isNegative?'-':'+')+termToBeCompared.RawContent+","+(isNegative?'-':'+')+RawContent+","+ParentNegative);
        if (MathOperator.Length != termToBeCompared.MathOperator.Length) return false;
        if (MathOperator.Length == 0)
            return RawContent == termToBeCompared.RawContent && !isNegative^ParentNegative^termToBeCompared.isNegative;
        bool EqualFlag = true;
        List<int> UsedTerms = new List<int>();
        bool PlusFlag = OperationPriority[MathOperator[0].ToString()] == 1;
        bool FoundMatch = false;
        bool PN = ParentNegative;
        for (int i = 0; i < termToBeCompared.Terms.Length; i++)
        {
            FoundMatch = false;
            for (int j = 0; j < Terms.Length; j++)
            {
                if (PlusFlag)
                {
                    PN = (j == 0? '+':MathOperator[j-1])!=(i == 0? '+':termToBeCompared.MathOperator[i-1]) ^isNegative^termToBeCompared.isNegative^ParentNegative^inverseOP ;
                    if (!UsedTerms.Contains(j) && Terms[j].IsTermEqualTo(termToBeCompared.Terms[i],PN))
                    {
                        UsedTerms.Add(j);
                        //Debug.Log(Terms[j].RawContent +"="+termToBeCompared.Terms[i].RawContent+","+PN);
                        FoundMatch = true;
                        break;
                    }
                }
                else
                {
                    if (!UsedTerms.Contains(j) && inverseOP^(j == 0? '*':MathOperator[j-1])==(i == 0? '*':termToBeCompared.MathOperator[i-1]))
                    {
                        if(Terms[j].IsTermEqualTo(termToBeCompared.Terms[i],true))
                        {
                            UsedTerms.Add(j);
                            //Debug.Log(Terms[j].RawContent +"="+termToBeCompared.Terms[i].RawContent+",T");
                            FoundMatch = true;
                            ParentNegative ^= true;
                            break;
                        }
                        if(Terms[j].IsTermEqualTo(termToBeCompared.Terms[i],false))
                        {
                            UsedTerms.Add(j);
                            FoundMatch = true;
                            //Debug.Log(Terms[j].RawContent +"="+termToBeCompared.Terms[i].RawContent+",F");
                            break;
                        }
                    }
                }
            }
            if (!FoundMatch)
            {
                EqualFlag = false;
                break;
            }
        }

        if (!PlusFlag && ParentNegative^isNegative^termToBeCompared.isNegative) EqualFlag = false;
        //Debug.Log(RawContent+","+termToBeCompared.RawContent+", equal: "+EqualFlag + ". PN/pn/PF: "+PN +","+ParentNegative+".."+":"+PlusFlag);
        return EqualFlag;
    }

    public void TryFind(MathExpression termToBeCompared)
    {
        if(MathOperator == "=")
            Debug.Log(Terms[0].IsTermContaining(termToBeCompared)+" term 0; term 1 : "+Terms[1].IsTermContaining(termToBeCompared));
    }
    //0 = not found, 1 = found direct operation, -1 = found inverse operation
    int IsTermContaining(MathExpression termToBeCompared)
    {
        //Debug.Log("Comparing: "+(termToBeCompared.isNegative?'-':'+')+termToBeCompared.RawContent+","+(isNegative?'-':'+')+RawContent+","+ParentNegative);
        if (MathOperator.Length < termToBeCompared.MathOperator.Length) return 0;
        if (MathOperator.Length == 0)
            return RawContent == termToBeCompared.RawContent ?  (!isNegative^termToBeCompared.isNegative? 1 : -1 ): 0;
        bool PlusFlag = OperationPriority[MathOperator[0].ToString()] == 1;
        if (termToBeCompared.MathOperator.Length == 0)
        {
            for (int i = 0; i < Terms.Length; i++)
            {
                if (PlusFlag)
                {
                    if (Terms[i].IsTermEqualTo(termToBeCompared, false))
                        return (i == 0 ? '+' : MathOperator[i]) == '+' ? 1 : -1;
                    if (Terms[i].IsTermEqualTo(termToBeCompared, true))
                        return (i == 0 ? '+' : MathOperator[i]) == '-' ? 1 : -1;
                }
                else
                {
                    if (Terms[i].IsTermEqualTo(termToBeCompared, false))
                        return (i == 0 ? '*' : MathOperator[i]) == '*' ? 1 : -1;
                    if (Terms[i].IsTermEqualTo(termToBeCompared, true))
                        return (i == 0 ? '*' : MathOperator[i]) == '*' ? 1 : -1;
                }
                
            }
            return 0;
        }
        int ContainFlag = 1;
        List<int> DirectUsedTerms = new List<int>();
        List<int> InverseUsedTerms = new List<int>();
        bool DirectFoundMatch = false;
        bool InverseFoundMatch = false;
        bool DirectFlag = true;
        bool InverseFlag = true;
        bool PN = isNegative^termToBeCompared.isNegative;
        if (PlusFlag)
        {
            for (int i = 0; i < termToBeCompared.Terms.Length; i++)
            {
                DirectFoundMatch = false;
                InverseFoundMatch = false;
                for (int j = 0; j < Terms.Length; j++)
                {
                    //not finished yet
                    if (!DirectUsedTerms.Contains(j) && !DirectFoundMatch)
                    {
                        if (Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], false) && (j == 0 ? '+' : MathOperator[j-1]) == (i == 0? '+':termToBeCompared.MathOperator[i-1]))
                        {
                            DirectUsedTerms.Add(j);
                            DirectFoundMatch = true;
                        }
                        else if (Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], true) && (j == 0 ? '+' : MathOperator[j-1]) != (i == 0 ? '+':termToBeCompared.MathOperator[i-1]))
                        {
                            DirectUsedTerms.Add(j);
                            DirectFoundMatch = true;
                        }
                    }
                    if (!InverseUsedTerms.Contains(j) && !InverseFoundMatch)
                    {
                        if (Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], true) && (j == 0 ? '+' : MathOperator[j-1]) == (i == 0? '+':termToBeCompared.MathOperator[i-1]))
                        {
                            InverseUsedTerms.Add(j);
                            InverseFoundMatch = true;
                        }
                        else if (Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], false) && (j == 0 ? '+' : MathOperator[j-1]) == (i == 0 ? '+':termToBeCompared.MathOperator[i-1]))
                        {
                            InverseUsedTerms.Add(j);
                            InverseFoundMatch = true;
                        }
                    }
                    if (InverseFoundMatch && DirectFoundMatch) break;
                }
                if (!InverseFoundMatch && !DirectFoundMatch) {
                    ContainFlag = 0;
                    break;
                }
            }
            if (DirectUsedTerms.Count == termToBeCompared.Terms.Length) ContainFlag = 1;
            if (InverseUsedTerms.Count == termToBeCompared.Terms.Length) ContainFlag = -1;
        }
        else
        {
            for (int i = 0; i < termToBeCompared.Terms.Length; i++)
            {
                DirectFoundMatch = false;
                InverseFoundMatch = false;
                for (int j = 0; j < Terms.Length; j++)
                {
                    if (InverseUsedTerms.Contains(j) && DirectUsedTerms.Contains(j)) continue;
                    if (Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], false) ||
                        Terms[j].IsTermEqualTo(termToBeCompared.Terms[i], true))
                    {
                        if ((j == 0 ? '*' : MathOperator[j - 1]) ==
                            (i == 0 ? '*' : termToBeCompared.MathOperator[i - 1]))
                        {
                            if (!DirectUsedTerms.Contains(j) && !DirectFoundMatch)
                            {
                                DirectUsedTerms.Add(j);
                                DirectFoundMatch = true;
                            }
                        }
                        else
                        {
                            if (!InverseUsedTerms.Contains(j) && !InverseFoundMatch)
                            {
                                InverseUsedTerms.Add(j);
                                InverseFoundMatch = true;
                            }
                        }
                    }
                    if (InverseFoundMatch && DirectFoundMatch) break;
                }
                if (!InverseFoundMatch && !DirectFoundMatch) {
                    ContainFlag = 0;
                    break;
                }
            }
            if (DirectUsedTerms.Count == termToBeCompared.Terms.Length) ContainFlag = 1;
            if (InverseUsedTerms.Count == termToBeCompared.Terms.Length) ContainFlag = -1;
        }
        //Debug.Log(RawContent+","+termToBeCompared.RawContent+", ContainFlag: "+ContainFlag + ". PN/PF: "+PN +",,,,,"+PlusFlag);
        return ContainFlag;
    }
}
