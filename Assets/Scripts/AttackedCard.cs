using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側
public class AttackedCard : MonoBehaviour, IDropHandler
{
    [SerializeField] GameObject Flash_03;

    public void OnDrop(PointerEventData eventData)
    {
        /* 攻撃 */
        // attackerカードを選択
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>();
        // defenderカードを選択（Playerフィールドから選択）
        CardController defender = GetComponent<CardController>();

        if (attacker == null || defender == null)
        {
            return;
        }

        if (attacker.model.isPlayerCard == defender.model.isPlayerCard)
        {
            return;
        }

        //　シールドカードがあればシールドカード以外は攻撃できない
        CardController[] enemyFieldCards = GameManager.instance.GetEnemyFieldCards(attacker.model.isPlayerCard);
        if (Array.Exists(enemyFieldCards, card => card.model.ability == ABILITY.SHIELD)
            && defender.model.ability != ABILITY.SHIELD)
        {
            return;
        }

        if (attacker.model.canAttack)
        {
            // attackerとdefenderを戦わせる
            GameManager.instance.CardsBattle(attacker, defender);

            //エフェクトの発生
            GameObject effect = Instantiate(Flash_03, transform.position, Quaternion.identity) as GameObject;

            Destroy(effect, 0.2f);
        }

    }
}