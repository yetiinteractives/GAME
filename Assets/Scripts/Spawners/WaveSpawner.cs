using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class WaveSpawner : MonoBehaviour
{
    public  UniversalEnemyAi enemyAi;

    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;

    public int enemiesPerWave = 5;
    public float spawnDelay =2f;
    public float timeBetweenWaves = 5f;

    int currentWave =1;
    int enemiesAlive =0;
    int enemiesDead =0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyAi.OnEnemyDeathEvent += EnemyDied;
    }
    IEnumerator StartWave()
    {
        Debug.Log("Wave:"+currentWave);
        for(int i=0; i<enemiesPerWave; i++)
        {
            if(currentWave == 1) spawnJinga();
            else spawnEnemy();

            
            enemiesAlive++;
            yield return new WaitForSeconds(spawnDelay);
        }

        while (enemiesAlive > 0)
        {
            yield return null;
        }

        Debug.Log("Wave "+currentWave+" completed!");
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        enemiesPerWave += 3;
        spawnDelay += 1f;


        StartCoroutine(StartWave());
        
    }
    void spawnJinga()
    {
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefabs[0], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
        
    }
    void spawnEnemy()
    {
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefabs[randomEnemy], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
    }
      
    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesDead++;
    }

    public void OnDisable()
    {
        enemyAi.OnEnemyDeathEvent -= EnemyDied;

    }
}
