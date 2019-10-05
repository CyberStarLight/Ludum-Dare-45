using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    [SerializeField] float spawnRate;
    public Tresure ore;
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

            board.spawnFollower(this.transform.position, ore);
            yield return new WaitForSeconds(60 / spawnRate);
        }
    }
}
