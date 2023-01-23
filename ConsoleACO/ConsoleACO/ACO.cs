using System;
using System.Linq;

namespace ConsoleACO
{
    public class ACO
    {


        public double alpha;
        public double beta;
        public double rho;
        public double Q;
        public double sigma;
        public int numCities;
        public int numAnts;

        public double[,] dists;
        public int[][] ants;
        public double bestLength;
        public double currBestLength;
        public double[][] pheromones;

        public int iterations;

        public int nBestPathAnts;
        public int nBestCurrentPathAnts;

        public string resultsFileName;
        public string resultsFolder;
        public int maxIterations;

        public ACO(double[,] dists, double alpha, double beta, double rho, double Q, int sigma, int numAnts)
        {
            this.dists = dists;
            this.alpha = alpha;
            this.beta = beta;
            this.rho = rho;
            this.Q = Q;
            this.sigma = sigma;
            this.numAnts = numAnts;

            bestLength = double.MaxValue;
            numCities = dists.GetLength(0);
            ants = InitAnts();
            pheromones = InitPheromones();
            iterations = 0;
        }
        public void NextIter()
        {
            UpdateAnts();
            UpdatePheromones();
            currBestLength = CurrBestLength();
            countBestPathAnts();
            // double pheremonesDiversity = PheromoneDiversity(pheromones);
            // double hammingDistance2 = AverageHammingDistance2(ants);
            // double diversityOfLength = DiversityOfLength(ants);
            iterations++;
        }




        // Diversity metrics

        double CurrBestLength()
        {
            double tmp = 1000000000000;
            for(int i = 0; i<numAnts; i++)
            {
                if(tmp > Length(ants[i]))
                {
                    tmp = Length(ants[i]);
                    nBestCurrentPathAnts = 1;
                }
                else if(tmp == Length(ants[i]))
                {
                    nBestCurrentPathAnts++;
                }
            }
            return tmp;
        }

        void countBestPathAnts()
        {
            int nBestPathAnts = 0;
            for (int i = 0; i < numAnts; i++)
            {
                if (bestLength == Length(ants[i]))
                {
                    nBestPathAnts++;
                }
            }
        }




        double PheromoneDiversity(double[][] pheromones)
        {
            int n = pheromones.Length;
            int m = pheromones[0].Length;

            double mean = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    mean += pheromones[i][j];
                }
            }
            mean = mean / (n * m);

            double variance = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    variance += Math.Pow(pheromones[i][j] - mean, 2);
                }
            }
            variance = variance / (n * m);

            return variance;
        }

        double AverageHammingDistance(int[][] solutions)
        {
            int n = solutions.Length;
            int m = solutions[0].Length;

            double totalDistance = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    int distance = 0;
                    for (int k = 0; k < m; k++)
                    {
                        if (solutions[i][k] != solutions[j][k])
                        {
                            distance++;
                        }
                    }
                    totalDistance += distance;
                }
            }

            return (totalDistance / (n * (n - 1) / 2));
        }

        double AverageHammingDistance2(int[][] solutions)
        {
            int n = solutions.Length;
            int m = solutions[0].Length;

            double totalDistance = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    int distance = 0;
                    for (int k = 0; k < m; k++)
                    {
                        // find index of solutions[i][k] in solutions[j]
                        int k_in_j = System.Array.IndexOf(solutions[j], solutions[i][k]);
                        int k1 = (k + 1) % m;
                        int k_1 = (k - 1 + m) % m;
                        int k_in_j1 = (k_in_j + 1) % m;
                        if (solutions[i][k1] != solutions[j][k_in_j1] && solutions[i][k_1] != solutions[j][k_in_j])
                        {
                            distance++;
                        }
                    }
                    totalDistance += distance;
                }
            }

            return (totalDistance / (n * (n - 1) / 2));
        }


        double DiversityOfLength(int[][] solutions)
        {
            int n = solutions.Length;
            double[] functionValues = new double[n];

            for (int i = 0; i < n; i++)
            {
                functionValues[i] = Length(solutions[i]);
            }

            double mean = functionValues.Average();
            double variance = functionValues.Sum(x => Math.Pow(x - mean, 2)) / n;
            return Math.Sqrt(variance);
        }

        private int[][] InitAnts()
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++)
            {
                Random rnd = new Random();
                int start = rnd.Next(0, numCities);
                ants[k] = RandomTrail(start);
            }
            return ants;
        }

        private double[][] InitPheromones()
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // otherwise first call to UpdateAnts -> BuiuldTrail -> fNode -> MoveProbs => all 0.0 => throws
                }
            }
            return pheromones;
        }

        // Update functions

        private int[] RandomTrail(int start)
        {
            Random rnd = new Random();
            // helper for InitAnts
            int[] trail = new int[numCities];

            // sequential
            for (int i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (int i = 0; i <= numCities - 1; i++)
            {
                int r = rnd.Next(i, numCities);

                int tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start);
            // put start at [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private int IndexOfTarget(int[] trail, int target)
        {
            // helper for RandomTrail
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }

        private double Length(int[] trail)
        {
            // total length of a trail
            double result = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1]);
            }
            return result;
        }

        private void UpdateAnts()
        {
            Random rnd = new Random();
            int numCities = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                int start = rnd.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start);
                ants[k] = newTrail;
            }
        }

        private int[] BuildTrail(int k, int start)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities + 1];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i < numCities - 1; i++)
            {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited);
                trail[i + 1] = next;
                visited[next] = true;
            }
            trail[numCities] = trail[0];
            return trail;
        }

        private int NextCity(int k, int cityX, bool[] visited)
        {
            Random rnd = new Random();
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            double[] probs = MoveProbs(k, cityX, visited);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cuml.Length-1] to 1.00
            }
            double p = rnd.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            return NextCity(k, cityX, visited);
        }

        private double[] MoveProbs(int k, int cityX, bool[] visited)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            // inclues cityX and visited cities
            double sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (int i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else
                {
                    taueta[i] = Math.Pow((1.0 / Distance(cityX, i)), alpha) * Math.Pow(pheromones[cityX][i], beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        private void UpdatePheromones()
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double length = Length(ants[k]);

                        double decrease = rho * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = Q * (1 / length);
                            if (length < bestLength)
                            {
                                bestLength = length;
                                increase = increase * sigma;
                            }

                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }
            else if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            else if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == cityY)
            {
                return true;
            }
            else if (trail[idx + 1] == cityY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double Distance(int cityX, int cityY)
        {
            return dists[cityX, cityY];
        }

    }

}
