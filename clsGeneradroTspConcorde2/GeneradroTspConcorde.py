import time
import argparse
import pprint as pp
import os

import pandas as pd
import numpy as np
#from concorde.tsp import TSPSolver
import elkai as TSPSolver
from scipy.spatial import distance_matrix

    
numero_ejemplos = 1500
numero_nodos = 100
fichero_salida = "d:/borrar/test.tsp"
set_nodes_coord = np.random.random([numero_ejemplos, numero_nodos,2])
with open(fichero_salida, "w") as f:
        start_time = time.time()
        #Se recorren los ejemplos (descomentar para obtener la solucion optima utilizando concorde)
        for nodes_coord in set_nodes_coord:
            #Obtener la matriz de distancias
            distM=distance_matrix(nodes_coord,nodes_coord)
            solution = TSPSolver.solve_float_matrix(distM)
            #(.from_data(nodes_coord[:,0], nodes_coord[:,1],
            #norm="GEO")
            #solution = solver.solve()
            f.write(" ".join(str(x) + str(" ") + str(y) for x,y in nodes_coord))
            f.write(str(" ") + str('output') + str(" "))
            f.write(str(" ").join(str(node_idx + 1) for node_idx in solution))
            f.write(str(" ") + str(solution[0] + 1) + str(" "))
            f.write("\n")
        end_time = time.time() - start_time
    
print(f"Tiempo: {end_time/3600:.1f}h")
    
