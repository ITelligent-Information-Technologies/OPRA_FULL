import os
import numpy as np
import torch
from utilidades.load_data import load_model
from utilidades.load_data import leer_fichero

import utilidades.predictor as predictor

# esto es para que no de un error al pintar
os.environ['KMP_DUPLICATE_LIB_OK']='True'


# Carga el modelo
model, _ = load_model('modelo/')
model.eval()  # Put in evaluation mode to not track gradients
# Carga un problema 
path="D:\\PytorchEnVisualStudio\\TSP_problem_data\\tsp_100_1.txt"
num_cities=100
tour_optimal,nodes_coord, tour_optimal_len = leer_fichero(path,num_cities)
#xy = np.random.rand(100, 2)
# Si se utiliza la mejor solucion (muestra=false) o una muestra (muestra=true), el ultimo caso se obtiene mejor resultado pero mas tiempo
muestra=False

# Prepara el modelo para el problema
predictor=predictor.crea_predictor(model,nodes_coord)
# Genera la solucion ciudad a ciudad
sample = False
tour = []
tour_p = []
while(len(tour) < len(nodes_coord)):
    # Calcula las probabilidades
    p = predictor(tour)
    # Obtiene el de mayor probabilidad
    i = np.argmax(p)
    tour.append(i)
    tour_p.append(p)
    
print(tour)


