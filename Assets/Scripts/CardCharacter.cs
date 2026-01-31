using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CardCharacter : MonoBehaviour
{
    public UnityEvent<CardInteraction> onInteractionReached;
    
    public enum CardFaceTypes { None, Joker, Jack, Queen, King }
    
    public CardData.CardSuitType suit;
    public CardFaceTypes face;
    
    public NavMeshAgent agent;
    
    private CardInteraction m_moveTargetInteraction;
    
    public bool IsDead { get; private set; }
    
    private void OnDrawGizmos()
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = previousColor;
    }

    private void Update()
    {
        if (m_moveTargetInteraction != null && agent.remainingDistance <= agent.stoppingDistance)
        {
            onInteractionReached?.Invoke(m_moveTargetInteraction);
            m_moveTargetInteraction = null;
        }
    }

    public void SetDestination(CardInteraction interaction)
    {
        m_moveTargetInteraction = interaction;
        agent.SetDestination(interaction.transform.position);
    }
}
