import numpy as np
from pathlib import Path, PureWindowsPath
from scipy.spatial.distance import pdist, squareform, euclidean
import util_grafos as grafo
import pandas as pd
import torch
from torch import nn, optim
import torch.nn.functional as F
from torch.utils.data import Dataset, DataLoader, IterableDataset
import OpraIterableDataset as opra_iterable
import NetPrediction as net_prediction
import beamsearch as bsearch


def make_train_step(model, loss_fn, optimizer):
    # Builds function that performs a step in the train loop
    def train_step(x, y):
        # Sets model to TRAIN mode
        model.train()
        # Makes predictions
        yhat = model(x)
        #suma=y.sum(dim=1)
        #suma2=yhat.sum(dim=1)
        #if suma!=50:
        #    print(suma)
        #if suma2!=50:
        #    print(suma2)
       
        # Computes loss
        loss = loss_fn( yhat,y)
        # Computes gradients
        loss.backward()
        # Updates parameters and zeroes gradients
        optimizer.step()
        optimizer.zero_grad()
        # Returns the loss
        return loss.item()
    
    # Returns the function that will be called inside the train loop
    return train_step

torch.manual_seed(42)


# Single-label binary
x = torch.randn(5)
yhat = torch.softmax(x,dim=0)
y = torch.randint(2, (5,), dtype=torch.float)
loss = nn.BCELoss()(yhat, y)


cuenta=0
num_cities=20
batch_size=32 
#data_rnd_20_triangle_train
strFilePath='D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\data_rnd_'+str(num_cities)+'_triangle_train.csv'
#strPathModelForRetraining='D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\modelC_10_24_old.txt'
dataset =opra_iterable. OpraIterableDatasetTriangle(strFilePath, num_cities)
dataloader = DataLoader(dataset, batch_size = batch_size)
torch.manual_seed(42)
device = 'cuda' if torch.cuda.is_available() else 'cpu'
#device='cpu'
print(device)
lr = 0.001
n_epochs = 1000
losses = []
model=net_prediction.NetPredictionTriangle(num_cities).to(device)
loss_fn = nn.BCELoss()
optimizer = optim.Adam(model.parameters(), lr=lr)
train_step = make_train_step(model, loss_fn, optimizer)
#model = torch.load(strPathModelForRetraining)
gran_cuenta=0
for epoch in range(n_epochs):
    cuenta=0
    total_loss=0
    for x_batch, y_batch, z_batch in dataloader:
        # the dataset "lives" in the CPU, so do our mini-batches
        # therefore, we need to send those mini-batches to the
        # device where the model "lives"
        
        
        x_batch = x_batch.to(device)
        y_batch = y_batch.to(device)
        loss = train_step(x_batch, y_batch)
        total_loss=total_loss+loss
        cuenta=cuenta+1
        print(gran_cuenta,epoch, cuenta, "{:.5f}".format(loss*100),"{:.5f}".format((total_loss/cuenta)*100))
        gran_cuenta=gran_cuenta+1
       # print(epoch, cuenta, loss*100)
       
    strFilePath='D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\modelC_'+str(num_cities)+'_'+str(epoch)+'_triangle.txt'
    torch.save(model.state_dict(), strFilePath)
    
print(model.state_dict())
