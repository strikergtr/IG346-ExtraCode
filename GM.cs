using UnityEngine;

public class GM : MonoBehaviour
{
    public static GM Instance;
    public bool sw1 = false;
    public bool sw2 = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
