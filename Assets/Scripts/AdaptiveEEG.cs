using System.Security.Authentication;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Threading.Tasks;

public class AdaptiveEEG: MonoBehaviour
{
    public RecordBaseline recordBaseline = null;
    public LSLInput lSLInput;
    public DataLogger logger;

    public PedestrianSpawner pedestrianSpawner;

    public bool isActive = false;

    public float adaptionRate = 20.0f;
    public int adaptationUp = 16;
    public int adaptationDown = 8;

    public double timeWindowInSeconds = 20.0;

    private float nextActionTime = 20.0f;

    public double fPS;
    public int totalCount;
    public int countPerWindow;
    public double average;
    public double percentageThreshold;
    public int minCount ;
    public int maxCount ;
    public ServerAdaptationResponse response;

    [ReadOnly] public int currencount;

    [SerializeField]
    private tcpClient tcp;
    private SignalSample signalsamp;
    public float tcpDelay = 0.01f;
    private double timeLastSendTcp = 0.0;

    private enum Attention {Internal, External}

    private Attention attention;

    public Mytask mytask;

    private string outputValues = "";


    private bool[] setCurrentCount = {true, true};

    public int currentCount
    {
        set { currencount = Math.Max(Math.Min(value, maxCount), minCount); }

        get { return currencount; }
    }


    private void Start() {

        //currencount = 100;
    }

    private void Update()
    {
        if (isActive == false)
            return;

        double time = UnixTime.GetTime();

        if (nextActionTime < tcpDelay) 
        {
            Debug.LogWarning("TCP Delay is smaller than next adaptation call!");
        }

        if(mytask.currentBlock == 4) 
        {
            if(setCurrentCount[0])
            {
                currentCount = 100;
                setCurrentCount[0] = false;
                outputValues = "";
                //nextActionTime += Time.realtimeSinceStartup; //NOTE: this is a hack to cover the initial lag
                nextActionTime = 20.0f;


            } 
        }
        if(mytask.currentBlock == 5) 
        {
            if(setCurrentCount[1])
            {
            currentCount = 100;
            setCurrentCount[1] = false;
            outputValues = "";
            nextActionTime = 20.0f;

            } 
        }
      
        // send data to py fast.       
        if (time - timeLastSendTcp > tcpDelay)
        {
            List<SignalSample1D> lstInput = lSLInput.samples;

            //Debug.Log(lSLInput.samples[0].values[0]);
            //Debug.Log(lstInput.Count);

            if (lstInput.Count > 0)
            {
                List<SignalSample> lst = SignalSample.convertEEG(lstInput);
            
                outputValues = "";
                int i = 0;
               
                foreach (SignalSample1D value in lstInput)
                {
                    if (value.time > timeLastSendTcp)
                    {
                        
                        //var cutValues = value.values.Take(20);
                        var cutValues = value.values.Take(64); //TODO: 

                        string tmp = String.Join(",", cutValues.Select(p=>p.ToString()).ToArray());

                        string arr = "[" + tmp + "],";
                        outputValues += arr;    
                        i++;
                    }
                }

                if (i > 0 ) 
                { 
                    timeLastSendTcp = time;
                    outputValues = outputValues.Remove(outputValues.Length-1);
                    if(mytask.currentBlock != 3) 
                    {
                        tcp.SendMessageNoReturn("{\"type\":\"eeg_data\",\"values\":[" + outputValues + "]}");
                        totalCount = lst.Count;
                   }
                }
                
                else if (mytask.blockDesigner.currentDuration > nextActionTime)// && (mytask.currentBlock == 4 || mytask.currentBlock == 5) 
                {
                    nextActionTime += adaptionRate;
                    if(mytask.currentBlock != 3) 
                        {
                        String s = tcp.SendMessage("{\"type\":\"calc_eeg\"}");                   
                        response = JsonUtility.FromJson<ServerAdaptationResponse>(s);
                        }

                    if(response.error == "")
                    {

                        float percentageDiffApha = ((response.curroi1 - response.basroi1) / response.basroi1) * 100; //NOTE: alpha channels: external attention
                        float percentageDiffTheta = ((response.curroi2 - response.basroi2) / response.basroi2) * 100; //NOTE: theta channels: internal attention

                        Debug.Log("Alpha Delta: " + percentageDiffApha);
                        Debug.Log("Theta Delta: " + percentageDiffTheta);

                        bool thetaIncrease = percentageDiffTheta > percentageThreshold ? true : false;
                        bool thetaDecrease = percentageDiffTheta < -percentageThreshold ? true : false;
                        bool alphaIncrease = percentageDiffApha > percentageThreshold ? true : false;
                        bool alphaDecrease =  percentageDiffApha < -percentageThreshold ? true : false;

                        // Internal Attention: Liam Increase
                        if (thetaIncrease && alphaIncrease)
                        {
                            if(mytask.currentBlock == 4) 
                            {
                                
                                //Debug.Log("TASK 4 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());
                                //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount += adaptationUp;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 4);
                                Debug.Log("More LIAMS");
                            }

                            if(mytask.currentBlock == 5) 
                            {
                                
                                //Debug.Log("TASK 5 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());

                                //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount -= adaptationDown;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "less", "external", currentCount,  response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 5); 
                                Debug.Log("Less LIAMS");
                            }
                        } 

                        // External Attention: Liam Decrease
                        if (thetaDecrease && alphaDecrease)
                        {
                            if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 4); 
                                    Debug.Log("Less LIAMS");
                                }
                                if(mytask.currentBlock == 5) 
                                {
                                    
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 5);
                                    Debug.Log("More LIAMS");
                                }
                        } 

                        //Ext-Int Competition     
                        if (thetaDecrease && alphaIncrease)
                        {
                            if(mytask.currentBlock == 4) 
                            {
                                
                                //Debug.Log("TASK 4 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());
                                //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount += adaptationUp;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 4);
                                Debug.Log("More LIAMS");
                            }

                            if(mytask.currentBlock == 5) 
                            {
                                
                                //Debug.Log("TASK 5 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());

                                //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount -= adaptationDown;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "less", "external", currentCount,  response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 5); 
                                Debug.Log("Less LIAMS");
                            }
                        } 

                        //Ext-Int Competition     
                        if (thetaIncrease && alphaDecrease)
                        {
                            if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 4); 
                                    Debug.Log("Less LIAMS");
                                }
                                if(mytask.currentBlock == 5) 
                                {
                                    
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    //currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.curroi2, response.basroi1, response.basroi2, percentageThreshold, 5);
                                    Debug.Log("More LIAMS");
                                }
                            } 
                       
                    }

                }
                else 
                {
                    //TODO: ask yagiz, does this really mean there is an error? 
                    //Debug.LogWarning("Server:" + response.error);
                }
                // Debug.Log(tonicEDA + " " + slopeBaseline + " " + (tonicEDA - slopeBaseline) + " " + percentageThreshold
            }
        } 
    }

    public List<SignalSample> getAffectedSamples(List<SignalSample> samples, double time)
    {
        double minAcceptableTime = time - this.timeWindowInSeconds;
    
        var ret =  samples.Where(o => o.Time > minAcceptableTime).ToList();
        this.fPS = ret.Count / this.timeWindowInSeconds;
        return ret;
    }
}

