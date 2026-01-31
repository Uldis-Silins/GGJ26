using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CardCharacter : MonoBehaviour
{
    public UnityEvent<CardInteraction> onInteractionReached;
    public UnityEvent onKilled;
    
    public enum CardFaceTypes { None, Joker, Jack, Queen, King }
    
    public CardData.CardSuitType suit;
    public CardFaceTypes face;
    
    public NavMeshAgent agent;
    
    [SerializeField] private Animator m_animator;
    [SerializeField] private CardData.CardSuitType m_strongAttackSuit;
    [SerializeField] private CardData.CardSuitType m_weakAttackSuit;
    [SerializeField] private CardData.CardSuitType m_strongHitSuit;
    [SerializeField] private CardData.CardSuitType m_weakHitSuit;
    
    private CardInteraction m_moveTargetInteraction;
    
    private static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");
    private static readonly int AttackBig = Animator.StringToHash("attackBig");
    private static readonly int AttackSmall = Animator.StringToHash("attackSmall");
    private static readonly int HitBig = Animator.StringToHash("hitBig");
    private static readonly int HitSmall = Animator.StringToHash("hitSmall");

    private int m_hp;
    private Coroutine m_interactionSequenceCoroutine;
    
    public bool IsDead { get; private set; }
    public CardEnemy TargetEnemy { get; set; }
    public int HP => m_hp;
    
    private void OnDrawGizmos()
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = previousColor;
    }

    private void OnValidate()
    {
        switch (suit)
        {
            case CardData.CardSuitType.Clubs:
                m_strongAttackSuit = CardData.CardSuitType.Clubs;
                m_weakAttackSuit = CardData.CardSuitType.Spades;
                m_strongHitSuit = CardData.CardSuitType.Diamonds;
                m_weakHitSuit = CardData.CardSuitType.Hearts;
                break;
            case CardData.CardSuitType.Spades:
                m_strongAttackSuit = CardData.CardSuitType.Spades;
                m_weakAttackSuit = CardData.CardSuitType.Spades;
                m_strongHitSuit = CardData.CardSuitType.Hearts;
                m_weakHitSuit = CardData.CardSuitType.Diamonds;
                break;
            case CardData.CardSuitType.Hearts:
                m_strongAttackSuit = CardData.CardSuitType.Hearts;
                m_weakAttackSuit = CardData.CardSuitType.Diamonds;
                m_strongHitSuit = CardData.CardSuitType.Spades;
                m_weakHitSuit = CardData.CardSuitType.Clubs;
                break;
            case CardData.CardSuitType.Diamonds:
                m_strongAttackSuit = CardData.CardSuitType.Diamonds;
                m_weakAttackSuit = CardData.CardSuitType.Hearts;
                m_strongHitSuit = CardData.CardSuitType.Clubs;
                m_weakHitSuit = CardData.CardSuitType.Spades;
                break;
        }
    }

    private void Reset()
    {
        m_animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if(IsDead) return;
        
        if (m_moveTargetInteraction != null && agent.remainingDistance <= agent.stoppingDistance &&
            m_interactionSequenceCoroutine == null)
        {
            if (Vector3.Angle(transform.position + transform.forward,
                    transform.position - TargetEnemy.transform.position) >= 5f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(TargetEnemy.transform.position - transform.position);
                transform.rotation = targetRotation;
            }

            m_interactionSequenceCoroutine = StartCoroutine(DoInteractionSequence());
            m_moveTargetInteraction = null;
        }

        if(m_animator) m_animator.SetFloat(MoveSpeed, agent.velocity.magnitude);
    }

    public void SetDestination(CardInteraction interaction)
    {
        m_moveTargetInteraction = interaction;
        agent.SetDestination(interaction.transform.position);
    }
    
    private void Interact()
    {
        if (m_moveTargetInteraction.suit == m_strongAttackSuit)
        {
            m_hp += m_moveTargetInteraction.value;
            m_animator.SetTrigger(AttackBig);
            TargetEnemy.Hit(m_moveTargetInteraction.value);
        }
        else if (m_moveTargetInteraction.suit == m_weakAttackSuit)
        {
            m_hp += m_moveTargetInteraction.value;
            m_animator.SetTrigger(AttackSmall);
            TargetEnemy.Hit(m_moveTargetInteraction.value);
        }
        else if (m_moveTargetInteraction.suit == m_strongHitSuit)
        {
            m_hp -= m_moveTargetInteraction.value;
            m_animator.SetTrigger(HitBig);
            TargetEnemy.AttackPlayer(true);
        }
        else if (m_moveTargetInteraction.suit == m_weakHitSuit)
        {
            m_hp -= m_moveTargetInteraction.value;
            m_animator.SetTrigger(HitSmall);
            TargetEnemy.AttackPlayer(false);
        }
        
        if (m_hp <= 0)
        {
            onKilled?.Invoke();
            m_animator.SetTrigger("dead");
            IsDead = true;
        }
    }
    
    private IEnumerator DoInteractionSequence()
    {
        Interact(); 
        
        yield return new WaitForSeconds(1.5f);
        onInteractionReached?.Invoke(m_moveTargetInteraction);
        m_interactionSequenceCoroutine = null;
    }
}
