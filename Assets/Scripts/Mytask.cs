using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class Mytask : MonoBehaviour
{
    enum STATES { start, end, wait, move, baseline };
    public BlockDesigner blockDesigner;
    public PedestrianSpawner pedestrianSpawner;
    
    STATES state = STATES.start;

    public GameObject feedbackCorrect;
    public GameObject feedbackWrong;
    private Coroutine disableFeedback;

    public GameObject questionner;
    public GameObject vivepointer;

    public GameObject feedbackStats;
    public int feedbackStatsPresentValue = 20;
    private int feedbackStatsCounter = 0;
    private Coroutine feedbackStatCoroutineDisable;

    public GameObject pilar;
    public GameObject CorrectTrash;
    public GameObject trash_square;
    private GameObject sphere = null;

    public int counterBalls = 0;
    public bool isLastCorrect = false;

    public Material[] materials;
    private List<int> colorList = new List<int>();
    public int nBackNumber ;

    private int lastColor = 0;
    private int nBackColor = 0;

    public int currentBlock ;

    //private double lastTimeStamp = 0;

    public bool showFeedback = true;

    public CountPeople counterPeople;
    public RecordBaseline recordBaseline = null;
    public bool isRecodingBaseline = false;

    public AdaptiveEDA adaptiveEDA = null;

    private int counter = 0;


    public DataLogger logger;

    public float TimeSinceStart = 0;
    public float TimeSinceStart2 = 0;

    public double secondscounter = 0;

    /*public int rInt ;
    List<int> frame2ints = new List<int>{-6,12};*/

    
    public LaserPointerEnhanced laserPointer;

    public string selectableObjectTag;

    public ServerIAFResponse iafResponse;


    private int currencount = 0;

    [SerializeField]
    private tcpClient tcp;
    private SignalSample signalsamp;
    public float tcpDelay = 3.0f;
    private double timeLastSendTcp = 0.0;
    //private bool newDataArrived = false;


    // Start is called before the first frame update
    void Start() //initializationstep 
    {

        if (logger == null)
        {
            Debug.LogError("Logger not set");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        if (pilar == null)
        {
            Debug.LogError("Pilar not set");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        if (materials.Length < 2)
        {
            Debug.LogError("Not enough colors");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        if (recordBaseline == null)
        {
            Debug.LogError("recordBaseline not set");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        feedbackCorrect.SetActive(false);
        feedbackWrong.SetActive(false);
        feedbackStats.SetActive(false);
        logger.blockDesigner = blockDesigner;
    }


    // Update is called once per frame
    void Update()
    {
        currentBlock = blockDesigner.getCurrentBlock();

        /* if (state == STATES.wait && currentBlock == 7)
         {
             TimeSinceStart += Time.deltaTime;
         }

         if (state == STATES.wait && currentBlock == 6 )
         {
             TimeSinceStart2 += Time.deltaTime;
         }
        */

        if (state == STATES.wait && currentBlock == 1)
        {
            pilar.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            sphere.SetActive(false);
            feedbackCorrect.SetActive(false);
            feedbackWrong.SetActive(false);
            feedbackStats.SetActive(false);
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = true;
            blockDesigner.duration = 60.00;
        }

        else if (state == STATES.wait && currentBlock == 2)
        {
            pilar.SetActive(false);
            sphere.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            feedbackCorrect.SetActive(false);
            feedbackWrong.SetActive(false);
            feedbackStats.SetActive(false);
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = false;
            pedestrianSpawner.pedestriansToSpawn = 0;
        }
        else if (state == STATES.wait && currentBlock == 3)
        {
            pilar.SetActive(false);
            sphere.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            feedbackCorrect.SetActive(false);
            feedbackWrong.SetActive(false);
            feedbackStats.SetActive(false);
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = false;
            pedestrianSpawner.pedestriansToSpawn = 50;
        }
        else if (state == STATES.wait && currentBlock == 4)
        {
            pilar.SetActive(false);
            sphere.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            feedbackCorrect.SetActive(false);
            feedbackWrong.SetActive(false);
            feedbackStats.SetActive(false);
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = false;
            pedestrianSpawner.pedestriansToSpawn = 200;
        }

        else if (state == STATES.wait && currentBlock == 5)
        {
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            feedbackCorrect.SetActive(true);
            feedbackWrong.SetActive(true);
            feedbackStats.SetActive(true);
            blockDesigner.IsIAfBaseline = false;           
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            pedestrianSpawner.pedestriansToSpawn = 0;

        }
        else if (state == STATES.wait && currentBlock == 6)
        {
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            feedbackCorrect.SetActive(true);
            feedbackWrong.SetActive(true);
            feedbackStats.SetActive(true);
            blockDesigner.IsIAfBaseline = false;
            questionner.SetActive(false);
            vivepointer.SetActive(false);
            pedestrianSpawner.pedestriansToSpawn = 50;

        }
        else if (state == STATES.wait && currentBlock == 7)
        {
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            feedbackCorrect.SetActive(true);
            feedbackWrong.SetActive(true);
            feedbackStats.SetActive(true);
            blockDesigner.duration = 360.00;
            blockDesigner.IsIAfBaseline = false;

            questionner.SetActive(false);
            vivepointer.SetActive(false);
            pedestrianSpawner.pedestriansToSpawn = 200;

        }
        else
        {
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            feedbackCorrect.SetActive(true);
            feedbackWrong.SetActive(true);
            feedbackStats.SetActive(true);
            questionner.SetActive(true);
            vivepointer.SetActive(true);
            blockDesigner.IsIAfBaseline = false;
            blockDesigner.duration = 360.00;
           
        }


        double timestamp = UnixTime.GetTime();

      /*  if (Input.GetKeyDown("a"))
        {
            adaptiveEDA.isActive = !adaptiveEDA.isActive;
        }*/


        if (Input.GetKeyDown("s") && STATES.start == state) //Start the task
        {
            Debug.LogWarning("Starting task...");
            state = STATES.wait;
            int nextBlock = blockDesigner.getNextBlock();
            Debug.Log("nextBlock: " + nextBlock);
            if (nextBlock == -1) {
                Debug.LogError("Wrong !");
            }
            else if (nextBlock == -2)
            {
                state = STATES.end;
                logger.writeState(timestamp, "end", nextBlock, -1, nBackNumber);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            if (nextBlock == 5) 
            {
                //adaptiveEDA.isActive = true;
                pedestrianSpawner.pedestriansToSpawn = 0;
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
                //Debug.Log("Start adaptation 7");
            }    
            else if (nextBlock == 6) 
            {
                //adaptiveEDA.isActive = true;  
                //rInt = UnityEngine.Random.Range(20, 120);
                pedestrianSpawner.pedestriansToSpawn = 50;                          
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
                //Debug.Log("Start adaptation 6");
               
            }

            else if (nextBlock == 7)
            {
                //adaptiveEDA.isActive = true;  
                //rInt = UnityEngine.Random.Range(20, 120);
                pedestrianSpawner.pedestriansToSpawn = 200;
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
                //Debug.Log("Start adaptation 6");

            }
            else 
            {
                adaptiveEDA.isActive = false;
                //pedestrianSpawner.pedestriansToSpawn = 0 + ((nextBlock-1)*100);
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
            }

            if (state == STATES.wait) {
                isRecodingBaseline = false;
                feedbackCorrect.SetActive(false);
                feedbackWrong.SetActive(false);
                counterBalls = 0;
                feedbackStatsCounter = 0;
                colorList.Clear();
                generateSpheres();
                counterPeople.setCounterEnabled(true);
                blockDesigner.startRecoding();
            }       
        }

        else if(Input.GetKeyDown("b") && STATES.start == state) //Start the baseline task
        {
            isRecodingBaseline = true;
            recordBaseline.startRecoding();
            adaptiveEDA.isActive = true;
            feedbackCorrect.SetActive(false);
            feedbackWrong.SetActive(false);
            counterBalls = 0;
            feedbackStatsCounter = 0;
            state = STATES.baseline;
            colorList.Clear();
            generateSpheres();
            counterPeople.setCounterEnabled(true);
            pedestrianSpawner.pedestriansToSpawn = recordBaseline.countForBaselineRecording;
            logger.writeState(timestamp, "baselineStart", recordBaseline.countForBaselineRecording, 1, nBackNumber);
        }
        else if (Input.GetKeyDown("left") && STATES.wait == state)
        {
            //print("left key was pressed");
            presentFeedback(timestamp, "red");
            state = STATES.wait;
            counterBalls++  ;
        }

        else if (Input.GetKeyDown("right") && STATES.wait == state)
        {
            //print("right key was pressed");
            presentFeedback(timestamp, "green");
            state = STATES.wait;
            counterBalls++;
        }

        if (recordBaseline.isBaselineRecoringDone() == true && isRecodingBaseline == true) {
            logger.writeState(timestamp, "baselineEnd", recordBaseline.countForBaselineRecording, recordBaseline.getBaselineSlope(), nBackNumber);
            isRecodingBaseline = false;
            counterPeople.setCounterEnabled(false);
            state = STATES.start;
            if (sphere != null)
            {
                Destroy(sphere);
            }
        }

        if(blockDesigner.IsIAfBaseline) 
        print("doing baseline");

        {
                if(blockDesigner.getIAF) 
                {
                    currencount += 1;
                    if (currencount==1)  {
                        if (timestamp - timeLastSendTcp > tcpDelay) 
                        {
                            String curID = logger.participantId.ToString();
                            String s = tcp.SendMessage("{\"type\":\"iaf\", \"values\": " + curID + "}");
                            iafResponse = JsonUtility.FromJson<ServerIAFResponse>(s); 
                            print("lower iaf is" + iafResponse.lowerAlpha);
                            timeLastSendTcp = timestamp;
                            blockDesigner.gotIAF = Convert.ToBoolean(iafResponse.iafDone); 
                        }
                    }
                    
                }                    
        }

        if (blockDesigner.isDone && STATES.start != state && isRecodingBaseline == false)
        {
            logger.writeState(timestamp, "end", -1, -1, nBackNumber);
            counterPeople.setCounterEnabled(false);
            state = STATES.start;
            if (sphere != null)
            {
                Destroy(sphere);
            }
        }
    }

    public void collision(double timestamp, string pickedTrash)
    {
        presentFeedback(timestamp, pickedTrash);
        state = STATES.wait;
        counterBalls++;
        generateSpheres();
        //lastTimeStamp = timestamp;

        // Count the numbers of correct feedbacks for the stats feedback
        if (isLastCorrect == true)
        {
            feedbackStatsCounter++;
        }

        // Present stats after X balls.
        if (counterBalls % feedbackStatsPresentValue == 0)
        {

            float accuracy = (float)feedbackStatsCounter / (float)feedbackStatsPresentValue * 100.0f;
            TextMeshPro tmp = feedbackStats.GetComponent<TextMeshPro>() as TextMeshPro;
            tmp.SetText(Math.Round(accuracy) + "% Accuracy");
            feedbackStats.SetActive(true);
            feedbackStatsCounter = 0;

            if (feedbackStatCoroutineDisable != null)
            {
                StopCoroutine(feedbackStatCoroutineDisable);
            }

            feedbackStatCoroutineDisable = StartCoroutine(waitFeedbackStatsCoroutine());
        }
    }

    public void generateSpheres()
    {
        if (sphere != null)
        {
            Destroy(sphere);
        }

        //Debug.Log("Sound Played");
        this.GetComponent<AudioSource>().Play();

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.AddComponent<CollisionDone>();
        sphere.AddComponent<Teleporter>();

        sphere.transform.position = pilar.transform.position + new Vector3(0, 0.8f, 0);
        sphere.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        //Destroy(sphere.GetComponent<SphereCollider>());
        //BoxCollider bc = sphere.AddComponent(typeof(BoxCollider)) as BoxCollider;
        //bc
        Rigidbody sphereRigidBody = sphere.AddComponent<Rigidbody>();
        sphereRigidBody.mass = 0.1f;
        sphereRigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        sphereRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        int randomColorId = UnityEngine.Random.Range(0, materials.Length);
        colorList.Add(randomColorId);
        sphere.GetComponent<Renderer>().material = materials[randomColorId];

        DestroyandPlay4s d = sphere.AddComponent<DestroyandPlay4s>() as DestroyandPlay4s;
        d.laserPointer = laserPointer;
        d.task = this;

        sphere.transform.parent = this.transform;
        sphere.name = "nbacktask";
        sphere.tag = this.selectableObjectTag;

        int LayerUI = LayerMask.NameToLayer("UI");
        sphere.layer = LayerUI;
    }

    public void presentFeedback(double timestamp, string pickedTrash)
    {
        if (colorList.Count - nBackNumber - 1 >= 0) //Here we start presenting the feedback after 2 initial trials
        {
            if (disableFeedback != null)
            {
                StopCoroutine(disableFeedback);
            }

            feedbackWrong.SetActive(false);  //remove wrong feedbackinfo
            feedbackCorrect.SetActive(false); //remove correct feedbackinfo

            lastColor = colorList[colorList.Count - 1];  //Last color of the ball defined as the last element of the color list
            nBackColor = colorList[colorList.Count - nBackNumber - 1];  //nbackColor defined as colorList element minus the NBack number (1) - 1, 


            if (lastColor == nBackColor && "green" == pickedTrash)   //If the color of the last ball presented matches the Nback Color & it's put in the green trash, feedback is correct
            {
                Debug.Log("correct");
                isLastCorrect = true;
                if (showFeedback) { 
                    feedbackCorrect.SetActive(true);
                }
            }

            else if (lastColor != nBackColor && "red" == pickedTrash)  //If the color of the last ball presented does not match the Nback Color & it's put in the red trash, feedback is correct
            {
                Debug.Log("correct");
                isLastCorrect = true;
                if (showFeedback)
                {
                    feedbackCorrect.SetActive(true);
                }
            }
            else  //Otherwise if the Last color matches the Nback color and it's put in the red trashcan OR Last color does not match the Nback color and it's put in the green trashcan: Feedback is wrong
            {
                Debug.Log("wrong");
                isLastCorrect = false;
                if (showFeedback)
                {
                    feedbackWrong.SetActive(true);
                }
            }

            logger.writeScore(timestamp, nBackColor, lastColor, pickedTrash, isLastCorrect, currentBlock);
            disableFeedback = StartCoroutine(myWaitCoroutine());
        }
    }

    IEnumerator myWaitCoroutine()
    {
        yield return new WaitForSeconds(1f); // Wait for one second
        feedbackWrong.SetActive(false);
        feedbackCorrect.SetActive(false);
    }

    IEnumerator waitFeedbackStatsCoroutine()
    {
        yield return new WaitForSeconds(4f); // Wait for one second
        feedbackStats.SetActive(false);
    }
}




