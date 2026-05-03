using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIScript : MonoBehaviour
{
    public GameObject visualPrefab;
    public Font textFont;


    public Sprite Default_Object;
    public Sprite Object_One_Image;
    public Sprite Object_Two_Image;

    
    public bool isStaggered;

    public static int OBJECTCOUNT = 7;

    private float preWaitTimer = 3.0f;
    private float preWaitCount = 0.0f;

    private float totalWaitTimer = 6.0f;
    private float totalWaitCount = 0.0f;

    // Set timers for continuous animations
    private float firstSetAnimationTimer = 1.25f;
    private float firstSetAnimationCount = 0.0f;

    private float secondSetAnimationTimer = 1.25f;
    private float secondSetAnimationCount = 0.0f;

    private float staggerTimer = 0.0f;
    private float staggerCount = 0.0f;

    private bool inPreWait = true;
    private bool inStagger = false;

    private float width;
    private float height;
    
    private float[] xPositions = new float[OBJECTCOUNT];
    private float[] xGoalPositions = new float[OBJECTCOUNT];
    private bool[] usedGoalIndexes = new bool[OBJECTCOUNT];
    private float yPosition;
    private GameObject[] visuals = new GameObject[OBJECTCOUNT];

    private bool[] isFirstSet = new bool[OBJECTCOUNT];

    private bool numbersOverlaid = false;

  private int[] movingObjectIndex = new int[2];
    private int[] uniqueObjectIndex = new int[2];

    // Start is called before the first frame update
    void Start()
    {
        // Get Canvas dimensions
        GameObject canvas = this.gameObject;
        height = canvas.GetComponent<RectTransform>().rect.height;
        width = canvas.GetComponent<RectTransform>().rect.width;

        yPosition = height/2.0f;
        for(int i = 0; i < xPositions.Length; i++)
        {
            // Evenly place out objects and set their sprite
            xPositions[i] = ((float) i+1.0f)*(width/(1.0f + (float) OBJECTCOUNT));
            visuals[i] = Instantiate(visualPrefab, new Vector3(xPositions[i], yPosition, 0), Quaternion.identity, this.transform);
            visuals[i].GetComponent<Image>().sprite = Default_Object;
        }

        // Adjust timers if the animation is staggered
        if (isStaggered)
        {
            firstSetAnimationTimer = 0.75f;
            secondSetAnimationTimer = 0.75f;
            staggerTimer = 0.5f;
        }

        pickPositions();
    }

    // Update is called once per frame
    void Update()
    {
        totalWaitCount += Time.deltaTime;
        if ((totalWaitCount > totalWaitTimer) & !numbersOverlaid)
        {
            numbersOverlaid = true;

            for(int i = 0; i < xPositions.Length; i++)
            {
                GameObject uiText = new GameObject();
                uiText.transform.parent = visuals[i].transform.parent;
                Vector3 approxPos = visuals[i].transform.position;
                uiText.transform.position = new Vector3(xPositions[i], (height/2.0f), approxPos.z);

                var textField = uiText.AddComponent<Text>();

                textField.alignment = TextAnchor.MiddleCenter;
                textField.font = textFont;
                textField.fontSize = 30;
                textField.text = (i+1).ToString();


            }
        }

        if (inPreWait)
        {
            preWaitCount += Time.deltaTime;
            if (preWaitCount > preWaitTimer)
            {

                for (int i = 0; i < uniqueObjectIndex.Length; i++) 
                {
                    visuals[uniqueObjectIndex[i]].GetComponent<Image>().sprite = Default_Object;
                }

                inPreWait = false;
                firstSetAnimationCount += preWaitTimer - preWaitCount;
                if (staggerTimer > 0)
                {
                    inStagger = true;
                    staggerCount += preWaitTimer - preWaitCount;
                } else
                {
                    secondSetAnimationCount += preWaitTimer - preWaitCount;
                }
                
            }
        } else if (inStagger)
        {
            staggerCount += Time.deltaTime;
            firstSetAnimationCount += Time.deltaTime;

            if (staggerCount > staggerTimer)
            {
                inStagger = false;
                secondSetAnimationCount += staggerTimer - staggerCount;
            }
        } else
        {
            firstSetAnimationCount += Time.deltaTime;
            secondSetAnimationCount += Time.deltaTime;
        }

        for(int i = 0; i < visuals.Length; i++)
        {
            Vector3 pos = visuals[i].transform.position;
            float timerCountFraction;
            if (isFirstSet[i])
            {
                timerCountFraction = firstSetAnimationCount/firstSetAnimationTimer;
            } else
            {
                timerCountFraction = secondSetAnimationCount/secondSetAnimationTimer;
            }
            visuals[i].transform.position = Vector3.Lerp(
                new Vector3(xPositions[i], pos.y, pos.z),
                new Vector3(xGoalPositions[i], pos.y, pos.z), 
                timerCountFraction);

        }
    }

    void resolveRemainingObjects(int[] remainingObjects)
    {

        //Define ending positions for all remaining objects based on available positions
        int remainingObjectIndex = 0;
        for (int i = 0; i < visuals.Length; i++)
        {
            if (!usedGoalIndexes[i])
            {
                usedGoalIndexes[i] = true;
                xGoalPositions[remainingObjects[remainingObjectIndex]] = xPositions[i];
                remainingObjectIndex++;

            }
        }

    }

    void increaseLayerHeight(int objectIndex)
    {
        Vector3 currentPos = visuals[objectIndex].transform.position;
        visuals[objectIndex].transform.position = new Vector3(currentPos.x, currentPos.y, 200);
        visuals[objectIndex].transform.SetAsLastSibling();

    }

    void pickPositions()
    {
        //Random number between [2, #Objects-1)
        int moveAmount = Random.Range(2, OBJECTCOUNT-1);
        int objOne;
        int objTwo;
        bool goLeft = Random.Range(0, 1+1) == 1;
        int[] remainingObjects = new int[OBJECTCOUNT-2];


        if (goLeft)
        {
            //Pick 2 possible objects
            objOne = Random.Range(moveAmount, OBJECTCOUNT);
            objTwo = Random.Range(moveAmount, OBJECTCOUNT-1);
            if (objTwo >= objOne)
            {
                objTwo++;
            }

            //Adjust their layer height 
            movingObjectIndex[0] = objOne;
            movingObjectIndex[1] = objTwo;
            increaseLayerHeight(objOne);
            increaseLayerHeight(objTwo);

            //Define their end positions
            xGoalPositions[objOne] = xPositions[objOne - moveAmount];
            xGoalPositions[objTwo] = xPositions[objTwo - moveAmount];
            usedGoalIndexes[objOne - moveAmount] = true;
            usedGoalIndexes[objTwo - moveAmount] = true;
            isFirstSet[objOne] = true;
            isFirstSet[objTwo] = true;

        } else
        {
            //Pick 2 possible objects
            objOne = Random.Range(0, OBJECTCOUNT-moveAmount-1);
            objTwo = Random.Range(1, OBJECTCOUNT-moveAmount-1);
            if (objTwo <= objOne)
            {
                objTwo--;
            }

            //Adjust their layer height 
            movingObjectIndex[0] = objOne;
            movingObjectIndex[1] = objTwo;
            increaseLayerHeight(objOne);
            increaseLayerHeight(objTwo);

            //Define their end positions
            xGoalPositions[objOne] = xPositions[objOne + moveAmount];
            xGoalPositions[objTwo] = xPositions[objTwo + moveAmount];
            usedGoalIndexes[objOne + moveAmount] = true;
            usedGoalIndexes[objTwo + moveAmount] = true;
            isFirstSet[objOne] = true;
            isFirstSet[objTwo] = true;
            

        }

        //Define which objects are left
        int currentArrayIndex = 0;
        for (int i = 0; i < visuals.Length; i++)
        {
            if (i == objOne || i == objTwo)
            {
                continue;
            }
            remainingObjects[currentArrayIndex] = i;
            currentArrayIndex++;
        }

        resolveRemainingObjects(remainingObjects);
        pickUnique(remainingObjects);

    }

    void highlightedObject(int objectIndex, bool isRed)
    {
        Vector3 currentPos = visuals[objectIndex].transform.position;
        visuals[objectIndex].transform.position = new Vector3(currentPos.x, currentPos.y, currentPos.z-100);
        if (isRed)
        {
            visuals[objectIndex].GetComponent<Image>().sprite = Object_One_Image;
        } else
        {
            visuals[objectIndex].GetComponent<Image>().sprite = Object_Two_Image;
        }
    }

    void pickUnique(int[] remainingObjects)
    {
        int randomFirstSet = Random.Range(0, 2);
        int randomSecondSet = Random.Range(0, remainingObjects.Length);

        bool movingObjectIsRed = Random.Range(0, 2) == 0;

        for (int i = 0; i < isFirstSet.Length; i++)
        {
            if (isFirstSet[i])
            {
                if (randomFirstSet == 0)
                {
                   
                    highlightedObject(i, movingObjectIsRed);
                    uniqueObjectIndex[0] = i;

                }
            
                randomFirstSet--;
            
            } else
            {
                if (randomSecondSet == 0)
                {
                    highlightedObject(i, !movingObjectIsRed);
                    uniqueObjectIndex[1] = i;
                }
                
                randomSecondSet--;
                
            }
        }
        
    }

}
