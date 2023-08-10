using Cysharp.Threading.Tasks;
using HarmonyLib;
using ngov3;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SolitaireScripts
{
    public class SolitaireShortcut : MonoBehaviour
    {
        private Button _button;
        private TMP_Text _text;
        void Awake()
        {
            _button = transform.GetComponent<Button>();   
            _text = transform.GetChild(1).GetComponent<TMP_Text>();

            _button.onClick.AddListener(OnSubmit);

            OnLanguageUpdated();
            SingletonMonoBehaviour<Settings>.Instance.CurrentLanguage.Subscribe((LanguageType _) =>
            {
                OnLanguageUpdated();
            }).AddTo(gameObject);
        }

        void OnLanguageUpdated()
        {
            switch (SingletonMonoBehaviour<Settings>.Instance.CurrentLanguage.Value)
            {
                case LanguageType.JP:
                    _text.text = "ソリティア";
                    return;
                case LanguageType.CN:
                case LanguageType.TW:
                    _text.text = "接龍"; 
                    return;
                case LanguageType.KO:
                    _text.text = "솔리테어"; 
                    return;
                case LanguageType.IT:
                case LanguageType.SP:
                    _text.text = "Solitario";
                    return;
                default:
                    _text.text = "Solitaire";
                    return;
            }
        }

        void OnSubmit()
        {
            if (SingletonMonoBehaviour<WindowManager>.Instance.isAppOpen((AppType)101))
            { return; }
            SingletonMonoBehaviour<WindowManager>.Instance.CleanOnCommand(false, false);
            SingletonMonoBehaviour<WindowManager>.Instance.NewWindow_Compact((AppType)101, true, true);
            SingletonMonoBehaviour<EventManager>.Instance.SetShortcutState(false, 0.4f);
            SingletonMonoBehaviour<TaskbarManager>.Instance.SetTaskbarInteractive(false);
            AwaitWindowClose();
        }

        private static async UniTask AwaitWindowClose()
        {
            await UniTask.WaitUntil(() => { return SingletonMonoBehaviour<WindowManager>.Instance.isAppOpen((AppType)101); });
            IDisposable disp = SingletonMonoBehaviour<WindowManager>.Instance.GetWindowFromApp((AppType)101).ObserveEveryValueChanged(w => w.windowState).Subscribe((WindowState w) =>
            {
                if (w == WindowState.closed)
                {
                    SingletonMonoBehaviour<EventManager>.Instance.SetShortcutState(true, 0.4f);
                    SingletonMonoBehaviour<TaskbarManager>.Instance.SetTaskbarInteractive(true);
                }
            });
            await UniTask.WaitUntil(() => { return SingletonMonoBehaviour<TaskbarManager>.Instance.TaskBarGroup.interactable || SceneManager.GetActiveScene().name == "BiosToLoad"; });
            disp.Dispose();
        }
    }
}
