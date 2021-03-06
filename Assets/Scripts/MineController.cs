﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public float MinSpawnDelay;
    public float MaxSpawnDelay;
    public float TrashRate;

    public TreasureInfo ore;
    public SpriteRenderer MainRenderer;
    public SpriteRenderer treasureSprite;
    public bool isRandom;
    public Transform spawnPoint;
    public Collider2D MainCollider;
    public SpriteRenderer deathMarkRenderer;
    private GameBoard board;

    private bool isTargeted;
    private float nextSpawnTime = 0f;

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

        nextSpawnTime = Time.time + Random.Range(MinSpawnDelay, MaxSpawnDelay);

        board.Mines.Add(this);
    }

    public void FixedUpdate_Managed()
    {
        if(!board.HasGameEnded && !board.ProgressPause && !board.IsSpawnDisabled && Time.time >= nextSpawnTime)
        {
            SpawnFollower();
            float spawnDelay = Random.Range(MinSpawnDelay, MaxSpawnDelay);
            spawnDelay *= 1f - (board.CenterDragon.PanicRatio * 0.5f);
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    private void SpawnFollower()
    {
        if (Time.time < board.NoSpawnBeforeTime)
            return;

        TreasureInfo treasure = Random.Range(0f, 1f) <= TrashRate ? board.GetRandomTrash() : board.GetRandomTreasure();

        if (isRandom)
            board.spawnFollower(spawnPoint.position, treasure);
        else
            board.spawnFollower(spawnPoint.position, ore);
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
