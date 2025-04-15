using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Learning.BellmanSolver
{
    internal static class BellmanDinkelbach
    {
        // Note - our lives are a lot easier here than a general RL problem
        // since our policy is empirical probabilities

        internal static (double[], bool[]) GetNodeValuesAndIsTerminal(ExplodedMarkovData data)
        {
            // solve using gain estimate and no discount factor

            // then any nodes that only can get worse --> terminate

            // then repeat, until no more terminal nodes are identified

            int n  = data.rewards.Length;
            int si = data.indices["null"]; // index of the starting node

            Vector<double> r = Vector<double>.Build.DenseOfArray(data.rewards);
            Vector<double> c = Vector<double>.Build.DenseOfArray(data.costs);          
            Matrix<double> P = Matrix<double>.Build.DenseOfArray(data.transitionProbs).Transpose(); // indexed [from, to], but need "from" to be the column indexer
            
            Vector<double> ei = Vector<double>.Build.Dense(n, j => j == si ? 1.0 : 0.0); // canonical basis vector for starting position

            bool[] isTerminated = new bool[n];

            Vector<double> bestV      = null;
            bool[] bestTerminatedMask = new bool[n];
            double bestGain           = double.NegativeInfinity;

            HashSet<int> GetReachableSet(int start, Matrix<double> mat)
            {
                var visited = new HashSet<int>();
                var queue = new Queue<int>();
                queue.Enqueue(start);
                visited.Add(start);

                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    // For each potential "row" (0..n-1):
                    for (int row = 0; row < n; row++)
                    {
                        // If there's a positive probability from cur -> row
                        if (mat[row, cur] > 0.0 && !visited.Contains(row))
                        {
                            visited.Add(row);
                            queue.Enqueue(row);
                        }
                    }
                }

                return visited;
            }

            (double gainVal, Vector<double> vSoln, double Er) SolveCurrentMarkov()
            {
                double Er = GetExpectationAccordingToPolicy(P, r, ei);
                double Ec = GetExpectationAccordingToPolicy(P, c, ei);

                double newGain = Math.Abs(Er / Ec);

                // Solve Bellman for rhat = r - gain * c
                var rhat = r - newGain * c;
                Vector<double> v = BellmanSolve(P, rhat);

                // because no entries to start node, the solver can't give it a value - lets do it manually
                v[si] = P.Column(si).DotProduct(v) - newGain * P.Column(si).DotProduct(c);

                return (newGain, v, Er);
            }

            // solve once initially
            var (currentGain, currentV, currentEr) = SolveCurrentMarkov();
            bestGain = currentEr > 0.0 ? currentGain : 0.0;

            bestV = currentV.Clone();

            bool improved = true;
            while (improved)
            {
                improved = false;

                // identify the lowest-value node among the non-terminated
                double minVal = double.PositiveInfinity;
                int minIndex = -1;
                for (int i = 0; i < n; i++)
                {
                    if (!isTerminated[i] && currentV[i] < minVal)
                    {
                        minVal = currentV[i];
                        minIndex = i;
                    }
                }

                if (minIndex == si) { break; }

                // if everything is terminated or we failed to find a candidate, just break
                if (minIndex < 0)
                    break;

                var oldCol = P.Column(minIndex).ToArray(); // store for revert
                // delete all transistions from this node, cols are the from indexers
                for (int row = 0; row < n; row++)
                {
                    P[row, minIndex] = 0.0;
                }

                isTerminated[minIndex] = true;

                // resolve Markov
                var (newGain, newV, newEr) = SolveCurrentMarkov();

                // check if gain improved, or if reward is negative, that we improved the reward
                // since in negative reward regime, talking about gain doesn't really make sense

                // condition 1: If we are in negative reward territory, see if Er improved
                if (currentEr < 0)
                {
                    currentEr = newEr;
                    currentV = newV;

                    // if we reached positive regime, then use real gain, otherwise use dummy value of 0.0
                    currentGain = (newEr > 0) ? newGain : 0.0;
                    improved = true;
                }
                // condition 2: otherwise, check if the gain improved
                if (currentEr > 0 && newGain > currentGain)
                {
                    currentGain = newGain;
                    currentEr = newEr;
                    currentV = newV;
                    improved = true;
                }

                // update “best so far” if needed
                if (improved)
                {
                    bestGain = currentGain;
                    bestV = currentV.Clone();
                    for (int i = 0; i < n; i++)
                        bestTerminatedMask[i] = isTerminated[i];
                }
                else
                {
                    // revert the change to P’s column
                    for (int row = 0; row < n; row++)
                    {
                        P[row, minIndex] = oldCol[row];
                    }
                    isTerminated[minIndex] = false;


                    // not improved => break out and revert to “best so far”
                    break;
                }
            }

            // return best solution
            return (bestV.ToArray(), bestTerminatedMask);
        }

        private static Vector<double> BellmanSolve(Matrix<double> P, Vector<double> r)
        {
            var I = DenseMatrix.CreateIdentity(P.RowCount);
            var A = I - P;
            var v = A.Solve(r);

            return v;
        }

        private static double GetExpectationAccordingToPolicy(Matrix<double> transitions, Vector<double> values, Vector<double> start, double eps = 1e-7)
        {
            var distribution = start.Clone(); // p0

            double ret   = 0.0;
            double delta = double.MaxValue;

            int steps = 0;

            while (delta > eps || steps < 10)
            {
                double stepExpectation = distribution.DotProduct(values);
                ret += stepExpectation;

                var nextDistribution = transitions * distribution;
                delta = Math.Abs(stepExpectation);
                distribution = nextDistribution;
                steps++;
            }

            return ret;
        }
    }
}
