using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showOptimal : MonoBehaviour
{

    public TextAsset tspFile;
    public GameObject parent;
    public GameObject samplePathLine;
    public bool showPath;

    [HideInInspector]
    public double optimalLength;

    private int dimension;
    private int[] optimalPath;

    [HideInInspector]
    public bool pathIsInitilized = false;
    
    private GameObject pathObject;

    // Start is called before the first frame update
    void Start()
    {
        dimension = getDimension();
        optimalPath = readPath();
        initPath(optimalPath);
        optimalLength = calculateLength(optimalPath);
        print("Optimal length: " + optimalLength);
    }

    // Update is called once per frame
    void Update()
    {  
        if (showPath)
            pathObject.SetActive(true);
        else
            pathObject.SetActive(false);
    }

    private bool parentIsInitialized(){
        return parent.transform.childCount == dimension;
    }
    
    private void initPath(int[] path)
    {
        pathObject = Instantiate(samplePathLine, new Vector3(0, 0, 0), Quaternion.identity);
        LineRenderer lineRenderer = pathObject.GetComponent<LineRenderer>();
        lineRenderer.gameObject.GetComponent<LineRenderer>().positionCount = dimension+1;
        for (int i = 0; i < dimension; i++)
        {
            lineRenderer.SetPosition(i, parent.transform.GetChild(path[i]-1).position);
        }
        lineRenderer.SetPosition(dimension, parent.transform.GetChild(path[0]-1).position);
        pathIsInitilized = true;
    }

    private int getDimension()
    {
        string[] lines = tspFile.text.Split('\n');
        for (int i=0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("DIMENSION"))
            {
                string[] line = lines[i].Split(' ');
                return int.Parse(line[1]);
            }
        }
        return -1;
    }

    private int[] readPath()
    {
        int[] path = new int[dimension];
        string[] lines = tspFile.text.Split('\n');
        bool readTour = false;
        int j = 0;
        for (int i=0; i < lines.Length; i++)
        {   
            if (lines[i].StartsWith("TOUR_SECTION"))
            {
                readTour = true;
                continue;
            }
            if (readTour){
                int pointIndex = int.Parse(lines[i]);
                if (pointIndex == -1)
                    break;
                path[j] = pointIndex;
                j++;
            }
        }
        return path;
    }

    private double calculateLength(int[] path)
    {
        double length = 0;
        for (int i = 0; i < dimension-1; i++)
        {
            length += Vector3.Distance(parent.transform.GetChild(path[i]-1).position, parent.transform.GetChild(path[i+1]-1).position);
        }
        length += Vector3.Distance(parent.transform.GetChild(path[dimension-1]-1).position, parent.transform.GetChild(path[0]-1).position);
        return length;
    }
}
