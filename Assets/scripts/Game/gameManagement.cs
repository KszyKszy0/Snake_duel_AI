using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
public class gameManagement : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField]
    public GameObject snakeAiPrefab;
    [SerializeField]
    public GameObject snakePlayerPrefab;
    [SerializeField]
    public GameObject trainSnakePrefab;
    [SerializeField]
    public GameObject emptySquare;


    [SerializeField]
    public Sprite apple;
    [SerializeField]
    public Sprite buildSquare;
    

    public double randomMoveProbability;
    public double minRandomMoveProbability;
    public double randomDecay;
    
    
    public AI AiManager;


    public GameEnviroment mainGame;
    public TestEnviroment testSpace;
    public TestEnviroment validationMap;


    public GameObject Normal;
    public GameObject Clear;
    public GameObject start;
    public GameObject back;
    public GameObject save;
    public GameObject read;
    public GameObject saveConfirm;
    public GameObject saveCancel;
    public GameObject readConfirm;
    public GameObject readCancel;
    public GameObject setup;
    public GameObject learn;
    public GameObject lrChange;
    public GameObject probabilityRateChange;
    public GameObject infoText;

    public bool showValidation;


    public List<TestEnviroment> tests = new List<TestEnviroment>();
    void Start()
    {
        mainGame = gameObject.AddComponent(typeof(GameEnviroment)) as GameEnviroment;
        mainGame.emptySquare=emptySquare;
        mainGame.buildSquare=buildSquare;
        mainGame.snakePlayerPrefab=snakePlayerPrefab;
        mainGame.snakeAiPrefab=snakeAiPrefab;
        mainGame.apple=apple;
        mainGame.init(8,16,0,0);
        testSpace = gameObject.AddComponent(typeof(TestEnviroment)) as TestEnviroment;
        testSpace.trainSnakePrefab=trainSnakePrefab;
        testSpace.init(8,16);


        validationMap = gameObject.AddComponent(typeof(TestEnviroment)) as TestEnviroment;
        validationMap.trainSnakePrefab=trainSnakePrefab;
        validationMap.init(8,16);
        validationMap.setValidationMap();
        validationMap.setupTrain();   //resp snake
        // for(int i=0; i<=9; i++)
        // {
        //     var testToAdd = gameObject.AddComponent(typeof(TestEnviroment)) as TestEnviroment;
        //     testToAdd.trainSnakePrefab=trainSnakePrefab;
        //     testToAdd.init(8,16);
        //     tests.Add(testToAdd);
        // }
    }
    

    // Update is called once per frame
    void Update()
    {

    }

    public void normal()
    {
        for (int i = 0; i <= mainGame.fDimension-1; i++)
        {
            for (int j = 0; j <= mainGame.sDimension-1; j++)
            {
                if (!mainGame.map[i, j].field.GetComponent<emptySquare>().isBuild)
                {
                    mainGame.map[i, j].field.GetComponent<emptySquare>().makeSquare();
                }
            }
        }
    }
    public void clear()
    {
        for (int i = 0; i <= mainGame.fDimension-1; i++)
        {
            for (int j = 0; j <= mainGame.sDimension-1; j++)
            {
                if (mainGame.map[i, j].field.GetComponent<emptySquare>().isBuild)
                {
                    mainGame.map[i, j].field.GetComponent<emptySquare>().makeSquare();
                }
            }
        }
    }
    

    public void StartGame()
    {
        if(mainGame.checkMap())
        {
            Normal.SetActive(false);
            Clear.SetActive(false);
            start.SetActive(false);
            back.SetActive(true);
            mainGame.CreateApple();
            mainGame.SpawnSnakes();
        }
        
        // Game = StartCoroutine("gameRun");
    }

    public void StartTestGame()
    {
        if(mainGame.checkMap())
        {
            Normal.SetActive(false);
            Clear.SetActive(false);
            start.SetActive(false);
            back.SetActive(true);
            mainGame.trainSnakeMatch();
        }
    }
    public void toEditor()
    {
        Normal.SetActive(true);
        Clear.SetActive(true);
        start.SetActive(true);
        back.SetActive(false);
        mainGame.gameEnd();
    }
    public void tryToSave()
    {
        saveConfirm.SetActive(true);
        saveCancel.SetActive(true);
    }
    public void tryToRead()
    {
        readConfirm.SetActive(true);
        readCancel.SetActive(true);
    }
    public void closeSave()
    {
        saveConfirm.SetActive(false);
        saveCancel.SetActive(false);
    }
    public void closeRead()
    {
        readConfirm.SetActive(false);
        readCancel.SetActive(false);
    }
    public void updateProbabilityRate(string newProbabilityRate)
    {
        newProbabilityRate = newProbabilityRate.Replace(".", ",");
        randomMoveProbability = float.Parse(newProbabilityRate, System.Globalization.NumberStyles.Float);
        Debug.Log(randomMoveProbability);
    }
    public void changeLearningRate(string inp)
    {
        inp = inp.Replace(".", ",");
        double newLearningRate = double.Parse(inp, System.Globalization.NumberStyles.Float);
        if(newLearningRate>0)
        {
            foreach(Layer l in AiManager.betterAgent.main.layers)
            {
                l.learningRate=newLearningRate;
            }
        }
        Debug.Log(newLearningRate);
    }
    
    public IEnumerator fullReplay()
    {
        while (true)
        {
            testSpace.makeRandomPracticeMap(80);
            testSpace.setupTrain();
            testSpace.startTrainSession();
            testSpace.clearPractice();
            // for(int i=0; i<=tests.Count-1; i++)
            // {
            //     tests[i].makeRandomPracticeMap(70+i*3);
            //     tests[i].setupTrain();
            //     tests[i].startTrainSession();
            //     tests[i].clearPractice();
            // }
            yield return null;
            if (Input.GetKeyDown(KeyCode.L))
            {
                yield break;
            }
        }
    }
    public void startReplayFulling()
    {
        StartCoroutine("fullReplay");
    }

    public double playValidation()
    {
        validationMap.startValidationSession();  //losowy start snakow
        // Debug.Log("mapa po zresetowaniu walidacja: ");
        // validationMap.showMap();
        // Debug.Log("1st reward: " + validationMap.firstSnakeRewards + " 2nd reward: " + validationMap.secondSnakeRewards);
        double finalResult = validationMap.firstSnakeRewards + validationMap.secondSnakeRewards;
        validationMap.firstSnakeRewards = 0;
        validationMap.secondSnakeRewards = 0;
        return finalResult;
    }
}
