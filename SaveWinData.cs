using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ngov3;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UniRx;
using UnityEngine;

namespace SolitaireScripts
{
    public class SaveWinData : MonoBehaviour
    {
        static string json;
        static ReactiveProperty<int> wins = new();
        static TextMeshProUGUI textMeshProUGUI;

        void Awake()
        {
            textMeshProUGUI = transform.GetComponent<TextMeshProUGUI>();
            wins.Subscribe((int win) =>
            {
                textMeshProUGUI.text = $"W: {win}";
            }).AddTo(this);
            LoadWins();
        }

        public static void LoadWins()
        {
            json = Path.Combine(Path.GetDirectoryName(Initializer.PInfo.Location), "Solitaire.json");
            try
            {
                wins.Value = JsonConvert.DeserializeObject<int>(File.ReadAllText(json));
            }
            catch
            {
                wins.Value = 0;
                File.WriteAllText(json, wins.ToString());
            }
        }
        public static async UniTask SaveWins()
        {
            List<string> picsList = SingletonMonoBehaviour<Settings>.Instance.imageHistory;
            wins.Value++;
            File.WriteAllText(json, wins.Value.ToString());
            if (wins.Value >= 1000 && !picsList.Contains("AllCards"))
            {
                SingletonMonoBehaviour<Settings>.Instance.addImage("AllCards");
            }
            if (wins.Value >= 100 && !picsList.Contains("SpadeCards"))
            {
                SingletonMonoBehaviour<Settings>.Instance.addImage("SpadeCards");
            }
            if (wins.Value >= 50 && !picsList.Contains("DiamondCards"))
            {
                SingletonMonoBehaviour<Settings>.Instance.addImage("DiamondCards");
            }
            if (wins.Value >= 10 && !picsList.Contains("ClubCards"))
            {
                SingletonMonoBehaviour<Settings>.Instance.addImage("ClubCards");
            }
            if (wins.Value >= 1 && !picsList.Contains("HeartCards"))
            {
                SingletonMonoBehaviour<Settings>.Instance.addImage("HeartCards");
            }
        }
    }
}
