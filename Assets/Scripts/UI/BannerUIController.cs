using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;
using TMPro;
using System.Text;
using UnityEngine.UI;

namespace Platformer.UI
{
    //In-game banner used to display title, deaths, timer, leaderboard, and language toggle
    public class BannerUIController : UIContentController
    {
        //info describes the information we need to input into each prefab
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

        //list of info (title, deaths, timer, leaderboard, and language toggle)
        public List<info> bannerInfo;
        //reference to player
        public GameObject player;
        //highscore text requires some work to concatenate so we're using a stringbuilder
        private StringBuilder highscoreText;

        //handy time format function 
        string FormatTime(float time)
        {
            float timeAdjusted = time * 1000;

            int minutes = (int)timeAdjusted / 60000;
            int seconds = (int)timeAdjusted / 1000 - 60 * minutes;
            int milliseconds = (int)timeAdjusted - minutes * 60000 - 1000 * seconds;
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        //override our setter for each prefab
        public override void SetPrefabInfo(GameObject obj, object[] args)
        {
            //these too arguments will be info.title and info.value respectively when called
            string arg0 = (string)args[0];
            string arg1 = (string)args[1];
            //object being spawned
            GameObject panelControl = obj;

            //check to make sure title is not empty. We do this just in case we want to leave the title and only set the value
            if (arg0 != "")
            {
                //set titel
                panelControl.name = arg0;
                panelControl.GetComponent<UIElementController>().GetItem("Title").GetComponent<TextMeshProUGUI>().text = arg0;
            }

            if (arg1 != "")
            {
                panelControl.GetComponent<UIElementController>().GetItem("Value").GetComponent<TextMeshProUGUI>().text = arg1;
            }
            else
            {
                //if value is empty we dont need it so destroy it
                //this is used mainly for the title to be displayed in the banner.
                //once the value is set you can set just the title if needed without fear of destroying the value panel as well
                if(panelControl.GetComponent<UIElementController>().GetItem("Value").GetComponent<TextMeshProUGUI>().text == "")
                    Destroy(panelControl.GetComponent<UIElementController>().GetItem("ValuePanel"));
            }
        }

        void Start()
        {
            highscoreText = new StringBuilder();
            player = GameObject.Find("Player");
            if (bannerInfo.Count == 0) bannerInfo = new List<info>();

            BuildContent();
        }

        void HookUpBindings()
        {
            //force re-init of our content just to make sure we can grab everything we need
            Initialize(true);

            //grab elements
            GameObject deaths = GetItem("Deaths");
            GameObject timer = GetItem("Time");
            GameObject best = GetItem("Best Times");
            GameObject title = GetItem("GRAMMAR BOY!");

            //bind deaths to instanceDeaths
            deaths.GetComponent<UIElementController>().CreateBind(g =>
            {
                //grab value in deaths and set it
                GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
                string text = player.GetComponent<PlayerController>().instanceDeaths.ToString();
                value.GetComponent<TextMeshProUGUI>().text = text;
            });

            //bind timer to roundTimer
            timer.GetComponent<UIElementController>().CreateBind(g =>
            {
                string time = FormatTime(player.GetComponent<PlayerController>().roundTimer);

                //grab value in timer and set it
                GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
                value.GetComponent<TextMeshProUGUI>().text = time;
            });

            //bind leaderboard to highscoreText
            best.GetComponent<UIElementController>().CreateBind(g =>
            {
                List<float> scores = player.GetComponent<PlayerController>().GetHighscores();
                highscoreText.Clear();
                for (int i = 0; i < scores.Count; i++)
                    highscoreText.Append($"{i + 1}) {FormatTime(scores[i])}\n");

                //grab value in leaderboard and set it
                GameObject value = g.GetComponent<UIElementController>().GetItem("Value");
                value.GetComponent<TextMeshProUGUI>().text = highscoreText.ToString();
            });

            //bind text localization to title
            title.GetComponent<UIElementController>().CreateBind(g =>
            {
                GameObject value = g.GetComponent<UIElementController>().GetItem("Title");
                if (player.GetComponent<PlayerSpeech>().GetLang() == "English")
                    value.GetComponent<TextMeshProUGUI>().text = "GRAMMAR BOY!";
                else
                    value.GetComponent<TextMeshProUGUI>().text = "¡EL NIÑO GRAMÁTICO!";
            });

            //Instantiate language toggle button
            GameObject langToggle = InstantiatePrefabInContent("pf_LangToggle");
            langToggle.GetComponent<Button>().onClick.AddListener(() =>
            {
                //toggle words asset being used by speech bubbles
                player.GetComponent<PlayerSpeech>().ToggleAsset();
                //change text on button to reflect current state
                langToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = player.GetComponent<PlayerSpeech>().GetLang();
            });
        }

        public override void BuildContent()
        {
            List<GameObject> added = new List<GameObject>();

            for (int i = 0; i < bannerInfo.Count; i++)
            {
                //get ref to details
                info details = bannerInfo[i];

                //create object
                GameObject newBannerObj = InstantiatePrefabInContent("pf_BannerPanel");
                
                //set values
                SetPrefabInfo(newBannerObj, new object[] { details.title, details.value });
            }

            HookUpBindings();
        }
    }
}
