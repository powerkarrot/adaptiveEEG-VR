import mne
import pandas as pd
from adaptation.utils import *
from adaptation.Settings import *
import pickle

#TODO: rename file to adaptation.py

lst_eeg_values = []   
alpha = None
    
#TODO: move to utils
def calculate_iaf_power(pid, lst_eeg_values = None, baseline=False):
    
    if baseline:   
        raw = make_raw_csv(pid=pid, preprocess=False)
    else:
        raw = samples = np.asarray(lst_eeg_values, dtype=object)
        raw = make_raw_arr(preprocess=False, samples=samples.T) 
        lst_eeg_values = []

    filename = './RunLSL/IAF/pickles/' + str(pid) + '-iaf.pickle'
    with open(filename, 'rb') as handle:
        alpha = pickle.load(handle)
    theta = [alpha[0]-4., alpha[0]-1]
    roi_power_1 = compute_freq_power(raw, alpha[0], alpha[1],  picks=roi1)  #TODO: double check which picks for iaf
    roi_power_2 = compute_freq_power(raw, theta[0], theta[1],  picks=roi2)  #TODO: double check which picks for iaf, check freq, range. ARDA CHANGE HERE
    #print("alpha: ", roi_power_1, "theta: ", roi_power_2)
    return [roi_power_1 ,  roi_power_2]
    
