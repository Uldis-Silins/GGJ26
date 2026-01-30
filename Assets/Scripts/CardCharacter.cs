using System;
using UnityEngine;

public class CardCharacter : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = previousColor;
    }
}