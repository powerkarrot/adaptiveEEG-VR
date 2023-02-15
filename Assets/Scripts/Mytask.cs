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

    public GameObject questionnaire;
    public GameObject vivepointer;

    public GameObject feedbackStats;
    public int feedbackStatsPresentValue = 20;
    private int feedbackStatsCounter = 0;

    public GameObject CountNr;

    public bool startactivateAdaptationCoroutine = true;

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

    public AdaptiveEEG adaptiveEEG = null;

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

    public ServerAlphaBaselineResponse alphabaselineResponse;
    
    private int currencount = 0;
    private int currencount2 = 0; //TODO: remove this lol

    [SerializeField]
    private tcpClient tcp;
    private SignalSample signalsamp;
    public float tcpDelay = 3.0f;
    private double timeLastSendTcp = 0.0;
    //private bool newDataArrived = false;

    private bool baselineDone = false;

    private bool TEST = true;


    // Start is called before the first frame update
    void Start() //initializationstep 
    {
        questionnaire.SetActive(false);
        adaptiveEEG.isActive = false;
        

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
         if (Input.GetKeyDown("q")) 
        {
            questionnaire.SetActive(!questionnaire.activeSelf);
        }

        currentBlock = blockDesigner.getCurrentBlock();

        double timestamp = UnixTime.GetTime();

        if (state == STATES.wait && currentBlock == 1)
        {
            pilar.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            sphere.SetActive(false);
            CountNr.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = true;
            //blockDesigner.isAdaptive = false;
            blockDesigner.duration = TEST ? 3f : 120f;
        }

        else if (state == STATES.wait && currentBlock == 2)
        {
            pilar.SetActive(false);
            sphere.SetActive(false);
            CorrectTrash.SetActive(false);
            trash_square.SetActive(false);
            CountNr.SetActive(false);
            vivepointer.SetActive(false);
            blockDesigner.IsIAfBaseline = false;
            pedestrianSpawner.pedestriansToSpawn = 0;
            blockDesigner.duration = TEST ? 3f : 360f;

            if(blockDesigner.isDone)
            {
                adaptiveEEG.isActive = true;
            }


        }

        else if (state == STATES.wait && currentBlock == 3)
        {
            if (startactivateAdaptationCoroutine == true) 
            {
                StartCoroutine(activateAdaptationCoroutine(timestamp));         
            } 
            CountNr.SetActive(false);
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);

            blockDesigner.IsIAfBaseline = false;
            vivepointer.SetActive(false);
            blockDesigner.duration = TEST ? 20f : 360f;
            pedestrianSpawner.pedestriansToSpawn = 160;
           
            if(blockDesigner.isDone)
            {
                if(baselineDone == false)
                {
                    String curID = logger.participantId.ToString();
                    
                    String s = tcp.SendMessage("{\"type\":\"alphapow_baseline\", \"values\": " + curID + "}");
                    alphabaselineResponse = JsonUtility.FromJson<ServerAlphaBaselineResponse>(s); 
                    if(alphabaselineResponse.error == "")
                    {
                        baselineDone = Convert.ToBoolean(alphabaselineResponse.baselineDone);
                        Debug.Log("baseline is done" + baselineDone);
                        blockDesigner.gotAlphaPowBaseline = true;
                        print("is done: " + alphabaselineResponse.baselineDone);
                    }
                }
            }        
        }

        else if (state == STATES.wait && currentBlock == 4)
        {
            CountNr.SetActive(false);
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            blockDesigner.IsIAfBaseline = false;
            vivepointer.SetActive(false);
            adaptiveEEG.isActive = true;
            blockDesigner.duration =  360;
            blockDesigner.duration = TEST ? 40f : 360f;
            blockDesigner.isAdaptive = true;

        }

        else if (state == STATES.wait && currentBlock == 5)
        {
            CountNr.SetActive(false);
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            blockDesigner.duration = TEST ? 40f : 360f;

            blockDesigner.IsIAfBaseline = false;
            adaptiveEEG.isActive = true;
            blockDesigner.isAdaptive = true;
            vivepointer.SetActive(false);
        }

        else
        {
            pilar.SetActive(true);
            CorrectTrash.SetActive(true);
            trash_square.SetActive(true);
            vivepointer.SetActive(true);
            blockDesigner.IsIAfBaseline = false;
            blockDesigner.duration = 360.00;
           
        }

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

            else if (nextBlock == 4) 
            {
                //adaptiveEDA.isActive = true;  
                //rInt = UnityEngine.Random.Range(20, 120);
                pedestrianSpawner.pedestriansToSpawn = 100;                          
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
               
            }

            else if (nextBlock == 5)
            {
                //adaptiveEDA.isActive = true;  
                //rInt = UnityEngine.Random.Range(20, 120);
                pedestrianSpawner.pedestriansToSpawn = 100;
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);

            }
            else 
            {
                adaptiveEDA.isActive = false;
                //adaptiveEEG.isActive = false;
                //pedestrianSpawner.pedestriansToSpawn = 0 + ((nextBlock-1)*100);
                logger.writeState(timestamp, "start", nextBlock, 1, nBackNumber);
            }

            if (state == STATES.wait) {
                isRecodingBaseline = false;
                //feedbackCorrect.SetActive(false);
                //feedbackWrong.SetActive(false);
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
            //feedbackCorrect.SetActive(false);
            //feedbackWrong.SetActive(false);
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
                //Debug.Log("correct");
                isLastCorrect = true;
                if (showFeedback) { 
                    feedbackCorrect.SetActive(true);
                }
            }

            else if (lastColor != nBackColor && "red" == pickedTrash)  //If the color of the last ball presented does not match the Nback Color & it's put in the red trash, feedback is correct
            {
                //Debug.Log("correct");
                isLastCorrect = true;
                if (showFeedback)
                {
                    feedbackCorrect.SetActive(true);
                }
            }
            else  //Otherwise if the Last color matches the Nback color and it's put in the red trashcan OR Last color does not match the Nback color and it's put in the green trashcan: Feedback is wrong
            {
                //Debug.Log("wrong");
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

    IEnumerator activateAdaptationCoroutine(double timestamp)
    {
        
        while(true) 
        {
                    double sendData = TEST ? 2f : (blockDesigner.duration - 20f);

                    yield return new WaitForSeconds((float)(sendData)); 
                    //do thing
                    startactivateAdaptationCoroutine = false;

                    //Debug.Log("send baseline Data");
                    blockDesigner.isAdaptive = true;
                    //adaptiveEEG.isActive = true;
        }
    }
}

