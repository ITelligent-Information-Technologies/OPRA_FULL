using System;
using System.Collections.Generic;
using System.Text;
using Keras;
using Keras.Layers;
using Keras.Models;
using Keras.Optimizers;
using Numpy;
using System.IO;


namespace conTspRL
{
    class clsKerasModel
    {
        public static void Run(string strPathFileTrain, string strPathFileTest)
        {
            //Load train data //D:\vbDll\Keras.NET-master\Keras.NET-master\Examples\BasicSamples
            //NDarray dataset = np.loadtxt(fname:strPathFile  @"D:\vbDll\Keras.NET-master\Keras.NET-master\Examples\BasicSamples\pima-indians-diabetes.data.csv", delimiter: ",");
            NDarray dataset = np.loadtxt(fname: strPathFileTrain, delimiter: ";");

            var X = dataset[":,0: 75"];
            var Y = dataset[":,75:"];


            //Build sequential model
            var model = new Sequential();
            model.Add(new Dense(75, input_dim: 75, kernel_initializer: "uniform", activation: "relu"));
            model.Add(new Dense(75, kernel_initializer: "uniform", activation: "relu"));
            model.Add(new Dense(75, kernel_initializer: "uniform", activation: "relu"));
            model.Add(new Dense(25));

            //Compile and train

            model.Compile(optimizer: "adam", loss: "mean_squared_error", metrics: new string[] { "mse", "mae" });
            model.Fit(X, Y, batch_size: 10, epochs: 10, verbose: 1);

            //Evaluate model
            var scores = model.Evaluate(X, Y, verbose: 1);
            Console.WriteLine("Mse: {0}", scores[1]);

            //Save model and weights
            string json = model.ToJson();
            File.WriteAllText("modelNoRl.json", json);
            model.SaveWeight("modelNoRl.h5");
            Console.WriteLine("Saved model to disk");
            //Load model and weight
            var loaded_model = Sequential.ModelFromJson(File.ReadAllText("model.json"));
            loaded_model.LoadWeight("model.h5");
            Console.WriteLine("Loaded model from disk");
            loaded_model.Compile(optimizer: "rmsprop", loss: "mean_squared_error", metrics: new string[] { "mse", "mae" });
            NDarray datasetTest = np.loadtxt(fname: strPathFileTest, delimiter: ";");

            var Xtest = datasetTest[":,0: 75"];
            var Ytest = datasetTest[":,75:"];

            scores = model.Evaluate(Xtest, Ytest, verbose: 1);
            Console.WriteLine("MSE: {0}", scores[1]);

            // Toma el primer vector X e Y y chequea el resutaldo
            var Xpredict = datasetTest["0:1,0:75"];
            var Ypredict = datasetTest["0:1,75:"];
            var Ymodel = model.Predict(Xpredict);
            Console.WriteLine("Model." + Ymodel);
            Console.WriteLine("Predict." + Ypredict);



        }

        public static Int32 AplicarRl(Queue<Int32> queTabu, string strPathFileModelH5, string strPathFileModelJson, string strFirma)
        {
            Int32 intNumCiudades = 25;
            Int32 intLenFirma = 3 * intNumCiudades;
            // Carga el modelo inicial
            var model1 = Sequential.ModelFromJson(File.ReadAllText(strPathFileModelJson));
            model1.LoadWeight(strPathFileModelH5);
            //Console.WriteLine("Loaded model from disk");
            model1.Compile(optimizer: "rmsprop", loss: "mean_squared_error", metrics: new string[] { "mse", "mae" });
            var X = np.fromstring(strFirma, sep: ";").reshape(1, 75);
            // Aplica al modelo a cada uno de los St_1 
            var Ymodel = model1.Predict(X);
            // Lee los valores para ordenar de mayor a menor
            double dblValueMin = double.MaxValue;
            Int32 intIndexMin = -1;
            for (Int32 intI = 0; intI < Ymodel.size; intI++)
            {
                if (!queTabu.Contains(intI))
                {
                    double dblValue = Ymodel["0," + intI].asscalar<double>();
                    if (dblValue < dblValueMin)
                    {
                        dblValueMin = dblValue;
                        intIndexMin = intI;
                    }
                }
            }
            return intIndexMin;

        }

