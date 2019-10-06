using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    [SerializeField] float spawnRate;
    public SpriteRenderer MainRenderer;
    public SpriteRenderer treasureSprite;
    [SerializeField] Collider2D MainCollider;
    [SerializeField] SpriteRenderer deathMarkRenderer;
    private GameBoard board;

    void Start()
    {
        StartCoroutine(spawnCycle());
    }
    IEnumerator spawnCycle()
    {
        while (true)
        {
            if (board == null)
                board = FindObjectOfType<GameBoard>();

            board.spawnFollower(this.transform.position);
            yield return new WaitForSeconds(60 / spawnRate);
        }
    }

    public void OnTargeted()
    {
        //show death mark
        deathMarkRenderer.gameObject.SetActive(true);

        //push sprites to the back
        MainRenderer.sortingOrder -= 10;
        deathMarkRenderer.sortingOrder -= 10;
        if (treasureSprite != null)
            treasureSprite.sortingOrder -= 10;

        //disable collider
        if (MainCollider != null)
            MainCollider.enabled = false;
    }
}
