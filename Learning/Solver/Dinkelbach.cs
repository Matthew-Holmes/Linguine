using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Serilog;

namespace Learning.Solver
{
    internal static class Dinkelbach
    {
        // Note - our lives are a lot easier here than a general RL problem
        // since our policy is empirical probabilities

        internal static (double[], double[], bool[], int) GetCostRewardExpectationsAndIsTerminated(ExplodedMarkovData data)
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
            double currentGain        = double.NegativeInfinity;
            double oldReward;

            
            (Vector<double> Er, Vector<double> Ec) GetExpectations()
            {
                Vector<double> Er = GetExpectationsForAllBasisVectors(P, r);
                Vector<double> Ec = GetExpectationsForAllBasisVectors(P, c);

                return (Er, Ec);

            }

            // solve once initially
            (Vector<double> Er, Vector<double> Ec) = GetExpectations();

            currentGain = Er[si] > 0.0 ? (Er[si] / Ec[si]) : 0.0;
            oldReward = Er[si];

            bool improved = true;
            while (improved)
            {
                improved = false;

                // identify the lowest-value gain node among the non-terminated
                double minGainVal   = double.PositiveInfinity;
                double minRewardVal = double.PositiveInfinity;

                int minIndex = -1;
                for (int i = 0; i < n; i++)
                {
                    if (isTerminated[i]) { continue; }

                    if (Er[i] < 0)
                    {
                        if (Er[i] < minRewardVal)
                        {
                            minRewardVal = Er[i];
                            minIndex = i;
                        }
                        continue;
                    }

                    if (minRewardVal == double.PositiveInfinity)
                    {
                        // haven't found any negative rewards, so delete a low gain node
                        if ((Er[i] / Ec[i]) < minGainVal)
                        {
                            minGainVal = Er[i] / Ec[i];
                            minIndex = i;
                        }
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

                (Er, Ec) = GetExpectations();

                double newGain = Er[si] / Ec[si];

                // check if gain improved, or if reward is negative, that we improved the reward
                // since in negative reward regime, talking about gain doesn't really make sense

                // condition 2: otherwise, check if the gain improved

                if (oldReward < 0)
                {
                    improved = true; // there must be some negative reward nodes, so removing them always helps
                    oldReward = Er[si];
                }
                else if (newGain > currentGain)
                {
                    currentGain = newGain;
                    improved = true;
                }

                // update “best so far” if needed
                if (improved)
                {
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
            return (Er.ToArray(), Ec.ToArray(), bestTerminatedMask, si);
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

            while (delta > eps || steps < 100)
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

        private static Vector<double> GetExpectationsForAllBasisVectors(Matrix<double> transitions, Vector<double> values, double eps = 1e-7)
        {
            int n = values.Count;
            var distributionMatrix = Matrix<double>.Build.DenseIdentity(n);
            var expectations = Vector<double>.Build.Dense(n); // accumulates expectation per basis vector

            double delta = double.MaxValue;
            int steps = 0;

            while ((delta > eps || steps < 10) && steps < 60) /* we aren't going to be testing one word 30 = 60 / 2 times, so don't worry about long tail asympotics! */
            {
                // each column of distributionMatrix is a probability distribution
                var stepExpectations = distributionMatrix.TransposeThisAndMultiply(values); // vector of expectations
                expectations += stepExpectations;

                var nextDistributionMatrix = transitions * distributionMatrix;
                delta = stepExpectations.AbsoluteMaximum(); // Largest change in this step

                distributionMatrix = nextDistributionMatrix;
                steps++;
            }

            Log.Verbose("stopped after {steps}", steps);

            return expectations;
        }
    }
}
