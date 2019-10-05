﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    [SerializeField] float spawnRate;
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
}
