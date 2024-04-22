using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using NGO;
using ngov3;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;


namespace SolitaireScripts
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Windose.exe")]
    public class Initializer : BaseUnityPlugin
    {
        public const string pluginGuid = "needy.girl.solitaire";
        public const string pluginName = "It's literally just Solitaire";
        public const string pluginVersion = "1.0.0.0";

        public static PluginInfo PInfo;
        // Start is called before the first frame update
        void Awake()
        {
            PInfo = Info;
            Harmony harmony = new(pluginGuid);
            harmony.PatchAll();
        }

        // Update is called once per frame
        /*  void Update()
          {
              if (SingletonMonoBehaviour<WindowManager>.Instance.isAppOpen(AppType.Calendar))
              {
                  SingletonMonoBehaviour<WindowManager>.Instance.CloseApp(AppType.Calendar);
                  return;
              }
              SingletonMonoBehaviour<WindowManager>.Instance.CleanOnCommand(false, false);
              SingletonMonoBehaviour<WindowManager>.Instance.NewWindow_Compact(AppType.Calendar, true, true);
              SingletonMonoBehaviour<EventManager>.Instance.SetShortcutState(false, 0.4f);
              SingletonMonoBehaviour<TaskbarManager>.Instance.SetTaskbarInteractive(false);
          }
        */

    }

    [HarmonyPatch]
    public class InitializeWindow
    {
        public static AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Initializer.PInfo.Location), "solitaire.bundle"));

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoadAppData), nameof(LoadAppData.ReadAppContent))]
        static bool ReplaceCalendar(AppType appType, ref AppTypeToData __result)
        {
            if (appType == (AppType)101)
            {
                __result = new AppTypeToData(true)
                {
                    //AppNameEN = "Solitaire",
                    // AppNameCN = "接龍",
                    AppNameJP = "ソリティア",
                    // AppNameKO = "솔리테어",
                    // AppNameTW = "接龍",
                    AppName = (SystemTextType)400,
                    appIcon = assetBundle.LoadAsset<Sprite>("icon_desktop_solitaire"),
                    appType = (AppType)101,
                    FirstPosX = 250,
                    FirstPosY = 765,
                    FirstHeight = 650,
                    FirstWidth = 920,
                    InnerContent = assetBundle.LoadAsset<GameObject>("App_CustomTest")

                };
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventManager), "Awake")]
        static void AddShortcuts()
        {
            Transform shortcuts = GameObject.Find("ShortCutParent").transform;
            Transform crazyShortcuts = GameObject.Find("HakkyoShortCutParent").transform;
            UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>("Solitaire"), shortcuts).GetComponent<Shortcut>();
            UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>("Solitears"), crazyShortcuts);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(NgoEx), nameof(NgoEx.getSystemTexts))]
        static void ApplyLabel(ref List<SystemTextMaster.Param> __result)
        {
            SystemTextMaster.Param param = new SystemTextMaster.Param()
            {
                Id = "400",
                BodyCn = "接龍",
                BodyEn = "Solitaire",
                BodyFr = "Solitaire",
                BodyGe = "Solitaire",
                BodyIt = "Solitario",
                BodyJp = "ソリティア",
                BodyKo = "솔리테어",
                BodySp = "Solitario",
                BodyTw = "接龍",
                BodyVn = "Solitaire"
            };

            if (!__result.Exists(x => x.Id == "400"))
            {
                __result.Add(param);
            }
        }

    }

    [HarmonyPatch]
    public class InitializeExtPics
    {
        private static List<ResourceLocal> awardPics = new List<ResourceLocal>()
        {

            new ResourceLocal
            {
                Id= "CARD1",
                FileName = "HeartCards",
                Path = ""
            },
            new ResourceLocal
            {
                Id= "CARD2",
                FileName = "ClubCards",
                Path = ""
            },
            new ResourceLocal
            {
                Id= "CARD3",
                FileName = "DiamondCards",
                Path = ""
            },
            new ResourceLocal
            {
                Id= "CARD4",
                FileName = "SpadeCards",
                Path = ""
            },
            new ResourceLocal
            {
                Id= "CARD5",
                FileName = "AllCards",
                Path = ""
            }
        };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ImageViewerHelper), "LoadResourcesList")]
        static void SetAwardPics(ref List<ResourceLocal> __result)
        {
            if (!__result.Exists(r => r.Id == "CARD1"))
            {
                __result.InsertRange(128, awardPics);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadPictures), "LoadPictureAsync")]
        static async UniTask<Sprite> LoadAwardPics(UniTask<Sprite> value, string address)
        {
            try
            {
                string assetName = awardPics.FirstOrDefault(a => a.FileName == address).FileName;
                if (assetName != null)
                {
                    Sprite customSprite = InitializeWindow.assetBundle.LoadAsset<Sprite>(assetName);
                    return customSprite;
                }
                return await value;
            }
            catch { return await value; }
        }
    }
}


