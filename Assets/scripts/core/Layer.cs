using UnityEngine;
using System;
[System.Serializable]
public class Layer
{
    public double[] values;


    public double[] bias;
    public double[] biasChange;
    public double[] biasMoment;
    public double[] biasVelocity;
    public double[] biasMomentCorrected;
    public double[] biasVelocityCorrected;


    public double[] error;


    public double[,] weightsToPast;
    public double[,] weightsChange;
    public double[,] moment;
    public double[,] velocity;
    public double[,] momentCorrected;
    public double[,] velocityCorrected;



    public double learningRate;
    public double leak;
    public double adamBeta;
    public double adamBetaSquare;
    public double epsilon;
    public double adamCounter = 1;
    public double dropout;

    public double adamBetaAdjusted = 1;
    public double adamBetaSquareAdjusted = 1;
    public void init(int size)
    {
        bias = new double[size];
        biasMoment = new double[size];
        biasVelocity = new double[size];
        biasMomentCorrected = new double[size];
        biasVelocityCorrected = new double[size];
        biasChange = new double[size];


        values = new double[size];


        error = new double[size];



    }

    // inicjalizuje wagi wg he sqrt(2/poprzedniua ilosc neruoronow)
    public void initHe(int sizeBack)
    {
        double scale = Mathf.Sqrt(2f / sizeBack);
        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
            {
                weightsToPast[i, j] = UnityEngine.Random.Range(-1, 1f) * scale;
            }
        }
    }
    // inicjalizuje biasy wg he sqrt(2/poprzedniua ilosc neruoronow)
    public void intiBiasHe(int sizeBack)
    {
        double scale = Mathf.Sqrt(2f / sizeBack);
        for (int i = 0; i <= bias.Length - 1; i++)
        {
            bias[i] = UnityEngine.Random.Range(-1, 1f) * scale;
        }
    }

    // to jako input dostaje wyjście poprzedniej warstwy, ale to ono działa na swojej warstwie, czyli żeby dostać layer2[] = layer2.predict[layer1];
    public double[] forwardPass(double[] input, bool withBias)
    {
        double sum;
        for (int i = 0; i <= values.Length - 1; i++)
        {
            sum = 0;
            for (int j = 0; j <= input.Length - 1; j++)
            {
                sum += input[j] * weightsToPast[j, i];
            }
            if (withBias)
            {
                sum += bias[i];
            }
            values[i] = leakyRelU(sum);
        }
        return values;
    }

    public double[] forwardPass(double[] input, bool withBias, int a, int s)
    {
        double sum;
        for (int i = 0; i <= values.Length - 1; i++)
        {
            sum = 0;
            for (int j = 0; j <= 127; j++)
            {
                sum += input[j] * weightsToPast[j, i];
            }
            sum+=input[128+a]*weightsToPast[128+a,i];
            sum+=input[256+s]*weightsToPast[256+s,i];
            if (withBias)
            {
                sum += bias[i];
            }
            values[i] = leakyRelU(sum);
        }
        return values;
    }

    public double[] forwardTestPass(double[] input, bool withBias)
    {
        double sum;
        for (int i = 0; i <= values.Length - 1; i++)
        {
            sum = 0;
            if (UnityEngine.Random.Range(0, 1f) > dropout)
            {
                values[i]=0;
                continue;
            }
            for (int j = 0; j <= input.Length - 1; j++)
            {
                sum += input[j] * weightsToPast[j, i];
            }
            if (withBias)
            {
                sum += bias[i];
            }
            values[i] = leakyRelU(sum);
        }
        return values;
    }

    public double[] forwardTestPass(double[] input, bool withBias, int a, int s)
    {
        double sum;
        for (int i = 0; i <= values.Length - 1; i++)
        {
            sum = 0;
            if (UnityEngine.Random.Range(0, 1f) > dropout)
            {
                values[i]=0;
                continue;
            }
            for (int j = 0; j <= 127; j++)
            {
                sum += input[j] * weightsToPast[j, i];
            }
            sum+=input[128+a]*weightsToPast[128+a,i];
            sum+=input[256+s]*weightsToPast[256+s,i];
            if (withBias)
            {
                sum += bias[i];
            }
            values[i] = leakyRelU(sum);
        }
        return values;
    }

    public double[] forwardLastPass(double[] input, bool withBias)
    {
        double sum;
        for (int i = 0; i <= values.Length - 1; i++)
        {
            sum = 0;
            for (int j = 0; j <= input.Length - 1; j++)
            {
                sum += input[j] * weightsToPast[j, i];
            }
            if (withBias)
            {
                sum += bias[i];
            }
            values[i] = sum;
        }
        return values;
    }






    //liczy błąd warstwy ale potrzebuje do tego dodatkowych informacji n -- waga --  kolejny neuron (jego błąd)
    public void calcError(double[] errorNext, double[,] weightsToNext)
    {
        for (int i = 0; i <= values.Length - 1; i++)
        {
            double sum = 0;
            for (int j = 0; j <= errorNext.Length - 1; j++)
            {
                sum += errorNext[j] * weightsToNext[i, j];
            }
            error[i] = clipError(sum * diffleakyRelU(values[i]));
        }
    }


    //teraz error dla ostatniej ukrytej bo ona tylko w jedno
    public void semiLastError(int action, double[,] weightsToNext, double errorNext)
    {
        for (int i = 0; i <= error.Length - 1; i++)
        {
            error[i] = errorNext * weightsToNext[i, action] * diffleakyRelU(values[i]);
            error[i] = clipError(error[i]);
        }
    }








    //propagacja sgd jedna warstwa w tyl trzeba przypisać najpierw wartość errorowi
    public void lastPropSgd(int action, double[] prevValues)
    {
        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            weightsChange[i, action] -= learningRate * error[action] * prevValues[i];
        }
    }
    public void lastPropSgdBias(int action)
    {
        biasChange[action] -= learningRate * error[action];
    }

    // teraz fakryczny backpropSGD
    public void backpropSGD(double[] prevValues)
    {
        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
            {
                weightsChange[i, j] -= learningRate * error[j] * prevValues[i];
            }
        }
    }
    //to samo dla biasów
    public void backpropSGDBias()
    {
        for (int j = 0; j <= bias.Length - 1; j++)
        {
            biasChange[j] -= learningRate * error[j];
        }
    }


















    // back propaguje adam najp[ierw liczy bład] to bardzo ważne ten cały gradient względem itp... potem momenty, popraweion momenty kjtóre są zabarwone wzgędem momentu startowego i liczy błąd
    public void backpropAdam(double[] prevValues)
    {
        adamBetaAdjusted *= adamBeta;
        adamBetaSquareAdjusted *= adamBetaSquare;
        double tempError;
        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            if(prevValues[i]==0)
            {
                continue;
            }
            for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
            {
                tempError = error[j] * prevValues[i];

                moment[i, j] = adamBeta * moment[i, j] + (1 - adamBeta) * tempError;

                velocity[i, j] = adamBetaSquare * velocity[i, j] + (1 - adamBetaSquare) * Math.Pow(tempError, 2);

                


                momentCorrected[i, j] = moment[i, j] / (1 - adamBetaAdjusted); // pamiętaj o zwiększaniu adam coutner
                velocityCorrected[i, j] = velocity[i, j] / (1 - adamBetaSquareAdjusted);




                weightsChange[i, j] -= learningRate * momentCorrected[i, j] / (Math.Sqrt(velocityCorrected[i, j]) + epsilon);
            }
        }
    }

    public void backpropAdam(double[] prevValues, int a, int s)
    {
        double tempError;
        adamBetaAdjusted *= adamBeta;
        adamBetaSquareAdjusted *= adamBetaSquare;
        for (int i = 0; i <= 127; i++)
        {
            for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
            {
                tempError = error[j] * prevValues[i];

                moment[i, j] = adamBeta * moment[i, j] + (1 - adamBeta) * tempError;

                velocity[i, j] = adamBetaSquare * velocity[i, j] + (1 - adamBetaSquare) * Math.Pow(tempError, 2);

                


                momentCorrected[i, j] = moment[i, j] / (1 - adamBetaAdjusted); // pamiętaj o zwiększaniu adam coutner
                velocityCorrected[i, j] = velocity[i, j] / (1 - adamBetaSquareAdjusted);




                weightsChange[i, j] -= learningRate * momentCorrected[i, j] / (Math.Sqrt(velocityCorrected[i, j]) + epsilon);
            }
        }
        for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
        {
            tempError = error[j] * prevValues[128+a];
            
            moment[128+a, j] = adamBeta * moment[128+a, j] + (1 - adamBeta) * tempError;
            velocity[128+a, j] = adamBetaSquare * velocity[128+a, j] + (1 - adamBetaSquare) * Math.Pow(tempError, 2);
            
            momentCorrected[128+a, j] = moment[128+a, j] / (1 - adamBetaAdjusted); // pamiętaj o zwiększaniu adam coutner
            velocityCorrected[128+a, j] = velocity[128+a, j] / (1 - adamBetaSquareAdjusted);

            weightsChange[128+a, j] -= learningRate * momentCorrected[128+a, j] / (Math.Sqrt(velocityCorrected[128+a, j]) + epsilon);




            tempError = error[j] * prevValues[256+s];
            
            moment[256+s, j] = adamBeta * moment[256+s, j] + (1 - adamBeta) * tempError;
            velocity[256+s, j] = adamBetaSquare * velocity[256+s, j] + (1 - adamBetaSquare) * Math.Pow(tempError, 2);
            
            momentCorrected[256+s, j] = moment[256+s, j] / (1 - adamBetaAdjusted); // pamiętaj o zwiększaniu adam coutner
            velocityCorrected[256+s, j] = velocity[256+s, j] / (1 - adamBetaSquareAdjusted);

            weightsChange[256+s, j] -= learningRate * momentCorrected[256+s, j] / (Math.Sqrt(velocityCorrected[256+s, j]) + epsilon);
        }
    }
    // to samo co wuyżej tylko z baisami im nie trzeba lcizyć bo partial derivative =1 więc error sam a XDD
    public void backpropAdamBias()
    {
        for (int i = 0; i <= bias.Length - 1; i++)
        {
            biasMoment[i] = adamBeta * biasMoment[i] + (1 - adamBeta) * error[i];


            biasVelocity[i] = adamBetaSquare * biasVelocity[i] + (1 - adamBetaSquare) * Math.Pow(error[i], 2);


            biasMomentCorrected[i] = biasMoment[i] / (1 - adamBetaAdjusted);
            biasVelocityCorrected[i] = biasVelocity[i] / (1 - adamBetaSquareAdjusted);



            biasChange[i] -= learningRate * biasMomentCorrected[i] / (Math.Sqrt(biasVelocityCorrected[i]) + epsilon);
        }
    }
    // to tylko do ostatniego biasu
    public void lastPropAdamBias(int action)
    {
        biasMoment[action] = adamBeta * biasMoment[action] + (1 - adamBeta) * error[action];


        biasVelocity[action] = adamBetaSquare * biasVelocity[action] + (1 - adamBetaSquare) * Math.Pow(error[action], 2);




        biasMomentCorrected[action] = biasMoment[action] / (1 - adamBetaAdjusted);
        biasVelocityCorrected[action] = biasVelocity[action] / (1 - adamBetaSquareAdjusted);



        biasChange[action] -= learningRate * biasMomentCorrected[action] / (Math.Sqrt(biasVelocityCorrected[action]) + epsilon);
    }
    // i teraz ptyma soirt mianowieicie ostatnie rećzeni backprollpagowanie

    public void lastPropAdam(int action, double[] prevValues)
    {
        double tempError;
        adamBetaAdjusted *= adamBeta;
        adamBetaSquareAdjusted *= adamBetaSquare;  //krok aktualizujemy na początku nauki
        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            tempError = error[action] * prevValues[i];

            moment[i, action] = adamBeta * moment[i, action] + (1 - adamBeta) * tempError;

            velocity[i, action] = adamBetaSquare * velocity[i, action] + (1 - adamBetaSquare) * Math.Pow(tempError, 2);


            momentCorrected[i, action] = moment[i, action] / (1 - adamBetaAdjusted); // pamiętaaction o zwiększaniu adam coutner
            velocityCorrected[i, action] = velocity[i, action] / (1 - adamBetaSquareAdjusted);


            weightsChange[i, action] -= learningRate * momentCorrected[i, action] / (Math.Sqrt(velocityCorrected[i, action]) + epsilon);
        }
    }













    public void applyWeights(int diviser)
    {
        if (diviser != 0)
            for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
            {
                for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
                {
                    weightsToPast[i, j] += weightsChange[i, j] / diviser;
                    weightsChange[i, j] = 0;
                }
            }
    }
    public void applyBias(int diviser)
    {
        if (diviser != 0)
            for (int i = 0; i <= bias.Length - 1; i++)
            {
                bias[i] += biasChange[i] / diviser;
                biasChange[i] = 0;
            }
    }






    public void applyWeightsLast(int[] diviser)
    {

        for (int i = 0; i <= weightsToPast.GetLength(0) - 1; i++)
        {
            for (int j = 0; j <= weightsToPast.GetLength(1) - 1; j++)
            {
                if (diviser[j] != 0)
                {
                    weightsToPast[i, j] += weightsChange[i, j] / diviser[j];
                    weightsChange[i, j] = 0;
                }
            }
        }
    }
    public void applyBiasLast(int[] diviser)
    {
        for (int i = 0; i <= values.Length - 1; i++)
        {
            if (diviser[i] != 0)
            {
                bias[i] += biasChange[i] / diviser[i];
                biasChange[i] = 0;
            }
        }

    }


    //zwraca tablice dwu wymiarową jako wagi
    public double[,] getWeights()
    {
        return weightsToPast;
    }
    //zwraca tablicę biasu
    public double[] getBias()
    {
        return bias;
    }




    //clippowanie erroru
    public double clipError(double d)
    {
        if (d > 1)
        {
            return 1;
        }
        if (d < -1)
        {
            return -1;
        }
        return d;
    }
    //funkcja aktywacji
    public double leakyRelU(double d)
    {
        return d > 0 ? d : leak * d;
    }

    // jej różniczka
    public double diffleakyRelU(double d)
    {
        return d > 0 ? 1 : leak;
    }




}
