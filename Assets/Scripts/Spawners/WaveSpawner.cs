using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class WaveSpawner : MonoBehaviour
{

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
        StartCoroutine(StartWave());
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
        GameObject enemy = Instantiate(enemyPrefabs[0], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
        UniversalEnemyAi enemyAi = enemy.GetComponent<UniversalEnemyAi>();
        if (enemyAi != null)
        {
            enemyAi.OnEnemyDeathEvent += EnemyDied;
        }
    }
    void spawnEnemy()
    {
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        GameObject enemy = Instantiate(enemyPrefabs[randomEnemy], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
        UniversalEnemyAi enemyAi = enemy.GetComponent<UniversalEnemyAi>();
        if (enemyAi != null)
        {
            enemyAi.OnEnemyDeathEvent += EnemyDied;
        }
    }
      
    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesDead++;
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
