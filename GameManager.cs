using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance;

    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private GameObject winnerPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }

    public void ShowWinner(PlayerRef winner)
    {
        winnerText.text = "Player " + winner.PlayerId + " Wins!";
        winnerPanel.SetActive(true);
        StartCoroutine(HideWinner());
    }

    public IEnumerator HideWinner()
    {
        yield return new WaitForSeconds(5f);
        winnerPanel.SetActive(false);
    }
}
