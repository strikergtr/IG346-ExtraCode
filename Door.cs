using UnityEngine;

public class Door : MonoBehaviour
{
   
    void Update()
    {
        if (GM.Instance.sw1 == true && GM.Instance.sw2 == true) 
        {
            Destroy(gameObject);
        } 
    }
}
