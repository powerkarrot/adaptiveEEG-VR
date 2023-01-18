
from itertools import chain, repeat
from adaptation.Settings import *
import pandas as pd
import mne
import numpy as np
from itertools import compress
from pathlib import Path
from scipy.integrate import simpson


def make_raw_csv(pid=1,preprocess=True):
    print(path)
    
    dfEEG = pd.read_csv(f"{path}\ID{pid}-EEG.csv")

    dfEEG.drop(["TimeLsl", "Time"], axis=1, inplace=True)
    info = mne.create_info(ch_names=ch_names, sfreq=sfreq, ch_types=ch_types)
    info.set_montage('standard_1020',  match_case=False)
    samples = dfEEG.T
    raw = mne.io.RawArray(samples, info)
    
    return raw

def make_raw_arr(preprocess=True, samples=None):
    info = mne.create_info(ch_names=ch_names, sfreq=sfreq, ch_types=ch_types)
    info.set_montage('standard_1020',  match_case=False)
    raw = mne.io.RawArray(samples, info)
    if preprocess:
        raw = preprocess_raw(raw)
    return raw

def preprocess_raw(raw):
    raw.notch_filter(60., n_jobs=2)       
    raw.filter(1., 70., None, fir_design='firwin', n_jobs=2)
    raw.set_eeg_reference('average', projection=True)
    return raw

#TODO: ask francesco if welch or multitaper
def compute_freq_power(raw, fmin, fmax, picks):
    spectrum = raw.compute_psd(method = 'multitaper', fmin=fmin, fmax = fmax, n_jobs=2, picks=picks)
    psds, freqs = spectrum.get_data(return_freqs=True)
    psds_mean= psds.mean(0)
    freq_res = freqs[1] - freqs[0]
    bp = simpson(psds_mean, dx=freq_res)

    return bp

    