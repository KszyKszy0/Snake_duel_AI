using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using TMPro;

[System.Serializable]
public class newDQN
{
    
    public int replaySize;
    public int replayCounter;
    public int minibatchSize = 128;
    public int minimumReplaysToTrain = 10_000;
    public double discountFactor = 0.99;
    public int epochCount;
    public double MSEpublic;
    public double MSEEpochCounter;
    
    [NonSerialized]
    public Replay[] replayBuffer;



    public NNReworkLayers main;
    public NNReworkLayers target;



    
    public List<double> averageMseOverEpochs = new List<double>();
    public int[] actions = { 0, 0, 0, 0 };
    public AI aiManager;

    //wstawia na nowe miejsce jeśli max wraca na początek
    public int updateCounter;   
    public int updateTime = 1_000;

    
    public int learningCounter = 0;


    //scheduler
    public double initialLearningRate = 5e-4;
    public double minLearningRate = 1e-10;
    public double gamma = 0.1; 
    public double stepSize = 40;
    public double decayFactor = 0.1;
    public double exponentialDecayRate = 0.01;
    public int patience = 9;    
    public int patienceCounter = 0;
    public double bestLoss = double.MaxValue;
    public double currentLoss = 0;


    // validation

    double bestValidation = double.MinValue;
    public List<double> validationResults = new List<double>();
    public void uploadReplay(Replay add)
    {
        replayBuffer[replayCounter % replaySize] = add;
        // indexesToVisit.Add(replayCounter%replaySize);
        replayCounter++;
        updateCounter++;
        train();
        if (updateCounter >= updateTime)
        {
            copyWeights();
            updateCounter = 0;
        }

    }
    //trzeba rozważyć trzy przypadki 
    //1. za mało indexów na minibatch, to wtedy po prostu do końca? bo problem to uśrednić albo w sumie nie bo przeczież diviser jA tO jEdNaK mĄdRy jestem
    //2. wystarczająco no to po prostu losuje się 
    //3. a jak się skończą no to odnawiamy listę bo jest kąstas
    // public void train()
    // {
    //     int[] indexes;
    //     if (replayCounter > minimumReplaysToTrain)
    //     {
    //         if (indexesToVisit.Count > minibatchSize)
    //         {
    //             indexes = new int[minibatchSize];
    //         }
    //         else
    //         {
    //             indexes = new int[indexesToVisit.Count];
    //         }
    //         for (int i = 0; i <= indexes.Length - 1; i++)
    //         {
    //             indexes[i] = UnityEngine.Random.Range(0, indexesToVisit.Count);
    //             indexesToVisit.RemoveAt(indexes[i]);
    //         }
    //         for (int i = 0; i <= indexes.Length - 1; i++)
    //         {
    //             double[] current = main.testPredict(replayBuffer[indexes[i]].state);
    //             double[] targetQ = target.predict(replayBuffer[indexes[i]].newState);
    //             double real = max(targetQ) * discountFactor + replayBuffer[indexes[i]].reward;
    //             //Debug.Log(MSEDiff(current[replayBuffer[indexes[i]].action],real));
    //             //Debug.Log(MSE(current[replayBuffer[indexes[i]].action],real));
    //             MSEpublic = MSE(current[replayBuffer[indexes[i]].action], real);
    //             MSEEpochCounter += MSEpublic;
    //             main.backPropagationSGD(MSEDiff(current[replayBuffer[indexes[i]].action], real), replayBuffer[indexes[i]].action);
    //             //main.backPropagationAdam(MSEDiff(current[replayBuffer[indexes[i]].action],real),replayBuffer[indexes[i]].action);
    //             actions[replayBuffer[indexes[i]].action]++;
    //         }
    //         updateWeightsAndBiases(minibatchSize);
    //         if (indexesToVisit.Count == 0) //koniec epoki
    //         {
    //             //updateWeightsAndBiases(replaySize);
    //             refillReplays();
    //             if (epochCount > 1)
    //             {
    //                 double averageMse = MSEEpochCounter / replaySize;
    //                 if (averageMse < bestLoss)
    //                 {
    //                     bestLoss = averageMse;
    //                     patienceCounter = 0;
    //                 }
    //                 else
    //                 {
    //                     patienceCounter++;
    //                     if (patienceCounter >= patience)
    //                     {
    //                         foreach (Layer l in main.layers)
    //                         {
    //                             l.learningRate *= decayFactor;
    //                             l.learningRate = Math.Max(l.learningRate, minLearningRate);
    //                         }
    //                         patienceCounter = 0;
    //                     }
    //                 }
    //                 MSEEpochCounter = 0;
    //                 averageMseOverEpochs.Add(averageMse);
    //                 epochs.Add(epochCount);
    //                 aiManager.addSnakeRewardsToList();
    //             }
    //         }
    //     }
    // }

    
    public void train()
    {
        if (replayCounter > replaySize) //zmiana na poczkeanie na caly
        {
            // Debug.Log(learningCounter+" przed sprawdzeniem epoki");
            if(learningCounter + minibatchSize >= replaySize)
            {
                endEpoch();
            }
    
            double limit = Mathf.Min(learningCounter + minibatchSize - 1, replaySize - 1);  //maksymalny limit nie może przekrocztyć replaysize -1 - ostatni rekord
            // Debug.Log(learningCounter+" "+limit);
            for (int i = learningCounter; i <= limit; i++)
            {
                //value do nauki
                double real;
                double[] current = main.predict(replayBuffer[i].state);
                if(replayBuffer[i].terminalState)
                {
                    real=replayBuffer[i].reward;
                }
                else
                {
                    double[] targetQ = target.predict(replayBuffer[i].newState);
                    real = max(targetQ) * discountFactor + replayBuffer[i].reward;
                }
                


                //error MSE
                //Debug.Log(MSEDiff(current[replayBuffer[i].action],real));
                //Debug.Log(MSE(current[replayBuffer[i].action],real));
                MSEpublic = MSE(current[replayBuffer[i].action], real);
                MSEEpochCounter += MSEpublic;


                //propagacja
                //main.backPropagationSGD(MSEDiff(current[replayBuffer[i].action], real),replayBuffer[i].action,replayBuffer[i].state);
                main.backPropagationAdam(MSEDiff(current[replayBuffer[i].action],real),replayBuffer[i].action,replayBuffer[i].state);


                //akcje do sredniej
                actions[replayBuffer[i].action]++;

            }

            //aktualizacja po każdym minbatchu
            learningCounter += minibatchSize;
            aiManager.GM.infoText.GetComponent<TMP_Text>().text=learningCounter+"           "+'\n'+"bestLoss: "+bestLoss+'\n'+"best validation: "+bestValidation; 
            updateWeightsAndBiases(minibatchSize);
        }
    }

