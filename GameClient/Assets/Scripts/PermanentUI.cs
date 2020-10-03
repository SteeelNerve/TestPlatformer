using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PermanentUI : MonoBehaviour
{
    // Player stats
    public int cherries = 0;
    public int health = 5;
    public TextMeshProUGUI cherryText;
    public TextMeshProUGUI healthAmount;

    public static PermanentUI perm;

    private void Start()
    {

        cherryText.text = "X " + cherries.ToString();
        healthAmount.text = "Health " + health.ToString();
        DontDestroyOnLoad(gameObject);
        // singleton
        if (!perm)
        {
            perm = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Reset()
    {
        cherries = 0;
        cherryText.text = "X " + cherries.ToString();
    }
}
