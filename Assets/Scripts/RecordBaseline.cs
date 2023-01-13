using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class RecordBaseline : MonoBehaviour
{
    public DataLogger logger;
    public LSLInput input = null;
    
    public int countForBaselineRecording;

    public double duration = double.NaN;
    public double currentDuration = double.NaN;
    private double timeStart = double.NaN;
    private double timeEnd = double.NaN;


    //public double avgStart = double.NaN;
    //public double avgEnd = double.NaN;
    //public double baselineAverage = double.NaN;

    public double tonic = double.NaN;

    private List<float> valuesStart = new List<float>();
    private List<float> valuesEnd = new List<float>();

    [SerializeField]
    public tcpClient tcp;
    public float tcpDelay = .1f;
    private double timeLastSendTcp = 0.0;

    public Mytask mytask;


    // Start is called before the first frame update
    public void Start()
    {
    
    }

    public void Update() 
    {
        if (timeStart == 0.0)
        {
            return;
        }

        if (timeEnd != 0.0)
        {
            return;
        }

        if (!double.IsNaN(tonic))
        {
            return;
        }

        double time = UnixTime.GetTime();

        // Fetch tonic data
        List<SignalSample1D> lstInput = input.samples;
        List<SignalSample> lst = SignalSample.convertToEDA(lstInput);

        /* List<SignalSample> clone = new List<SignalSample> (lst);
         foreach (var data in lst)
         {
             clone.Add(data.Clone());
         }*/

        string outputStr = "{\"type\":\"data\", \"values\":\"";
        int i = 0;
        foreach (var value in lst)
        {
            if (value.time > timeLastSendTcp)
            {
                outputStr += value.ToString(); // Convert data to appropriate format for server
                i++;
            }
        }
        outputStr += "\" }";

        if (i != 0){
            timeLastSendTcp = time;
            tcp.SendMessageNoReturn(outputStr);
        }

        currentDuration = time - timeStart;
      /*  if ((baselineAverage < currentDuration)  & double.IsNaN(avgStart))
        {
            getEDAStart();
        }

        if ((duration > currentDuration) & (duration - baselineAverage < currentDuration) & double.IsNaN(avgEnd))
        {
            getEDAEnd();
        }
      */

        if ((duration < currentDuration) & (i != 0))
        {
            timeEnd = time;
            //valuesEnd = tcp.SendMessage("{\"type\":\"calc\"}");
            //logger.writeTonic(tonicData, time);

            //tonic = valuesEnd.Average();


            // Fetch data back
            //tonic = await tcp.GetTonicFromServer();

            /*timeEnd = time;
            float avgStart = valuesStart.Average();
            float avgEnd = valuesEnd.Average();
            tonic = (avgStart - avgEnd) / duration * 60.0;*/
        }
    }


    public void startRecoding()
    {
        Debug.Log("Baseline startRecoding");
        timeStart = UnixTime.GetTime();
        timeEnd = 0.0;
        //valuesStart.Clear();
        //valuesEnd.Clear();
    }

    public bool isBaselineRecoringDone()
    {
        if (timeEnd != 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public double getBaselineSlope()
    {
        return tonic;
    }
}
