using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;



public class AI : MonoBehaviour
{
    // Start is called before the first frame update
    public gameManagement GM;

    public newDQN betterAgent;

    public List<int> layersSizes;
    public List<double> firstReward= new List<double>();
    public List<double> secondReward= new List<double>();
    public double maxReward;
    void Start()
    {
        GM=GameObject.Find("GameManager").GetComponent<gameManagement>();

        betterAgent = new newDQN(128_000,layersSizes);
        betterAgent.copyWeights();
        betterAgent.aiManager=GetComponent<AI>();

        string s ="";
        for(int i=0; i<=layersSizes.Count-1; i++)
        {
            s+=layersSizes[i]+" ";
        }
        GM.infoText.GetComponent<TMP_Text>().text=s;
        // theBestAgent = new DQNVectorized(131072,layersSizes);
        // theBestAgent.copyWeights();

        // agent = new DQN(1048576);
        // agent.main.randomizeWeightsHe();
        // agent.main.agent=agent;
        // agent.main.randomizeBiasHe();
        // agent.copyWeights();

        maxReward=double.MinValue;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    // public void uploadReplay(double[] env, int act, double rew, double[] newEnv, bool isEnd)
    // {
    //     Replay toAdd = new Replay(env, act, rew, newEnv, isEnd);
    //     betterAgent.replayBuffer[betterAgent.replayCounter % betterAgent.replaySize] = toAdd;
    //     betterAgent.replayCounter++;
    //     // agent.testDuringWalk();
    // }
    








































    public void dropLogFile()   //przycisk bez argumentu
    {
        newDQNJsonSave drop = new newDQNJsonSave();
        drop.bias1 = betterAgent.main.layers[1].bias;
        drop.bias2 = betterAgent.main.layers[2].bias;
        drop.bias3 = betterAgent.main.layers[3].bias;
        // drop.bias4 = betterAgent.main.layers[4].bias;
        drop.weights1 = flattenTable(betterAgent.main.layers[1].weightsToPast);
        drop.weights2 = flattenTable(betterAgent.main.layers[2].weightsToPast);
        drop.weights3 = flattenTable(betterAgent.main.layers[3].weightsToPast);
        // drop.weights4 = flattenTable(betterAgent.main.layers[4].weightsToPast);
        drop.moments1=flattenTable(betterAgent.main.layers[1].momentCorrected);
        drop.moments2=flattenTable(betterAgent.main.layers[2].momentCorrected);
        drop.moments3=flattenTable(betterAgent.main.layers[3].momentCorrected);

        drop.velocity1=flattenTable(betterAgent.main.layers[1].velocityCorrected);
        drop.velocity2=flattenTable(betterAgent.main.layers[2].velocityCorrected);
        drop.velocity3=flattenTable(betterAgent.main.layers[3].velocityCorrected);

        drop.biasMoments1=betterAgent.main.layers[1].biasMomentCorrected;
        drop.biasMoments2=betterAgent.main.layers[2].biasMomentCorrected;
        drop.biasMoments3=betterAgent.main.layers[3].biasMomentCorrected;

        drop.biasVelocity1=betterAgent.main.layers[1].biasVelocityCorrected;
        drop.biasVelocity2=betterAgent.main.layers[2].biasVelocityCorrected;
        drop.biasVelocity3=betterAgent.main.layers[3].biasVelocityCorrected;
        
        string json = JsonUtility.ToJson(drop, true);
        File.WriteAllText(Application.dataPath + "/LOGI.json", json);
        dropMSE();
        printArray(betterAgent.main.layers[1].moment);

        replayBuffer buf = new replayBuffer();
        buf.Buffer = betterAgent.replayBuffer;
        string bufToSave = JsonUtility.ToJson(buf);
        File.WriteAllText(Application.dataPath + "/buffer.json", bufToSave);
    }

    public void dropLogFile(string fileName)   //to dla poszczegolnych funkcji wiec nazwa nadawana na miejscu
    {
        newDQNJsonSave drop = new newDQNJsonSave();
        drop.bias1 = betterAgent.main.layers[1].bias;
        drop.bias2 = betterAgent.main.layers[2].bias;
        drop.bias3 = betterAgent.main.layers[3].bias;
        // drop.bias4 = betterAgent.main.layers[4].bias;
        drop.weights1 = flattenTable(betterAgent.main.layers[1].weightsToPast);
        drop.weights2 = flattenTable(betterAgent.main.layers[2].weightsToPast);
        drop.weights3 = flattenTable(betterAgent.main.layers[3].weightsToPast);
        // drop.weights4 = flattenTable(betterAgent.main.layers[4].weightsToPast);

        drop.moments1=flattenTable(betterAgent.main.layers[1].momentCorrected);
        drop.moments2=flattenTable(betterAgent.main.layers[2].momentCorrected);
        drop.moments3=flattenTable(betterAgent.main.layers[3].momentCorrected);

        drop.velocity1=flattenTable(betterAgent.main.layers[1].velocityCorrected);
        drop.velocity2=flattenTable(betterAgent.main.layers[2].velocityCorrected);
        drop.velocity3=flattenTable(betterAgent.main.layers[3].velocityCorrected);

        drop.biasMoments1=betterAgent.main.layers[1].biasMomentCorrected;
        drop.biasMoments2=betterAgent.main.layers[2].biasMomentCorrected;
        drop.biasMoments3=betterAgent.main.layers[3].biasMomentCorrected;

        drop.biasVelocity1=betterAgent.main.layers[1].biasVelocityCorrected;
        drop.biasVelocity2=betterAgent.main.layers[2].biasVelocityCorrected;
        drop.biasVelocity3=betterAgent.main.layers[3].biasVelocityCorrected;

        dropMSE();
        string json = JsonUtility.ToJson(drop, true);
        File.WriteAllText(Application.dataPath + fileName, json);

        replayBuffer buf = new replayBuffer();
        buf.Buffer = betterAgent.replayBuffer;
        string bufToSave = JsonUtility.ToJson(buf);
        File.WriteAllText(Application.dataPath + fileName.Substring(0,6) + "buffer.json", bufToSave);
    }
    // public void readWeightsFromFile()
    // {
    //     string json = File.ReadAllText(Application.dataPath + "/LOGI.json");
    //     dataSave toRead = JsonUtility.FromJson<dataSave>(json);
    //     betterAgent.main.setBias(toRead.hiddenFirstBias, toRead.hiddenSecondBias, toRead.outputBias);
    //     betterAgent.main.setWeights(unFlatTable(toRead.inputToHiddenFlat,betterAgent.main.input.Length,betterAgent.main.hiddenFirst.Length), unFlatTable(toRead.hiddenToSecondFlat,betterAgent.main.hiddenFirst.Length,betterAgent.main.hiddenSecond.Length), unFlatTable(toRead.secondToOutputFlat,betterAgent.main.hiddenSecond.Length,betterAgent.main.output.Length));
    // }
    public void readBuffer()
    {
        string json = File.ReadAllText(Application.dataPath + "/buffer.json");
        betterAgent.replayBuffer = JsonUtility.FromJson<replayBuffer>(json).Buffer;
    }
    public void readWeights()  //czyta sie tylko z pliku logi
    {
        string json = File.ReadAllText(Application.dataPath + "/LOGI.json");
        newDQNJsonSave data = JsonUtility.FromJson<newDQNJsonSave>(json);
        betterAgent.main.layers[1].weightsToPast=unFlatTable(data.weights1,betterAgent.main.layers[0].values.Length,betterAgent.main.layers[1].values.Length);
        betterAgent.main.layers[2].weightsToPast=unFlatTable(data.weights2,betterAgent.main.layers[1].values.Length,betterAgent.main.layers[2].values.Length);
        betterAgent.main.layers[3].weightsToPast=unFlatTable(data.weights3,betterAgent.main.layers[2].values.Length,betterAgent.main.layers[3].values.Length);
        //betterAgent.main.layers[4].weightsToPast=unFlatTable(data.weights4,betterAgent.main.layers[3].values.Length,betterAgent.main.layers[4].values.Length);
        betterAgent.main.layers[1].bias=data.bias1;
        betterAgent.main.layers[2].bias=data.bias2;
        betterAgent.main.layers[3].bias=data.bias3;

        betterAgent.main.layers[1].momentCorrected=unFlatTable(data.moments1,betterAgent.main.layers[0].values.Length,betterAgent.main.layers[1].values.Length);
        betterAgent.main.layers[1].moment=unFlatTable(data.moments1,betterAgent.main.layers[0].values.Length,betterAgent.main.layers[1].values.Length);

        betterAgent.main.layers[2].momentCorrected=unFlatTable(data.moments2,betterAgent.main.layers[1].values.Length,betterAgent.main.layers[2].values.Length);
        betterAgent.main.layers[2].moment=unFlatTable(data.moments2,betterAgent.main.layers[1].values.Length,betterAgent.main.layers[2].values.Length);

        betterAgent.main.layers[3].momentCorrected=unFlatTable(data.moments3,betterAgent.main.layers[2].values.Length,betterAgent.main.layers[3].values.Length);
        betterAgent.main.layers[3].moment=unFlatTable(data.moments3,betterAgent.main.layers[2].values.Length,betterAgent.main.layers[3].values.Length);

        betterAgent.main.layers[1].velocityCorrected=unFlatTable(data.velocity1,betterAgent.main.layers[0].values.Length,betterAgent.main.layers[1].values.Length);
        betterAgent.main.layers[1].velocity=unFlatTable(data.velocity1,betterAgent.main.layers[0].values.Length,betterAgent.main.layers[1].values.Length);

        betterAgent.main.layers[2].velocityCorrected=unFlatTable(data.velocity2,betterAgent.main.layers[1].values.Length,betterAgent.main.layers[2].values.Length);
        betterAgent.main.layers[2].velocity=unFlatTable(data.velocity2,betterAgent.main.layers[1].values.Length,betterAgent.main.layers[2].values.Length);

        betterAgent.main.layers[3].velocityCorrected=unFlatTable(data.velocity3,betterAgent.main.layers[2].values.Length,betterAgent.main.layers[3].values.Length);
        betterAgent.main.layers[3].velocity=unFlatTable(data.velocity3,betterAgent.main.layers[2].values.Length,betterAgent.main.layers[3].values.Length);

        betterAgent.main.layers[1].biasVelocity=data.biasVelocity1;
        betterAgent.main.layers[1].biasVelocityCorrected=data.biasVelocity1;

        betterAgent.main.layers[2].biasVelocity=data.biasVelocity2;
        betterAgent.main.layers[2].biasVelocityCorrected=data.biasVelocity2;

        betterAgent.main.layers[3].biasVelocity=data.biasVelocity3;
        betterAgent.main.layers[3].biasVelocityCorrected=data.biasVelocity3;

        betterAgent.main.layers[1].biasMoment=data.biasMoments1;
        betterAgent.main.layers[1].biasMomentCorrected=data.biasMoments1;

        betterAgent.main.layers[2].biasMoment=data.biasMoments2;
        betterAgent.main.layers[2].biasMomentCorrected=data.biasMoments2;

        betterAgent.main.layers[3].biasMoment=data.biasMoments3;
        betterAgent.main.layers[3].biasMomentCorrected=data.biasMoments3;

        readBuffer();

       // betterAgent.main.layers[4].bias=data.bias4;
        betterAgent.copyWeights();
    }


    public void printArray(double[,] a)
    {
        string s="";
        for(int i=0; i<=a.GetLength(0)-1; i++)
        {
            for(int j=0; j<a.GetLength(1)-1; j++)
            {
                s+=a[i,j].ToString()+'\n';
            }
        }
        Debug.Log(s);
    }
    public void dropMSE()
    {
        JSONMse save = new JSONMse();
        save.Mses=betterAgent.averageMseOverEpochs;
        string json = JsonUtility.ToJson(save,true);
        File.WriteAllText(Application.dataPath + "/MSE.json", json);



        snakeRewardsJson FirstSnakeReward = new snakeRewardsJson();
        snakeRewardsJson SecondSnakeReward = new snakeRewardsJson();
        FirstSnakeReward.rewards=firstReward;
        SecondSnakeReward.rewards=secondReward;
        json = JsonUtility.ToJson(FirstSnakeReward,true);
        File.WriteAllText(Application.dataPath + "/firstSnakeRewards.json", json);
        json = JsonUtility.ToJson(SecondSnakeReward,true);
        File.WriteAllText(Application.dataPath + "/secondsSnakeRewards.json", json);

        JSONMse validation = new JSONMse();
        validation.Mses=betterAgent.validationResults;
        string validateJson = JsonUtility.ToJson(validation,true);
        File.WriteAllText(Application.dataPath + "/validate.json", validateJson);
    }
    

    public void addSnakeRewardsToList()
    {
        if(GM.testSpace.firstSnakeRewards + GM.testSpace.secondSnakeRewards > maxReward)
        {
            maxReward=GM.testSpace.firstSnakeRewards + GM.testSpace.secondSnakeRewards;
            dropLogFile("/BestWeights.json");
        }
        firstReward.Add(GM.testSpace.firstSnakeRewards);
        GM.testSpace.firstSnakeRewards=0;
        secondReward.Add(GM.testSpace.secondSnakeRewards);
        GM.testSpace.secondSnakeRewards=0;
    }
    public double[] flattenTable(double[,] table)
    {
        double[] flatten = table.Cast<double>().ToArray();
        return flatten;
    }
    public double[,] unFlatTable(double[] flat,int rows, int columns)
    {
        double[,] unFlatten = new double[rows,columns];
        int index=0;
        for(int i=0; i<=rows-1; i++)
        {
            for(int j=0; j<=columns-1; j++)
            {
                unFlatten[i,j]=flat[index];
                index++;
            }
        }
        return unFlatten;
    }
}








/* pamiątka starych dobrych koń czasów

    public void testTrain()
    {
        double[] result = agent.main.predict(agent.ReplayBuffer[agent.replayCounter].state);
        double target;
        if(agent.ReplayBuffer[agent.replayCounter].terminalState)
        {
            target = agent.ReplayBuffer[agent.replayCounter].reward;
        }else
        {
            target = agent.ReplayBuffer[agent.replayCounter].reward + agent.discountFactor * agent.getMax(agent.target.predict(agent.ReplayBuffer[agent.replayCounter].newState));
        }
        agent.main.backpropagationSingle(result[agent.ReplayBuffer[agent.replayCounter].action],target,agent.ReplayBuffer[agent.replayCounter].action);
        }
    public IEnumerator testSet()
    {
        double testCount=0;
        while(true)
        {
            agent.train();
            testCount+=agent.BatchSize;
            yield return null;
            desc.GetComponent<TMP_Text>().text="wykonano: " + testCount;
            if (Input.GetKeyDown(KeyCode.U))
            {
                desc.GetComponent<TMP_Text>().text="przerwano";
                yield break;
            }
        }
    }
    
    public void updateLearningRates(string newRate)
    {
        newRate=newRate.Replace(".",",");
        agent.main.learningRate=double.Parse(newRate);
        agent.target.learningRate=double.Parse(newRate);
        Debug.Log(agent.main.learningRate);
    }
}
*/
