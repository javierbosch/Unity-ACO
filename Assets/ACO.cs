using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;
using System.IO;
using System.Linq;


public class ACO : MonoBehaviour
{

    // influence of pheromone on direction
    public int alpha = 2;
    // influence of adjacent node distance
    public int beta = 2;

    // pheromone decrease factor
    public double rho = 0.2;
    // pheromone increase factor
    public double Q = 20;
    // pheromne increase factor for optimal path
    public double sigma = 50;


    public Text uiText;

    public GameObject trailObject;

    public GameObject stops;

    private int numCities;
    public int numAnts = 50;

    private double[,] dists;
    private int[][] ants;
    private int[] bestTrail;
    private double bestLength;
    private double currBestLength;
    private double[][] pheromones;

    public int iterations = 0;

    private Vector3[] positions;
    private GameObject pheromonesObjectParent;

    public Material lineMaterial;

    private int nBestPathAnts;
    private int nBestCurrentPathAnts;

    public bool showBestCurrentPath = false;
    public bool showPheromones = false;
    public bool display = false;

    private string resultsFileName;
    public string resultsFolder = "results";


    void Start()
    {
        numCities = stops.gameObject.transform.childCount;
        positions = StopsPositions();
        dists = GenerateCostMatrix();
        ants = InitAnts();
        bestTrail = BestTrail();
        bestLength = Length(bestTrail);
        pheromones = InitPheromones();
        pheromonesObjectParent = CreatePheromonesLineRenderer();
        resultsFileName = this.name + ".csv";
        CreateFile();
    }

    void Update()
    {
        UpdateAnts();
        UpdatePheromones();

        int[] currBestTrail = BestTrail();
        currBestLength = Length(currBestTrail);

        if (currBestLength < bestLength)
        {
            bestLength = currBestLength;
            bestTrail = currBestTrail;
        }
        UpdateDiversity(currBestLength);


        if (display)
        {
            UpdateUI();

            if (showBestCurrentPath)
                DisplayTrail(currBestTrail);
            else
                DisplayTrail(bestTrail);
            trailObject.SetActive(true);
            if (showPheromones)
            {
                pheromonesObjectParent.SetActive(true);
                DisplayPheromones();
            }
            else
            {
                pheromonesObjectParent.SetActive(false);
            }
        }
        else
        {
            pheromonesObjectParent.SetActive(false);
            uiText.text = "";
            trailObject.SetActive(false);
        }


        double pheremonesDiversity = PheromoneDiversity(pheromones);
        double hammingDistance2 = AverageHammingDistance2(ants);
        double diversityOfLength = DiversityOfLength(ants);

        WriteLine(pheremonesDiversity, hammingDistance2, diversityOfLength);

        iterations++;
    }