        public static void RunRl(string strPathFileNoRl, string strPathFileModelH5, string strPathFileModelJson)
        {
            Int32 intNumCiudades = 25;
            Int32 intLenFirma = 3 * intNumCiudades;
            // Carga el modelo inicial
            var model1 = Sequential.ModelFromJson(File.ReadAllText(strPathFileModelJson));
            model1.LoadWeight(strPathFileModelH5);
            //Console.WriteLine("Loaded model from disk");
            model1.Compile(optimizer: "rmsprop", loss: "mean_squared_error", metrics: new string[] { "mse", "mae" });
            // Carga el modelo de RL
            var modelRl = new Sequential();
            modelRl.Add(new Dense(75, input_dim: 75, kernel_initializer: "uniform", activation: "relu"));
            modelRl.Add(new Dense(75, kernel_initializer: "uniform", activation: "relu"));
            modelRl.Add(new Dense(75, kernel_initializer: "uniform", activation: "relu"));
            modelRl.Add(new Dense(25));
            modelRl.Compile(optimizer: "adam", loss: "mean_squared_error", metrics: new string[] { "mse", "mae" });
            // Lee el fichero
            NDarray datasetRl = np.loadtxt(fname: strPathFileNoRl, delimiter: ";");
            // Toma una muestra de los datos

            // Recorre la muestra

            // Hace la prediccion utilizando el Modelo1
            np.random.shuffle(datasetRl);
            var St = datasetRl[":,0:" + intLenFirma]; // Estado actual (estado en t)
            var Rta = datasetRl[":," + intLenFirma + ":" + (intNumCiudades + intLenFirma)];// Rewards al aplicar al St una accion a (a0,a1,...an) n:numciudades
            var St_1 = datasetRl[":," + (intLenFirma + intNumCiudades) + ":"];
            Int32 intCuenta = 0;
            Int32 intCuentaGenerarModelo = 1000;
            var Y = np.zeros(intCuentaGenerarModelo, intNumCiudades);
            var X = np.zeros(intCuentaGenerarModelo, 3 * intNumCiudades);
            for (Int32 intIndex = 0; intIndex < St.len; intIndex++)
            {
                //Console.WriteLine(St[intIndex]);
                //Console.WriteLine(Rta[intIndex]);
                // Console.WriteLine(St_1[intIndex]);
                Y[intCuenta] = ObtenerTarget(0.8, model1, intNumCiudades, Rta[intIndex], St_1[intIndex]);
                X[intCuenta] = St[intIndex];
                if (intCuenta >= (intCuentaGenerarModelo - 1))
                {
                    // Entrena el modelo
                    modelRl.Fit(X, Y, batch_size: 10, epochs: 200, verbose: 1);
                    //Evaluate model
                    var scores = modelRl.Evaluate(X, Y, verbose: 1);
                    Console.WriteLine("Mse: {0}", scores[1]);
                    // Guarda el modelo en disco
                    string json = modelRl.ToJson();
                    File.WriteAllText(strPathFileModelJson, json);
                    modelRl.SaveWeight(strPathFileModelH5);
                    // Actualiza el modelo1
                    model1 = Sequential.ModelFromJson(File.ReadAllText(strPathFileModelJson));
                    model1.LoadWeight(strPathFileModelH5);
                    intCuenta = -1;
                }
                intCuenta++;
            }
            modelRl.Fit(X, Y, batch_size: 10, epochs: 10, verbose: 1);
            //var X1 = datasetTest[":,0: 75"];
            //var Y1 = datasetTest[":,75:"];
            //var X1predict = datasetTest["0:1,0:75"];
            //var Y1predict = datasetTest["0:1,75:"];
            //  var Y1model = loaded_model1.Predict(X1predict);

            // 

            //Console.WriteLine("Model." + Y1model);
            //Console.WriteLine("Predict." + Y1predict);

            //Load train data //D:\vbDll\Keras.NET-master\Keras.NET-master\Examples\BasicSamples
            //NDarray dataset = np.loadtxt(fname:strPathFile  @"D:\vbDll\Keras.NET-master\Keras.NET-master\Examples\BasicSamples\pima-indians-diabetes.data.csv", delimiter: ",");
            //NDarray dataset = np.loadtxt(fname: strPathFileTrain, delimiter: ";");




        }

        private static NDarray ObtenerTarget(double dblGamma, BaseModel model, Int32 intNumCities, NDarray Rta, NDarray St_1)
        {
            NDarray target = np.zeros(1, intNumCities);
            // Recorre los numcities St_1 para obtener de cada uno el maximo Q
            for (Int32 intIndex = 0; intIndex < intNumCities; intIndex++)
            {
                Int32 intIni = intIndex * intNumCities;
                Int32 intFin = intIni + intNumCities * 3;
                string strSlice = intIni + ":" + intFin;
                var X = St_1[strSlice].reshape(1, 75);
                // Aplica al modelo a cada uno de los St_1 
                var Ymodel = model.Predict(X);
                // Obtien el maximo de los resultados
                target[0, intIndex] = Rta[intIndex] + dblGamma * np.min(Ymodel);
            }

            return target;
        }

    }
}
