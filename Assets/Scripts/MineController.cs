using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    [SerializeField] float spawnRate;
    [SerializeField] float TrashRate;

    public TreasureInfo ore;
    public SpriteRenderer MainRenderer;
    public SpriteRenderer treasureSprite;
    public bool isRandom;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Collider2D MainCollider;
    [SerializeField] SpriteRenderer deathMarkRenderer;
    private GameBoard board;

    private bool isTargeted;
    
    void Start()
    {
        if (board == null)
            board = FindObjectOfType<GameBoard>();

        board.MineCount += 1;

        var closestRoadPos = board.GetClosestPoint(spawnPoint.position).PointPosition;

        //Flip mine
        if (closestRoadPos.x < spawnPoint.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        if(isRandom)
        {
            treasureSprite.gameObject.SetActive(false);
        }
        else
        {
            treasureSprite.sprite = ore.UISprite;
        }
        
        StartCoroutine(spawnCycle());
    }
    IEnumerator spawnCycle()
    {
        while (true)
        {
            if (board == null)
                board = FindObjectOfType<GameBoard>();

            while (board.HasGameEnded)
            {
                yield return null;
            }

            if (Random.Range(0f, 1f) <= TrashRate)
            {
                //Spawn trash
                board.spawnFollower(spawnPoint.position, board.GetRandomTrash());
            }
            else
            {
                //Spawn treasure
                if(isRandom)
                {
                    board.spawnFollower(spawnPoint.position, board.GetRandomTreasure());
                }
                else
                {
                    board.spawnFollower(spawnPoint.position, ore);
                }
            }

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

        isTargeted = true;
    }

    private void OnDestroy()
    {
        if (isTargeted)
            board.CenterDragon.PlayFireballHitSound();

        board.MineCount -= 1;
    }
}
