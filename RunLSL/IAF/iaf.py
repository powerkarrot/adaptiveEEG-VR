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

   
def get_alpha_bands(subject):
    
    eyesclosed = make_raw(subject, "01")#.crop(tmin = 4.0, tmax = 116.) #TODO: add crop back!!!
    raws = [eyesclosed] #NOTE: no need for array anymore 
    
    alpha = []
    for g, grp in enumerate(alpha_ch_groups):  
        
        try:
            picks =  select_channels_picks(raws[0], grp)   
            alpha_iaf = philistine.mne.savgol_iaf(raws[0], picks=picks, resolution=.1)
            alpha = [alpha_iaf.AlphaBand[0], alpha_iaf.AlphaBand[1]]
            break
        except Exception as e:
            print("Didn't work ", e)
            
    if not alpha: alpha = [7., 14.] 
        
    print("ID", subject , "ch_group ", g, alpha )
    return alpha


#TODO: consider excluding failed channels 
def test_subj_channels(subject):
    test = test_channels(subject[0],subject[1], channels)
    print("Subject", subject , "problematic channels:" , test)
    
    
    
def get_alpha_bands_old(subject):
    
    eyesopen = make_raw(subject, "01")#.crop(tmin = 4.0, tmax = 116.)
    #eyesclosed = make_raw(subject, "02").crop(tmin = 4.0, tmax = 116.)
    #raws = [eyesopen,eyesclosed]
    raws = [eyesopen] # not actually eyes open, this is a test

    
    #TODO dont do for all channel groups, just have one lul
    for g, grp in enumerate(alpha_ch_groups):   
         
        #picks =  select_channels_picks(raws[0], grp)  
        picks =  select_channels_picks(raws[0], alpha_ch_groups[0])   
        
        bad_channels = test_channels_savgol_iaf(raws[0], picks)
        #TODO: remove bad_channels from picks. if no picks left use std alpha range
        print(bad_channels)

        clean_picks = np.setdiff1d(picks, bad_channels)
        print(clean_picks)

        #alpha = philistine.mne.attenuation_iaf([raws[0],raws[1]], picks=picks, savgol='diff', resolution=.1)
        try:
            #TODO: ask francesco if we manually set upper/lower bands separately or throw everything away
            # i dont think we do, replace with savgol_iaf_test (keep valueError)
            alpha_iaf = philistine.mne.savgol_iaf(raws[0], picks=picks, resolution=.1)
            alpha = [alpha_iaf.AlphaBand[0], alpha_iaf.AlphaBand[1]]
        except Exception as e:
            alpha = [7., 14.]
            

        print("ID", subject , "ch_group ", g, alpha )
        return alpha
    

    
# %%
