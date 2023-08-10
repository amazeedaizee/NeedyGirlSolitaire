using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ngov3;
using UniRx;
using Cysharp.Threading.Tasks;
using TMPro;
using NGO;

public class PlayAgainButton : MonoBehaviour
{
    private TextMeshProUGUI buttonText;
    void Awake()
    {
        gameObject.SetActive(false);
        buttonText = transform.GetComponentInChildren<TextMeshProUGUI>();
        OnLanguagedChanged();
        SingletonMonoBehaviour<Settings>.Instance.CurrentLanguage.Subscribe((LanguageType _) =>
        {
            OnLanguagedChanged();
        }).AddTo(gameObject);
    }

    // Update is called once per frame
    void OnLanguagedChanged()
    {
        var lang = SingletonMonoBehaviour<Settings>.Instance.CurrentLanguage.Value;
        buttonText.text = NgoEx.SystemTextFromType(SystemTextType.Start_Menu_Newgame00, lang);
    }
}
