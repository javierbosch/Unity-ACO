using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;
using System.IO;
using System.Linq;


public class Controller : MonoBehaviour
{

    public int nCopies;
    public int maxIteratios;
    private bool[] isAlive;
    public GameObject stops;
    private int numCities;
    public ACOJob[] jobs;
    public int iterations = 0;

    // Start is called before the first frame update
    void Start()
    {
        CreateFile();
        numCities = stops.gameObject.transform.childCount;

        double[,] costMatrix = GenerateCostMatrix();

        int[] sigmas = new int[2] { 1, 20 };
        double[] rhos = Enumerable.Range(0, 101).Select(i => i * 0.01).ToArray();
        double[] Qs = Enumerable.Range(0, 101).Select(i => (double)i).ToArray();

        jobs = new ACOJob[nCopies * sigmas.Length * rhos.Length * Qs.Length];

        for (int s = 0; s < sigmas.Length; s++)
        {
            for (int r = 0; r < rhos.Length; r++)
            {
                for (int q = 0; q < Qs.Length; q++)
                {
                    for (int j = 0; j < nCopies; j++)
                    {
                        jobs[j + s * nCopies + r * nCopies * sigmas.Length + q * nCopies * sigmas.Length * rhos.Length] = new ACOJob(costMatrix, 1, 2, rhos[r], Qs[q], sigmas[s], 20, "folder", "file.csv");
                    }
                }
            }
        }
    }

    void StorePoints()
    {
        Vector3[] points = new Vector3[numCities];
        for (int i = 0; i < numCities; i++)
        {
            points[i] = stops.gameObject.transform.GetChild(i).position;
        }
        StreamWriter sw = new StreamWriter("results/points.csv", true);
        for (int i = 0; i < numCities; i++)
        {
            string line = points[i].x.ToString() + "," + points[i].y.ToString();
            sw.WriteLine(line);
        }
        sw.Close();
    }

    void Update()
    {
        if (iterations < maxIteratios)
        {
            for (int i = 0; i < jobs.Length; i++)
            {
                jobs[i].NextIter();
            }
            Debug.Log(String.Format("{0:0.00}", ((double)iterations / (double)maxIteratios) * 100) + "%");
        }
        else
        {
            Store();
            print("ALL DONE");
            UnityEditor.EditorApplication.isPlaying = false;
        }
        iterations++;
    }
    void CreateFile()
    {
        StreamWriter sw = new StreamWriter("results/heatmap.csv", true);
        string headers =
            "iteration," +
            "sigma," +
            "rho," +
            "Q," +
            "bestLength";
        sw.WriteLine(headers);
        sw.Close();
    }
    void Store()
    {
        StreamWriter sw = new StreamWriter("results/heatmap.csv", true);
        for (int i = 0; i < jobs.Length; i++)
        {
            string line =
                iterations.ToString() + "," +
                jobs[i].sigma.ToString() + "," +
                jobs[i].rho.ToString() + "," +
                jobs[i].Q.ToString() + "," +
                jobs[i].bestLength.ToString();
            sw.WriteLine(line);
        }
        sw.Close();
    }

    private double[,] GenerateCostMatrix()
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
            }
        }
        return m;
    }
}
