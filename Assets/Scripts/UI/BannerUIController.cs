using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class BannerUIController : UIContentController
{
    [System.Serializable]
    public struct info
    { 
        [field: SerializeField] public string title { get; private set; }
        [field: SerializeField] public string value { get; private set; }

        public info(string t, string v)
        {
            title = t;
            value = v;
        }
    };

    public List<info> bannerInfo;
    public GameObject player;
    private StringBuilder highscoreText;

    string FormatTime(float time)
    {
        float timeAdjusted = time * 1000;

        int minutes = (int)timeAdjusted / 60000;
        int seconds = (int)timeAdjusted / 1000 - 60 * minutes;
        int milliseconds = (int)timeAdjusted - minutes * 60000 - 1000 * seconds;
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public override void SetPrefabInfo(GameObject obj, object[] args)
    {
        string arg0 = (string)args[0];
        string arg1 = (string)args[1];
        
        GameObject panelControl = obj;

        if (arg0 != "")
        {
            panelControl.name = arg0;
            panelControl.GetComponent<UIElementController>().GetItem("Title").GetComponent<TMPro.TextMeshProUGUI>().text = arg0;
        }
        if (arg1 != "")
            panelControl.GetComponent<UIElementController>().GetItem("Value").GetComponent<TMPro.TextMeshProUGUI>().text = arg1;
        else
            Destroy(panelControl.GetComponent<UIElementController>().GetItem("ValuePanel"));
    }

    // Start is called before the first frame update
    void Start()
    {
        highscoreText = new StringBuilder();
        player = GameObject.Find("Player");
        if (bannerInfo.Count == 0) bannerInfo = new List<info>();
        BuildContent();
    }

    public override void BuildContent()
    {
        List<GameObject> added = new List<GameObject>();
        for (int i = 0; i < bannerInfo.Count; i++)
        {
            info details = bannerInfo[i];

            GameObject newBannerObj = InstantiatePrefabInContent("pf_BannerPanel");
            added.Add(newBannerObj);
        }
        for (int i = 0; i < bannerInfo.Count; i++)
        {
            info details = bannerInfo[i];
            SetPrefabInfo(added[i], new object[] { details.title, details.value });
        }

        Initialize(true);

        GameObject deaths = GetItem("Deaths");
        GameObject timer = GetItem("Time");
        GameObject best = GetItem("Best Times");
        GameObject title = GetItem("GRAMMAR BOY!");

        deaths.GetComponent<UIElementController>().CreateBind(g =>
        {
            GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
            string text = player.GetComponent<PlayerController>().instanceDeaths.ToString();
            value.GetComponent<TextMeshProUGUI>().text = text;
        });

        timer.GetComponent<UIElementController>().CreateBind(g =>
        {
            string time = FormatTime(player.GetComponent<PlayerController>().roundTimer);
            GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
            value.GetComponent<TextMeshProUGUI>().text = time;
        });

        best.GetComponent<UIElementController>().CreateBind(g =>
        {
            List<float> scores = player.GetComponent<PlayerController>().GetHighscores();
            highscoreText.Clear();
            for(int i = 0; i < scores.Count; i++)
                highscoreText.Append($"{i+1}) {FormatTime(scores[i])}\n");

            GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
            value.GetComponent<TextMeshProUGUI>().text = highscoreText.ToString();
        });

        title.GetComponent<UIElementController>().CreateBind(g =>
        {
            GameObject value = g.GetComponent<UIElementController>().GetItem("Title");
            if (player.GetComponent<PlayerSpeech>().GetLang() == "English")
                value.GetComponent<TextMeshProUGUI>().text = "GRAMMAR BOY!";
            else
                value.GetComponent<TextMeshProUGUI>().text = "¡EL NIÑO GRAMÁTICO!";
        });

        GameObject langToggle = InstantiatePrefabInContent("pf_LangToggle");
        langToggle.GetComponent<Button>().onClick.AddListener(() => {
            player.GetComponent<PlayerSpeech>().ToggleAsset();
            langToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = player.GetComponent<PlayerSpeech>().GetLang();
        });
    }
}
