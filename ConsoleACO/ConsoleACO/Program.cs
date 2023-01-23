using System;
using System.IO;
using System.Linq;

namespace ConsoleACO
{
    class Program
    {

        static void Main(string[] args)
        {
            CreateFile();
            int nCopies = 5;
            int maxIteratios = 500;


            Console.WriteLine(DateTime.Now.ToString() + ": Starting simulation");
            string filename = "../../../att48.txt";
            double[,] costMatrix = ReadProblem(filename);


            int[] nAnts = new int[1] { 20 };
            double[] alphas = Enumerable.Range(0, 20).Select(i => 1 + i * 0.5).ToArray();
            double[] betas = Enumerable.Range(0, 20).Select(i => 1 + i * 0.5).ToArray();
            double[] rhos = new double[1] { 0.8 };
            double[] Qs = new double[1] { 20 };
            int[] sigmas = new int[1] { 1 };


            ACO[] colonies = new ACO[nAnts.Length * alphas.Length * betas.Length * sigmas.Length * rhos.Length * Qs.Length * nCopies];

            for (int n = 0; n < nAnts.Length; n++)
            {
                int numAnts = nAnts[n];
                for (int a = 0; a < alphas.Length; a++)
                {
                    double alpha = alphas[a];
                    for (int b = 0; b < betas.Length; b++)
                    {
                        double beta = betas[b];
                        for (int s = 0; s < sigmas.Length; s++)
                        {
                            int sigma = sigmas[s];
                            for (int r = 0; r < rhos.Length; r++)
                            {
                                double rho = rhos[r];
                                for (int q = 0; q < Qs.Length; q++)
                                {
                                    double Q = Qs[q];
                                    for (int j = 0; j < nCopies; j++)
                                    {
                                        int index = n + a * nAnts.Length + 
                                            b * nAnts.Length * alphas.Length +
                                            s * nAnts.Length * alphas.Length * betas.Length +
                                            r * nAnts.Length * alphas.Length * betas.Length * sigmas.Length +
                                            q * nAnts.Length * alphas.Length * betas.Length * sigmas.Length * rhos.Length +
                                            j * nAnts.Length * alphas.Length * betas.Length * sigmas.Length * rhos.Length * Qs.Length;
                                        colonies[index] = new ACO(costMatrix, alpha, beta, rho, Q, sigma, numAnts); ;

                                    }
                                }
                            }
                        }
                    }
                }
            }

            int iterations = 0;
            while(iterations < maxIteratios){
                for (int i = 0; i < colonies.Length; i++)
                {
                    colonies[i].NextIter();
                }
                iterations++;
                Console.WriteLine(DateTime.Now.ToString() + String.Format(": {0:0.00}%", ((double)iterations / (double)maxIteratios) * 100));
                Store(colonies, iterations);
            }
            Console.WriteLine(DateTime.Now.ToString() +  ": All done - good job, Javi");
        }

        static double[,] ReadProblem(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found.");
                throw new ArgumentNullException();

            }
            string[] lines = File.ReadAllLines(fileName);
            double[][] coordinates = new double[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                coordinates[i] = new double[] { double.Parse(lines[i].Split(",")[0]), double.Parse(lines[i].Split(",")[1]) };
            }
            return BuildCostMatrix(coordinates, lines.Length);
        }


        static void CreateFile()
        {
            StreamWriter sw = new StreamWriter("../../../heatmap.csv", true);
            string headers =
                "iteration," +
                "alpha," +
                "beta," +
                "sigma," +
                "rho," +
                "Q," +
                "bestLength," +
                "bestCurrent," +
                "nBestPathAnts," +
                "nBestCurrentPathAnts";
            sw.WriteLine(headers);
            sw.Close();
        }
        static void Store(ACO[] colonies, int iterations)
        {
            StreamWriter sw = new StreamWriter("../../../heatmap.csv", true);
            for (int i = 0; i < colonies.Length; i++)
            {
                string line =
                    iterations.ToString() + "," +
                    colonies[i].alpha.ToString() + "," +
                    colonies[i].beta.ToString() + "," +
                    colonies[i].sigma.ToString() + "," +
                    colonies[i].rho.ToString() + "," +
                    colonies[i].Q.ToString() + "," +
                    colonies[i].bestLength.ToString() + "," +
                    colonies[i].currBestLength.ToString() + "," +
                    colonies[i].nBestPathAnts.ToString() + "," +
                    colonies[i].nBestCurrentPathAnts.ToString();
                sw.WriteLine(line);
            }
            sw.Close();
        }

        static double[,] BuildCostMatrix(double[][] coordinates, int dim)
        {
            double[,] costMatrix = new double[dim,dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (i == j)
                    {
                        costMatrix[i,j] = 0;
                    }
                    else
                    {
                        double xDiff = coordinates[i][0] - coordinates[j][0];
                        double yDiff = coordinates[i][1] - coordinates[j][1];
                        costMatrix[i,j] = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
                    }
                }
            }
            return costMatrix;
        }

    }
}
