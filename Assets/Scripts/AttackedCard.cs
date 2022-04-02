using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// �U������鑤
public class AttackedCard : MonoBehaviour, IDropHandler
{
    [SerializeField] GameObject Flash_03;

    public void OnDrop(PointerEventData eventData)
    {
        /* �U�� */
        // attacker�J�[�h��I��
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>();
        // defender�J�[�h��I���iPlayer�t�B�[���h����I���j
        CardController defender = GetComponent<CardController>();

        if (attacker == null || defender == null)
        {
            return;
        }

        if (attacker.model.isPlayerCard == defender.model.isPlayerCard)
        {
            return;
        }

        //�@�V�[���h�J�[�h������΃V�[���h�J�[�h�ȊO�͍U���ł��Ȃ�
        CardController[] enemyFieldCards = GameManager.instance.GetEnemyFieldCards(attacker.model.isPlayerCard);
        if (Array.Exists(enemyFieldCards, card => card.model.ability == ABILITY.SHIELD)
            && defender.model.ability != ABILITY.SHIELD)
        {
            return;
        }

        if (attacker.model.canAttack)
        {
            // attacker��defender���킹��
            GameManager.instance.CardsBattle(attacker, defender);

            //�G�t�F�N�g�̔���
            GameObject effect = Instantiate(Flash_03, transform.position, Quaternion.identity) as GameObject;

            Destroy(effect, 0.2f);
        }

    }
}