    public void endEpoch()
    {
            epochCount++;
            //updateWeightsAndBiases(replaySize);
            //odnowienie i przeshufflwoanie datasetu
            // Debug.Log("koniec epoki");
            refillReplays();
            // Debug.Log("przelosowano");

            //sprawdzanie lossu / scheduler
            // currentLoss = MSEEpochCounter / replaySize;
            // if (currentLoss < bestLoss)
            // {
            //     bestLoss = currentLoss;
            //     patienceCounter = 0;
            // }
            // else
            // {
            //     patienceCounter++;
            //     if (patienceCounter >= patience)
            //     {
            //         foreach (Layer l in main.layers)
            //         {
            //             // Debug.Log("before: " + l.learningRate.ToString());
            //             l.learningRate *= decayFactor;
            //             // Debug.Log("after: " + l.learningRate.ToString());
            //             l.learningRate = Math.Max(l.learningRate, minLearningRate);
            //         }
            //         patienceCounter = 0;
            //     }
            // }

            
            // double tempLearningRate = initialLearningRate * Math.Pow(gamma,epochCount/stepSize);
            // foreach (Layer l in main.layers)
            // {
            //     Debug.Log("before: " + l.learningRate.ToString());
            //     l.learningRate = tempLearningRate;
            //     Debug.Log("after: " + l.learningRate.ToString());
            // }


            currentLoss = MSEEpochCounter / replaySize;
            if (currentLoss < bestLoss)
            {
                bestLoss = currentLoss;
            }

            //scheduler
            // double tempLearningRate = initialLearningRate * Math.Exp(-exponentialDecayRate*epochCount);
            // Debug.Log("before: " + main.layers[1].learningRate.ToString());
            // foreach (Layer l in main.layers)
            // {
            //    l.learningRate = tempLearningRate;
            //    l.learningRate = Math.Max(l.learningRate, minLearningRate);
            // }
            // Debug.Log("after: " + main.layers[1].learningRate.ToString());

            
            //validacja
            // Debug.Log("rozpoczeto walidacje");
            double validation = aiManager.GM.playValidation();
            validationResults.Add(validation);
            if (validation > bestValidation)
            {
                bestValidation = validation;
                aiManager.dropLogFile("/BestValidation.json");
            }
            // Debug.Log("skonczono walidacje");

            //aktualizacja informacji o epokach
            MSEEpochCounter = 0;
            averageMseOverEpochs.Add(currentLoss);
            aiManager.addSnakeRewardsToList();
    }

