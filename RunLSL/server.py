#%%

import socket
import sys
from xmlrpc.client import DateTime
#from ledapy.deconvolution import sdeconv_analysis
# import cvxEDA as cvx
import numpy as np
import neurokit2 as nk
import time
import pandas as pd
# import keyboard as kb
#from iaf import *
import json
from IAF.iaf import *
from adaptation.baseline import *
import pickle


# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# Bind the socket to the port
server_address = ('localhost', 8052)
print('starting up on %s port %s' % server_address)
sock.bind(server_address)

# Listen for incoming connections
sock.listen(1)

#lstDataValues = []
lstDataTimes = []
lstDataTonicStart = []
lstDataTonicEnd = []

lst_eeg_pws = []
lst_eeg_values = []   
pws_baseline = [] #TODO: overwrite this every 20 secs

def current_milli_time():
    return round(time.time() * 1000)

while True:
    # Wait for a connection
    print('waiting for a connection')
    connection, client_address = sock.accept()

    #tonic_old = []

    try:
        print(f'connection from {0}', client_address)

        # Receive the data in small chunks and retransmit it
        while True:
            total_data=[]
            while True:
                data = connection.recv(4096*8*8*8*8).decode("utf-8")
                #if not data: break
                #total_data.extend(data)
   
                try:
                    obj = json.loads(data)
                    
                    if (obj["type"] == "data"):
                        lstDataValues.extend(obj["values"])
                        lstDataTimes.extend(obj["times"])
                        print(len(lstDataValues))
                        
                    elif (obj["type"] == "eeg_data"):
                        lst_eeg_values.extend(obj["values"])
                        #print(len(lst_eeg_values))

                        
                    elif (obj["type"] == "iaf"):
                        print("DO IAF")
                        curId = obj["values"]
                        alpha = get_alpha_bands(curId)
                        data = {"lowerAlpha":alpha[0],"upperAlpha":alpha[1], "iafDone":1, "error":""}
                        signals_data = json.dumps(data) 
                        connection.sendall(signals_data.encode("utf-8"))
                       
                        filename = './RunLSL/IAF/pickles/' + str(curId) + '-iaf.pickle'
                        with open(filename, 'wb') as handle:
                            pickle.dump(alpha, handle, protocol=pickle.HIGHEST_PROTOCOL)
                            
                    #TODO: change here for baseline
                    elif (obj["type"] == "alphapow_baseline"):
                        curId = obj["values"]
                        

                        lst_eeg_values , pws_baseline = calculate_iaf_power(curId, lst_eeg_values, baseline=False)
                        #filename = './RunLSL/adaptation/pickles/' + str(curId) + '-baseline.pickle'
                        #with open(filename, 'wb') as handle:
                        #    pickle.dump(pws_baseline, handle, protocol=pickle.HIGHEST_PROTOCOL)

                        print("baseline is " , str(pws_baseline))
                        data = {"baselineDone":1, "error":""}
                        signals_data = json.dumps(data) 
                        connection.sendall(signals_data.encode("utf-8"))
                        
                       
                    elif (obj["type"] == "calc_eeg"):
                        #print("calculating pws for adaptation")
                        #print("server 1: ", len(lst_eeg_values))
                        lst_eeg_values, cur = calculate_iaf_power(curId, lst_eeg_values, baseline=False)
                        #print("server 2: ", len(lst_eeg_values))

                        filename = './RunLSL/adaptation/pickles/' + str(curId) + '-baseline.pickle'
                        #with open(filename, 'rb') as handle:
                        #    baseline = pickle.load(handle)
                            
                        
                        signals_data = json.dumps({"curroi1":cur[0], "curroi2":cur[1],"basroi1":pws_baseline[0], "basroi2":pws_baseline[1],"error":""})
                        
                        pws_baseline = cur # TODO: double check flow

                        connection.sendall(signals_data.encode("utf-8"))
                                            
                    elif (obj["type"] == "calc"):
                    
                        span = 5*30
                        # Only keep the last span second of data
                        values = np.array(lstDataValues)
                        times = np.array(lstDataTimes)
                        t1 = len(times)
                        cTime = current_milli_time()/1000
                        lim = cTime - (span * 1000)
                        print(times.max(), lim)
                        values = values[times > lim]
                        times = times[times > lim]
                        t2 = len(times)
                        lstDataValues = lstDataValues[-len(times):]
                        lstDataTimes = lstDataTimes[-len(times):]
                        print(t1, t2)
                        #print(values)
                        if len(values) != 0:
                        
                            times = times - times.max()
                            
                            # Process it
                            sampling_rate = 250
                            signals, info =  nk.eda_process(values, sampling_rate=sampling_rate)

                            t = np.array(list(range(len(signals))))/sampling_rate
                            t = t - t.max()
                            signals["Time"] = t
                            print(t)

                            if len(t[t < -200]) > 0:
                                t1Tonic = signals[(signals.Time  <= -180) & (signals.Time  >= -200)].EDA_Tonic.mean()
                                t2Tonic = signals[(signals.Time  <= -20) & (signals.Time  >= -40)].EDA_Tonic.mean()
                                t3Tonic = signals[signals.Time >= -20].EDA_Tonic.mean()

                                t1t3 = 180 # Sec
                                t2t3 = 20 # Sec
                                slope3min = (t3Tonic - t1Tonic) / t1t3
                                slope1min = (t3Tonic - t2Tonic) / t2t3


                                # Visualise the processing
                                # nk.eda_plot(signals, sampling_rate=1000)
                                # average = np.average(signals['EDA_Tonic'])
                                
                                
                                data = {"slopet1":slope3min, "slopet2":slope1min, "error":""}
                                signals_data = json.dumps(data) 
                                connection.sendall(signals_data.encode("utf-8"))
                            else:
                                signals_data = json.dumps({"slopet1":0, "slopet2":0,"error":"not ready"}) 
                                connection.sendall(signals_data.encode("utf-8"))
                         
                        else:
                            signals_data = json.dumps({"slopet1":0, "slopet2":0,"error":"no data"}) 
                            connection.sendall(signals_data.encode("utf-8"))

                except Exception as e:       
                    #f = open("error.txt", "a")
                    #f.write(e)
                    #f.close()  
                    print("Exception:", e)          
            
    finally:
        # Clean up the connection
        connection.close()
        
        
        