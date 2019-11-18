import time
import argparse
import pprint as pp
import os

import pandas as pd
import numpy as np
#from concorde.tsp import TSPSolver
import elkai as TSPSolver
from scipy.spatial import distance_matrix

# Prueba realizada libreria concorde con elkai
distM=distance_matrix(nodes_coord,nodes_coord)
solution = TSPSolver.solve_float_matrix(distM)
    