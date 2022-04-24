using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class AgentsTest : Agent
{
    GameManager gameManager;
    int max_hands = 8;
    int max_field = 5;

    public override void OnEpisodeBegin()
    {
        gameManager = GameManager.instance;
        gameManager.Restart();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (gameManager.isPlayerTurn)
        {
            sensor.AddObservation(gameManager.player.heroHp);
            sensor.AddObservation(gameManager.enemy.heroHp);
            CardController[] enemyhandCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
            sensor.AddObservation(enemyhandCardList.Length);
            CardController[] playrehandCardList = gameManager.playerHandTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, playrehandCardList, max_hands);
            CardController[] enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, enemyFieldCardList, max_field);
            CardController[] playerFieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, playerFieldCardList, max_field);
            sensor.AddObservation(gameManager.player.manaCost);
        }
        else
        {
            sensor.AddObservation(gameManager.player.heroHp);
            sensor.AddObservation(gameManager.enemy.heroHp);
            CardController[] playerhandCardList = gameManager.playerHandTransform.GetComponentsInChildren<CardController>();
            sensor.AddObservation(playerhandCardList.Length);
            CardController[] enemyrehandCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, enemyrehandCardList, max_hands);
            CardController[] enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, enemyFieldCardList, max_field);
            CardController[] playerFieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
            AddCardlist(sensor, playerFieldCardList, max_field);
            sensor.AddObservation(gameManager.enemy.manaCost);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int SelectCard = (int)Math.Round((actionBuffers.ContinuousActions[0] + 1.0f) * 6) - 1;
        int Attack = (int)Math.Round((actionBuffers.ContinuousActions[1] + 1.0f) * 6) - 1;
        int AttackCard = (int)Math.Round((actionBuffers.ContinuousActions[2] + 1.0f) * 6) - 1;
        
        if (gameManager.isPlayerTurn)
        {
            int i;
            CardController[] enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
            gameManager.SettingCanAttackView(enemyFieldCardList, true);
            CardController[] fieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
            CardController[] handCardList = gameManager.playerHandTransform.GetComponentsInChildren<CardController>();
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => (card.model.cost <= gameManager.enemy.manaCost) && (!card.IsSpell || (card.IsSpell && card.CanUseSpell())));
            Debug.Log(SelectCard);
            Debug.Log(gameManager.timeCount);
            while (gameManager.timeCount > 0)
            {
                Debug.Log(SelectCard);
                if (SelectCard != -1)/*カードを場に出す*/
                {
                    if (Array.Exists(handCardList, card => card.model.label == SelectCard))
                    {
                        i = Array.FindIndex(handCardList, card => card.model.label == SelectCard);
                        CardController selectCard = handCardList[i];
                        if (selectCard.model.cost > gameManager.player.manaCost)
                        {
                            AddReward(((float)gameManager.player.manaCost - (float)selectCard.model.cost) / 10.0f);
                        }
                        else
                        {
                            selectCard.Show();
                            StartCoroutine(selectCard.movement.MoveToField(gameManager.playerFieldTransform));
                            Wait(0.25f);
                            selectCard.OnFiled();
                            AddReward((float)(selectCard.model.hp + selectCard.model.at) / 2.0f);
                            handCardList = gameManager.playerHandTransform.GetComponentsInChildren<CardController>();
                        }
                    }
                }
                else/*カード選択の修了*/
                {
                    break;
                }
            }
            Wait(0.25f);
            while (gameManager.timeCount > 0)
            {
                if (Attack != -1)/*攻撃を行う*/
                {
                    if (Array.Exists(fieldCardList, card => card.model.label == Attack))
                    {
                        i = Array.FindIndex(fieldCardList, card => card.model.label == Attack);
                        CardController attacker = fieldCardList[i];
                        AddReward(1.0f);
                        if (AttackCard == -1)
                        {
                            StartCoroutine(attacker.movement.MoveToTarget(gameManager.enemyHero));
                            Wait(0.25f);
                            gameManager.AttackToHero(attacker);
                            if (gameManager.enemy.heroHp <= 0)
                            {
                                AddReward(5.0f);
                                EndEpisode();
                            }
                            gameManager.CheckHeroHP();
                            fieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
                            AddReward((float)attacker.model.at);
                        }
                        else
                        {
                            if (Array.Exists(enemyFieldCardList, card => card.model.label == AttackCard))
                            {
                                i = Array.FindIndex(enemyFieldCardList, card => card.model.label == AttackCard);
                                Debug.Log(enemyFieldCardList.Length);
                                Debug.Log(i);
                                CardController defender = enemyFieldCardList[i];
                                StartCoroutine(attacker.movement.MoveToTarget(defender.transform));
                                Wait(0.25f);
                                gameManager.CardsBattle(attacker, defender);
                                Wait(0.25f);
                                enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
                                fieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
                                AddReward(((float)attacker.model.at - (float)defender.model.at) / 10.0f);
                            }
                            else
                            {
                                AddReward(-0.5f);
                            }
                        }
                    }
                    else
                    {
                        AddReward(-0.5f);
                    }
                }
                else
                {
                    break;
                }
            }
            if (handCardList.Length > max_hands)
            {
                AddReward(-0.5f);
                Debug.Log("End");
                EndEpisode();
            }
            gameManager.ChangeTurn();
        }
        else
        {
            int i;
            CardController[] enemyFieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
            gameManager.SettingCanAttackView(enemyFieldCardList, true);
            CardController[] fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
            CardController[] handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => (card.model.cost <= gameManager.enemy.manaCost) && (!card.IsSpell || (card.IsSpell && card.CanUseSpell())));

            while (gameManager.timeCount > 0)
            {
                if (SelectCard != -1)/*カードを場に出す*/
                {
                    if (Array.Exists(handCardList, card => card.model.label == SelectCard))
                    {
                        i = Array.FindIndex(handCardList, card => card.model.label == SelectCard);
                        CardController selectCard = handCardList[i];
                        if (selectCard.model.cost > gameManager.enemy.manaCost)
                        {
                            AddReward(((float)gameManager.enemy.manaCost - (float)selectCard.model.cost) / 10.0f);
                        }
                        else
                        {
                            selectCard.Show();
                            StartCoroutine(selectCard.movement.MoveToField(gameManager.enemyFieldTransform));
                            Wait(0.25f);
                            selectCard.OnFiled();
                            AddReward((float)(selectCard.model.hp + selectCard.model.at) / 2.0f);
                            handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
                        }
                    }
                }
                else/*カード選択の修了*/
                {
                    break;
                }
            }
            while (gameManager.timeCount > 0)
            {
                if (Attack != -1)/*攻撃を行う*/
                {
                    if (Array.Exists(fieldCardList, card => card.model.label == Attack))
                    {
                        i = Array.FindIndex(fieldCardList, card => card.model.label == Attack);
                        CardController attacker = fieldCardList[i];
                        AddReward(1.0f);
                        if (AttackCard == -1)
                        {
                            StartCoroutine(attacker.movement.MoveToTarget(gameManager.playerHero));
                            Wait(0.25f);
                            gameManager.AttackToHero(attacker);
                            if (gameManager.enemy.heroHp <= 0)
                            {
                                AddReward(5.0f);
                                EndEpisode();
                            }
                            gameManager.CheckHeroHP();
                            fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
                            AddReward((float)attacker.model.at);
                        }
                        else
                        {
                            if (Array.Exists(enemyFieldCardList, card => card.model.label == AttackCard))
                            {
                                i = Array.FindIndex(enemyFieldCardList, card => card.model.label == AttackCard);
                                CardController defender = enemyFieldCardList[i];
                                StartCoroutine(attacker.movement.MoveToTarget(defender.transform));
                                gameManager.CardsBattle(attacker, defender);
                                enemyFieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();
                                fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
                                AddReward(((float)attacker.model.at - (float)defender.model.at) / 10.0f);
                            }
                            else
                            {
                                AddReward(-0.5f);
                            }
                        }
                    }
                    else
                    {
                        AddReward(-0.5f);
                    }
                }
                else
                {
                    break;
                }
            }
            if (handCardList.Length > max_hands)
            {
                AddReward(-0.5f);
                EndEpisode();
            }
            gameManager.ChangeTurn();
        }

    }

    IEnumerator CastSpellOf(CardController card)
    {
        CardController target = null;
        Transform movePosition = null;
        switch (card.model.spell)
        {
            case SPELL.DAMAGE_ENEMY_CARD:
                target = gameManager.GetEnemyFieldCards(card.model.isPlayerCard)[0];
                movePosition = target.transform;
                break;
            case SPELL.HEAL_FRIEND_CARD:
                target = gameManager.GetFriendFieldCards(card.model.isPlayerCard)[0];
                movePosition = target.transform;
                break;
            case SPELL.DAMAGE_ENEMY_CARDS:
                movePosition = gameManager.playerFieldTransform;
                break;
            case SPELL.HEAL_FRIEND_CARDS:
                movePosition = gameManager.enemyFieldTransform;
                break;
            case SPELL.DAMAGE_ENEMY_HERO:
                movePosition = gameManager.playerHero;
                break;
            case SPELL.HEAL_FRIEND_HERO:
                movePosition = gameManager.enemyHero;
                break;

        }
        // 移動先としてターゲット/それぞれのフィールド/それぞれのHeroのTransformが必要
        StartCoroutine(card.movement.MoveToField(movePosition));
        yield return new WaitForSeconds(0.25f);
        card.UseSpellTo(target); // カードを使用したら破壊する
    }

    void AddCardlist(VectorSensor sensor, CardController[] CardList, int max)
    {
        int i = 0;
        while (CardList.Length > i)
        {
            sensor.AddObservation(CardList[i].model.label);
            i++;
        }
        i = CardList.Length;
        while (i < max)
        {
            sensor.AddObservation(0);
            i++;
        }
    }
    private IEnumerator Wait(float time)
    {

        yield return new WaitForSeconds(time);

    }
}
