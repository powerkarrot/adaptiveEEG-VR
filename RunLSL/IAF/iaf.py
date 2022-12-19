#%%

import matplotlib.pyplot as plt
import matplotlib
import mne
import philistine
from IAF.Settings import *
from IAF.utils import *

Path(path).mkdir(parents=True, exist_ok=True)

mne.set_log_level(False)
#mne.cuda.init_cuda(verbose=True)
plt.rcParams.update({'figure.max_open_warning': 0})


matplotlib.use('Agg') # supress plots

#TODO: consider excluding failed channels 
def test_subj_channels(subject):
    test = test_channels(subject[0],subject[1], channels)
    print("Subject", subject , "problematic channels:" , test)

def get_alpha_bands(subject):
    
    eyesopen = make_raw(subject, "01")#.crop(tmin = 4.0, tmax = 116.)
    #eyesclosed = make_raw(subject, "02").crop(tmin = 4.0, tmax = 116.)
    #raws = [eyesopen,eyesclosed]
    raws = [eyesopen] # not actually eyes open, this is a test

    
    #TODO dont do for all channel groups, just have one lul
    for g, grp in enumerate(alpha_ch_groups):   
         
        #picks =  select_channels_picks(raws[0], grp)  
        picks =  select_channels_picks(raws[0], alpha_ch_groups[1])   
        
        bad_channels = test_channels_savgol_iaf(raws[0], picks)
        #TODO: remove bad_channels from picks. if no picks left use std alpha range

        #alpha = philistine.mne.attenuation_iaf([raws[0],raws[1]], picks=picks, savgol='diff', resolution=.1)
        alpha = philistine.mne.savgol_iaf(raws[0], picks=picks, resolution=.1)

        print("ID", subject , "ch_group ", g, alpha )
        return alpha
    
# %%
