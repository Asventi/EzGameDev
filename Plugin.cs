using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EzGameDev;

[BepInPlugin("me.asventi.plugins.mtg2.ezgamedev", "EzGameDev", "1.0.0")]
[BepInProcess("Mad Games Tycoon 2.exe")]
public class Plugin : BaseUnityPlugin
{
    static ConfigEntry<bool> configTarget;
    static ConfigEntry<bool> configGenres;
    static ConfigEntry<bool> configThemes;
    static ConfigEntry<bool> configSliders;

    private void Awake()
    {
        configTarget = Config.Bind(
            "General.Toggles",
            "TargetGroup",
            true,
            "Toggle if the correct target group should be highlighted when choosing it");
        
        configGenres = Config.Bind(
            "General.Toggles",
            "Genres",
            true,
            "Toggle if the correct genre should be highlighted when choosing it");
        
        configThemes = Config.Bind(
            "General.Toggles",
            "Themes",
            true,
            "Toggle if the correct theme should be highlighted when choosing it");
        
        configSliders = Config.Bind(
            "General.Toggles",
            "Sliders",
            true,
            "Toggle if the slider should automatically be set to the correct value when pressing the optimal settings button event if it's not discovered"
        );

        Harmony.CreateAndPatchAll(typeof(PatchGenre));
        Harmony.CreateAndPatchAll(typeof(PatchTheme));
        Harmony.CreateAndPatchAll(typeof(PatchGroup));
        Harmony.CreateAndPatchAll(typeof(PatchGroupActivate));
        Harmony.CreateAndPatchAll(typeof(PatchSliders));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    [HarmonyPatch(typeof(Menu_DevGame_Genre), nameof(Menu_DevGame_Genre.Init))]
    class PatchGenre
    {
        static void Postfix(Menu_DevGame_Genre __instance, int g)
        {
            if (configGenres.Value && g == 1)
            {
                genres _genres = GameObject.FindWithTag("Main").GetComponent<genres>();
                GUI_Main guiMain = GameObject.Find("CanvasInGameMenu").GetComponent<GUI_Main>();
                Menu_DevGame mDevGame = guiMain.uiObjects[56].GetComponent<Menu_DevGame>();

                foreach (Transform transform in __instance.uiObjects[0].transform)
                {
                    Item_DevGame_Genre component = transform.gameObject.GetComponent<Item_DevGame_Genre>();
                    
                    
                    
                    if (_genres.IsGenreCombination(mDevGame.g_GameMainGenre, component.myID))
                    {
                        component.GetComponent<Image>().color = Color.magenta;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Menu_DevGame_Theme), nameof(Menu_DevGame_Theme.Init))]
    class PatchTheme
    {
        static void Postfix(Menu_DevGame_Theme __instance)
        {
            
            if (configThemes.Value)
            {
                themes _themes = GameObject.FindWithTag("Main").GetComponent<themes>();
                GUI_Main guiMain = GameObject.Find("CanvasInGameMenu").GetComponent<GUI_Main>();
                Menu_DevGame mDevGame = guiMain.uiObjects[56].GetComponent<Menu_DevGame>();

                foreach (Transform transform in __instance.uiObjects[0].transform)
                {
                    
                    Item_DevGame_Theme component = transform.gameObject.GetComponent<Item_DevGame_Theme>();

                    if (_themes.IsThemesFitWithGenre(component.myID, mDevGame.g_GameMainGenre))
                    {
                        component.GetComponent<Image>().color = Color.magenta;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Menu_DevGame_Zielgruppe), nameof(Menu_DevGame_Zielgruppe.BUTTON_GameZielgruppe))]
    class PatchGroup
    {
        static void Postfix(Menu_DevGame_Zielgruppe __instance, int i)
        {
            if (configTarget.Value)
            {
                genres _genres = GameObject.FindWithTag("Main").GetComponent<genres>();
                GUI_Main guiMain = GameObject.Find("CanvasInGameMenu").GetComponent<GUI_Main>();
                Menu_DevGame mDevGame = guiMain.uiObjects[56].GetComponent<Menu_DevGame>();
                
                mDevGame.SetZielgruppe(i);
                
                for (int j = 0; j < 5; j++)
                {
                    if (_genres.IsTargetGroup(mDevGame.g_GameMainGenre, j))
                    {
                        __instance.uiObjects[j + 1].GetComponent<Image>().color = Color.magenta;
                    }
                    else
                    {
                        __instance.uiObjects[j + 1].GetComponent<Image>().color = Color.white;
                    }
                    
                }
                
                if (_genres.IsTargetGroup(mDevGame.g_GameMainGenre, i))
                {
                    __instance.uiObjects[i + 1].GetComponent<Image>().color = Color.green;
                }
                else
                {
                    __instance.uiObjects[i + 1].GetComponent<Image>().color = Color.red;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(GUI_Main), nameof(GUI_Main.ActivateMenu))]
    class PatchGroupActivate
    {
        static void Postfix(GUI_Main __instance, GameObject go)
        {
            if (configTarget.Value && go == __instance.uiObjects[60])
            {
                genres _genres = GameObject.FindWithTag("Main").GetComponent<genres>();
                Menu_DevGame mDevGame = __instance.uiObjects[56].GetComponent<Menu_DevGame>();
                Menu_DevGame_Zielgruppe mDevGroup = __instance.uiObjects[60].GetComponent<Menu_DevGame_Zielgruppe>();
                
                for (int j = 0; j < 5; j++)
                {
                    if (_genres.IsTargetGroup(mDevGame.g_GameMainGenre, j))
                    {
                        mDevGroup.uiObjects[j + 1].GetComponent<Image>().color = Color.magenta;
                    }
                    else
                    {
                        mDevGroup.uiObjects[j + 1].GetComponent<Image>().color = Color.white;
                    }
                }
                
                if (_genres.IsTargetGroup(mDevGame.g_GameMainGenre, mDevGame.g_GameZielgruppe))
                {
                    mDevGroup.uiObjects[mDevGame.g_GameZielgruppe + 1].GetComponent<Image>().color = Color.green;
                }
                else
                {
                    mDevGroup.uiObjects[mDevGame.g_GameZielgruppe + 1].GetComponent<Image>().color = Color.red;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Menu_DevGame), nameof(Menu_DevGame.BUTTON_AutoDesignSettings))]
    class PatchSliders
    {
        static void Postfix(Menu_DevGame __instance)
        {
            if (configSliders.Value)
            {
                genres _genres = GameObject.FindWithTag("Main").GetComponent<genres>();
                int mainGenre = __instance.g_GameMainGenre;
                int subGenre = __instance.g_GameSubGenre;
                
                for (int i = 0; i < __instance.g_Designschwerpunkt.Length; i++)
                {
                    __instance.g_Designschwerpunkt[i] = _genres.GetFocus(i, mainGenre, subGenre);
                }

                for (int j = 0; j < __instance.g_Designausrichtung.Length; j++)
                {
                    __instance.uiDesignausrichtung[j].transform.GetChild(2).transform.GetChild(0).GetComponent<Text>()
                        .text = __instance.g_Designausrichtung[j].ToString();
                    __instance.g_Designausrichtung[j] = _genres.GetAlign(j, mainGenre, subGenre);
                }

                int gameplay = (int)_genres.genres_GAMEPLAY[__instance.g_GameMainGenre];
                int graphic = (int)_genres.genres_GRAPHIC[__instance.g_GameMainGenre];
                int sound = (int)_genres.genres_SOUND[__instance.g_GameMainGenre];
                int control = (int)_genres.genres_CONTROL[__instance.g_GameMainGenre];
                __instance.uiObjects[97].GetComponent<Slider>().value = (float)(gameplay / 5);
                __instance.uiObjects[98].GetComponent<Slider>().value = (float)(graphic / 5);
                __instance.uiObjects[99].GetComponent<Slider>().value = (float)(sound / 5);
                __instance.uiObjects[100].GetComponent<Slider>().value = (float)(control / 5);
                __instance.SetAP_Gameplay();
                __instance.SetAP_Grafik();
                __instance.SetAP_Sound();
                __instance.SetAP_Technik();
                __instance.UpdateDesignSettings();
                __instance.UpdateDesignSlider();
            }
        }
    }
}
