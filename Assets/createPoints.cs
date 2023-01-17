using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createPoints : MonoBehaviour
{
    public GameObject samplePoint;
    public TextAsset tspFile;
    public GameObject parent;
    
    private float maxX;
    private float maxY;
    public float xScale;
    public float yScale;


    // Start is called before the first frame update
    void Start()
    {
        readTspFile();
        scalePoints();

        showOptimal showOptimalScript = GetComponent<showOptimal>();
        showOptimalScript.enabled = true;
        duplicator duplicatorScript = GetComponent<duplicator>();
        duplicatorScript.enabled = true;
    }

    void readTspFile()
    {
        string[] lines = tspFile.text.Split('\n');
        bool readCoords = false;
        for (int i=0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("NODE_COORD_SECTION"))
            {
                readCoords = true;
                continue;
            }
            if (readCoords){
                string[] line = lines[i].Split(' ');
                if( line.Length < 3)
                    break;
                Vector3 position = new Vector3(float.Parse(line[1]), float.Parse(line[2]), 0);
                GameObject newPoint = Instantiate(samplePoint,position, Quaternion.identity, parent.transform);
            }
        }
    }

    void findMaxMin()
    {
        foreach (Transform child in parent.transform)
        {
            if (child.position.x > maxX)
            {
                maxX = child.position.x;
            }
            if (child.position.y > maxY)
            {
                maxY = child.position.y;
            }
        }
    }
    
    void scalePoints(){
        findMaxMin();
        foreach (Transform child in parent.transform)
        {
            child.position = new Vector3( xScale * child.position.x / maxX, yScale * child.position.y / maxX, 0);
        }
    }

}
