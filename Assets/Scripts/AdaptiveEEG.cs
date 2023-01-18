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

    public double percentageThreshold;
    //public double offset; 
    // Pedestrian Spawning Variables

    public int minCount ;
    
    public int maxCount ;

    public List<float> slope = new List<float>();

    public ServerAdaptationResponse response;


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
            //Debug.Log(lSLInput.samples[0].values[0]);
            //Debug.Log(lstInput.Count);

            //TODO: suggestion: start script in beginning and then only run this part on blocks 4 and 5 to get rid of the initial delay
            if (lstInput.Count > 0)
            {
                List<SignalSample> lst = SignalSample.convertEEG(lstInput);
            
                string outputValues = "";
                //string outputTimes = "";
                int i = 0;
               
                foreach (SignalSample1D value in lstInput)
                {

                    if (value.time > timeLastSendTcp)
                    {
                        
                        //var cutValues = value.values.Take(20);
                        var cutValues = value.values.Take(64);
                        //string tmp = String.Join(",", value.values.Select(p=>p.ToString("0.000000")).ToArray());
                        //string tmp = String.Join(",", cutValues.Select(p=>p.ToString("0.000000")).ToArray());
                        string tmp = String.Join(",", cutValues.Select(p=>p.ToString()).ToArray());

                        string arr = "[" + tmp + "],";
                        outputValues += arr;
                        //outputTimes += arr; //TODO: use the correct array. or don't use it at all :D
    
                        i++;
                    }
                }

                if (i > 0) 
                { 
                //Debug.Log(outputValues);

                    timeLastSendTcp = time;
                    outputValues = outputValues.Remove(outputValues.Length-1);
                    //outputTimes = outputTimes.Remove(outputTimes.Length-1);
                    //string message = "{\"type\":\"eeg_data\",\"values\":[" + outputValues + "],\"times\":[" + outputTimes + "]}";
                   
                    tcp.SendMessageNoReturn("{\"type\":\"eeg_data\",\"values\":[" + outputValues + "]}");
                totalCount = lst.Count;
                }
                
                
                // get data after every 20s
                //else if (mytask.TimeSinceStart > nextActionTime )
                else if (Time.realtimeSinceStartup > nextActionTime )

                {
                    
                    nextActionTime += adaptionRate;
                    Debug.Log(nextActionTime.ToString() + " , cur: " + Time.realtimeSinceStartup.ToString());
                    Debug.Log("Sending calc_eeg");
                    String s = tcp.SendMessage("{\"type\":\"calc_eeg\"}");                   
                    response = JsonUtility.FromJson<ServerAdaptationResponse>(s);
                    Debug.Log("got response " + response);

                    if(response.error == "")
                    {

                        float percentageDiff = ((response.ratio2 - response.ratio1) / response.ratio1) * 100;
                        Debug.Log("PercentageDiff" + percentageDiff);
                         
                        //TODO task 4 and task 5 are inverted here
                        if(percentageDiff >  percentageThreshold) 
                        {
                            if(mytask.currentBlock == 4) 
                            {
                                Debug.Log("TASK 4 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());
                                currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount -= adaptationDown;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "less", currentCount, response.ratio1, response.ratio2, 6);
                                Debug.Log("Less LIAMS");
                            }

                            if(mytask.currentBlock == 5) 
                            {
                                Debug.Log("TASK 5 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());

                                currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount += adaptationUp;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "more", currentCount, response.ratio1, response.ratio2, 7); 
                                Debug.Log("More LIAMS");
                            }
                        }
                        
                        if (percentageDiff < - percentageThreshold)
                        {
                            if(mytask.currentBlock == 4) 
                            {
                                Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount += adaptationUp;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "more", currentCount, response.ratio1, response.ratio2, 6); 
                                Debug.Log("More LIAMS");
                            }
                            if(mytask.currentBlock == 5) 
                            {
                                Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                currentCount = pedestrianSpawner.pedestriansToSpawn;
                                currentCount -= adaptationDown;
                                pedestrianSpawner.pedestriansToSpawn = currentCount;
                                logger.writeAdaption(time, "less", currentCount, response.ratio1, response.ratio2, 7);
                                Debug.Log("Less LIAMS");
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

