using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;

public class GameManager1 : MonoBehaviour
{
    public TMP_Text youwinText;
    public GameObject youwinObj;

    public static GameManager1 instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    public void ShowWinner(PlayerRef player) 
    {
        youwinText.text = "Player "+player.PlayerId+" Win!! \r\n Wait 5 second for next Round.";
        youwinObj.SetActive(true);
        StartCoroutine(HideWinner());
    }
    public IEnumerator HideWinner() 
    {
        yield return new WaitForSeconds(5);
        youwinObj.SetActive(false);
    }
}
