using Fusion;
using UnityEngine;

public class Sw1 : NetworkBehaviour
{
    public Color off_color;
    public Color on_color;
    public SpriteRenderer imageSwitch;
    [Networked]
    public bool isOn { get; set; }

    public bool isSpawn = false;

    public override void Spawned()
    {
        isSpawn = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") 
        {
            if (HasStateAuthority) 
            {
                isOn = true;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") 
        {
            if (HasStateAuthority)
            {
                isOn = false;
            }
        }
    }

    void Update()
    {
        if (isSpawn == false) 
        {
            return;
        }

        if (isOn)
        {
            imageSwitch.color = on_color;
            GM.Instance.sw1 = true;
        }
        else 
        {
            imageSwitch.color = off_color;
            GM.Instance.sw1 = false;
        }
    }
}
