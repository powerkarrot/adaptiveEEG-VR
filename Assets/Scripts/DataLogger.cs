using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;

public class DataLogger : MonoBehaviour
{
    public int participantId = 0;

    public string rootFolder = "./LogData/";

    StreamWriter swVisitor;
    StreamWriter swSphere;
    StreamWriter swFeedback;
    StreamWriter swState;
    StreamWriter swFlow;
    StreamWriter swVisitorCount;
    StreamWriter swAdaption;
   
    StreamWriter swEda, swEeg, swEcg, swTonic, swIAF, swBaseline;

    StringBuilder stringbuilderEda = new StringBuilder();
    StringBuilder stringbuilderEeg = new StringBuilder();
    StringBuilder stringbuilderIAF = new StringBuilder();
    StringBuilder stringbuilderBaseline = new StringBuilder();
    StringBuilder stringbuilderEcg = new StringBuilder();
    StringBuilder stringbuilderTonic = new StringBuilder();

    [SerializeField]
    private tcpClient tcp;

    private int countedEda = 0, countedEeg = 0, countedEcg = 0;              //, countedTonic = 0;

    public BlockDesigner blockDesigner;



    // Start is called before the first frame update
    public void Start()
    {
        
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }

        string filepath;
        filepath = rootFolder + "ID" + participantId + "-visitor.csv";

        while (File.Exists(filepath)) {
            participantId++;
            filepath = rootFolder + "ID" + participantId + "-visitor.csv";
        }

