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

    [ReadOnly] public int currencount = 0;

    [SerializeField]
    private tcpClient tcp;
    private SignalSample signalsamp;
    public float tcpDelay = 0.01f;
    private double timeLastSendTcp = 0.0;

    private enum Attention {Internal, External}

    private Attention attention;

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

                        string tmp = String.Join(",", cutValues.Select(p=>p.ToString()).ToArray());

                        string arr = "[" + tmp + "],";
                        outputValues += arr;    
                        i++;
                    }
                }

                if (i > 0) 
                { 

                    timeLastSendTcp = time;
                    outputValues = outputValues.Remove(outputValues.Length-1);
                    tcp.SendMessageNoReturn("{\"type\":\"eeg_data\",\"values\":[" + outputValues + "]}");
                    totalCount = lst.Count;
                }
                
                else if (Time.realtimeSinceStartup > nextActionTime )

                {
                    nextActionTime += adaptionRate;
                    String s = tcp.SendMessage("{\"type\":\"calc_eeg\"}");                   
                    response = JsonUtility.FromJson<ServerAdaptationResponse>(s);
                    //Debug.Log("got response " + response);

                    if(response.error == "")
                    {

                        float percentageDiffExternalAtt = ((response.curroi1 - response.basroi1) / response.basroi1) * 100; 
                        float percentageDiffInternalAtt = ((response.curroi2 - response.basroi2) / response.basroi2) * 100;

                        Debug.Log("percentageDiffExternalAtt " + percentageDiffExternalAtt);
                        Debug.Log("percentageDiffInternalAtt " + percentageDiffInternalAtt);
                        
                        
                        //NOTE: this is just so i don't get confused :D
                        attention = (percentageDiffExternalAtt > percentageDiffInternalAtt) ? Attention.External : Attention.Internal;
                    

                        if(attention == Attention.External) {
                            float percentageDiff = percentageDiffExternalAtt;
                            Debug.Log("EXTERNAL ATTENTION");
                            

                            if (percentageDiff > percentageThreshold)
                            {
                                if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());
                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.basroi1, 4);
                                    Debug.Log("More LIAMS");
                                }

                                if(mytask.currentBlock == 5) 
                                {
                                    //Debug.Log("TASK 5 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "external", currentCount, response.curroi1, response.basroi1, 5); 
                                    Debug.Log("Less LIAMS");
                                }
                            }
                            
                            if (percentageDiff < - percentageThreshold)
                            {
                                if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "external", currentCount, response.curroi1, response.basroi1, 4); 
                                    Debug.Log("Less LIAMS");
                                }
                                if(mytask.currentBlock == 5) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "external", currentCount, response.curroi1, response.basroi1, 5);
                                    Debug.Log("More LIAMS");
                                }
                            } 
                        }  

                         else if(attention == Attention.Internal) {
                        float percentageDiff = percentageDiffInternalAtt;
                            Debug.Log("INTERNAL ATTENTION");

                            
                            if (percentageDiff > percentageThreshold)
                            {
                                if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());
                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "internal", currentCount, response.curroi2, response.basroi2, 4);
                                    Debug.Log("More LIAMS");
                                }

                                if(mytask.currentBlock == 5) 
                                {
                                    //Debug.Log("TASK 5 " + percentageDiff.ToString()  + ">" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "internal", currentCount, response.curroi2, response.basroi2, 5); 
                                    Debug.Log("Less LIAMS");
                                }
                            }
                            
                            if (percentageDiff < - percentageThreshold)
                            {
                                if(mytask.currentBlock == 4) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount -= adaptationUp;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "less", "internal",currentCount, response.curroi2, response.curroi2, 4); 
                                    Debug.Log("Less LIAMS");
                                }
                                if(mytask.currentBlock == 5) 
                                {
                                    //Debug.Log("TASK 4 " + percentageDiff.ToString()  + "< -" + percentageThreshold.ToString());

                                    currentCount = pedestrianSpawner.pedestriansToSpawn;
                                    currentCount += adaptationDown;
                                    pedestrianSpawner.pedestriansToSpawn = currentCount;
                                    logger.writeAdaption(time, "more", "internal", currentCount, response.curroi2, response.curroi2, 5);
                                    Debug.Log("More LIAMS");
                                }
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

