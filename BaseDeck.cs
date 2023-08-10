using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseDeck: MonoBehaviour
{
    public Suit BaseSuit = Suit.None;
    public List<Card> BaseCards = new List<Card>();
    public bool IsTarget;


    public delegate UniTask GameWin();
    public static event GameWin DecksCompleted;

    public void Awake()
    {
        BaseCards.Clear();
        SolitaireBehaviour.gameReset += OnGameReset;
    }

    public async UniTask OnGameReset()
    {
        BaseCards.Clear();
        transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase();
    }
    public bool AddBaseCard(Card card)
    {
        if (!IsTarget) { return false; }
        if (!SolitaireBehaviour.UsedSuit.Contains(card.CardSuit) && BaseCards.Count == 0 && card.CardNum.Num == 1)
        {
            BaseSuit = card.CardSuit;
            BaseCards.Add(card);
            SolitaireBehaviour.UsedSuit.Add(BaseSuit);
            transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(BaseCards[^1]);
            return true;
        }
        if (card.CardSuit == BaseSuit && card.CardNum.Num == BaseCards[^1].CardNum.Num + 1)
        {
            BaseCards.Add(card);
            transform.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(BaseCards[^1]);
            if (BaseCards.Count == 13)
            {
                switch (BaseSuit)
                {
                    case Suit.Heart:
                        SolitaireBehaviour.HeartsFilled = true;
                        break;
                    case Suit.Spade:
                        SolitaireBehaviour.SpadesFilled = true;
                        break;
                    case Suit.Club:
                        SolitaireBehaviour.ClubsFilled = true;
                        break;
                    case Suit.Diamond:
                        SolitaireBehaviour.DiamondsFilled = true;
                        break;
                    default:
                        return false;
                }
            }
            if (SolitaireBehaviour.HeartsFilled && SolitaireBehaviour.SpadesFilled && SolitaireBehaviour.ClubsFilled && SolitaireBehaviour.DiamondsFilled && !SolitaireBehaviour.PlayerWon)
            { 
                DecksCompleted?.Invoke();
            }
                return true;
        }
        return false;
    }

    public void RemoveBaseCard(Card card)
    {
        if (SolitaireBehaviour.PlayerWon)
        {
            return;
        }
        BaseCards.Remove(card);
        if (card.CardNum.Num == 1)
        {
            BaseSuit = Suit.None;
            SolitaireBehaviour.UsedSuit.Remove(card.CardSuit);
        }
        try
        {
            transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(BaseCards[^1]);
        }
        catch 
        {
            transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase(); 
        }
    }
    public void ChangeCardFront(int index)
    {
        try
        {
           Card card = BaseCards[^index];
           transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
        }
        catch
        {
            transform.gameObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase();
        }
    }
}
