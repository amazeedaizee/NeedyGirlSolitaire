using SolitaireScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardArtManager : MonoBehaviour
{
   public static Sprite SearchCardFront(Card card)
    {
        Sprite sprite = null;
        try
        {
            string suitName = card.CardSuit.ToString().ToLower() + "s";
            sprite = InitializeWindow.assetBundle.LoadAsset<Sprite>($"card_{ suitName}{ card.CardNum.Num}");
        }
        catch { Debug.LogError("Could not load front!"); }
        return sprite;
    }

    public static Sprite SearchCardBack(string name)
    {
        Sprite sprite = null;
        try
        {
            sprite = InitializeWindow.assetBundle.LoadAsset<Sprite>(name);
        }
        catch { Debug.LogError("Could not load back!"); }
        return sprite;
    }

    public static Sprite ApplyCardReset()
    {
        return InitializeWindow.assetBundle.LoadAsset<Sprite>($"card_reset");
    }
    public static Sprite ApplyCardBase()
    {
        return InitializeWindow.assetBundle.LoadAsset<Sprite>($"card_base");
    }

    public static Sprite ApplyCardSolve()
    {
        return InitializeWindow.assetBundle.LoadAsset<Sprite>($"card_solve");
    }
}
