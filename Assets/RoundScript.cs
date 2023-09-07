using UnityEngine;
using UnityEngine.UI;

public class RoundScript : MonoBehaviour
{
    [Header("Components")]
    public Image stocks2Player1;
    public Image stocks1Player1;
    public Image stocks2Player2;
    public Image stocks1Player2;

    [Header("Rounds")]
    public int maxRounds;
    public int currentRounds;
    public int wonRounds1;
    public int wonRounds2;

    [Header("Bools")]
    public bool p1HasWon;
    public bool p2HasWon;
    public bool timer0;

    private void Update()
    {
        CheckingBools();
    }

    private void CheckingBools()
    {
        if (timer0) { currentRounds++; SceneManagerExtended.ReloadScene(); }
        else if (p2HasWon || p1HasWon)
        {
            currentRounds++; SceneManagerExtended.ReloadScene();
        }
        else if (currentRounds > maxRounds)
        {
            SceneManagerExtended.LoadNextScene();
        }
    }

    void PlayerHasWon()
    {
        if (!p1HasWon)
        {
            // Move camera towards player
            // Possibly use timeline (cutscene)


        }
    }
}