    // Diversity metrics

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
                variance += Pow(pheromones[i][j] - mean, 2);
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
        double variance = functionValues.Sum(x => Pow(x - mean, 2)) / n;
        return Sqrt(variance);
    }


    // Display functions

    void UpdateUI()
    {
        uiText.text = "Iterations: " + iterations.ToString() +
                      "\nBest length: " + bestLength.ToString("0.00") +
                      "\nBest global path: " + nBestPathAnts.ToString() +
                      "\nBest current path: " + nBestCurrentPathAnts.ToString();
    }

    void DisplayPheromones()
    {
        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                LineRenderer line = pheromonesObjectParent.gameObject.transform.GetChild(i * numCities + j).gameObject.GetComponent<LineRenderer>();
                line.SetPosition(0, positions[i]);
                line.SetPosition(1, positions[j]);
                line.startWidth = (float)pheromones[i][j] / 10;
                line.endWidth = (float)pheromones[i][j] / 10;
            }
        }
    }

    void UpdateDiversity(double currBestLength)
    {
        nBestPathAnts = 0;
        nBestCurrentPathAnts = 0;
        for (int i = 0; i < numAnts; i++)
        {
            if (Length(ants[i]) == bestLength)
            {
                nBestPathAnts++;
            }
            else if (Length(ants[i]) == currBestLength)
            {
                nBestCurrentPathAnts++;
            }
        }
    }

    void DisplayTrail(int[] trail)
    {
        trailObject.gameObject.GetComponent<LineRenderer>().positionCount = numCities + 1;
        Vector3[] linePositions = new Vector3[numCities + 1];
        for (int i = 0; i < numCities + 1; i++)
        {
            linePositions[i] = positions[trail[i]];
        }
        trailObject.gameObject.GetComponent<LineRenderer>().SetPositions(linePositions);
    }


    // Init functions

    Vector3[] StopsPositions()
    {
        Vector3[] stopPositions = new Vector3[numCities];
        for (int i = 0; i < numCities; i++)
        {
            stopPositions[i] = stops.gameObject.transform.GetChild(i).position;
        }
        return stopPositions;
    }

    GameObject CreatePheromonesLineRenderer()
    {
        GameObject parent = new GameObject();
        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                GameObject child = new GameObject();
                child.gameObject.AddComponent<LineRenderer>();
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                line.material = lineMaterial;
                line.positionCount = 2;
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
                line.startColor = new Color(1f, 0.73f, 0.31f);
                line.endColor = new Color(1f, 0.73f, 0.31f);
                child.transform.parent = parent.transform;
            }
        }
        return parent;
    }

    void CreateFile()
    {
        StreamWriter sw = new StreamWriter(resultsFolder + "/" + resultsFileName, true);
        string headers =
            "iterations," +
            "currBestLength," +
            "bestLength," +
            "nBestPathAnts," +
            "nBestCurrentPathAnts," +
            "apha," +
            "beta," +
            "rho," +
            "Q," +
            "sigma," +
            "nAnts," +
            "nCities," +
            "pheremonesDiversity," +
            "hammingDistance2," +
            "diversityOfLength";
        sw.WriteLine(headers);
        sw.Close();
    }

    private int[][] InitAnts()
    {
        int[][] ants = new int[numAnts][];
        for (int k = 0; k <= numAnts - 1; k++)
        {
            int start = Random.Range(0, numCities);
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

    double[,] GenerateCostMatrix()
    {
        double[,] m = new double[numCities, numCities];
        Vector3[] points = new Vector3[numCities];

        for (int i = 0; i < numCities; i++)
        {
            points[i] = stops.gameObject.transform.GetChild(i).position;
        }

        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                m[i, j] = Vector3.Distance(points[i], points[j]);
                //print(points[i] + "   " + points[j] + "   " + m[i, j]);
            }
        }
        return m;
    }


    // Update functions

    void WriteLine(double pheremonesDiversity, double hammingDistance, double diversityOfLength)
    {
        StreamWriter sw = new StreamWriter(resultsFolder + "/" + resultsFileName, true);
        string line =
            iterations.ToString() + "," +
            currBestLength.ToString() + "," +
            bestLength.ToString() + "," +
            nBestPathAnts.ToString() + "," +
            nBestCurrentPathAnts.ToString() + "," +
            alpha.ToString() + "," +
            beta.ToString() + "," +
            rho.ToString() + "," +
            Q.ToString() + "," +
            sigma.ToString() + "," +
            numAnts.ToString() + "," +
            numCities.ToString() + "," +
            pheremonesDiversity.ToString() + "," +
            hammingDistance.ToString() + "," +
            diversityOfLength.ToString();
        sw.WriteLine(line);
        sw.Close();
    }

    private int[] RandomTrail(int start)
    {
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
            int r = Random.Range(i, numCities);
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

    private int[] BestTrail()
    {
        // best trail has shortest total length
        double bestLengthInPopulation = Length(ants[0]);
        int idxBestLength = 0;
        for (int k = 1; k <= ants.Length - 1; k++)
        {
            double len = Length(ants[k]);
            if (len < bestLengthInPopulation)
            {
                bestLengthInPopulation = len;
                idxBestLength = k;
            }
        }
        int numCities = ants[0].Length;
        //INSTANT VB NOTE: The local variable bestTrail was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
        int[] bestTrail_Renamed = new int[numCities + 1];
        ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
        return bestTrail_Renamed;
    }

    private void UpdateAnts()
    {
        int numCities = pheromones.Length;
        for (int k = 0; k <= ants.Length - 1; k++)
        {
            int start = Random.Range(0, numCities);
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
        // for ant k (with visited[]), at nodeX, what is next node in trail?
        double[] probs = MoveProbs(k, cityX, visited);

        double[] cumul = new double[probs.Length + 1];
        for (int i = 0; i <= probs.Length - 1; i++)
        {
            cumul[i + 1] = cumul[i] + probs[i];
            // consider setting cumul[cuml.Length-1] to 1.00
        }

        double p = Random.value;

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
                taueta[i] = Pow((1.0 / Distance(cityX, i)), alpha) * Pow(pheromones[cityX][i], beta);
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
