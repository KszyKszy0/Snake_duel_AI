using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnviroment:MonoBehaviour
{
    [SerializeField]
    public GameObject emptySquare;
    [SerializeField]
    public Sprite buildSquare;
    [SerializeField]
    public GameObject snakeAiPrefab;
    [SerializeField]
    public GameObject snakePlayerPrefab;
    [SerializeField]
    public Sprite apple;


    public GameObject firstSnake;
    public GameObject secondSnake;


    public fieldInGame[,] map;


    public int applePlacement;
    public int fDimension;
    public int sDimension;
    public int fullSize;
    public int upBound;
    public int bottomBound;
    public int rightBound;
    public int leftBound;

    public float gameSpeed = 0.1f;


    public void init(int mapsize, int mapsize2, float offsetX, float offsetY)
    {
        fDimension=mapsize;
        sDimension=mapsize2;
        fullSize=fDimension*sDimension;
        map = new fieldInGame[mapsize, mapsize2];
        for (int i = 0; i <= mapsize - 1; i++)
        {
            for (int j = 0; j <= mapsize2 - 1; j++)
            {
                var newField = new fieldInGame();
                newField.field = Instantiate(emptySquare, new Vector3(-7.5f + j + offsetX, 3.5f - i + offsetY, 0), Quaternion.identity);
                int result = 0;
                if (j < 9)
                {
                    int a = (i + 1) * 10 + (j + 1);
                    string s = a.ToString();
                    s = s.Insert(1, "0");
                    a = int.Parse(s);
                    result = a;
                }
                else
                {
                    result = (i + 1) * 100 + (j + 1);
                }
                newField.field.GetComponent<emptySquare>().id = result;
                newField.isRoad = false;
                map[i, j] = newField;
            }
        }
        upBound=101;
        bottomBound=fDimension*100+sDimension;
        leftBound=0;
        rightBound=sDimension;
    }
    public bool checkMap()
    {
        int fields = 0;
        for (int i = 0; i <= fDimension-1; i++)
        {
            for (int j = 0; j <= sDimension-1; j++)
            {
                if (map[i, j].isRoad)
                {
                    fields++;
                    if (checkAdj(i, j) < 2)
                        return false;
                }
            }
        }
        if (fields < 10)
            return false;

        return true;
        // StartGame(); rozpocznij/zwróć prawdę
    }
    int checkAdj(int x, int y)
    {
        int result = 0;
        if (x > 0)
        {
            if (map[x - 1, y].isRoad)
                result++;
        }
        if (x < fDimension-1)
        {
            if (map[x + 1, y].isRoad)
                result++;
        }
        if (y > 0)
        {
            if (map[x, y - 1].isRoad)
                result++;
        }
        if (y < sDimension-1)
        {
            if (map[x, y + 1].isRoad)
                result++;
        }
        return result;
    }
    public void moveSecondSnake()
    {
        secondSnake.GetComponent<aiSnake>().move();
    }
    public void DestroySnakes(bool isPlayer)
    {
        if (firstSnake != null)
        {
            if(isPlayer)
            {
                foreach (GameObject G in firstSnake.GetComponent<playerSnake>().snakeBody)
                {
                    Destroy(G);
                }
            }else
            {
                foreach (GameObject G in firstSnake.GetComponent<aiSnake>().snakeBody)
                {
                    Destroy(G);
                }
            }
            
        }
        if (secondSnake != null)
        {
            foreach (GameObject G in secondSnake.GetComponent<aiSnake>().snakeBody)
            {
                Destroy(G);
            }
        }
        Destroy(firstSnake);
        Destroy(secondSnake);
    }


    public void SpawnSnakes()
    {
        arePlaying=true;
        int i = UnityEngine.Random.Range(0, fDimension-1);
        int j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad || map[i, j].isApple)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        firstSnake = Instantiate(snakePlayerPrefab, map[i, j].field.transform.position, Quaternion.identity);
        firstSnake.GetComponent<playerSnake>().start = 100 * (i + 1) + j + 1;
        i = UnityEngine.Random.Range(0, fDimension-1);
        j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad || map[i, j].isApple || 100 * (i + 1) + j + 1 == firstSnake.GetComponent<playerSnake>().start)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        secondSnake = Instantiate(snakeAiPrefab, map[i, j].field.transform.position, Quaternion.identity);
        secondSnake.GetComponent<aiSnake>().start = 100 * (i + 1) + j + 1;
        firstSnake.GetComponent<playerSnake>().instance=this;
        secondSnake.GetComponent<aiSnake>().instance=this;
        secondSnake.GetComponent<aiSnake>().setup();
    }

    public void spawnTrainSnakes()
    {
        int i = UnityEngine.Random.Range(0, fDimension-1);
        int j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad || map[i, j].isApple)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        firstSnake = Instantiate(snakeAiPrefab, map[i, j].field.transform.position, Quaternion.identity);
        firstSnake.GetComponent<aiSnake>().start = 100 * (i + 1) + j + 1;
        i = UnityEngine.Random.Range(0, fDimension-1);
        j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad || map[i, j].isApple || 100 * (i + 1) + j + 1 == firstSnake.GetComponent<aiSnake>().start)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        secondSnake = Instantiate(snakeAiPrefab, map[i, j].field.transform.position, Quaternion.identity);
        secondSnake.GetComponent<aiSnake>().start = 100 * (i + 1) + j + 1;
        firstSnake.GetComponent<aiSnake>().instance=this;
        secondSnake.GetComponent<aiSnake>().instance=this;
        firstSnake.GetComponent<aiSnake>().setup();
        secondSnake.GetComponent<aiSnake>().setup();
    }
    public void clearMap()
    {
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 15; j++)
            {
                if (map[i, j].isApple)
                {
                    map[i, j].field.GetComponent<SpriteRenderer>().sprite = buildSquare;
                    map[i, j].isApple = false;
                }
                map[i, j].isSnake = false;
                map[i, j].isTail = false;
            }
        }
    }
    public void CreateApple()
    {
        int i = UnityEngine.Random.Range(0, fDimension-1);
        int j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad && !map[i,j].isSnake)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        map[i, j].isApple = true;
        map[i, j].field.GetComponent<SpriteRenderer>().sprite = apple;
        applePlacement = 100 * (i + 1) + j + 1;
    }
    public void DestroyApple()
    {
        int appleRow = applePlacement / 100;
        int appleCol = applePlacement % 100;
        map[appleRow - 1, appleCol - 1].isApple = false;
        map[appleRow - 1, appleCol - 1].field.GetComponent<SpriteRenderer>().sprite = buildSquare;
        applePlacement = 0;
    }
    public void gameEnd()
    {
        arePlaying=false;
        if(firstSnake != null)
        {
            if(firstSnake.GetComponent<playerSnake>() != null)
            {
                DestroySnakes(true);
            }else
            {
                DestroySnakes(false);
            }
        clearMap();
        }
        
    }


    bool arePlaying = false;
    public void trainSnakeMatch()
    {
        if(!arePlaying)
        {   
            arePlaying=true;
            spawnTrainSnakes();
            CreateApple();
            StartCoroutine("snakeFight");
        }
        
    }
    public IEnumerator snakeFight()
    {
        while(arePlaying)
        {
            yield return new WaitForSeconds(gameSpeed);
            if(arePlaying)
            firstSnake.GetComponent<aiSnake>().move();
            yield return new WaitForSeconds(gameSpeed);
            if(arePlaying)
            {
                secondSnake.GetComponent<aiSnake>().move(); 
            }
            
        }
    }
}
