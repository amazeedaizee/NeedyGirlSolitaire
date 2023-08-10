using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ngov3;
using SolitaireScripts;

public enum Suit { None, Heart, Spade, Club, Diamond}
public class Card
{

    public Suit CardSuit;
    public CardInt CardNum;
    public bool isHidden = true;
    public bool isStacked = true;

    public Card(Suit suit, int num, bool isShowing = true)
    {
        CardSuit = suit;
        CardNum.Num = num;
        isHidden = !isShowing;
    }
}

public struct CardInt
{
    private int Min => 1;

    private int _num;
    public int Num
    {
        get => _num;
        set 
        { 
            if (value < Min) { _num = Min; }
            else if (value > Max) { _num = Max; }
            else { _num = value; };
        }
    }
    private int Max => 13;
}



public class SolitaireBehaviour : MonoBehaviour
{
    public static List<Card> AllCardsList;

    public static List<Card> MainDeckList = new List<Card>();
    public static List<Suit> UsedSuit = new List<Suit>();

    public static List<Card> SetOneList = new List<Card>();
    public static List <Card> SetTwoList = new List<Card>();
    public static List<Card> SetThreeList = new List<Card>();
    public static List<Card> SetFourList = new List<Card>();
    public static List<Card> SetFiveList = new List<Card>();
    public static List<Card> SetSixList = new List<Card>();
    public static List<Card> SetSevenList = new List<Card>();

    private static GameObject SetGroup;

    private static GameObject SetOneObject;
    private static GameObject SetTwoObject;
    private static GameObject SetThreeObject;
    private static GameObject SetFourObject;
    private static GameObject SetFiveObject;
    private static GameObject SetSixObject;
    private static GameObject SetSevenObject;

    private static GameObject BaseDeckOne;
    private static GameObject BaseDeckTwo;
    private static GameObject BaseDeckThree;
    private static GameObject BaseDeckFour;

    public static GameObject PreventAction;
    public static GameObject PlayAgainButton;

    public static int backNum = 100;

    public static bool SpadesFilled;
    public static bool HeartsFilled;
    public static bool ClubsFilled;
    public static bool DiamondsFilled;

    public static bool PlayerWon;

    public delegate UniTask GameEvent();
    public static event GameEvent gameStart;
    public static event GameEvent gameReset;

    private bool win;

    private async void Start()
    {
        Transform content = transform.Find("BG/ContentParent/Content");
        Transform setGroup = content.Find("SetGroup");
        Transform baseGroup = content.Find("BaseGroup");
        SetOneObject = setGroup.Find("SetOne").gameObject;
        SetTwoObject = setGroup.Find("SetTwo").gameObject;
        SetThreeObject = setGroup.Find("SetThree").gameObject;
        SetFourObject = setGroup.Find("SetFour").gameObject;
        SetFiveObject = setGroup.Find("SetFive").gameObject;
        SetSixObject = setGroup.Find("SetSix").gameObject;
        SetSevenObject = setGroup.Find("SetSeven").gameObject;

        BaseDeckOne = baseGroup.Find("BaseOne").gameObject;
        BaseDeckTwo = baseGroup.Find("BaseTwo").gameObject;
        BaseDeckThree = baseGroup.Find("BaseThree").gameObject;
        BaseDeckFour = baseGroup.Find("BaseFour").gameObject;

        PreventAction = transform.Find("BG/ContentParent/NoTouching").gameObject;
        PlayAgainButton = transform.Find("BG/ContentParent/PlayAgain").gameObject;
        

        PlayAgainButton.GetComponent<Button>().onClick.AddListener(() => { gameReset?.Invoke(); });

        RandomBackNum();
        await StartCardGame();
        BaseDeck.DecksCompleted += WhenPlayerWin;
        gameReset += OnGameReset;
    }

