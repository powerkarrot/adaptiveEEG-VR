import mne
import pandas as pd
from adaptation.utils import *
from adaptation.Settings import *
import pickle

    
def calculate_alpha_baseline(pid):

    filename = './RunLSL/IAF/pickles/' + str(pid) + '-iaf.pickle'
    with open(filename, 'rb') as handle:
        alpha = pickle.load(handle)
        
    raw = make_raw(pid)
    return compute_freq_power(raw, alpha[0], alpha[1])