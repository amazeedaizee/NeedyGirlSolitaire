using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainDeck : MonoBehaviour
{
    public static GameObject mainDeck;
    public static List<Card> Cards = new List<Card>();
    public static List<Card> UsedCards = new List<Card>();

    public delegate void ShowingCard(Card card);
    public static event ShowingCard showCard;
    private void Awake()
    {
        MainDeck.Cards.Clear();
        MainDeck.UsedCards.Clear();
        mainDeck = transform.gameObject;
        SolitaireBehaviour.gameStart += OnGameStart;
        SolitaireBehaviour.gameReset += OnGameReset;
    }
    private async UniTask OnGameStart()
    {
        Cards.AddRange(SolitaireBehaviour.MainDeckList);
        SolitaireBehaviour.MainDeckList.Clear();
        SolitaireBehaviour.UsedSuit.Clear();
        await mainDeck.GetComponent<CardObject>().BackOfCard();
    }

    private async UniTask OnGameReset()
    {
        MainDeck.UsedCards.Clear();
    }
    public static async UniTask OnClicked()
    {   if (Cards.Count ==0 && UsedCards.Count ==0) { return; }
        if (Cards.Count == 0)
        {
            UsedCards.Reverse();
            Cards.AddRange(UsedCards);
            UsedCards.Clear();
            await mainDeck.GetComponent<CardObject>().BackOfCard();
            showCard?.Invoke(null);
            return;
        }
        showCard?.Invoke(Cards[^1]);
        UsedCards.Add(Cards[^1]);
        Cards.RemoveAt(Cards.Count - 1);
        if (Cards.Count == 0)
        {
            mainDeck.GetComponent<Image>().sprite = CardArtManager.ApplyCardReset();
            if (UsedCards.Count == 0) { SolitaireBehaviour.CheckForSolve(); }
        }

    }
}
