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
    [SerializeField] Transform spawnPoint;
    [SerializeField] Collider2D MainCollider;
    [SerializeField] SpriteRenderer deathMarkRenderer;
    private GameBoard board;



    void Start()
    {
        //Flip mine
        if (transform.position.x > 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        treasureSprite.sprite = ore.UISprite;
        
        StartCoroutine(spawnCycle());
    }
    IEnumerator spawnCycle()
    {
        while (true)
        {
            if (board == null)
                board = FindObjectOfType<GameBoard>();
            
            if(Random.Range(0f, 1f) <= TrashRate)
            {
                //Spawn trash
                board.spawnFollower(spawnPoint.position, board.GetRandomTrash());
            }
            else
            {
                //Spawn treasure
                board.spawnFollower(spawnPoint.position, ore);
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
    }
}
