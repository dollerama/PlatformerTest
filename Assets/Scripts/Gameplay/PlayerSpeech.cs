using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpeech : MonoBehaviour
{
    public UIElementController speechBubbleC;
    public TextAsset[] adjectiveAssets;

    public int assetIndex;
    public List<string> wordsInUse;

    public float bubbleOffset = 122.78f;

    public float showTimeMax;
    private float showTimer;

    private void Awake()
    {
        wordsInUse = new List<string>();
        LoadContent();
    }

    private void Start()
    {
        showTimer = 0;
    }

    private void Update()
    {
        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            Vector3 toScale = speechBubbleC.GetComponent<RectTransform>().localScale;
            speechBubbleC.GetComponent<RectTransform>().localScale = Vector3.MoveTowards(toScale, Vector3.one, Time.deltaTime * 5);
            Vector3 pos = speechBubbleC.GetComponent<RectTransform>().position;
            Vector3 playerToScreen = Camera.main.WorldToScreenPoint(transform.position);
            playerToScreen.y = bubbleOffset; playerToScreen.z = pos.z;
            speechBubbleC.GetComponent<RectTransform>().position = playerToScreen;
        }
        else
        {
            Vector3 toScale = speechBubbleC.GetComponent<RectTransform>().localScale;
            speechBubbleC.GetComponent<RectTransform>().localScale = Vector3.MoveTowards(toScale, Vector3.zero, Time.deltaTime*5);
            Vector3 pos = speechBubbleC.GetComponent<RectTransform>().position;
            Vector3 playerToScreen = Camera.main.WorldToScreenPoint(transform.position);
            speechBubbleC.GetComponent<RectTransform>().position = Vector3.MoveTowards(pos, playerToScreen, Time.deltaTime*500);
        }
    }

    public void LoadContent()
    {
        wordsInUse.Clear();
        string[] textSplit = adjectiveAssets[assetIndex].text.Split('\n');
        for(int i=0; i < textSplit.Length-1; i++)
        {
            wordsInUse.Add(textSplit[i]);
        }
    }

    public string GetLang()
    {
        return (assetIndex == 0) ? "English" : "Spanish";
    }

    public string GetToggleLang()
    {
        return (assetIndex == 1) ? "English" : "Spanish";
    }

    public void SwitchAsset(int newIndex)
    {
        assetIndex = newIndex;
        LoadContent();
    }

    public void ToggleAsset()
    {
        SwitchAsset((assetIndex == 0) ? 1 : 0);
    }

    public string GetRandomWord() => wordsInUse[Random.Range(0, wordsInUse.Count - 1)];

    public void SetWord(string word)
    {
        speechBubbleC.GetItem("Text").GetComponent<TMPro.TextMeshProUGUI>().text = word;
    }

    public void SetRandomWord()
    {
        if (showTimer > 0) speechBubbleC.GetComponent<RectTransform>().localScale += Vector3.one/3;

        showTimer = showTimeMax;
        SetWord(GetRandomWord());
    }
}
