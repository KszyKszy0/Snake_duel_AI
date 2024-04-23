using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NNReworkLayers
{
    // Start is called before the first frame update
    public List<Layer> layers = new List<Layer>();
    public double learningRate;
    public double leak = 0;
    public double adamBeta = 0.9;
    public double adamBetaSquare = 0.999;
    public double epsilon = 0.00000001;
    public double dropout = 0.7;   //keep rate


    public NNReworkLayers(List<int> sizes,double lr)
    {
        learningRate=lr;
        for (int i = 0; i <= sizes.Count - 1; i++)
        {
            var newLayer = new Layer();
            newLayer.init(sizes[i]);
            newLayer.adamBeta = adamBeta;
            newLayer.adamBetaSquare = adamBetaSquare;
            newLayer.leak = leak;
            newLayer.learningRate = learningRate;
            newLayer.epsilon = epsilon;
            newLayer.dropout = dropout;
            layers.Add(newLayer);
            if (i > 0)
            {
                newLayer.weightsToPast = new double[sizes[i - 1], sizes[i]];
                newLayer.moment = new double[sizes[i - 1], sizes[i]];
                newLayer.velocity = new double[sizes[i - 1], sizes[i]];
                newLayer.momentCorrected = new double[sizes[i - 1], sizes[i]];
                newLayer.velocityCorrected = new double[sizes[i - 1], sizes[i]];
                newLayer.weightsChange = new double[sizes[i - 1], sizes[i]];
                newLayer.initHe(sizes[i - 1]);
                newLayer.intiBiasHe(sizes[i - 1]);
            }
        }
    }

    // public void init(List<int> sizes)
    // {
    //     for (int i = 0; i <= sizes.Count - 1; i++)
    //     {
    //         var newLayer = new Layer();
    //         newLayer.init(sizes[i]);
    //         newLayer.adamBeta = adamBeta;
    //         newLayer.adamBetaSquare = adamBetaSquare;
    //         newLayer.leak = leak;
    //         newLayer.learningRate = learningRate;
    //         newLayer.epsilon = epsilon;
    //         newLayer.dropout = dropout;
    //         layers.Add(newLayer);
    //         if (i > 0)
    //         {
    //             newLayer.weightsToPast = new double[sizes[i - 1], sizes[i]];
    //             newLayer.moment = new double[sizes[i - 1], sizes[i]];
    //             newLayer.velocity = new double[sizes[i - 1], sizes[i]];
    //             newLayer.momentCorrected = new double[sizes[i - 1], sizes[i]];
    //             newLayer.velocityCorrected = new double[sizes[i - 1], sizes[i]];
    //             newLayer.weightsChange = new double[sizes[i - 1], sizes[i]];

    //             newLayer.initHe(sizes[i - 1]);
    //             newLayer.intiBiasHe(sizes[i - 1]);
    //         }
    //     }
    // }
    
    
    // public double[] predict(double[] state)
    // {
    //     for (int i = 0; i <= state.Length - 1; i++)
    //     {
    //         layers[0].values[i] = state[i];
    //     }
    //     for (int i = 1; i <= layers.Count - 2; i++)
    //     {
    //         layers[i].values = layers[i].forwardPass(layers[i - 1].values, true);
    //     }
    //     layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
    //     return layers[layers.Count - 1].values;
    // }

    public double[] predict(double[] state)
    {
        for (int i = 0; i <= layers[1].values.Length - 1; i++)
        {
            double sum = 0;
            for (int j = 0; j <= state.Length - 1; j++)
            {
                sum += state[j] * layers[1].weightsToPast[j, i];
            }
            sum += layers[1].bias[i];
            layers[1].values[i] = leakyRelU(sum);
        }
        for (int i = 2; i <= layers.Count - 2; i++)
        {
            layers[i].values = layers[i].forwardPass(layers[i - 1].values, true);
        }
        layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
        return layers[layers.Count - 1].values;
    }
    

    public double[] predict(double[] state, int appIndex, int headIndex)
    {
        for (int i = 0; i <= 127; i++)
        {
            layers[0].values[i] = state[i];
        }
        layers[0].values[128 + appIndex] = 1;
        layers[0].values[256 + headIndex] = 1;
        layers[1].forwardPass(layers[0].values, true, appIndex, headIndex);
        for (int i = 2; i <= layers.Count - 2; i++)
        {
            layers[i].values = layers[i].forwardPass(layers[i - 1].values, true);
        }
        layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
        return layers[layers.Count - 1].values;
    }

    // public double[] testPredict(double[] state)
    // {
    //     for (int i = 0; i <= state.Length - 1; i++)
    //     {
    //         layers[0].values[i] = state[i];
    //     }
    //     for (int i = 1; i <= layers.Count - 2; i++)
    //     {
    //         layers[i].values = layers[i].forwardTestPass(layers[i - 1].values, true);
    //     }
    //     layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
    //     return layers[layers.Count - 1].values;
    // }

    public double[] testPredict(double[] state)
    {
        for (int i = 0; i <= layers[1].values.Length - 1; i++)
        {
            double sum = 0;
            for (int j = 0; j <= state.Length - 1; j++)
            {
                sum += state[j] * layers[1].weightsToPast[j, i];
            }
            sum += layers[1].bias[i];
            layers[1].values[i] = leakyRelU(sum);
        }
        for (int i = 2; i <= layers.Count - 2; i++)
        {
            layers[i].values = layers[i].forwardTestPass(layers[i - 1].values, true);
        }
        layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
        return layers[layers.Count - 1].values;
    }

    public double[] testPredict(double[] state, int appIndex, int headIndex)
    {
        for (int i = 0; i <= 127; i++)
        {
            layers[0].values[i] = state[i];
        }
        layers[0].values[128 + appIndex] = 1;
        layers[0].values[256 + headIndex] = 1;
        layers[1].forwardTestPass(layers[0].values, true, appIndex, headIndex);
        for (int i = 2; i <= layers.Count - 2; i++)
        {
            layers[i].values = layers[i].forwardTestPass(layers[i - 1].values, true);
        }
        layers[layers.Count - 1].forwardLastPass(layers[layers.Count - 2].values, true);
        return layers[layers.Count - 1].values;
    }


    // ostatnią warstwę liczymy ręcznie, potem robimy wg wzoru weż warstwę -> policz błąd -> propagacja
    public void calcError()
    {

    }
    public void backPropagationSGD(double error, int action)
    {
        //ogółem ta linijka liczy błąd neuronu, który miał akcję, różniczka * mse i klipowanie
        layers[layers.Count - 1].error[action] = clipError(error);
        layers[layers.Count - 1].error[action] = clipError(layers[layers.Count - 1].error[action]);
        //teraz ta linijka zrobi propagację jedną warstwę wstecz a nawet dwie
        layers[layers.Count - 1].lastPropSgd(action, layers[layers.Count - 2].values);
        layers[layers.Count - 1].lastPropSgdBias(action);
        //teraz linijka ma policzyć błąd w sposób specjalny czyli do jednego
        layers[layers.Count - 2].semiLastError(action, layers[layers.Count - 1].weightsToPast, layers[layers.Count - 1].error[action]);
        //zrobic recznie propagacje bo żeby pętla wyszła równo
        layers[layers.Count - 2].backpropSGD(layers[layers.Count - 3].values);
        layers[layers.Count - 2].backpropSGDBias();
        //no i teraz młotek programu - pętla

        for (int i = layers.Count - 3; i >= 1; i--)
        {
            layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);
            layers[i].backpropSGD(layers[i - 1].values);
            layers[i].backpropSGDBias();
        }
    }

    public void backPropagationSGD(double error, int action, double[] state)
    {
        //ogółem ta linijka liczy błąd neuronu, który miał akcję, różniczka * mse i klipowanie
        layers[layers.Count - 1].error[action] = clipError(error);
        layers[layers.Count - 1].error[action] = clipError(layers[layers.Count - 1].error[action]);
        //teraz ta linijka zrobi propagację jedną warstwę wstecz a nawet dwie
        layers[layers.Count - 1].lastPropSgd(action, layers[layers.Count - 2].values);
        layers[layers.Count - 1].lastPropSgdBias(action);
        //teraz linijka ma policzyć błąd w sposób specjalny czyli do jednego
        layers[layers.Count - 2].semiLastError(action, layers[layers.Count - 1].weightsToPast, layers[layers.Count - 1].error[action]);
        //zrobic recznie propagacje bo żeby pętla wyszła równo
        layers[layers.Count - 2].backpropSGD(layers[layers.Count - 3].values);
        layers[layers.Count - 2].backpropSGDBias();
        //no i teraz młotek programu - pętla

        for (int i = layers.Count - 3; i >= 1; i--)
        {
            layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);
            if(i==1)
            {
                layers[i].backpropSGD(state);
            }else
            {
                layers[i].backpropSGD(layers[i - 1].values);
            }
            layers[i].backpropSGDBias();
        }
    }
    public void backPropagationAdam(double error, int action)
    {
        //ogółem ta linijka liczy błąd neuronu, który miał akcję, różniczka * mse i klipowanie
        layers[layers.Count - 1].error[action] = clipError(error);
        layers[layers.Count - 1].error[action] = clipError(layers[layers.Count - 1].error[action]);
        //teraz ta linijka zrobi propagację jedną warstwę wstecz a nawet dwie ADameem
        layers[layers.Count - 1].lastPropAdam(action, layers[layers.Count - 2].values);
        layers[layers.Count - 1].lastPropAdamBias(action);
        //teraz linijka ma policzyć błąd w sposób specjalny czyli do jednego
        layers[layers.Count - 2].semiLastError(action, layers[layers.Count - 1].weightsToPast, layers[layers.Count - 1].error[action]);
        //zrobic recznie propagacje bo żeby pętla wyszła równo
        layers[layers.Count - 2].backpropAdam(layers[layers.Count - 3].values);
        layers[layers.Count - 2].backpropAdamBias();
        //no i teraz młotek programu - pętla

        for (int i = layers.Count - 3; i >= 1; i--)
        {
            layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);//klejnosc jest ultra wazżna bo inaczej naliczza się adambetaadjsueted wyrownany moment i predkosc
            layers[i].backpropAdam(layers[i - 1].values);
            layers[i].backpropAdamBias();
        }
    }

    public void backPropagationAdam(double error, int action, double[] state)
    {
        //ogółem ta linijka liczy błąd neuronu, który miał akcję, różniczka * mse i klipowanie
        layers[layers.Count - 1].error[action] = clipError(error);
        layers[layers.Count - 1].error[action] = clipError(layers[layers.Count - 1].error[action]);
        //teraz ta linijka zrobi propagację jedną warstwę wstecz a nawet dwie ADameem
        layers[layers.Count - 1].lastPropAdam(action, layers[layers.Count - 2].values);
        layers[layers.Count - 1].lastPropAdamBias(action);
        //teraz linijka ma policzyć błąd w sposób specjalny czyli do jednego
        layers[layers.Count - 2].semiLastError(action, layers[layers.Count - 1].weightsToPast, layers[layers.Count - 1].error[action]);
        //zrobic recznie propagacje bo żeby pętla wyszła równo
        layers[layers.Count - 2].backpropAdam(layers[layers.Count - 3].values);
        layers[layers.Count - 2].backpropAdamBias();
        //no i teraz młotek programu - pętla

        for (int i = layers.Count - 3; i >= 1; i--)
        {
            layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);//klejnosc jest ultra wazżna bo inaczej naliczza się adambetaadjsueted wyrownany moment i predkosc
            if(i==1)
            {
                layers[i].backpropAdam(state);
            }else
            {
                layers[i].backpropAdam(layers[i - 1].values);
            }
            layers[i].backpropAdamBias();
        }
    }

    public void backPropagationAdam(double error, int action, int a, int s)
    {
        //ogółem ta linijka liczy błąd neuronu, który miał akcję, różniczka * mse i klipowanie
        layers[layers.Count - 1].error[action] = clipError(error);
        layers[layers.Count - 1].error[action] = clipError(layers[layers.Count - 1].error[action]);
        //teraz ta linijka zrobi propagację jedną warstwę wstecz a nawet dwie ADameem
        layers[layers.Count - 1].lastPropAdam(action, layers[layers.Count - 2].values);
        layers[layers.Count - 1].lastPropAdamBias(action);
        //teraz linijka ma policzyć błąd w sposób specjalny czyli do jednego
        layers[layers.Count - 2].semiLastError(action, layers[layers.Count - 1].weightsToPast, layers[layers.Count - 1].error[action]);
        //zrobic recznie propagacje bo żeby pętla wyszła równo
        layers[layers.Count - 2].backpropAdam(layers[layers.Count - 3].values);
        layers[layers.Count - 2].backpropAdamBias();
        //no i teraz młotek programu - pętla

        for (int i = layers.Count - 3; i >= 1; i--)
        {
            if (i != 0)
            {
                layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);//klejnosc jest ultra wazżna bo inaczej naliczza się adambetaadjsueted wyrownany moment i predkosc
                layers[i].backpropAdam(layers[i - 1].values);
                layers[i].backpropAdamBias();
            }
            else
            {
                layers[i].calcError(layers[i + 1].error, layers[i + 1].weightsToPast);//klejnosc jest ultra wazżna bo inaczej naliczza się adambetaadjsueted wyrownany moment i predkosc
                layers[i].backpropAdam(layers[i - 1].values,a,s);
                layers[i].backpropAdamBias();
                layers[0].values[128+a]=0;
                layers[0].values[256+s]=0;
            }
        }
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
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
    public double leakyRelU(double d)
    {
        return d > 0 ? d : leak * d;
    }
    public double diffleakyRelU(double d)
    {
        return d > 0 ? 1 : leak;
    }
}
