using UnityEngine;

public class CardEnemy : MonoBehaviour
{
    public int health = 100;
    
    [SerializeField] private Animator m_animator;

    public void AttackPlayer(bool isLarge)
    {
        m_animator.SetTrigger(isLarge ? "attackBig" : "attackSmall");
    }

    public void Hit(int damage)
    {
        m_animator.SetTrigger(damage > 5 ? "hitBig" : "hitSmall");
        health -= damage;

        if (health <= 0)
        {
            m_animator.SetTrigger("death");
        }
    }
}