using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.UI;

public class PlayerSpeech : MonoBehaviour
{
    //element of speech bubble
    public UIElementController speechBubbleC;

    //asset list
    public TextAsset[] adjectiveAssets;
    //asset to use by index
    public int assetIndex;
    //cache of parsed asset information
    public List<string> wordsInUse;
    //offset to place bubble
    public float bubbleOffset = 122.78f;
    //max time to show bubble
    public float showTimeMax;

    private bool loadingContent = false;
    private float showTimer;
    private string currentWord;

    private void Awake()
    {
        wordsInUse = new List<string>();
        //pre-load words
        LoadContent();
    }

    private void Start()
    {
        showTimer = 0;

        //bind speech bubble to current selected word
        speechBubbleC.CreateBind(g =>
        {
            g.GetComponent<UIElementController>().GetItem("Text").GetComponent<TMPro.TextMeshProUGUI>().text = currentWord;
        });
    }

    private void Update()
    {
        if (showTimer > 0)
        {
            //when timer is positive scale the speech bubble up and center on player X
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
            //scale speech bubble to zero and move towards player
            Vector3 toScale = speechBubbleC.GetComponent<RectTransform>().localScale;
            speechBubbleC.GetComponent<RectTransform>().localScale = Vector3.MoveTowards(toScale, Vector3.zero, Time.deltaTime*5);
            Vector3 pos = speechBubbleC.GetComponent<RectTransform>().position;
            Vector3 playerToScreen = Camera.main.WorldToScreenPoint(transform.position);
            speechBubbleC.GetComponent<RectTransform>().position = Vector3.MoveTowards(pos, playerToScreen, Time.deltaTime*500);
        }
    }

    public IEnumerator LoadContentC()
    {
        if (loadingContent) yield return null;
        loadingContent = true;
        //parse file and add to list for easy access
        wordsInUse.Clear();
        string[] textSplit = adjectiveAssets[assetIndex].text.Split('\n');
        for(int i=0; i < textSplit.Length-1; i++)
        {
            wordsInUse.Add(textSplit[i]);
            yield return new WaitForEndOfFrame();
        }
        loadingContent = false;
    }

    public void LoadContent()
    {
        StartCoroutine(LoadContentC());
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
        //reload content after asset switch
        LoadContent();
    }

    public void ToggleAsset()
    {
        SwitchAsset((assetIndex == 0) ? 1 : 0);
    }

    public string GetRandomWord() => wordsInUse[Random.Range(0, wordsInUse.Count)];

    public void SetWord(string word)
    {
        currentWord = word;
    }

    public void SetRandomWord()
    {
        //give the speech bubble a little bump everytime you collect a consecutive gem
        if (showTimer > 0) speechBubbleC.GetComponent<RectTransform>().localScale += Vector3.one/3;

        showTimer = showTimeMax;
        SetWord(GetRandomWord());
    }
}
