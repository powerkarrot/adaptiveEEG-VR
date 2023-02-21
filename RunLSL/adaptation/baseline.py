import mne
import pandas as pd
from adaptation.utils import *
from adaptation.Settings import *
import pickle

#TODO: rename file to adaptation.py

#lst_eeg_values = []   
alpha = None
    
#TODO merge this file with utils
def calculate_iaf_power(pid, values = None, baseline=False):
    
    if baseline:   
        raw = make_raw_csv(pid=pid, preprocess=False)
    else:
        raw = samples = np.asarray(values, dtype=object)
        raw = make_raw_arr(preprocess=False, samples=samples.T) 
        signal_len = raw._data.shape[1] / raw.info['sfreq']
        print('Duration of EEG recording = ', signal_len, 's')           
        values = []

    filename = './RunLSL/IAF/pickles/' + str(pid) + '-iaf.pickle'
    with open(filename, 'rb') as handle:
        alpha = pickle.load(handle)
    theta = [alpha[0]-5., alpha[0]-1.]
    
    roi_power_1 = compute_freq_power(raw, alpha[0], alpha[1],  picks=roi1)  #TODO: double check which picks for iaf
    roi_power_2 = compute_freq_power(raw, theta[0], theta[1],  picks=roi2)  #TODO: double check which picks for iaf, check freq, range. ARDA CHANGE HERE

    return values, [roi_power_1 ,  roi_power_2]
    


