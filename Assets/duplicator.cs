using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class duplicator : MonoBehaviour
{

    public GameObject sample;
    public int nCopies;
    public int maxIteratios;
    private GameObject[] copies;
    private bool[] isAlive;

    // Start is called before the first frame update
    void Start()
    {
        double[] sigmas = new double[2] { 1, 20 };

        // Instantiate the sample object nCopies times with i name
        copies = new GameObject[nCopies * sigmas.Length];
        isAlive = new bool[nCopies * sigmas.Length];

        for (int s = 0; s < sigmas.Length; s++)
        {
            for (int j = 0; j < nCopies; j++)
            {
                copies[j + s * nCopies] = Instantiate(sample, new Vector3(1, 0, 0), Quaternion.identity);
                copies[j + s * nCopies].name = "ACO - clone " + j.ToString() + " sigma " + sigmas[s].ToString();
                copies[j + s * nCopies].GetComponent<ACO>().sigma = sigmas[s];
            }
        }
        for (int i = 0; i < nCopies * sigmas.Length; i++)
        {
            copies[i].SetActive(true);
            isAlive[i] = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < nCopies; i++)
        {
            if (copies[i].GetComponent<ACO>().iterations < maxIteratios)
            {
                copies[i].GetComponent<ACO>().enabled = true;
            }
            else
            {
                copies[i].GetComponent<ACO>().enabled = false;
                isAlive[i] = false;
            }
        }
        // check if all the copies are alive stop
        bool allDead = true;
        for (int i = 0; i < nCopies; i++)
        {
            if (isAlive[i])
            {
                allDead = false;
            }
        }
        if (allDead)
        {
            print("ALL DONE");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}
