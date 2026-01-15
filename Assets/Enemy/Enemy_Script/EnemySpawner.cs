using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public List<GameObject> Enemy = new List<GameObject>();
    public int numberOfEnemies = 5;
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 Spawnposition = new Vector3(Random.Range(-100, 100), 2, Random.Range(-100, 100));
            GameObject newEnemy = Instantiate(EnemyPrefab, Spawnposition, Quaternion.identity);
            Enemy.Add(newEnemy);

        }
    }

    
}
