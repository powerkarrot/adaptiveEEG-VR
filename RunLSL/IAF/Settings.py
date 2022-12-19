from fooof.bands import Bands

sfreq = 250
path = r'D:\adaptive-eeg - Copy\LogData\IAF'
logpath =  r'D:\adaptive-eeg - Copy\LogData'

#ch_names = ['Fp1','Fz','F3','F7','F9','FC5','FC1','C3','T7','CP5','CP1',
#            'Pz','P3','P7','P9','O1','Oz','O2','P10','P8','P4','CP2','CP6',
#            'T8','C4','Cz','FC2','FC6','F10','F8','F4','Fp2']


ch_names = ['Fp1','Fz','F3','F7','F9','FC5','FC1','C3','T7','CP5','CP1',
            'Pz','P3','P7','P9','O1','Oz','O2','P10','P8','P4','CP2','CP6',
            'T8','C4','Cz','FC2','FC6','F10','F8','F4','Fp2','AF7','AF3','AFz'
            ,'F1','F5','FT7','FC3','C1','C5','TP7','CP3','P1','P5','PO7','PO3','Iz'
            ,'POz','PO4','PO8','P6','P2','CPz','CP4','TP8','C6','C2','FC4','FT8','F6','F2','AF4','AF8']

ch_types = ['eeg'] * 64

channels = ['Fp1','Fz','F3','F7','F9','FC5','FC1','C3','T7','CP5','CP1',
            'Pz','P3','P7','P9','O1','Oz','O2','P10','P8','P4','CP2','CP6',
            'T8','C4','Cz','FC2','FC6','F10','F8','F4','Fp2','AF7','AF3','AFz'
            ,'F1','F5','FT7','FC3','C1','C5','TP7','CP3','P1','P5','PO7','PO3','Iz'
            ,'POz','PO4','PO8','P6','P2','CPz','CP4','TP8','C6','C2','FC4','FT8','F6','F2','AF4','AF8']

alpha_ch_groups = [channels] # otherwise out of bounds error/cant automatically detect upper end of alpha band
alpha_ch_groups = [
                   ['Pz','P3', 'P4','O1','O2', 'Oz'],
                   channels
                   ]


#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
