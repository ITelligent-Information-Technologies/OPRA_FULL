import torch.nn.functional as F
import torch
from torch import nn, optim

class NetPrediction(nn.Module):
    #https://discuss.pytorch.org/t/a-model-with-multiple-outputs/10440/27
    #https://discuss.pytorch.org/t/apply-softmax-over-multiple-segments-of-output/95287/4
    #https://discuss.pytorch.org/t/cant-apply-nn-softmax-along-each-dimension-or-different-parts-of-tensor/97856/2
    # CNN: https://towardsdatascience.com/pytorch-basics-how-to-train-your-neural-net-intro-to-cnn-26a14c2ea29
  def __init__(self, number_cities):
     
    super(NetPrediction, self).__init__()
    n_features=int(number_cities*number_cities)
    self.number_cities=number_cities
    self.fc1 = nn.Linear(n_features, n_features)
    self.fc2 = nn.Linear(n_features, n_features)
    self.fc3 = nn.Linear(n_features, n_features)
    self.fc4 = nn.Linear(n_features, n_features)
    
  def forward(self, x):
    x = F.relu(self.fc1(x))
    x = F.relu(self.fc2(x))
    x = F.relu(self.fc3(x))
    output= torch.sigmoid( self.fc4(x))
    #output=my_softmaxDouble(x,self.number_cities)
    #output=my_softmax(x, self.number_cities)
    #output=F.sigmoid(x)
    return output

class NetPredictionTriangle(nn.Module):
    #https://discuss.pytorch.org/t/a-model-with-multiple-outputs/10440/27
    #https://discuss.pytorch.org/t/apply-softmax-over-multiple-segments-of-output/95287/4
    #https://discuss.pytorch.org/t/cant-apply-nn-softmax-along-each-dimension-or-different-parts-of-tensor/97856/2
    # CNN: https://towardsdatascience.com/pytorch-basics-how-to-train-your-neural-net-intro-to-cnn-26a14c2ea29
  def __init__(self, number_cities):
     
    super(NetPredictionTriangle, self).__init__()
    n_features=int(number_cities*(number_cities-1)/2)
    self.number_cities=number_cities
    self.fc1 = nn.Linear(n_features, n_features)
    self.fc2 = nn.Linear(n_features, n_features)
    self.fc3 = nn.Linear(n_features, n_features)
    self.fc4 = nn.Linear(n_features, n_features)
    
  def forward(self, x):
    x = F.relu(self.fc1(x))
    x = F.relu(self.fc2(x))
    x = F.relu(self.fc3(x))
    x = self.fc4(x)
    output=F.softmax(x)
    return output

def my_softmax( x, num_cities):
    a = []
    x_len=x.shape[1]
    vector_len=int(x_len/num_cities)
    for i in range(num_cities):
        ini=int(i*vector_len)
        part_tensor=torch.narrow(x,1,ini,vector_len)
        sof=F.softmax(part_tensor)
        a.append(sof)
    output=torch.cat(a, dim=1)
    return output

def my_softmaxDouble( x, num_cities):
    number_in_batch=x.shape[0]
    x=x.view(number_in_batch,num_cities,num_cities)
    # realiza la normalizacion softmax en cada linea
    y1 = torch.softmax(x, dim=2)
    z1=y1.flatten(start_dim=1)
    # realiza la normalizacion softmax en cada columna
    y2 = torch.softmax(x, dim=1)
    z2=y2.flatten(start_dim=1)
    output=torch.cat((z1,z2),1)
    return output


