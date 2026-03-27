using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using TMPro;



public class WaveSpawner : MonoBehaviour
{
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI timeText;


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
        waveText.text = "Wave: " + currentWave;
        Debug.Log("Wave:"+currentWave);


        StartCoroutine(ShowWaveStart());
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
        float countdown = timeBetweenWaves;

    while (countdown > 0)
   {
      timeText.text = "Next Wave in: " + Mathf.Ceil(countdown);
      countdown -= Time.deltaTime;
      yield return null;
   }

        timeText.text = "";

        currentWave++;
        enemiesPerWave += 3;
        spawnDelay -= 0.1f;
        spawnDelay = Mathf.Clamp(spawnDelay, 0.5f, 5f);


        StartCoroutine(StartWave());
        
    }
    void spawnJinga()
    {
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        GameObject enemy = Instantiate(enemyPrefabs[0], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
        UniversalEnemyAi enemyAi = enemy.GetComponent<UniversalEnemyAi>();
        enemyText.text = "Enemies: " + enemiesAlive;
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
        enemyText.text = "Enemies Alive: " + enemiesAlive;
        if (enemyAi != null)
        {
            enemyAi.OnEnemyDeathEvent += EnemyDied;
        }
    }
      
       IEnumerator ShowWaveStart()
    {
        timeText.text = "WAVE " + currentWave + " START!";
        yield return new WaitForSeconds(2f);
        timeText.text = "";
   }
    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesDead++;

        enemyText.text = "Enemies: " + enemiesAlive;
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
