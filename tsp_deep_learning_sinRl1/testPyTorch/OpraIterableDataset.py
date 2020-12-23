import numpy as np
import pandas as pd
import torch
from torch import nn, optim
import torch.nn.functional as F
from torch.utils.data import Dataset, DataLoader, IterableDataset

class OpraIterableDataset(IterableDataset):
    #Ver https://medium.com/swlh/how-to-use-pytorch-dataloaders-to-work-with-enormously-large-text-files-bbd672e955a0
    def __init__(self, filename, num_cities):

        #Store the filename in object's memory
        self.filename = filename
        self.num_cities=num_cities
        self.vector_size=int( num_cities*num_cities)
        #And that's it, we no longer need to store the contents in the memory

    def preprocess(self, text):
        ### Do something with text here
        text_pp = text.lower().strip()
        ###

        return text_pp

    def line_mapper(self, line):
        
        #Splits the line into text and label and applies preprocessing to the text
        values_str = line.split(';')
        cuenta_valores=int(len(values_str)/2)
        values_float = list(map(lambda x: float(x.replace(",", "")), values_str))
        # separa en dos vectores
        tensor=torch.FloatTensor(values_float)
        X=tensor[:self.vector_size] #Matriz de distancias aplanada
        Y=tensor[self.vector_size:(self.vector_size+self.vector_size)] #Matriz de conexiones solucion optima aplanada
        Z=tensor[(self.vector_size+self.vector_size):]     #Matriz de coordenadas aplanada
        return X, Y, Z


    def __iter__(self):

        #Create an iterator
        file_itr = open(self.filename)

        #Map each element using the line_mapper
        mapped_itr = map(self.line_mapper, file_itr)
        
        return mapped_itr


class OpraIterableDatasetTriangle(IterableDataset):
    #Ver https://medium.com/swlh/how-to-use-pytorch-dataloaders-to-work-with-enormously-large-text-files-bbd672e955a0
    def __init__(self, filename, num_cities):

        #Store the filename in object's memory
        self.filename = filename
        self.num_cities=num_cities
        self.vector_size=int( num_cities*(num_cities-1)/2)
        #And that's it, we no longer need to store the contents in the memory

    def preprocess(self, text):
        ### Do something with text here
        text_pp = text.lower().strip()
        ###

        return text_pp

    def line_mapper(self, line):
        
        #Splits the line into text and label and applies preprocessing to the text
        values_str = line.split(';')
        cuenta_valores=int(len(values_str)/2)
        values_float = list(map(lambda x: float(x.replace(",", "")), values_str))
        # separa en dos vectores
        tensor=torch.FloatTensor(values_float)
        X=tensor[:self.vector_size] #Matriz de distancias aplanada
        Y=tensor[self.vector_size:(self.vector_size+self.vector_size)] #Matriz de conexiones solucion optima aplanada
        Z=[]     #Matriz de coordenadas aplanada
        return X, Y, Z


    def __iter__(self):

        #Create an iterator
        file_itr = open(self.filename)

        #Map each element using the line_mapper
        mapped_itr = map(self.line_mapper, file_itr)
        
        return mapped_itr