    public void updateWeightsAndBiases(int div)
    {
        main.layers[main.layers.Count - 1].applyWeightsLast(actions);
        main.layers[main.layers.Count - 1].applyBiasLast(actions);
        actions = new int[4];
        main.layers[main.layers.Count - 1].adamCounter++;
        for (int k = main.layers.Count - 2; k >= 1; k--)   //// nie dawaj APLLYU na INMPUTY!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            main.layers[k].applyWeights(div);
            main.layers[k].applyBias(div);
            main.layers[k].adamCounter++;
        }
    }
    //zapełnienie listy cyferkami od nowa
    // public void refillReplays()
    // {
    //     epochCount++;
    //     int limit = Mathf.Min(replayCounter, replaySize - 1);
    //     for (int i = 0; i <= limit; i++)
    //     {
    //         indexesToVisit.Add(i);
    //     }
    // }

    public void refillReplays()
    {
        learningCounter = 0;
        int limit = Mathf.Min(replayCounter, replaySize - 1);
        for (int i = 0; i <= limit; i++)
        {
            int rand = UnityEngine.Random.Range(i, limit);
            if (replayBuffer[rand] != null)
            {
                Replay temp = replayBuffer[rand];
                replayBuffer[rand] = replayBuffer[i];
                replayBuffer[i] = temp;
            }
        }
    }
    //kopiuje od pierwszej uklrytej bo w tej implementacji input nie ma wag ani biasów jest przrsunięcie taki "offset" o jeden w prawo
    public void copyWeights()
    {
        List<double[,]> weightsToCopy = new List<double[,]>();
        List<double[]> biasToCopy = new List<double[]>();
        foreach (Layer l in main.layers)
        {
            weightsToCopy.Add(l.weightsToPast);
            biasToCopy.Add(l.bias);
        }
        for (int i = 1; i <= target.layers.Count - 1; i++)
        {
            target.layers[i].weightsToPast = (double[,])weightsToCopy[i].Clone();
        }
        for (int i = 1; i <= target.layers.Count - 1; i++)
        {
            target.layers[i].bias = (double[])biasToCopy[i].Clone();
        }
    }

    //definicja błędu (diff z MSE)
    public double MSEDiff(double current, double target)
    {
        return current - target;
    }

    public double MSE(double current, double target)
    {
        return Math.Pow((current - target), 2) / 2;
    }
    // konstruktor z ai menedżera dostaje wielkości warstw potem je tworzy zgodnie z ich rozmairem
    public newDQN(int r, List<int> size)
    {
        replaySize = r;
        replayBuffer = new Replay[replaySize];
        main = new NNReworkLayers(size,initialLearningRate);
        // main.init(size);
        target = new NNReworkLayers(size,initialLearningRate);
        // target.init(size);
    }

    //archaiczna funkcja do znalezienia maxa
    public double max(double[] toCheck)
    {
        double max = double.MinValue;
        for (int i = 0; i <= toCheck.Length - 1; i++)
        {
            if (toCheck[i] > max)
            {
                max = toCheck[i];
            }
        }
        return max;
    }

    //zrzut wag do pliku
    public void saveWeights()
    {
        var newSave = new newDQNJsonSave();
        newSave.weights1 = main.layers[1].weightsToPast.Cast<double>().ToArray();
        newSave.bias1 = main.layers[1].bias;
        newSave.weights2 = main.layers[2].weightsToPast.Cast<double>().ToArray();
        newSave.bias2 = main.layers[2].bias;
        newSave.weights3 = main.layers[3].weightsToPast.Cast<double>().ToArray();
        newSave.bias3 = main.layers[3].bias;
        string json = JsonUtility.ToJson(newSave, true);
        File.WriteAllText(Application.dataPath + "/logs.json", json);
    }
    public void readWeights()
    {
        string json = File.ReadAllText(Application.dataPath + "/logs.json");
        newDQNJsonSave data = JsonUtility.FromJson<newDQNJsonSave>(json);
        target.layers[1].weightsToPast = twoDimensionArray(data.weights1, target.layers[0].values.Length, target.layers[1].values.Length);
        target.layers[2].weightsToPast = twoDimensionArray(data.weights2, target.layers[1].values.Length, target.layers[2].values.Length);
        target.layers[3].weightsToPast = twoDimensionArray(data.weights3, target.layers[2].values.Length, target.layers[3].values.Length);
        target.layers[1].bias = data.bias1;
        target.layers[2].bias = data.bias2;
        target.layers[3].bias = data.bias3;
    }

    public double[,] twoDimensionArray(double[] a, int x, int y)
    {
        int index = 0;
        double[,] newArray = new double[x, y];
        for (int i = 0; i <= x - 1; i++)
        {
            for (int j = 0; j <= y - 1; j++)
            {
                newArray[i, j] = a[index];
                index++;
            }
        }
        return newArray;
    }
}
