using System;

[Serializable]
public class Replay
{
        public double[] state;
        public int action;
        public double reward;
        public double[] newState;
        public bool terminalState;
        public int snakeIndex;
        public int appleIndex;

        public Replay(double[] map, int act, double rew, double[] newSta, bool isEnd)
        {
            state=new double[map.Length];
            for (int i = 0; i <= map.Length - 1; i++)
            {
                state[i] = map[i];
            }
            if (newSta != null)
            {
                newState=new double[newSta.Length];
                for (int i = 0; i <= newSta.Length - 1; i++)
                {
                    newState[i] = newSta[i];
                }
            }
            action = act;
            reward = rew;
            terminalState = isEnd;
        }
        public Replay(double[] map, int act, double rew, double[] newSta, bool isEnd, int a, int s)
        {
            for (int i = 0; i <= map.Length - 1; i++)
            {
                state[i] = map[i];
            }
            if (newSta != null)
            {
                for (int i = 0; i <= newSta.Length - 1; i++)
                {
                    newState[i] = newSta[i];
                }
            }
            action = act;
            reward = rew;
            terminalState = isEnd;
            appleIndex=a;
            snakeIndex=s;
        }
}
