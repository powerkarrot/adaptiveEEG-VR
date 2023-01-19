using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Threading.Tasks;

public class AdaptiveEDA: MonoBehaviour
{
    public RecordBaseline recordBaseline = null;
    public LSLInput lSLInput;
    public DataLogger logger;

    public PedestrianSpawner pedestrianSpawner;

    public bool isActive = false;

    public float adaptionRate = 20.0f;
    public int adaptationUp;
    public int adaptationDown;



    public double timeWindowInSeconds = 20.0;
    public double butterworthLowPassFrequency;
    public double butterworthHighPassFrequency;

    public double adaptiveFactor = 0.0;
    //private double averageLast = 0.0;
    public double proportional = 0.5;

    private float nextActionTime = 20.0f;
    private float nextActionTime2 = 20.0f;

    private float firstadaptationtime = 20.0f;
    private float firstadaptationtime2 = 20.0f;

    public double fPS;
    public int totalCount;
    public int countPerWindow;
    public double average;

    public double slopeThreshold;
    //public double offset; 
    // Pedestrian Spawning Variables

    public int minCount ;
    
    public int maxCount ;

    public List<float> slope = new List<float>();

    public ServerResponse response;


    [ReadOnly] public int currencount = 0;

    [SerializeField]
    private tcpClient tcp;
    private SignalSample signalsamp;
    public float tcpDelay = 0.01f;
    private double timeLastSendTcp = 0.0;
    //private bool newDataArrived = false;

    public Mytask mytask;

    public int currentCount
    {
        set { currencount = Math.Max(Math.Min(value, maxCount), minCount); }

        get { return currencount; }
    }

    private void Update()
    {
        if (isActive == false)
            return;

        double time = UnixTime.GetTime();
        //Debug.Log(mytask.TimeSinceStart);

        if (nextActionTime < tcpDelay) 
        {
            Debug.LogWarning("TCP Delay is smaller than next adaptation call!");
        }

        // send data to py fast.
        if (time - timeLastSendTcp > tcpDelay)
        {
            List<SignalSample1D> lstInput = lSLInput.samples;
            //Debug.Log(lstInput.Count);

            if (lstInput.Count > 0)
            {
                List<SignalSample> lst = SignalSample.convertToEDA(lstInput);
            
                string outputValues = "";
                string outputTimes = "";
                int i = 0;
               
                foreach (SignalSample value in lst)
                {
                    if (value.time > timeLastSendTcp)
                    {
                        outputValues += value.values.ToString("0.000000") + ",";
                        //outputValues.literal_eval();
                        
                        outputTimes += value.time.ToString("0.000000") + ",";
                        //outputTimes.literal_eval();
                        
                        i++;
                    }
                        
                }


                if (i > 0) 
                { 
                    timeLastSendTcp = time;
                    outputValues = outputValues.Remove(outputValues.Length-1);
                    outputTimes = outputTimes.Remove(outputTimes.Length-1);
                    tcp.SendMessageNoReturn("{\"type\":\"data\", \"values\": [" + outputValues + "], \"times\": [" + outputTimes + "]}");
                    totalCount = lst.Count;
                }
                
                // get data after every 20s
                if (mytask.TimeSinceStart > nextActionTime )
                {
                    nextActionTime += adaptionRate;
                    String s = tcp.SendMessage("{\"type\":\"calc\"}");                   
                    response = JsonUtility.FromJson<ServerResponse>(s);
                }

                if (mytask.TimeSinceStart2 > nextActionTime2 )
                {
                    nextActionTime2 += adaptionRate;
                    String s = tcp.SendMessage("{\"type\":\"calc\"}");                   
                    response = JsonUtility.FromJson<ServerResponse>(s);
                }

                // adaptation in every 20s logged as 6
                

                if (mytask.TimeSinceStart2 > firstadaptationtime2 && mytask.currentBlock == 6)
                {
                    firstadaptationtime2 += adaptionRate;

                    if(response.error == ""){
                    
                        //if(mytask.currentBlock == 6 && response.slopet2 > response.slopet1) 
                        if(mytask.currentBlock == 6 && (response.slopet2 - response.slopet1) >  slopeThreshold)
                        {
                            currentCount = pedestrianSpawner.pedestriansToSpawn;
                            currentCount -= adaptationDown;
                            pedestrianSpawner.pedestriansToSpawn = currentCount;
                            logger.writeAdaption(time, "less",  "",currentCount, response.slopet1, response.slopet2, 6);
                            Debug.Log("Less LIAMS");
                            
                        }
                        //else if(mytask.currentBlock == 6 && response.slopet2 < response.slopet1) 
                        else if(mytask.currentBlock == 6 && (response.slopet2 - response.slopet1) < - slopeThreshold)
                        {
                            currentCount = pedestrianSpawner.pedestriansToSpawn;
                            currentCount += adaptationUp;
                            pedestrianSpawner.pedestriansToSpawn = currentCount;
                            logger.writeAdaption(time, "more", "", currentCount, response.slopet1, response.slopet2, 6); 
                            Debug.Log("More LIAMS");
                            
                        }

                    }       

                }   

                // invere adaptation in every 20s logged as 7
                if (mytask.TimeSinceStart > firstadaptationtime && mytask.currentBlock == 7 )
                {
                    firstadaptationtime += adaptionRate;                                 
                    if(response.error == ""){
                     

                        if(mytask.currentBlock == 7 && (response.slopet2 - response.slopet1)  >  slopeThreshold)
                        //if (mytask.currentBlock == 7 && (response.slopet2/  response.slopet1) < 1 - slopeThreshold) 
                        {   
                            currentCount = pedestrianSpawner.pedestriansToSpawn;
                            currentCount += adaptationUp;
                            pedestrianSpawner.pedestriansToSpawn = currentCount;
                            logger.writeAdaption(time, "more",  "",currentCount, response.slopet1, response.slopet2, 7); 
                            Debug.Log("More LIAMS");
                        
                        }
                        else if(mytask.currentBlock == 7 && (response.slopet2 - response.slopet1)  < - slopeThreshold)
                        //else if(mytask.currentBlock == 7 && (response.slopet2 / response.slopet1) > 1 + slopeThreshold)
                        {
                            currentCount = pedestrianSpawner.pedestriansToSpawn;
                            currentCount -= adaptationDown;
                            pedestrianSpawner.pedestriansToSpawn = currentCount;
                            logger.writeAdaption(time, "less",  "",currentCount, response.slopet1, response.slopet2, 7);
                            Debug.Log("Less LIAMS");
                            
                        }
                    }
                }
                
                
                else 
                    {
                        Debug.LogWarning("Server:" + response.error);
                    }
                   // Debug.Log(tonicEDA + " " + slopeBaseline + " " + (tonicEDA - slopeBaseline) + " " + slopeThreshold);
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