    async UniTask StartCardGame()
    {
        SpadesFilled = false;
        HeartsFilled = false;
        ClubsFilled = false;
        DiamondsFilled= false;
        PlayerWon = false;
        ShowCard.CanSolve = false;
        MainDeckList.Clear();
        UsedSuit.Clear();
        AllCardsList = new List<Card>()
    {
        new Card(Suit.Heart,1),
        new Card(Suit.Heart,2),
        new Card(Suit.Heart,3),
        new Card(Suit.Heart,4),
        new Card(Suit.Heart,5),
        new Card(Suit.Heart,6),
        new Card(Suit.Heart,7),
        new Card(Suit.Heart,8),
        new Card(Suit.Heart,9),
        new Card(Suit.Heart,10),
        new Card(Suit.Heart,11),
        new Card(Suit.Heart,12),
        new Card(Suit.Heart,13),
        new Card(Suit.Club,1),
        new Card(Suit.Club,2),
        new Card(Suit.Club,3),
        new Card(Suit.Club,4),
        new Card(Suit.Club,5),
        new Card(Suit.Club,6),
        new Card(Suit.Club,7),
        new Card(Suit.Club,8),
        new Card(Suit.Club,9),
        new Card(Suit.Club,10),
        new Card(Suit.Club,11),
        new Card(Suit.Club,12),
        new Card(Suit.Club,13),
        new Card(Suit.Diamond,1),
        new Card(Suit.Diamond,2),
        new Card(Suit.Diamond,3),
        new Card(Suit.Diamond,4),
        new Card(Suit.Diamond,5),
        new Card(Suit.Diamond,6),
        new Card(Suit.Diamond,7),
        new Card(Suit.Diamond,8),
        new Card(Suit.Diamond,9),
        new Card(Suit.Diamond,10),
        new Card(Suit.Diamond,11),
        new Card(Suit.Diamond,12),
        new Card(Suit.Diamond,13),
        new Card(Suit.Spade,1),
        new Card(Suit.Spade,2),
        new Card(Suit.Spade,3),
        new Card(Suit.Spade,4),
        new Card(Suit.Spade,5),
        new Card(Suit.Spade,6),
        new Card(Suit.Spade,7),
        new Card(Suit.Spade,8),
        new Card(Suit.Spade,9),
        new Card(Suit.Spade,10),
        new Card(Suit.Spade,11),
        new Card(Suit.Spade,12),
        new Card(Suit.Spade,13)


    };
        await UniTask.WaitUntil(() => { return AllCardsList.Count == 52; });
        for (int i = 0; i <= AllCardsList.Count; i++)
        {
            i = 0;
            int index = Random.Range(0, AllCardsList.Count);
            Card card = AllCardsList[index];
            if (SetOneList.Count != 1)
            {
                SetOneList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetTwoList.Count != 2)
            {
                SetTwoList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetThreeList.Count != 3)
            {
                SetThreeList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetFourList.Count != 4)
            {
                SetFourList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetFiveList.Count != 5)
            {
                SetFiveList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetSixList.Count != 6)
            {
                SetSixList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            if (SetSevenList.Count != 7)
            {
                SetSevenList.Add(card);
                AllCardsList.Remove(card);
                continue;
            }
            MainDeckList.Add(card);
            AllCardsList.Remove(card);
        }
        gameStart?.Invoke();
        CardHold.ToggleCollider(true);
    }

    void RandomBackNum()
    {        
        backNum = Random.Range(0, 100);       

    }

    async UniTask WhenPlayerWin()
    {
        if (PlayerWon)
        {
            return;
        }
        PlayerWon = true;
        Debug.Log("Game completed: Winner!");
        SaveWinData.SaveWins().Forget();
        CardHold.ToggleCollider(false);
        PreventAction.SetActive(true);
        PlayAgainButton.SetActive(true);
        PreventAction.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        AudioManager.Instance.PlaySeByType(NGO.SoundType.SE_Tetehen);
    }

    async UniTask OnGameReset()
    {
        RandomBackNum();
        PreventAction.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        PreventAction.SetActive(false);
        PlayAgainButton.SetActive(false);
        SetOneObject.GetComponent<SetDeck>().Cards.Clear();
        SetTwoObject.GetComponent<SetDeck>().Cards.Clear();
        SetThreeObject.GetComponent<SetDeck>().Cards.Clear();
        SetFourObject.GetComponent<SetDeck>().Cards.Clear();
        SetFiveObject.GetComponent<SetDeck>().Cards.Clear();
        SetSixObject.GetComponent<SetDeck>().Cards.Clear();
        SetSevenObject.GetComponent<SetDeck>().Cards.Clear();
        await UniTask.WaitUntil(() => { 
            return BaseDeckFour.GetComponent<BaseDeck>().BaseCards.Count == 0 &&
            BaseDeckThree.GetComponent<BaseDeck>().BaseCards.Count == 0 &&
            BaseDeckTwo.GetComponent<BaseDeck>().BaseCards.Count == 0 &&
            BaseDeckOne.GetComponent<BaseDeck>().BaseCards.Count == 0 && 
            SetOneObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetTwoObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetThreeObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetFourObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetFiveObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetSixObject.GetComponent<SetDeck>().Cards.Count == 0 &&
            SetSevenObject.GetComponent<SetDeck>().Cards.Count == 0
            ; });
        await StartCardGame();
        await MainDeck.mainDeck.GetComponent<CardObject>().BackOfCard();
        ShowCard.showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase();
    }

    public static SetDeck GetSetDeck(CardObject cardObj)
    {
        Dictionary<string, SetDeck> setDeck = new Dictionary<string, SetDeck>()
        {
            {"SetOne", SetOneObject.GetComponent<SetDeck>() },
            {"SetTwo", SetTwoObject.GetComponent<SetDeck>() },
            {"SetThree", SetThreeObject.GetComponent<SetDeck>() },
            {"SetFour", SetFourObject.GetComponent<SetDeck>() },
            {"SetFive", SetFiveObject.GetComponent<SetDeck>() },
            {"SetSix", SetSixObject.GetComponent<SetDeck>() },
            {"SetSeven", SetSevenObject.GetComponent<SetDeck>() },
        };
        bool tryGetSetDeck = setDeck.TryGetValue(cardObj.transform.parent.name, out SetDeck set);
        return tryGetSetDeck ? set : null;
    }

    public static void CheckForSolve()
    {
        if (!(MainDeck.Cards.Count ==0 && MainDeck.UsedCards.Count == 0))
        {
            return;
        }
        if (SetOneObject.GetComponent<SetDeck>().NoHiddenCardsInSet && 
        SetTwoObject.GetComponent<SetDeck>().NoHiddenCardsInSet &&
        SetThreeObject.GetComponent<SetDeck>().NoHiddenCardsInSet &&
        SetFourObject.GetComponent<SetDeck>().NoHiddenCardsInSet &&
        SetFiveObject.GetComponent<SetDeck>().NoHiddenCardsInSet &&
        SetSixObject.GetComponent<SetDeck>().NoHiddenCardsInSet &&
        SetSevenObject.GetComponent<SetDeck>().NoHiddenCardsInSet )
        {
            ShowCard.ChangeToSolve();
        }
    }

    public static async UniTask SolveForWin()
    {
        CardHold.ToggleCollider(false);
        PreventAction.SetActive(true);
        var AllSetsList = new List<SetDeck>()
        {
            SetOneObject.GetComponent<SetDeck>(),
            SetTwoObject.GetComponent<SetDeck>(),
            SetThreeObject.GetComponent<SetDeck>(),
            SetFourObject.GetComponent<SetDeck>(),
            SetFiveObject.GetComponent<SetDeck>(),
            SetSixObject.GetComponent<SetDeck>(),
            SetSevenObject.GetComponent <SetDeck>()
        };
        var BaseOne = BaseDeckOne.GetComponent<BaseDeck>();
        var BaseTwo = BaseDeckTwo.GetComponent<BaseDeck>();
        var BaseThree = BaseDeckThree.GetComponent<BaseDeck>();
        var BaseFour = BaseDeckFour.GetComponent<BaseDeck>();
        BaseOne.IsTarget = true;
        BaseTwo.IsTarget = true;
        BaseThree.IsTarget = true;
        BaseFour.IsTarget = true;


        for (int i = 0; i < AllSetsList.Count; i++)
        {
            await UniTask.Delay(5);           
            var CardsInSet = AllSetsList[i].gameObject.GetComponentsInChildren<CardObject>();
            for (int j = CardsInSet.Length - 1; j >= 0; j--)
            {
                if (BaseOne.AddBaseCard(CardsInSet[j].card))
                {
                    AllSetsList[i].Cards.Remove(CardsInSet[j].card);
                    Destroy(CardsInSet[j].gameObject);
                }
                else if (BaseTwo.AddBaseCard(CardsInSet[j].card))
                {

                    AllSetsList[i].Cards.Remove(CardsInSet[j].card);
                    Destroy(CardsInSet[j].gameObject);
                }
                else if (BaseThree.AddBaseCard(CardsInSet[j].card))
                {
                    AllSetsList[i].Cards.Remove(CardsInSet[j].card);
                    Destroy(CardsInSet[j].gameObject);
                }
                else if (BaseFour.AddBaseCard(CardsInSet[j].card))
                {
                    AllSetsList[i].Cards.Remove(CardsInSet[j].card);
                    Destroy(CardsInSet[j].gameObject);
                }
                break;
            }

            if (i == 6 && !PlayerWon)
            {
                i = -1;
            }
        }
    }
}
