using System;
using System.Linq;
using System.Text;

namespace SimpleReinforcementLearningExample
{
    public enum Actions
    {
        Left, Right
    }

    internal static class Program
    {
        private const int StatesNumber = 6; // The length of the one-dimensional world
        private const double Epsilon = 0.9; // Greedy policy
        private const double Alpha = 0.1; // Learning rate
        private const double Gamma = 0.9; // Discount factor
        private const int EpochsLimit = 13;
        private const int RefreshRateMs = 300;

        private static readonly Random Random = new Random();

        private static double[][] BuildQTable(int statesNumber)
        {
            var table = new double[statesNumber - 1][];
            
            var actionsCount = Enum.GetNames(typeof(Actions)).Length;
            for (var i = 0; i < table.Length; i++)
            {
                table[i] = new double[actionsCount];
            }

#if DEBUG
            PrintQTable(table);
#endif

            return table;
        }

        private static void PrintQTable(double[][] qTable)
        {
            Console.WriteLine();
            Console.WriteLine("Q-Table content");

            var actionNames = Enum.GetValues(typeof(Actions));
            foreach (var actionName in actionNames)
            {
                Console.Write($"\t{actionName}");
            }

            Console.WriteLine();
            for (var i = 0; i < qTable.Length; i++)
            {
                Console.Write($"{i}");
                foreach (var actionName in actionNames)
                {
                    Console.Write($"\t{qTable[i][(int)actionName]:0.####}");
                }
                Console.WriteLine();
            }
        }

        private static StringBuilder CreatedRepeatedStringBuilder(string value, int count)
        {
            return new StringBuilder(value.Length * count).Insert(0, value, count);
        }

        private static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop - 1;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (var i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }

            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static void UpdateEnvironment(int? s, int epoch, int stepCounter)
        {
#if RELEASE
            if (stepCounter != 0)
            {
                ClearCurrentConsoleLine();
            }
#endif
            var environment =  CreatedRepeatedStringBuilder("_", StatesNumber - 1).Append('T');
            if (s.HasValue)
            {
                environment[s.Value] = 'o';
                Console.WriteLine(environment.ToString());
            }
            else
            {
                Console.WriteLine($"Epoch: {epoch}, total steps: {stepCounter}");
            }
            
            CustomWait.Wait(RefreshRateMs);
        }

        private static Actions ChooseAction(int s, double[][] qTable)
        {
            var stateRow = qTable[s];
            if (Random.NextDouble() > Epsilon || stateRow.All(d => Math.Abs(d) < 0.001))
            {
                var actionNames = Enum.GetValues(typeof(Actions));
                return (Actions)Random.Next(actionNames.Length);
            }

            var max = stateRow.Max();
            return (Actions)stateRow.ToList().IndexOf(max);
        }

        private static void GetFeedback(Actions action, int s, out int reward, out int? s_)
        {
            reward = 0;
            if (action == Actions.Right)
            {
                s_ = s + 1;
            }
            else
            {
                s_ = s - 1;
            }

            if (s_ < 0)
            {
                s_ = 0;
            } 
            else if (s_ == StatesNumber - 1)
            {
                s_ = null;
                reward = 1;
            }
        }

        private static double[][] RlLoop()
        {
            var qTable = BuildQTable(StatesNumber);
            
            for (var i = 0; i < EpochsLimit; i++)
            {
                var stepCounter = 0;
                int? s = 0;
                UpdateEnvironment(s, i, stepCounter);

                var isTerminated = false;
                while (!isTerminated)
                {
                    var action = ChooseAction(s.Value, qTable);
#if DEBUG
                    Console.WriteLine($"Action: {action}");
#endif
                    GetFeedback(action, s.Value, out var reward, out var s_);

                    var predictedReward = qTable[s.Value][(int) action];
                    double targetReward;
                    if (s_ != null)
                    {
                        targetReward = predictedReward + Gamma * qTable[s_.Value].Max();
                    }
                    else
                    {
                        targetReward = reward;
                        isTerminated = true;
                    }

                    qTable[s.Value][(int) action] += Alpha * (targetReward - predictedReward);
                    s = s_;
                    
                    UpdateEnvironment(s, i, ++stepCounter);
                }
            }

            return qTable;
        }

        public static void Main()
        {
            var qTable = RlLoop();
            PrintQTable(qTable);
        }
    }
}