import numpy as np
from pathlib import Path, PureWindowsPath
from scipy.spatial.distance import pdist, squareform, euclidean
import util_grafos as grafo
import pandas as pd
import torch
from torch import nn, optim
import torch.nn.functional as F
from torch.utils.data import Dataset, DataLoader, IterableDataset
import util_grafos as grafo
import OpraIterableDataset as opra_iterable
import NetPrediction as net_prediction
from munkres import Munkres
import bottleneck as bn
import math
import beamsearch as bsearch
import os 
os.environ["KMP_DUPLICATE_LIB_OK"]="TRUE"

""" 
    Modelo basado en la matriz de distancias para la resolucion del TSP basado en aprendizaje supervisado
    este modelo no utiliza embedding de grafos si no directamente las matriz de distancia
    para aprender de ella. Ademas a la salida se monta un algoritmo para mejorar la solucion
"""

def my_inverse_softmax(x, num_cities):
    index = []
    value=[]
    x_len=x.shape[1]
    for i in range(num_cities):
        ini=int(i*num_cities)
        part_tensor=torch.narrow(x,1,ini,num_cities)
        val,id=torch.max(part_tensor,1)
        index.append((i,id.item()))
        value.append(val.item())
    return index, value

def evaluate(matrix_y, pairs):
    cuentaOn=0
    cuentaTotal=0
    matrix=torch.clone(matrix_y)
    results = np.zeros(len(matrix_y[0]) , dtype=int)
    corrects=[]
    for x,y in pairs:
        if(matrix[0][x][y]==1 and matrix[0][y][x]==1):
            cuentaOn=cuentaOn+1
            matrix[0][y][x]=0
            corrects.append((x,y))
    return cuentaOn, corrects

def evaluateCorrectIncorrect(matrix_y, pairs):
    cuentaOn=0
    cuentaTotal=0
    matrix=torch.clone(matrix_y)
    #genera la matriz y_hat
    matrix_yhat=np.zeros((len(matrix_y[0]),len(matrix_y[0])),dtype=int)
    for x,y in pairs:
        matrix_yhat[x][y]=1

    results = np.zeros(len(matrix_y[0]) , dtype=int)
    doubleC=[]
    doubleI=[]
    singleC=[]
    singleI=[]
    other=[]
    for x,y in pairs:
        if(matrix[0][x][y]==1 and matrix_yhat[x][y]==1 and matrix_yhat[y][x]==1):
            cuentaOn=cuentaOn+1
            matrix[0][y][x]=-1
            doubleC.append((x,y))
        elif (matrix[0][x][y]==1 and matrix_yhat[x][y]==1 and matrix_yhat[y][x]==0):
            singleC.append((x,y))
        elif(matrix[0][x][y]==0 and matrix_yhat[x][y]==1 and matrix_yhat[y][x]==1):
            doubleI.append((x,y))
            matrix[0][y][x]=-1
        elif (matrix[0][x][y]==0 and matrix_yhat[x][y]==1 and matrix_yhat[y][x]==0):
            singleI.append((x,y))
        else:
            other.append((x,y))       
    return cuentaOn, corrects

def get_solution(row,col,coordenadas):
    paths=[]
    maxlen=0
    paths, maxlen=generate_path(row,col,0,paths,maxlen)
    paths, maxlen=generate_path(row,col,1,paths,maxlen)
    paths, maxlen=generate_path(col,row,0,paths,maxlen)
    paths, maxlen=generate_path(col,row,1,paths,maxlen)
    # si la solucion obtenida no contiene todo lso puntos la amplia
    if(maxlen<len(row)):
        paths=ampliar_path(paths)
    #calcula el coste de cada una y devuelve el minimo
    return (calcular_coste_min(paths,coordenadas))

def ampliar_path(paths):
    return paths

def calcular_coste_min(paths,coordenadas):
    x,y =grafo.covert_flat_coords_to_coords(coordenadas)
    tourmin=-1
    indexmin=-1
    index=0
    for path in paths:
        oldNode=-1
        tour_len=0        
        for node in path:
            if(oldNode>-1):
                tour_len += math.sqrt((x[node]-x[oldNode])**2+(y[node]-y[oldNode])**2)
            oldNode=node
        if(indexmin==-1 or tour_len<tourmin):
            tourmin=tour_len
            indexmin=index
        index=index+1
    return paths[indexmin],tour_len
