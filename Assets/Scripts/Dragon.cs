using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dragon : MonoBehaviour
{
    public Transform FireballOrigin;
    public CircleCollider2D FireballArea;
    public Fireball FireballPrefab;

    public GameObject[] GoldPiles;

    public int GoldCoins;
    public int MaxGoldCoins;
    public float GoldRatio { get { return (float)GoldCoins / (float)MaxGoldCoins; } }

    public float Rage;
    public float MaxRage;
    public float RageRatio { get { return Rage / MaxRage; } }

    public float Greed;
    public float MaxGreed;
    public float GreedRatio { get { return Greed / MaxGreed; } }

    void Update()
    {
        //Shoot fire ball when right clicking on followers within range
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(FireballArea.OverlapPoint(mousePos))
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, 0f, LayerMask.GetMask("Follower"));

                if (hit)
                {
                    var target = hit.transform;
                    var newFireball = Instantiate(FireballPrefab, FireballOrigin.position, Quaternion.identity, null);
                    newFireball.Target = target;
                    newFireball.LastKnownPosition = target.position;
                }
            }
        }

        //Update gold piles
        int pileCount = Mathf.FloorToInt(GoldRatio * 10f);
        for (int i = 0; i < 10; i++)
        {
            GoldPiles[i].SetActive(i < pileCount);
        }

        //Check victory / defeat
        if (RageRatio > 0.999f)
        {
            //Game over
            SceneManager.LoadScene("GameOver");
        }

        if (GoldRatio > 0.999f)
        {
            //Victory
            SceneManager.LoadScene("Victory");
        }

    }
}
