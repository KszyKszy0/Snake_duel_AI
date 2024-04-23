using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestEnviroment : MonoBehaviour
{
    [SerializeField]
    public GameObject trainSnakePrefab;


    public GameObject firstSnake;
    public GameObject secondSnake;


    public field[,] map;


    public int applePlacement;
    public int fDimension;
    public int sDimension;
    public int fullSize;
    public int upBound;
    public int bottomBound;
    public int rightBound;
    public int leftBound;


    public bool isPracticeMapSet;
    public bool isTraining;


    public double firstSnakeRewards;
    public double secondSnakeRewards;


    public bool showValidation;

    public void init(int mapsize, int mapsize2)
    {
        fDimension = mapsize;
        sDimension = mapsize2;
        map = new field[mapsize, mapsize2];
        for (int i = 0; i <= mapsize - 1; i++)
        {
            for (int j = 0; j <= mapsize2 - 1; j++)
            {
                var newField = new field();
                map[i, j] = newField;
            }
        }
        fullSize = fDimension * sDimension;
        upBound=101;
        bottomBound=fDimension*100+sDimension;
        leftBound=0;
        rightBound=sDimension;
    }
    public bool checkPracticeMap()
    {
        int fields = 0;
        for (int i = 0; i <= fDimension - 1; i++)
        {
            for (int j = 0; j <= sDimension - 1; j++)
            {
                if (map[i, j].isRoad)
                {
                    fields++;
                    if (checkAdjPractice(i, j) < 2)
                        return false;
                }
            }
        }
        if (fields < 10)
        {
            return false;
        }
        return true;
    }
    int checkAdjPractice(int x, int y)
    {
        int result = 0;
        if (x > 0)
        {
            if (map[x - 1, y].isRoad)
                result++;
        }
        if (x < fDimension - 1)
        {
            if (map[x + 1, y].isRoad)
                result++;
        }
        if (y > 0)
        {
            if (map[x, y - 1].isRoad)
                result++;
        }
        if (y < sDimension - 1)
        {
            if (map[x, y + 1].isRoad)
                result++;
        }
        return result;
    }


    
    public void trainingTime()
    {
        isTraining = true;
        while (isTraining)
        {
            // Debug.Log(isTraining + " przed ruchem pierwszym");
            firstSnake.GetComponent<trainSnake>().trainMove();          // pierwszego węża nie trzeba sprawdzać bo sprawdzanyt jhest wcześniej
            // Debug.Log("trening ruch pierwszy");
            if (isTraining)
            {
                // Debug.Log(isTraining + " po ruchu pierwszym");
                secondSnake.GetComponent<trainSnake>().trainMove();     // pierwszy mógł się zabić do tego momentu
                // Debug.Log("trening ruch drugi");
            }
            
            // showMap();
        }
    }

    // valdiacja bez losowości
    public void validationTraining()
    {
        isTraining = true;
        while (isTraining)
        {
            // Debug.Log(isTraining + " przed ruchem pierwszym");
            firstSnake.GetComponent<trainSnake>().validationMove();          // pierwszego węża nie trzeba sprawdzać bo sprawdzanyt jhest wcześniej
            // Debug.Log("walidaacja ruch pierwszego");
            if (isTraining)
            {
                // Debug.Log(isTraining + " po ruchu pierwszym");
                secondSnake.GetComponent<trainSnake>().validationMove();     // pierwszy mógł się zabić do tego momentu
                // Debug.Log("walidacja ruch drugi");
            }
            // if(showValidation)
            // {
            //     showMap();
            // }
        }
    }
    public void randomizeSnakeStart()
    {
        int i = UnityEngine.Random.Range(0, fDimension - 1);
        int j = UnityEngine.Random.Range(0, sDimension - 1);
        while (!map[i, j].isRoad || map[i, j].isApple)
        {
            i = UnityEngine.Random.Range(0, fDimension - 1);
            j = UnityEngine.Random.Range(0, sDimension - 1);
        }
        firstSnake.GetComponent<trainSnake>().start = 100 * (i + 1) + j + 1;
        i = UnityEngine.Random.Range(0, fDimension - 1);
        j = UnityEngine.Random.Range(0, sDimension - 1);
        while (!map[i, j].isRoad || map[i, j].isApple || 100 * (i + 1) + j + 1 == firstSnake.GetComponent<trainSnake>().start) 
        {
            i = UnityEngine.Random.Range(0, fDimension - 1);
            j = UnityEngine.Random.Range(0, sDimension - 1);
        }
        secondSnake.GetComponent<trainSnake>().start = 100 * (i + 1) + j + 1;
    }
    public void startTrainSession()
    {
        if (isPracticeMapSet)
        {
            if (!isTraining)
            {  
                CreateAppleTrain();
                randomizeSnakeStart();
                // for (int i = 0; i <= UnityEngine.Random.Range(0, 11); i++)
                // {
                //     firstSnake.GetComponent<trainSnake>().segmentsPlacement.Add(0);
                // }
                // for (int i = 0; i <= UnityEngine.Random.Range(0, 11); i++)
                // {
                //     secondSnake.GetComponent<trainSnake>().segmentsPlacement.Add(0);
                // }
                firstSnake.GetComponent<trainSnake>().setupSnake();
                secondSnake.GetComponent<trainSnake>().setupSnake();
                trainingTime();
            }
        }
    } 
    // wersja validaacji
    public void startValidationSession()
    {
        if (!isTraining)
        {
            CreateAppleTrain();
            randomizeSnakeStart();
            // for (int i = 0; i <= UnityEngine.Random.Range(0, 11); i++)
            // {
            //     firstSnake.GetComponent<trainSnake>().segmentsPlacement.Add(0);
            // }
            // for (int i = 0; i <= UnityEngine.Random.Range(0, 11); i++)
            // {
            //     secondSnake.GetComponent<trainSnake>().segmentsPlacement.Add(0);
            // }
            firstSnake.GetComponent<trainSnake>().setupSnake();
            secondSnake.GetComponent<trainSnake>().setupSnake();
            validationTraining();
            isTraining=false;
        }
    } 
    
    public void CreateAppleTrain() 
    {
        int i = UnityEngine.Random.Range(0, fDimension-1);
        int j = UnityEngine.Random.Range(0, sDimension-1);
        while (!map[i, j].isRoad)
        {
            i = UnityEngine.Random.Range(0, fDimension-1);
            j = UnityEngine.Random.Range(0, sDimension-1);
        }
        map[i, j].isApple = true;
        applePlacement = 100 * (i + 1) + j + 1;
    }
    public void DestroyAppleTrain()
    {
        if(applePlacement != 0)
        {
            int appleRow = applePlacement / 100;
            int appleCol = applePlacement % 100;
            map[appleRow - 1, appleCol - 1].isApple = false;
            applePlacement = 0;
        }
        
    }
    public void endTrainSession()
    {
        isTraining=false;
        firstSnakeRewards+=firstSnake.GetComponent<trainSnake>().episodeReward;
        secondSnakeRewards+=secondSnake.GetComponent<trainSnake>().episodeReward;
        firstSnake.GetComponent<trainSnake>().episodeReward=0;
        secondSnake.GetComponent<trainSnake>().episodeReward=0;
        firstSnake.GetComponent<trainSnake>().resetSnake();
        secondSnake.GetComponent<trainSnake>().resetSnake();
        DestroyAppleTrain();
        // Debug.Log("Koniec sesji treningowej/walidacyjnej");
    }
    public void spawnTrainSnakes()
    {
        if (!firstSnake)
        {
            firstSnake = Instantiate(trainSnakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            firstSnake.GetComponent<trainSnake>().instance=this;
        }   
        if (!secondSnake)
        {
            secondSnake = Instantiate(trainSnakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            secondSnake.GetComponent<trainSnake>().instance=this;
        }
            
    }
    public void setupTrain()
    {
        spawnTrainSnakes();
    }
    public void makeRandomPracticeMap(float probability)
    {
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 15; j++)
            {
                if (UnityEngine.Random.Range(0, 100) <= probability)
                {
                    if (!map[i, j].isRoad)
                        map[i, j].isRoad = true;
                }
            }
        }
        while (!checkPracticeMap())
        {
            for (int i = 0; i <= 7; i++)
            {
                for (int j = 0; j <= 15; j++)
                {
                    if (UnityEngine.Random.Range(0, 100) <= probability)
                    {
                        if (!map[i, j].isRoad)
                            map[i, j].isRoad = true;
                    }
                }
            }
        }
        isPracticeMapSet = true;
    }
    public void setValidationMap()
    {
        for(int i=0; i<=fDimension-1; i++)
        {
            for(int j=0; j<=sDimension-1; j++)
            {
                map[i, j].isRoad = true;
            }
        }
        for(int i=2; i<=5; i++)
        {
            map[i, 3].isRoad = false;
            map[i, 12].isRoad = false;
        }
        for(int i=2; i<=5; i+=3)
        {
            for(int j=4; j<=6; j++)
            {
                map[i, j].isRoad = false;
            }
            for(int j=9; j<=11; j++)
            {
                map[i, j].isRoad = false;
            }
        }
        showMap();
    }
    public void clearPractice()
    {
        for (int i = 0; i <= fDimension-1; i++)
        {
            for (int j = 0; j <= sDimension-1; j++)
            {
                if (map[i, j].isRoad)
                {
                    map[i, j].isRoad = false;
                }
            }
        }
    }

    public void showMap()
    {
        string tempMap="";
        string toAdd="";
        for(int i=0; i<=fDimension-1; i++)
        {
            for(int j=0; j<=sDimension-1; j++)
            {
                if(map[i, j].isRoad)
                    toAdd=" *";
                if(!map[i, j].isRoad)
                    toAdd="  ";
                if(map[i, j].isApple)
                    toAdd=" A";
                if(map[i, j].isSnake)
                    toAdd=" S";
                if(map[i, j].isTail)
                    toAdd=" T";
                tempMap+=toAdd;
            }
            tempMap+='\n';
        }
        Debug.Log(tempMap);
    }
}