def generate_path(row, col, index, paths, maxlen):
   
    for i in range(len(row)):
        colOld=i
        check=np.zeros((len(row)), dtype=int)
        path=[]
        path.append(i)
        blnEnWhile=True
        r=row[i][index]
        rowOld=r
        path.append(r)
        check[r]=1
        while(blnEnWhile):
            c=col[r][0]
            if(c==colOld or c==-1):
                c=col[r][1]
            colOld=c
            if(c==-1 or c==i or check[c]==1):
                blnEnWhile=False
            else:
                path.append(c)
                check[c]=1
                r=row[c][0]
                if(r==rowOld or r==-1):
                    r=row[c][1]
                rowOld=r
                if(r==-1 or r==i or check[r]==1):
                    blnEnWhile=False
                else:
                    path.append(r)
                    check[r]=1        
        if(len(path)>maxlen):
            paths=[]
            paths.append(path)
            maxlen=len(path)
        elif (len(path)==maxlen):
            paths.append(path)
    print(paths)
    print(maxlen)
    return paths, maxlen

def calcular_coste_min(mindicator, mdistance):
    # Calcula el coste a partir de la matriz indicadora (mindicator) pasada
    # y de la matriz de distancias (mdistance)

    # Obtiene los dos mayores indices de cada fila
    index = bn.argpartition(-1*mindicator, kth=1)
    actual=0
    next=index[actual][0]
    distancia=0
    solution=[]
    solution.append(actual)
    solution.append(next)
    distancia=distancia+mdistance[actual][next]
    blnEnBucle=True
    while(blnEnBucle):
        if(index[next][0]==actual):
            actual=next
            next=index[next][1]
        else:
            actual=next
            next=index[next][0]
        distancia=distancia+mdistance[actual][next]
        if(next==0):            
            blnEnBucle=False
        else:
            solution.append(next)

    return distancia, solution

strFilePath='D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\modelC_10_999_triangle.txt'
num_cities=10
batch_size=1
beam_size=10

net = net_prediction.NetPrediction(num_cities)
net.load_state_dict(torch.load(strFilePath))

torch.manual_seed(42)

cuenta=0
strFilePath='D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\data_rnd_10_varios_test.csv'

dataset = opra_iterable.OpraIterableDataset(strFilePath, num_cities)
dataloader = DataLoader(dataset, batch_size = 1)

torch.manual_seed(42)
device = 'cuda' if torch.cuda.is_available() else 'cpu'
#device='cpu'
print(device)
evaluator=np.zeros(num_cities+1, dtype=float)
dif_total=0
cuenta_dif=0
num_cities_2=num_cities*num_cities
# chequear Munkres https://software.clapper.org/munkres/
for x_batch, y_batch, coordenadas in dataloader:
    # Obtiene la matriz de distancia
    matrix_xLimited=x_batch[:,:num_cities_2]
    matrix_x=matrix_xLimited.view(1,num_cities,num_cities)
    mdistance_tmp=matrix_x.detach().numpy()
    mdistance=np.squeeze(mdistance_tmp)
    # Obtiene la matriz de probabilidad
    yhat=net(x_batch) #Obtiene el resultado del modelo
    yhatLimited=yhat[:,:num_cities_2]
    matrix=yhatLimited.view(1,num_cities,num_cities)
    mprobability_tmp=matrix.detach().numpy()
    mprobability=np.squeeze(mprobability_tmp)
     # Obtiene los dos mayores indices de cada fila
    index = bn.argpartition(-1*mprobability, kth=1)
    # Obtiene la matrix indicadoras de la solucion
    indicator_matrix=np.zeros((num_cities,num_cities))
    for i in range(num_cities):
        for j in range(2):
            ind=index[i][j]
            indicator_matrix[i][ind]=1
    # Obtiene las coordenadas
    coordenadas_new=coordenadas[:,:num_cities]
    coordenadas_new2=coordenadas.view(num_cities,2)
    coordenadas_tmp=coordenadas_new2.detach().numpy()
    coord=np.squeeze(coordenadas_tmp)
    coord2=np.array(coord)
    grafo.plot_indicator_matrix_graph(coord2,indicator_matrix)
    # Obtain optimal cost (y_batch is optimal cost)
    matrix_yLimited=y_batch[:,:num_cities_2]
    matrix_y=matrix_yLimited.view(1,num_cities,num_cities)
    moptimal_tmp=matrix_y.detach().numpy()
    moptimal=np.squeeze(moptimal_tmp)
    
