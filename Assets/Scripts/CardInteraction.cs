using UnityEngine;

public class CardInteraction : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = previousColor;
    }
}