        init();

    }

    private void init() {
        // this should happen only once during the runtime
        Debug.Log("Init Files");
        string filepath = rootFolder + "ID" + participantId + "-visitor.csv";


        if (swVisitor == null)
        {
            swVisitor = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swVisitor.WriteLine("Time,Name,Type");
            swVisitor.Flush();
        }

        if (swSphere == null)
        {
            filepath = rootFolder + "ID" + participantId + "-sphere.csv";
            swSphere = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swSphere.WriteLine("Time,Type,Feedback");
            swSphere.Flush();
        }

        if (swFeedback == null)
        {
            filepath = rootFolder + "ID" + participantId + "-feedback.csv";
            swFeedback = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swFeedback.WriteLine("Time,NbackColor,CurrentColors,Trash,IsCorrect,CurrentBlock");
            swFeedback.Flush();
        }

        if (swState == null)
        {
            filepath = rootFolder + "ID" + participantId + "-state.csv";
            swState = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swState.WriteLine("Time,State,BlockNumber,AdaptationStatus,NBackN");
            swState.Flush();
        }

        if (swFlow == null)
        {
            filepath = rootFolder + "ID" + participantId + "-flow.csv";
            swFlow = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swFlow.WriteLine("Time,Name,Shirtcolor,Hair");
            swFlow.Flush();
        }

        if (swEda == null)
        {
            filepath = rootFolder + "ID" + participantId + "-EDA.csv";
            swEda = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swEda.WriteLine("Time,TimeLsl,Value");
            swEda.Flush();
        }

        if (swTonic == null)
        {
            filepath = rootFolder + "ID" + participantId + "-Tonic.csv";
            swTonic = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swTonic.WriteLine("Time,Value");
            swTonic.Flush();
        }

        if (swEeg == null)
        {
            filepath = rootFolder + "ID" + participantId + "-EEG.csv";
            //swEeg = File.CreateText(filepath);
            swEeg = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swEeg.WriteLine("Time,TimeLsl,Fp1,Fz,F3,F7,F9,FC5,FC1,C3,T7,CP5,CP1,Pz,P3,P7,P9,O1,Oz,O2,P10,P8,P4,CP2,CP6,T8,C4,Cz,FC2,FC6,F10,F8,F4,Fp2,AF7,AF3,AFz,F1,F5,FT7,FC3,C1,C5,TP7,CP3,P1,P5,PO7,PO3,Iz,POz,PO4,PO8,P6,P2,CPz,CP4,TP8,C6,C2,FC4,FT8,F6,F2,AF4,AF8");
            swEeg.Flush();
        }

        if (swIAF == null)
        {
            filepath = rootFolder + "IAF/" + "ID" + participantId + "-01-EEG.csv"; //FIXME: at some point pass var here instead of 01
            //swEeg = File.CreateText(filepath);
            swIAF = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swIAF.WriteLine("Time,TimeLsl,Fp1,Fz,F3,F7,F9,FC5,FC1,C3,T7,CP5,CP1,Pz,P3,P7,P9,O1,Oz,O2,P10,P8,P4,CP2,CP6,T8,C4,Cz,FC2,FC6,F10,F8,F4,Fp2,AF7,AF3,AFz,F1,F5,FT7,FC3,C1,C5,TP7,CP3,P1,P5,PO7,PO3,Iz,POz,PO4,PO8,P6,P2,CPz,CP4,TP8,C6,C2,FC4,FT8,F6,F2,AF4,AF8");
            swIAF.Flush();
        }

        if (swBaseline == null)
        {
            filepath = rootFolder + "Baseline/" + "ID" + participantId + "-EEG.csv"; //FIXME: at some point pass var here instead of 01
            //swEeg = File.CreateText(filepath);
            swBaseline = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swBaseline.WriteLine("Time,TimeLsl,Fp1,Fz,F3,F7,F9,FC5,FC1,C3,T7,CP5,CP1,Pz,P3,P7,P9,O1,Oz,O2,P10,P8,P4,CP2,CP6,T8,C4,Cz,FC2,FC6,F10,F8,F4,Fp2,AF7,AF3,AFz,F1,F5,FT7,FC3,C1,C5,TP7,CP3,P1,P5,PO7,PO3,Iz,POz,PO4,PO8,P6,P2,CPz,CP4,TP8,C6,C2,FC4,FT8,F6,F2,AF4,AF8");
            swBaseline.Flush();
        }

        if (swEcg == null)
        {
            filepath = rootFolder + "ID" + participantId + "-ECG.csv";
            swEcg = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swEcg.WriteLine("Time,TimeLsl,Value");
            swEcg.Flush();
        }

        if (swVisitorCount == null)
        {
            filepath = rootFolder + "ID" + participantId + "-visitorCount.csv";
            swVisitorCount = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swVisitorCount.WriteLine("Time,Count,CountActual");
            swVisitorCount.Flush();
        }


        if (swAdaption == null)
        {
            filepath = rootFolder + "ID" + participantId + "-adaptation.csv";
            swAdaption = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swAdaption.WriteLine("Time,Direction,CurrentCount,SlopeT1,SlopeT2,Status");
            swAdaption.Flush();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void writeVisitorClick(double timestamp, string name, string type)
    {
        if (swVisitor == null) {
            init();
        }
        swVisitor.WriteLine(timestamp + "," + name + "," + type);
        swVisitor.Flush();
    }

    public void writeSphereClick(double timestamp, string type)
    {
        if (swSphere == null)
        {
            init();
        }
        swSphere.WriteLine(timestamp + "," + type);
        swSphere.Flush();
    }

    public void writeScore(double timestamp, int nbackColor, int currentColor, string trash, bool isCorrect , int currentBlock)
    {
        if (swFeedback == null)
        {
            init();
        }
        swFeedback.WriteLine(timestamp + "," + nbackColor + "," + currentColor + "," + trash + "," + isCorrect + "," + currentBlock);
        swFeedback.Flush();
    }
   
    public void writeState(double timestamp, string state, int blockNumber, double adaptationStatus, int NBackN )
    {
        if (swState == null)
        {
            init();
        }
        swState.WriteLine(timestamp + "," + state + "," + blockNumber + "," + adaptationStatus + "," + NBackN);
        swState.Flush();
    }


    public void writeFlow(double timestamp, string name, string shirtcolor, string hair)
    {
        if (swFlow == null)
        {
            init();
        }
        swFlow.WriteLine(timestamp + "," + name + "," + shirtcolor + "," + hair);
        swFlow.Flush();
    }

    public void writeVisitorCount(double timestamp, int count, int countActual)
    {
        if (swVisitorCount == null)
        {
            init();
        }
        swVisitorCount.WriteLine(timestamp + "," + count + "," + countActual);
        swVisitorCount.Flush();
    }


    internal void writeAdaption(double timestamp, string direction, int currentCount, double slopet1, double slopet2, int status) //(double timestamp, string direction, int currentCount, float slopeBaseline, double slopeEDA)
    {

        if (swAdaption == null)
        {
            init();
        }
        swAdaption.WriteLine(timestamp  + "," + direction + "," + currentCount + "," + slopet1 + "," + slopet2 + "," + status); //(timestamp + "," + direction + "," + currentCount + "," + slopeBaseline + "," + slopeEDA);
        swAdaption.Flush();
    }

    internal void writeTonic(List<float> values, double time)
    {
        if (swTonic == null)
        {
            init();
        }
        foreach (var value in values)
        {
            swTonic.WriteLine(time + "," + value);
        }
        swTonic.Flush();
    }


    internal void write(string name, SignalSample1D s)
    {
        
        if (swEda == null || swEeg == null || swEcg == null)
        {
            init();
        }
        if (name.ToLower() == "eda")
        {
            stringbuilderEda.AppendFormat("{0},{1},{2}{3}", s.time, s.timeLsl, s.values[32], Environment.NewLine);

            countedEda++;
            if (countedEda % 1000 == 0)
            {
                swEda.WriteLine(stringbuilderEda);
                stringbuilderEda.Clear();
                swEda.Flush();
            }
        }

        else if (name.ToLower() == "eeg")
        {

            if (s.values.Length == 65)
            {

                stringbuilderEeg.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66}{67}", s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);
                
                if(blockDesigner.counter == 0 && !blockDesigner.getIAF)
                {
                    stringbuilderIAF.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66}{67}",s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);

                }
                if(blockDesigner.counter == 2)// && !blockDesigner.getIAF)
                {
                    stringbuilderBaseline.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66}{67}",s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);

                }
            }
            else if (s.values.Length == 64)
            {
                stringbuilderEeg.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65}{66}", s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);
                
                if(blockDesigner.counter == 0 && !blockDesigner.getIAF)
                {
                    //print("Sending tcp at insane rates");
                    //tcp.SendMessageNoReturn("{\"type\":\"iafdata\", \"values\": " + s.values  + "}");

                    //print("Block 1 strinbuilder IAF appending");
                    stringbuilderIAF.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65}{66}",s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);

                    //stringbuilderIAF.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63}", s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63]);
                    //tcp.SendMessageNoReturn("{\"type\":\"iafdata\", \"values\": [" + stringbuilderIAF + "]}");

                }
                if(blockDesigner.counter == 2)// && !blockDesigner.getIAF)
                {
                    stringbuilderBaseline.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65}{66}",s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], s.values[8], s.values[9], s.values[10], s.values[11], s.values[12], s.values[13], s.values[14], s.values[15], s.values[16], s.values[17], s.values[18], s.values[19], s.values[20], s.values[21], s.values[22], s.values[23], s.values[24], s.values[25], s.values[26], s.values[27], s.values[28], s.values[29], s.values[30], s.values[31],s.values[32], s.values[33], s.values[34], s.values[35], s.values[36], s.values[37], s.values[38], s.values[39], s.values[40], s.values[41], s.values[42], s.values[43], s.values[44], s.values[45], s.values[46], s.values[47], s.values[48], s.values[49], s.values[50], s.values[51], s.values[52], s.values[53], s.values[54], s.values[55], s.values[56], s.values[57], s.values[58], s.values[59], s.values[60], s.values[61], s.values[62], s.values[63], Environment.NewLine);

                }
            }

            else {
                throw new NotImplementedException("Your electrode count is not correct please adjust the script");
            }

            countedEeg++;
            if (countedEeg % 1000 == 0)
            {
                
                if(blockDesigner.counter == 0 && !blockDesigner.getIAF)
                {
                    //print("iaf writing line, tcp sending");
                    swIAF.WriteLine(stringbuilderIAF);
                    //tcp.SendMessageNoReturn("{\"type\":\"iafdata\", \"values\": [" + stringbuilderIAF + "]}");
                    stringbuilderIAF.Clear();
                    swIAF.Flush();
                }

                
                if(blockDesigner.counter == 2)// && !blockDesigner.getIAF)
                {
                    //print("iaf writing line, tcp sending");
                    swBaseline.WriteLine(stringbuilderBaseline);
                    //tcp.SendMessageNoReturn("{\"type\":\"iafdata\", \"values\": [" + stringbuilderIAF + "]}");
                    stringbuilderBaseline.Clear();
                    swBaseline.Flush();
                }
                
                if(blockDesigner.getIAF) {
                    //print("close iaf sw");
                    //swIAF.Close();
                }
                
                swEeg.WriteLine(stringbuilderEeg);
                stringbuilderEeg.Clear();
                swEeg.Flush();
            
            }
        }
        else if (name.ToLower() == "ecg")
        {
            if (s.values.Length == 1)
            {
                stringbuilderEcg.AppendFormat("{0},{1},{2}{3}", s.time, s.timeLsl, s.values[0], Environment.NewLine);
            }
            else
            {
                throw new NotImplementedException("Your electrode count is not 1 please ajust the script");
            }
            countedEcg++;
            if (countedEcg % 1000 == 0)
            {
                swEcg.WriteLine(stringbuilderEcg);
                stringbuilderEcg.Clear();
                swEcg.Flush();
            }
        }
        else
        {
            Debug.LogWarning("Logger Data Dropped");
        }
        
    }

    void OnDestroy()
    {
        if (swEda != null)
        {
            swEda.Flush();
        }
        if (swEeg != null)
        {
            swEeg.Flush();
        }
        
        if (swIAF != null)
        {
            swIAF.Flush();
        }
        if (swBaseline != null)
        {
            swBaseline.Flush();
        }
        if (swEcg != null)
        {
            swEcg.Flush();
        }
        if (swTonic != null)
        {
            swTonic.Flush();
        }
    }
}
