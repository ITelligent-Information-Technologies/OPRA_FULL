"""
Routes and views for the flask application.
"""
from FlaskWebProject1 import app
from flask import request, jsonify
import numpy as np
import os
import time
import utilidades.predictor as pred
from utilidades.load_data import load_model
import json
import traceback


#Precargamos el modelo
app.logger.warning("Cargando modelo")
model, _ = load_model('modelo/')
#model.eval()  # Put in evaluation mode to not track gradients

@app.route('/')
def index():
    return 'Servidor listo'

@app.route('/problem', methods=['POST','GET'])
def addproblem():
    try:
        #Comenzamos a medir el tiempo
        ini = time.time()

        # esto es para que no de un error al pintar
        os.environ['KMP_DUPLICATE_LIB_OK']='True'

        matriz = json.loads(request.json['coordenadas'])

        xy = np.array(matriz)
   
        # Si se utiliza la mejor solucion (muestra=false) o una muestra (muestra=true), el ultimo caso se obtiene mejor resultado pero mas tiempo
        muestra=False
        
        # Prepara el modelo para el problema
        predictor=pred.crea_predictor(model,xy)
        # Genera la solucion ciudad a ciudad
        sample = False
        tour = []
        tour_p = [] #Matriz
        while(len(tour) < len(xy)):
            p = predictor(tour)
    
            if muestra:
                # Advertising the Gumbel-Max trick
                g = -np.log(-np.log(np.random.rand(*p.shape)))
                i = np.argmax(np.log(p) + g)
            else:
                # Greedy
                i = np.argmax(p)  #Devuelve el valor maximo del predictor
            tour.append(int(i))
            tour_p.append(p)
        fin =  time.time()
    
        solution = {"Indices": tour, "Tiempo": fin-ini}
        return jsonify(solution)

    except:
        e = traceback.format_exc()
        app.logger.error(e)