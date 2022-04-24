using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GamePlayerManager player;
    public GamePlayerManager enemy;

    [SerializeField] AI enemyAI;
    [SerializeField] UIManager uiManager;

    public Transform playerHandTransform,
                               playerFieldTransform,
                               enemyHandTransform,
                               enemyFieldTransform;

    [SerializeField] CardController cardPrefab;

    public bool isPlayerTurn;
    public Transform playerHero;
    public Transform enemyHero;
    public int turn;


    // 時間管理
    public int timeCount;

    // シングルトン化（どこからでもアクセスできるようにする）
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public void StartGame()
    {
        turn = 0;
        uiManager.HideResultPanel();
        player.Init(new List<int>() { 1, 2, 3, 4});
        enemy.Init(new List<int>() {9, 10, 11, 12 });

        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
        SettingInitHand();
        int flag = UnityEngine.Random.Range(0, 2);
        if (flag == 0)
        {
            isPlayerTurn = true;
        }
        else
        {
            isPlayerTurn = false;
        }
        TurnCalc();
    }


    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            player.manaCost -= cost;
        }
        else
        {
            enemy.manaCost -= cost;
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
    }

    public void Restart()
    {
        // handとFiledのカードを削除
        foreach (Transform card in playerHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in playerFieldTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyFieldTransform)
        {
            Destroy(card.gameObject);
        }

        // デッキを生成
        //player.deck = new List<int>() {};
        //enemy.deck = new List<int>() {};

        StartGame();
    }

    public void SettingInitHand()
    {
        // カードをそれぞれに3まい配る
        for (int i = 0; i < 2; i++)
        {
            GiveCardToHand(player.deck, playerHandTransform);
            GiveCardToHand(enemy.deck, enemyHandTransform);
        }
    }

    void GiveCardToHand(List<int> deck, Transform hand)
    {
        if (deck.Count == 0)
        {
            return;
        }
        int i = UnityEngine.Random.Range(0, deck.Count);
        int cardID = deck[i];
        //deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }

    void CreateCard(int cardID, Transform hand)
    {
        // カードの生成とデータの受け渡し
        CardController card = Instantiate(cardPrefab, hand, false);
        if (hand.name == "PlayerHand")
        {
            card.Init(cardID, true);
        }
        else
        {
            card.Init(cardID, false);
        }
    }

    public void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        timeCount = 20;
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1); // 1秒待機
            timeCount--;
            uiManager.UpdateTime(timeCount);
        }

    }

    public CardController[] GetEnemyFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
    }
    public CardController[] GetFriendFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
    }


    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn)
        {
            ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        turn++;
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
        }
        else
        {
            isPlayerTurn = true;
        }

        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false);
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false);


        if (isPlayerTurn)
        {
            player.IncreaseManaCost();
            CardController[] playerhandCardList = playerHandTransform.GetComponentsInChildren<CardController>();
            if (playerhandCardList.Length < 7)
            {
                GiveCardToHand(player.deck, playerHandTransform);
            }
        }
        else
        {
            enemy.IncreaseManaCost();
            CardController[] enemyhandCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
            if (enemyhandCardList.Length < 7)
            {
                GiveCardToHand(enemy.deck, enemyHandTransform);
            }
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
        TurnCalc();
    }

    public void SettingCanAttackView(CardController[] fieldCardList, bool canAttack)
    {
        foreach (CardController card in fieldCardList)
        {
            card.SetCanAttack(canAttack);
        }
    }

    public void CardsBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardsBattle");
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);

        attacker.Attack(defender);
        defender.Attack(attacker);
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);
        attacker.CheckAlive();
        defender.CheckAlive();
    }


    public void AttackToHero(CardController attacker)
    {
        if (isPlayerTurn)
        {
            enemy.heroHp -= attacker.model.at;
        }
        else
        {
            player.heroHp -= attacker.model.at;
        }
        attacker.SetCanAttack(false);
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
    }
    public void HealToHero(CardController healer)
    {
        if (healer.model.isPlayerCard)
        {
            player.heroHp += healer.model.at;
        }
        else
        {
            enemy.heroHp += healer.model.at;
        }
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
    }
    public void CheckHeroHP()
    {
        if (player.heroHp <= 0 || enemy.heroHp <= 0)
        {
            ShowResultPanel(player.heroHp);
        }
    }
    public void ShowResultPanel(int heroHp)
    {
        StopAllCoroutines();
        uiManager.ShowResultPanel(heroHp);
    }
}
