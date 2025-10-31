using Fusion;
using UnityEngine;

public class Coiin : NetworkBehaviour
{
    [Networked]
    public bool hidecoin  { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HasStateAuthority) 
        {
            hidecoin = true;
        }
    }
    public override void FixedUpdateNetwork() 
    {
        if (HasStateAuthority && hidecoin == true) 
        {
            Destroy(gameObject);
        }
    }
}
