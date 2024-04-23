using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class trainSnake : MonoBehaviour
{
    // Start is called before the first frame update
    public int lastDir;
    public int dir;
    public int tail_dir;
    public gameManagement GM;
    [SerializeField]
    public List<int> segmentsPlacement = new List<int>();
    public int start;
    public AI aiManager;
    public double reward;
    public TestEnviroment instance;
    public double episodeReward;
    public double stepsTakenFromReward;
    public double move_limit = 256;

    void Awake()
    {
        GM = GameObject.Find("GameManager").GetComponent<gameManagement>();
        aiManager = GameObject.Find("AiManager").GetComponent<AI>();
        for (int i = 0; i <= 2; i++)
        {
            segmentsPlacement.Add(start);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // z losowością / eksploracją

    public void trainMove()
    {
        stepsTakenFromReward++;
        double[] state;
        double[] nextState;
        double beforeMoveDistanceToApple = newRewardCalc();
        state = prepareReplay();
        if (stepsTakenFromReward >= move_limit)
        {
            die(state);
            return;
        }
        if (GM.randomMoveProbability > Random.Range(0, 1f))
        {
            GetMove();
            if (GM.randomMoveProbability > GM.minRandomMoveProbability)
            {
                GM.randomMoveProbability -= GM.randomDecay;
            }
        }
        else
        {
            getAiMove(state);
        }
        moveTailFlag();
        for (int i = segmentsPlacement.Count - 1; i >= 1; i--)
        {
            segmentsPlacement[i] = segmentsPlacement[i - 1];
            int temp_tile = segmentsPlacement[i];                        // usuwa właściwości ogon i wąż z ostatniego i przedostatniego pola
            int tile_row = temp_tile / 100;
            int tile_col = temp_tile % 100;
            instance.map[tile_row - 1, tile_col - 1].isSnake = true;
        }
        if (dir != -lastDir)
        {
            lastDir = dir;
        }
        int destination = segmentsPlacement[0] + dir;
        if (destination < instance.upBound || destination > instance.bottomBound || destination % 100 > instance.rightBound || destination % 100 == instance.leftBound)
        {
            die(state);
            return;
        }
        if (!isFieldGood(destination))
        {
            die(state);
            return;
        }
        segmentsPlacement[0] += lastDir;
        instance.map[(segmentsPlacement[0] / 100) - 1, (segmentsPlacement[0] % 100) - 1].isSnake = true;
        // reward = calcReward();
        reward = 0;
        // Debug.Log(reward);
        if (segmentsPlacement[0] == instance.applePlacement)
        {
            reward = 10;
            grow();
            instance.DestroyAppleTrain();
            instance.CreateAppleTrain();
        }
        // else if (beforeMoveDistanceToApple < newRewardCalc() && beforeMoveDistanceToApple == 1)
        // {
        //     reward = -5;
        // }
        // else
        // {
        //     if (newRewardCalc() < beforeMoveDistanceToApple)
        //     {
        //         reward = 1;
        //     }
        // }

        nextState = prepareReplay();
        aiManager.betterAgent.uploadReplay(new Replay(state, getActionIndexFromDir(), reward, nextState, false));
        
        episodeReward += reward;
    }


    //bez losowości   nie ma przyszłych stanów         bez dawania powtórki
    public void validationMove()
    {
        stepsTakenFromReward++;
        double[] state;
        state = prepareReplay();
        if (stepsTakenFromReward >= move_limit)
        {
            validationDie();
            return;
        }
        getAiMove(state);
        moveTailFlag();
        // Debug.Log("mapa przed ruchem walidacja: ");
        if (GM.showValidation)
            instance.showMap();
        for (int i = segmentsPlacement.Count - 1; i >= 1; i--)
        {
            segmentsPlacement[i] = segmentsPlacement[i - 1];
            int temp_tile = segmentsPlacement[i];                        // usuwa właściwości ogon i wąż z ostatniego i przedostatniego pola
            int tile_row = temp_tile / 100;
            int tile_col = temp_tile % 100;
            instance.map[tile_row - 1, tile_col - 1].isSnake = true;
        }
        if (dir != -lastDir)
        {
            lastDir = dir;
        }
        int destination = segmentsPlacement[0] + dir;
        if (destination < instance.upBound || destination > instance.bottomBound || destination % 100 > instance.rightBound || destination % 100 == instance.leftBound)
        {
            validationDie();
            return;
        }
        if (!isFieldGood(destination))
        {
            validationDie();
            return;
        }
        segmentsPlacement[0] += lastDir;
        instance.map[(segmentsPlacement[0] / 100) - 1, (segmentsPlacement[0] % 100) - 1].isSnake = true;
        // reward = calcReward();
        reward = 0;
        // Debug.Log(reward);
        if (segmentsPlacement[0] == instance.applePlacement)
        {
            reward = 10;
            grow();
            instance.DestroyAppleTrain();
            instance.CreateAppleTrain();
        }
        episodeReward += reward;
        // Debug.Log("mapa po ruchu walidacja: ");
        if (GM.showValidation)
            instance.showMap();
    }


    public void setupSnake()
    {
        stepsTakenFromReward = 0;
        for (int i = 0; i <= segmentsPlacement.Count - 1; i++)
        {
            segmentsPlacement[i] = start;
        }
    }
    public void resetSnake()
    {
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 15; j++)
            {
                instance.map[i, j].isSnake = false;
                instance.map[i, j].isTail = false;
                instance.map[i, j].isApple = false;
            }
        }
        // for (int i = 0; i <= segmentsPlacement.Count - 1; i++)
        // {
        //     if(segmentsPlacement[i]<instance.upBound || segmentsPlacement[i]>instance.bottomBound || segmentsPlacement[i]%100>instance.rightBound || segmentsPlacement[i]%100==instance.leftBound)
        //     {
        //         continue;
        //     }
        //     int row = segmentsPlacement[i] / 100;
        //     int col = segmentsPlacement[i] % 100;
        //     instance.map[row - 1, col - 1].isSnake = false;
        //     instance.map[row - 1, col - 1].isTail = false;
        // }
        if (segmentsPlacement.Count > 3)
        {
            segmentsPlacement.RemoveRange(3, segmentsPlacement.Count - 3);
        }
    }

    void GetMove()
    {
        int m = Random.Range(1, 5);
        switch (m)
        {
            case 1:
                if (lastDir != 100)
                {
                    dir = -100;
                }
                break;
            case 2:
                if (lastDir != -100)
                {
                    dir = 100;
                }
                break;
            case 3:
                if (lastDir != 1)
                {
                    dir = -1;
                }
                break;
            case 4:
                if (lastDir != -1)
                {
                    dir = 1;
                }
                break;
            default:
                GetMove();
                break;
        }
    }
    public void grow()
    {
        stepsTakenFromReward = 0;
        segmentsPlacement.Add(segmentsPlacement[segmentsPlacement.Count - 1]);
    }
    public double calcReward()
    {
        int EnemyHead;
        if (instance.firstSnake == gameObject)
        {
            EnemyHead = instance.secondSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        }
        else
        {
            EnemyHead = instance.firstSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        }
        int EnemyHeadRow = EnemyHead / 100;
        int EnemyHeadCol = EnemyHead % 100;
        int HeadRow = segmentsPlacement[0] / 100;
        int HeadCol = segmentsPlacement[0] % 100;
        int bonus = Mathf.Abs(EnemyHeadRow - HeadRow) + Mathf.Abs(EnemyHeadCol - HeadCol);
        int appleRow = instance.applePlacement / 100;
        int appleCol = instance.applePlacement % 100;
        int appleBonus = Mathf.Abs(appleRow - HeadRow) + Mathf.Abs(appleCol - HeadCol);
        return 16 - (bonus <= 2 ? 1 : -1) - appleBonus;
    }

    public double newRewardCalc()
    {
        int HeadRow = segmentsPlacement[0] / 100;
        int HeadCol = segmentsPlacement[0] % 100;
        int appleRow = instance.applePlacement / 100;
        int appleCol = instance.applePlacement % 100;
        int appleBonus = Mathf.Abs(appleRow - HeadRow) + Mathf.Abs(appleCol - HeadCol);
        return appleBonus;
    }
    public double[] GetClosestObstacles()
    {
        double[] results = new double[12];
        //gora
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 101 + i) % 100 == instance.leftBound || (segmentsPlacement[0] - 101 + i) % 100 > instance.rightBound)
            {
                results[i] = (double)1 / instance.fDimension;
                continue;
            }
            results[i] = (double)(segmentsPlacement[0] / 100) / instance.fDimension;  //odleglosc od gory
            for (int j = segmentsPlacement[0] - 101 + i; j >= 101; j -= 100)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i] = (double)((segmentsPlacement[0] / 100) - placeRow) / instance.fDimension;
                    break;
                }
            }
        }
        //prawo
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 99 + (100 * i)) / 100 < 1 || (segmentsPlacement[0] - 99 + (100 * i)) / 100 > (instance.fDimension))
            {
                results[i + 3] = (double)1 / instance.sDimension;
                continue;
            }
            results[i + 3] = (double)(instance.sDimension + 1 - (segmentsPlacement[0] % 100)) / instance.sDimension;  //odleglosc od prawej
            for (int j = segmentsPlacement[0] - 99 + (100 * i); j % 100 <= instance.rightBound; j++)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i + 3] = (double)(placeCol - (segmentsPlacement[0] % 100)) / instance.sDimension;
                    break;
                }
            }
        }
        //dol
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] + 99 + i) % 100 == instance.leftBound || (segmentsPlacement[0] + 99 + i) % 100 > instance.rightBound)
            {
                results[i + 6] = (double)1 / instance.fDimension;
                continue;
            }
            results[i + 6] = (double)(instance.fDimension + 1 - (segmentsPlacement[0] / 100)) / instance.fDimension;  //odleglosc od dolu
            for (int j = segmentsPlacement[0] + 99 + i; j <= instance.bottomBound; j += 100)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i + 6] = (double)(placeRow - (segmentsPlacement[0] / 100)) / instance.fDimension;
                    break;
                }
            }
        }
        //lewo
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 101 + (100 * i)) / 100 < 1 || (segmentsPlacement[0] - 101 + (100 * i)) / 100 > (instance.fDimension))
            {
                results[i + 9] = (double)1 / instance.sDimension;
                continue;
            }
            results[i + 9] = (double)(segmentsPlacement[0] % 100) / instance.sDimension;
            for (int j = segmentsPlacement[0] - 101 + (100 * i); j % 100 >= 1; j--)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i + 9] = (double)((segmentsPlacement[0] % 100) - placeCol) / instance.sDimension;
                    break;
                }
            }
        }
        return results;
    }
    public double[] getAppleAndHeadDelta()
    {
        double[] delta = new double[2];
        int HeadRow = segmentsPlacement[0] / 100;
        int HeadCol = segmentsPlacement[0] % 100;
        int appleRow = instance.applePlacement / 100;
        int appleCol = instance.applePlacement % 100;
        delta[0] = ((double)(appleRow - HeadRow)) / (instance.fDimension - 1);
        delta[1] = ((double)(appleCol - HeadCol)) / (instance.sDimension - 1);

        // double[] delta = new double[4];
        // int otherSnake;
        // int HeadRow = segmentsPlacement[0] / 100;
        // int HeadCol = segmentsPlacement[0] % 100;
        // int appleRow = instance.applePlacement / 100;
        // int appleCol = instance.applePlacement % 100;
        // delta[0] = ((double)(appleRow - HeadRow))/(instance.fDimension-1);
        // delta[1] = ((double)(appleCol - HeadCol))/(instance.sDimension-1);
        // if (gameObject == instance.firstSnake)
        // {
        //     otherSnake = instance.secondSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        // }
        // else
        // {
        //     otherSnake = instance.firstSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        // }
        // int otherSnakeRow = otherSnake / 100;
        // int otherSnakeCol = otherSnake % 100;
        // delta[2] = ((double)(otherSnakeRow - HeadRow))/(instance.fDimension-1);    // maksymalna różnica wynosi wymiar -1 bo ekstremum = <1,16>  => 16-1 = 15 = wymiar - 1 ,  wymiar = 16 
        // delta[3] = ((double)(otherSnakeCol - HeadCol))/(instance.sDimension-1);

        // double[] delta = new double[4];
        // int otherSnake;
        // int HeadRow = segmentsPlacement[0] / 100;
        // int HeadCol = segmentsPlacement[0] % 100;
        // int appleRow = instance.applePlacement / 100;
        // int appleCol = instance.applePlacement % 100;
        // delta[0] = ((double)(appleRow - HeadRow)+instance.fDimension-1)/((instance.fDimension*2)-1);
        // delta[1] = ((double)(appleCol - HeadCol)+instance.sDimension-1)/((instance.sDimension*2)-1);
        // if (gameObject == instance.firstSnake)
        // {
        //     otherSnake = instance.secondSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        // }
        // else
        // {
        //     otherSnake = instance.firstSnake.GetComponent<trainSnake>().segmentsPlacement[0];
        // }
        // int otherSnakeRow = otherSnake / 100;
        // int otherSnakeCol = otherSnake % 100;
        // delta[2] = ((double)(otherSnakeRow - HeadRow)+instance.fDimension-1)/((instance.fDimension*2)-1);
        // delta[3] = ((double)(otherSnakeCol - HeadCol)+instance.sDimension-1)/((instance.sDimension*2)-1);


        // double right = 0;
        // double left = 0;
        // double down = 0;
        // double up = 0;
        // for (int i = HeadRow; i <= 7; i++)              //headrow jest o 1 większy więc nie trzeba odejmować 
        // {
        //     for (int j = 0; j <= 15; j++)
        //     {
        //         if (instance.map[i, j].isSnake || !instance.map[i, j].isSnake)
        //         {
        //             down++;
        //         }
        //     }
        // }
        // for (int i = HeadRow - 2; i >= 0; i--)
        // {
        //     for (int j = 0; j <= 15; j++)
        //     {
        //         if (instance.map[i, j].isSnake || !instance.map[i, j].isSnake)
        //         {
        //             up++;
        //         }
        //     }
        // }
        // for (int i = 0; i <= 7; i++)
        // {
        //     for (int j = HeadCol; j <= 15; j++)
        //     {
        //         if (instance.map[i, j].isSnake || !instance.map[i, j].isSnake)
        //         {
        //             right++;
        //         }
        //     }
        // }
        // for (int i = 0; i <= 7; i++)
        // {
        //     for (int j = HeadCol - 2; j >= 0; j--)
        //     {
        //         if (instance.map[i, j].isSnake || !instance.map[i, j].isSnake)
        //         {
        //             left++;
        //         }
        //     }
        // }
        // delta[4] = up/instance.fullSize;
        // delta[5] = right/instance.fullSize;
        // delta[6] = down/instance.fullSize;
        // delta[7] = left/instance.fullSize;
        return delta;
    }
    public double[] prepareReplay()
    {
        // double[] replayData = new double[385];

        // for(int i=0; i<=7; i++)
        // {
        //     for(int j=0; j<=15; j++)
        //     {
        //         if(instance.map[i,j].isRoad && !instance.map[i,j].isSnake)
        //         {
        //             replayData[i*16+j]=1;
        //         }
        //     }
        // }
        // int applePlacement = (instance.applePlacement / 100-1)*16 + (instance.applePlacement % 100)-1;
        // int snakeHeadIndex = (segmentsPlacement[0] / 100-1)*16 + (segmentsPlacement[0] % 100)-1;
        // replayData[applePlacement+128]=1;
        // replayData[snakeHeadIndex+256]=1;
        // replayData[384]=stepsTakenFromReward/20d;

        // double [] replayData = new double[76];
        // int headRow = segmentsPlacement[0] / 100 -1;  //0-7
        // int headCol = segmentsPlacement[0] % 100 -1;  //0-15
        // int startIndexRow = headRow -2;
        // int startIndexCol = headCol -2; 

        // for(int i=0; i<=4; i++)
        // {
        //     if(startIndexRow+i<0 || startIndexRow+i>7)
        //     {
        //         for(int j=0; j<=4; j++)
        //         {
        //             replayData[i*15+j*3+2]=1;
        //         }
        //         continue;
        //     }
        //     for(int j=0; j<=4; j++)
        //     {
        //         if(startIndexCol+j<0 || startIndexCol+j>15)
        //         {
        //             replayData[i*15+j*3+2]=1;
        //             continue;
        //         }
        //         if(instance.map[startIndexRow+i,startIndexCol+j].isRoad && !instance.map[startIndexRow+i,startIndexCol+j].isSnake)
        //         {
        //             replayData[i*15+j*3+1]=1;
        //         }else
        //         {
        //             replayData[i*15+j*3+2]=1;
        //         }
        //         if(instance.map[startIndexRow+i,startIndexCol+j].isApple)
        //         {
        //             replayData[i*15+j*3]=1;
        //         }
        //     }
        // }
        // replayData[75]=stepsTakenFromReward/20d;




        // double [] replayData = new double[148];
        // int headRow = segmentsPlacement[0] / 100 -1;  //0-7
        // int headCol = segmentsPlacement[0] % 100 -1;  //0-15
        // int startIndexRow = headRow -3;
        // int startIndexCol = headCol -3; 

        // for(int i=0; i<=6; i++)
        // {
        //     if(startIndexRow+i<0 || startIndexRow+i>7)
        //     {
        //         for(int j=0; j<=6; j++)
        //         {
        //             replayData[i*21+j*3+2]=1;
        //         }
        //         continue;
        //     }
        //     for(int j=0; j<=6; j++)
        //     {
        //         if(startIndexCol+j<0 || startIndexCol+j>15)
        //         {
        //             replayData[i*21+j*3+2]=1;
        //             continue;
        //         }
        //         if(instance.map[startIndexRow+i,startIndexCol+j].isRoad && !instance.map[startIndexRow+i,startIndexCol+j].isSnake)
        //         {
        //             replayData[i*21+j*3+1]=1;
        //         }else
        //         {
        //             replayData[i*21+j*3+2]=1;
        //         }
        //         if(instance.map[startIndexRow+i,startIndexCol+j].isApple)
        //         {
        //             replayData[i*21+j*3]=1;
        //         }
        //     }
        // }
        // replayData[147]=stepsTakenFromReward/20d;





        double [] replayData = new double[83];
        int headRow = segmentsPlacement[0] / 100 -1;  //0-7
        int headCol = segmentsPlacement[0] % 100 -1;  //0-15
        int startIndexRow = headRow -4;
        int startIndexCol = headCol -4; 

        for(int i=0; i<=8; i++)
        {
            if(startIndexRow+i<0 || startIndexRow+i>7)
            {
                for(int j=0; j<=8; j++)
                {
                    replayData[i*9+j]=0;
                }
                continue;
            }
            for(int j=0; j<=8; j++)
            {
                if(startIndexCol+j<0 || startIndexCol+j>15)
                {
                    replayData[i*9+j]=0;
                    continue;
                }
                if(instance.map[startIndexRow+i,startIndexCol+j].isRoad && !instance.map[startIndexRow+i,startIndexCol+j].isSnake)
                {
                    replayData[i*9+j]=1;
                }else
                {
                    replayData[i*9+j]=0;
                }
            }
        }
        double[] end = getAppleAndHeadDelta();
        replayData[81]=end[0];
        replayData[82]=end[1];
        return replayData;

        // double[] replayData = new double[15];
        // double[] obst = GetClosestObstacles();
        // double[] distDelta = getAppleAndHeadDelta();
        // for (int i = 0; i <= obst.Length - 1; i++)
        // {
        //     replayData[i] = obst[i];
        // }
        // for (int i = 0; i <= distDelta.Length-1; i++)
        // {
        //     replayData[12 + i] = distDelta[i];
        // }
        // replayData[14]=stepsTakenFromReward/20d;
        // return end_State();
    }

    public double[] mature_state()
    {
        double[] results = new double[44];
        //gora
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 101 + i) % 100 == instance.leftBound || (segmentsPlacement[0] - 101 + i) % 100 > instance.rightBound)
            {
                results[i * 3] = 0;
                continue;
            }
            results[i * 3] = (double)(segmentsPlacement[0] / 100) / instance.fDimension;  //odleglosc od gory
            for (int j = segmentsPlacement[0] - 101 + i; j >= 101; j -= 100)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)   //to jest dystans absolutny w dana strone
                {
                    results[i * 3 + 1] = (double)((segmentsPlacement[0] / 100) - placeRow) / instance.fDimension;
                    break;
                }
                if (instance.map[placeRow - 1, placeCol - 1].isApple)
                {
                    results[i * 3 + 2] = (double)((segmentsPlacement[0] / 100) - placeRow) / instance.fDimension;
                    break;
                }
            }
        }
        //prawo
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 99 + (100 * i)) / 100 < 1 || (segmentsPlacement[0] - 99 + (100 * i)) / 100 > (instance.fDimension))
            {
                results[i * 3 + 9] = 0;
                continue;
            }
            results[i * 3 + 9] = (double)(instance.sDimension - (segmentsPlacement[0] % 100)) / instance.sDimension;  //odleglosc od prawej
            for (int j = segmentsPlacement[0] - 99 + (100 * i); j % 100 <= instance.rightBound; j++)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i * 3 + 10] = (double)((segmentsPlacement[0] % 100) - placeCol) / instance.sDimension;
                    break;
                }
                if (instance.map[placeRow - 1, placeCol - 1].isApple)
                {
                    results[i * 3 + 11] = (double)((segmentsPlacement[0] % 100) - placeCol) / instance.sDimension;
                    break;
                }
            }
        }
        //dol
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] + 99 + i) % 100 == instance.leftBound || (segmentsPlacement[0] + 99 + i) % 100 > instance.rightBound)
            {
                results[i * 3 + 18] = 0;
                continue;
            }
            results[i * 3 + 18] = (double)(instance.fDimension - (segmentsPlacement[0] / 100)) / instance.fDimension;  //odleglosc od dolu
            for (int j = segmentsPlacement[0] + 99 + i; j <= instance.bottomBound; j += 100)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i * 3 + 19] = (double)((segmentsPlacement[0] / 100) - placeRow) / instance.fDimension;
                    break;
                }
                if (instance.map[placeRow - 1, placeCol - 1].isApple)
                {
                    results[i * 3 + 20] = (double)((segmentsPlacement[0] / 100) - placeRow) / instance.fDimension;
                    break;
                }
            }
        }
        //lewo
        for (int i = 0; i <= 2; i++)
        {
            if ((segmentsPlacement[0] - 101 + (100 * i)) / 100 < 1 || (segmentsPlacement[0] - 101 + (100 * i)) / 100 > (instance.fDimension))
            {
                results[i * 3 + 27] = 0;
                continue;
            }
            results[i * 3 + 27] = (double)(segmentsPlacement[0] % 100) / instance.sDimension;
            for (int j = segmentsPlacement[0] - 101 + (100 * i); j % 100 >= 1; j--)
            {
                int placeRow = j / 100;
                int placeCol = j % 100;
                if (instance.map[placeRow - 1, placeCol - 1].isSnake || !instance.map[placeRow - 1, placeCol - 1].isRoad)
                {
                    results[i * 3 + 28] = (double)((segmentsPlacement[0] % 100) - placeCol) / instance.sDimension;
                    break;
                }
                if (instance.map[placeRow - 1, placeCol - 1].isApple)
                {
                    results[i * 3 + 29] = (double)((segmentsPlacement[0] % 100) - placeCol) / instance.sDimension;
                    break;
                }
            }
        }
        results[getActionIndexFromDir() + 36] = 1;
        results[getActionIndexFromDir(tail_dir) + 40] = 1;
        // Debug.Log(segmentsPlacement[0]);
        // instance.showMap();
        // printReplay(results);
        return results;
    }

    // public double[] backup(){
    //     double[] results = new double[44];
    //     //gora
    //     for(int i=0; i<=2; i++)
    //     {
    //         if((segmentsPlacement[0]-101+i)%100==instance.leftBound || (segmentsPlacement[0]-101+i)%100>instance.rightBound)
    //         {
    //             results[i*3]=(double)1/instance.fDimension;
    //             continue;
    //         }
    //         results[i*3]=(double)(segmentsPlacement[0]/100)/instance.fDimension;  //odleglosc od gory
    //         for(int j=segmentsPlacement[0]-101+i; j>=101; j-=100)
    //         {
    //             int placeRow=j/100;
    //             int placeCol=j%100;
    //             if(instance.map[placeRow-1,placeCol-1].isSnake || !instance.map[placeRow-1,placeCol-1].isRoad)   //to jest dystans absolutny w dana strone
    //             {
    //                 results[i*3+1]=(double)((segmentsPlacement[0]/100)-placeRow)/instance.fDimension;
    //                 break;
    //             }
    //             if(instance.map[placeRow-1,placeCol-1].isApple)
    //             {
    //                 results[i*3+2]=(double)((segmentsPlacement[0]/100)-placeRow)/instance.fDimension;
    //                 break;
    //             }
    //         }
    //     }
    //     //prawo
    //     for(int i=0; i<=2; i++)
    //     {
    //         if((segmentsPlacement[0]-99+(100*i))/100<1 || (segmentsPlacement[0]-99+(100*i))/100>(instance.fDimension))
    //         {
    //             results[i*3+9]=(double)1/instance.sDimension;
    //             continue;
    //         }
    //         results[i*3+9]=(double)(instance.sDimension+1-(segmentsPlacement[0]%100))/instance.sDimension;  //odleglosc od prawej
    //         for(int j=segmentsPlacement[0]-99+(100*i); j%100<=instance.rightBound; j++)
    //         {
    //             int placeRow=j/100;
    //             int placeCol=j%100;
    //             if(instance.map[placeRow-1,placeCol-1].isSnake || !instance.map[placeRow-1,placeCol-1].isRoad)
    //             {
    //                 results[i*3+10]=(double)((segmentsPlacement[0]%100)-placeCol)/instance.sDimension;
    //                 break;
    //             }
    //             if(instance.map[placeRow-1,placeCol-1].isApple)
    //             {
    //                 results[i*3+11]=(double)((segmentsPlacement[0]%100)-placeCol)/instance.sDimension;
    //                 break;
    //             }
    //         }
    //     }
    //     //dol
    //     for(int i=0; i<=2; i++)
    //     {
    //         if((segmentsPlacement[0]+99+i)%100==instance.leftBound || (segmentsPlacement[0]+99+i)%100>instance.rightBound)
    //         {
    //             results[i*3+18]=(double)1/instance.fDimension;
    //             continue;
    //         }
    //         results[i*3+18]=(double)(instance.fDimension+1-(segmentsPlacement[0]/100))/instance.fDimension;  //odleglosc od dolu
    //         for(int j=segmentsPlacement[0]+99+i; j<=instance.bottomBound; j+=100)
    //         {
    //             int placeRow=j/100;
    //             int placeCol=j%100;
    //             if(instance.map[placeRow-1,placeCol-1].isSnake || !instance.map[placeRow-1,placeCol-1].isRoad)
    //             {
    //                 results[i*3+19]=(double)((segmentsPlacement[0]/100)-placeRow)/instance.fDimension;
    //                 break;
    //             }
    //             if(instance.map[placeRow-1,placeCol-1].isApple)
    //             {
    //                 results[i*3+20]=(double)((segmentsPlacement[0]/100)-placeRow)/instance.fDimension;
    //                 break;
    //             }
    //         }
    //     }
    //     //lewo
    //     for(int i=0; i<=2; i++)
    //     {
    //         if((segmentsPlacement[0]-101+(100*i))/100<1 || (segmentsPlacement[0]-101+(100*i))/100>(instance.fDimension))
    //         {
    //             results[i*3+27]=(double)1/instance.sDimension;
    //             continue;
    //         }
    //         results[i*3+27]=(double)(segmentsPlacement[0]%100)/instance.sDimension;
    //         for(int j=segmentsPlacement[0]-101+(100*i); j%100>=1; j--)
    //         {
    //             int placeRow=j/100;
    //             int placeCol=j%100;
    //             if(instance.map[placeRow-1,placeCol-1].isSnake || !instance.map[placeRow-1,placeCol-1].isRoad)
    //             {
    //                 results[i*3+28]=(double)((segmentsPlacement[0]%100)-placeCol)/instance.sDimension;
    //                 break;
    //             }
    //             if(instance.map[placeRow-1,placeCol-1].isApple)
    //             {
    //                 results[i*3+29]=(double)((segmentsPlacement[0]%100)-placeCol)/instance.sDimension;
    //                 break;
    //             }
    //         }
    //     }
    //     results[getActionIndexFromDir()+36]=1;
    //     results[getActionIndexFromDir(tail_dir)+40]=1;
    //     Debug.Log(segmentsPlacement[0]);
    //     instance.showMap();
    //     printReplay(results);
    //     return results;
    // }

    public double[] finalState()
    {
        double[] end = new double[40];
        int headIndex = segmentsPlacement[0];
        int headRow = segmentsPlacement[0] / 100;
        int headCol = segmentsPlacement[0] % 100;


        int topRow = headRow - 1;
        int topCol = headCol - 1;
        for (int i = 0; i <= 2; i++)
        {
            if (topRow <= 0 || topCol + i <= 0 || topCol + i > 16)
            {
                end[i] = 0;
            }
            else
            {
                end[i] = topRow / 7d;
            }
        }

        int rightRow = headRow - 1;
        int rightCol = headCol + 1;
        for (int i = 0; i <= 2; i++)
        {
            if (rightRow + i <= 0 || rightRow + i > 8 || rightCol > 16)
            {
                end[3 + i] = 0;
            }
            else
            {
                end[3 + i] = (17 - rightCol) / 15d;
            }
        }

        int downRow = headRow + 1;
        int downCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            if (downRow > 8 || downCol + i <= 0 || downCol + i > 16)
            {
                end[8 - i] = 0;
            }
            else
            {
                end[8 - i] = (9 - downRow) / 7d;
            }
        }

        int leftRow = headRow - 1;
        int leftCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            if (leftRow + i <= 0 || leftRow + i > 8 || leftCol <= 0)
            {
                end[11 - i] = 0;
            }
            else
            {
                end[11 - i] = leftCol / 15d;
            }
        }


        topRow = headRow - 1;
        topCol = headCol - 1;
        for (int i = 0; i <= 2; i++)
        {
            if (topRow <= 0 || topCol + i <= 0 || topCol + i > 16)
            {
                end[i + 12] = 0;
            }
            else
            {
                for (int j = topRow; j >= 1; j--)
                {
                    if (!instance.map[j - 1, topCol + i - 1].isRoad || instance.map[j - 1, topCol + i - 1].isSnake)
                    {
                        end[i + 12] = (headRow - j) / 7d;
                        break;
                    }
                }

                for (int j = topRow; j >= 1; j--)
                {
                    if (instance.map[j - 1, topCol + i - 1].isApple)
                    {
                        end[i + 24] = (headRow - j) / 7d;
                        break;
                    }
                }
            }
        }

        rightRow = headRow - 1;
        rightCol = headCol + 1;
        for (int i = 0; i <= 2; i++)
        {
            if (rightRow + i <= 0 || rightRow + i > 8 || rightCol > 16)
            {
                end[i + 15] = 0;
            }
            else
            {
                for (int j = rightCol; j <= 16; j++)
                {
                    if (!instance.map[rightRow + i - 1, j - 1].isRoad || instance.map[rightRow + i - 1, j - 1].isSnake)
                    {
                        end[i + 15] = (j - headCol) / 15d;
                        break;
                    }
                }

                for (int j = rightCol; j <= 16; j++)
                {
                    if (instance.map[rightRow + i - 1, j - 1].isApple)
                    {
                        end[i + 27] = (j - headCol) / 15d;
                        break;
                    }
                }
            }
        }

        downRow = headRow + 1;
        downCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            if (downRow > 8 || downCol + i <= 0 || downCol + i > 16)
            {
                end[20 - i] = 0;
            }
            else
            {
                for (int j = downRow; j <= 8; j++)
                {
                    if (!instance.map[j - 1, downCol + i - 1].isRoad || instance.map[j - 1, downCol + i - 1].isSnake)
                    {
                        end[20 - i] = (j - headRow) / 7d;
                        break;
                    }

                    if (instance.map[j - 1, downCol + i - 1].isApple)
                    {
                        end[32 - i] = (j - headRow) / 7d;
                        break;
                    }
                }
            }
        }

        leftRow = headRow - 1;
        leftCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            if (leftRow + i <= 0 || leftRow + i > 8 || leftCol <= 0)
            {
                end[23 - i] = 0;
            }
            else
            {
                for (int j = leftCol; j >= 1; j--)
                {
                    if (!instance.map[leftRow + i - 1, j - 1].isRoad || instance.map[leftRow + i - 1, j - 1].isSnake)
                    {
                        end[23 - i] = (headCol - j) / 15d;
                        break;
                    }

                    if (instance.map[leftRow + i - 1, j - 1].isApple)
                    {
                        end[35 - i] = (headCol - j) / 15d;
                        break;
                    }
                }
            }
        }

        end[getActionIndexFromDir() + 36] = 1;

        return end;
    }



    public double[] end_State()
    {
        double[] end = new double[40];
        int headIndex = segmentsPlacement[0];
        int headRow = segmentsPlacement[0] / 100;
        int headCol = segmentsPlacement[0] % 100;


        int topRow = headRow - 1;
        int topCol = headCol - 1;


        int rightRow = headRow - 1;
        int rightCol = headCol + 1;
        

        int downRow = headRow + 1;
        int downCol = headCol - 1;
        

        int leftRow = headRow - 1;
        int leftCol = headCol - 1;
        


        topRow = headRow - 1;
        topCol = headCol - 1;
        bool checkWallDistance = true;
        for (int i = 0; i <= 2; i++)
        {
            checkWallDistance=true;
            if (topRow <= 0 || topCol + i <= 0 || topCol + i > 16)
            {
                end[i] = 1 / 8d;
                continue;
            }
            for (int j = topRow; j >= 1; j--)
            {
                if (!instance.map[j - 1, topCol + i - 1].isRoad || instance.map[j - 1, topCol + i - 1].isSnake)
                {
                    end[i + 12] = (headRow - j) / 7d;
                    checkWallDistance = false;
                    break;
                }
                if (instance.map[j - 1, topCol + i - 1].isApple)
                {
                    end[i + 24] = (headRow - j) / 7d;
                    checkWallDistance = false;
                    break;
                }
            }
            if (checkWallDistance)
            {
                end[i] = headRow / 8d;
            }
        }

        rightRow = headRow - 1;
        rightCol = headCol + 1;
        for (int i = 0; i <= 2; i++)
        {
            checkWallDistance=true;
            if (rightRow + i <= 0 || rightRow + i > 8 || rightCol > 16)
            {
                end[3 + i] = 1/16d;
                continue;
            }
            for (int j = rightCol; j <= 16; j++)
            {
                if (!instance.map[rightRow + i - 1, j - 1].isRoad || instance.map[rightRow + i - 1, j - 1].isSnake)
                {
                    end[i + 15] = (j - headCol) / 15d;
                    checkWallDistance=false;
                    break;
                }
            }
            for (int j = rightCol; j <= 16; j++)
            {
                if (instance.map[rightRow + i - 1, j - 1].isApple)
                {
                    end[i + 27] = (j - headCol) / 15d;
                    checkWallDistance=false;
                    break;
                }
            }
            if(checkWallDistance)
            {
                end[3 + i] = (17 - headCol) / 16d;
            }
        }

        downRow = headRow + 1;
        downCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            checkWallDistance=true;
            if (downRow > 8 || downCol + i <= 0 || downCol + i > 16)
            {
                end[8 - i] = 1 / 8d;
                continue;
            }
        
                for (int j = downRow; j <= 8; j++)
                {
                    if (!instance.map[j - 1, downCol + i - 1].isRoad || instance.map[j - 1, downCol + i - 1].isSnake)
                    {
                        end[20 - i] = (j - headRow) / 7d;
                        checkWallDistance=false;
                        break;
                    }

                    if (instance.map[j - 1, downCol + i - 1].isApple)
                    {
                        end[32 - i] = (j - headRow) / 7d;
                        checkWallDistance=false;
                        break;
                    }
                }
                if(checkWallDistance)
                {
                    end[8 - i] = (9 - headRow) / 8d;
                }
            
        }

        leftRow = headRow - 1;
        leftCol = headCol - 1;
        for (int i = 2; i >= 0; i--)
        {
            checkWallDistance=true;
            if (leftRow + i <= 0 || leftRow + i > 8 || leftCol <= 0)
            {
                end[11 - i] = 1 / 16d;
                continue;
            }
            for (int j = leftCol; j >= 1; j--)
            {
                if (!instance.map[leftRow + i - 1, j - 1].isRoad || instance.map[leftRow + i - 1, j - 1].isSnake)
                {
                    end[23 - i] = (headCol - j) / 15d;
                    checkWallDistance=false;
                    break;
                }
                if (instance.map[leftRow + i - 1, j - 1].isApple)
                {
                    end[35 - i] = (headCol - j) / 15d;
                    checkWallDistance=false;
                    break;
                }
            }
            if(checkWallDistance)
            {
                end[11 - i] = headCol / 16d;
            }
        }

        end[getActionIndexFromDir() + 36] = 1;
        return end;
    }

    public void printReplay(double[] t)
    {
        string s = "";
        for (int i = 0; i <= t.Length - 1; i++)
        {
            s += t[i] + " ";
        }
        Debug.Log(s);
    }
    public int getActionIndexFromDir()
    {
        if (dir == -100)
        {
            return 0;
        }
        if (dir == 1)
        {
            return 1;
        }
        if (dir == 100)
        {
            return 2;
        }
        if (dir == -1)
        {
            return 3;
        }
        return 0;
    }

    public int getActionIndexFromDir(int val)
    {
        if (val == -100)
        {
            return 0;
        }
        if (val == 1)
        {
            return 1;
        }
        if (val == 100)
        {
            return 2;
        }
        if (val == -1)
        {
            return 3;
        }
        return 0;
    }
    public bool isFieldGood(int field)
    {
        int playerRow = field / 100;
        int playerCol = field % 100;
        if (instance.map[playerRow - 1, playerCol - 1].isSnake || !instance.map[playerRow - 1, playerCol - 1].isRoad || instance.map[playerRow - 1, playerCol - 1].isTail)
        {
            return false;
        }
        return true;
    }
    public void die(double[] state)
    {
        reward = -10;
        aiManager.betterAgent.uploadReplay(new Replay(state, getActionIndexFromDir(), reward, null, true));
        episodeReward += reward;
        instance.endTrainSession();
        // aiManager.betterAgent.copyWeights(); pamiętaj o krokach XD
    }

    //bez dawania powtórki
    public void validationDie()
    {
        // Debug.Log("Smierc walidacja");
        reward = -10;
        episodeReward += reward;
        // Debug.Log("mapa umieram walidacja: ");
        if (GM.showValidation)
            instance.showMap();
        instance.endTrainSession();
    }
    public void moveTailFlag()
    {
        int playerTail = segmentsPlacement[segmentsPlacement.Count - 1];                        // usuwa właściwości ogon i wąż z ostatniego i przedostatniego pola
        int playerRowTail = playerTail / 100;
        int playerColTail = playerTail % 100;
        instance.map[playerRowTail - 1, playerColTail - 1].isSnake = false;
        instance.map[playerRowTail - 1, playerColTail - 1].isTail = false;
        int playerChangeTail = segmentsPlacement[segmentsPlacement.Count - 2];
        int playerRowChange = playerChangeTail / 100;
        int playerColChange = playerChangeTail % 100;
        instance.map[playerRowChange - 1, playerColChange - 1].isTail = true;
        tail_dir = playerChangeTail - playerTail;
    }
    public void getAiMove(double[] state)
    {
        double[] results = aiManager.betterAgent.target.predict(state);
        double max = double.MinValue;
        for (int i = 0; i <= 3; i++)
        {
            if (results[i] > max && lastDir != -getMoveFromActionIndex(i))
            {
                max = results[i];
                dir = getMoveFromActionIndex(i);
            }
        }
    }
    public int getMoveFromActionIndex(int i)
    {
        if (i == 0)
        {
            return -100;
        }
        if (i == 1)
        {
            return 1;
        }
        if (i == 2)
        {
            return 100;
        }
        if (i == 3)
        {
            return -1;
        }
        return 0;
    }
}
