using System;
using System.Collections.Generic;
using System.Text;

namespace conTspRL
{
    public class clsPunto
    {
        public double dblX; // Coordenada X
        public double dblY; // Coordenadad Y
    }
    public class clsTresValores
    {
        public Int32 intMenos1;
        public Int32 intMas1;
        public Int32 intMas2;
    }

    class clsGenerarTsp
    {
        private Random _rnd = new Random(100);

        public void ResolverRL(string strPathFileModelH5, string strPathFileModelJson, Int32 intNumCities, Int32 intMaxTabo)
        {
            Queue<Int32> queTabu = new Queue<int>();
            clsImagenes cImg = new clsImagenes();

            (List<clsPunto> lstCiudades, List<Int32> lstRecorrido) = ObtenerCiudades(intNumCities);
            // Obtiene la matriz de distancia para las ciudades
            double[][] dblDistancias = ObtenerMatrizDistancia(lstCiudades);
            // Guarda una imagen solucion inicial
            cImg.PintarRecorrido(lstCiudades, lstRecorrido, dblDistancias, 600, 600, @"D:\vbDll\clsTspRL\Imagenes\ResolverRL_A.jpg", false);
            for (Int32 intI = 0; intI < 1000; intI++)
            {
                // Genera las iteraciones para la ciudad
                (string strFirma, string strIncremento, Int32 intIndexMin, double dblIncrementoMin) = ObtenerFirmaIncremento(lstCiudades, dblDistancias, lstRecorrido);
                Int32 intIndexMinKeras= clsKerasModel.AplicarRl(queTabu, strPathFileModelH5, strPathFileModelJson, strFirma);
                // Obtiene el indice del menor valor
                queTabu.Enqueue(intIndexMinKeras);
                if (queTabu.Count > intMaxTabo)
                    queTabu.Dequeue();
                RealizaMovimiento(intIndexMinKeras, lstRecorrido);
                double dblCosteAntes = ObtenerCoste(dblDistancias, lstRecorrido);
                Console.WriteLine(intIndexMinKeras + "->"+ dblCosteAntes);
            }
            // Guarda una imagen solucion final
            cImg.PintarRecorrido(lstCiudades, lstRecorrido, dblDistancias, 600, 600, @"D:\vbDll\clsTspRL\Imagenes\ResolverRL_B.jpg", false);

        }

        public void GenerarTspEpsilonGreedy(Int32 intMuestrearCada, Boolean blnFicheroParaRl, string strPathFile, Int32 intNumCities, Int32 intNumEjemplos, Int32 intNumIteraciones, double dblEpsilon)
        {
            clsImagenes cImg = new clsImagenes();
            StringBuilder sbCsv = new StringBuilder();
            HashSet<string> hsFrimas = new HashSet<string>();
            string strFichero = "";
            for (Int32 intEjemplos = 0; intEjemplos < intNumEjemplos; intEjemplos++)
            {
                // Obtiene las ciudades
                (List<clsPunto> lstCiudades, List<Int32> lstRecorrido) = ObtenerCiudades(intNumCities);
                // Obtiene la matriz de distancia para las ciudades
                double[][] dblDistancias = ObtenerMatrizDistancia(lstCiudades);
                // Guarda una imagen solucion inicial
                cImg.PintarRecorrido(lstCiudades, lstRecorrido, dblDistancias, 600, 600, @"D:\vbDll\clsTspRL\Imagenes\" + intEjemplos + "_A.jpg", false);
                // Genera las iteraciones para la ciudad
                strFichero = "";
                (lstRecorrido, strFichero) = GenerarIteracionesCiudad(intMuestrearCada, blnFicheroParaRl, lstCiudades, dblDistancias, lstRecorrido, intNumIteraciones, dblEpsilon);
                // Guarda una imagen solucion final
                cImg.PintarRecorrido(lstCiudades, lstRecorrido, dblDistancias, 600, 600, @"D:\vbDll\clsTspRL\Imagenes\" + intEjemplos + "_B.jpg", false);
                if (intEjemplos == 0)
                    System.IO.File.AppendAllText(strPathFile, strFichero);
                else
                    System.IO.File.AppendAllText(strPathFile, Environment.NewLine + strFichero);
            }// Fin intNumEJemplos
        }

        /// <summary>
        /// obtiene una lista con las coordenadas de las ciudades
        /// </summary>
        /// <param name="intNumCities"></param>
        /// <returns></returns>
        public (List<clsPunto>, List<Int32>) ObtenerCiudades(Int32 intNumCities)
        {
            List<clsPunto> lstCiudades = new List<clsPunto>();
            List<Int32> lstRecorrido = new List<int>();
            // Genera las coordenadas de las ciudades
            for (Int32 intI = 0; intI < intNumCities; intI++)
            {
                lstRecorrido.Add(intI);
                clsPunto cPunto = new clsPunto();
                cPunto.dblX = _rnd.NextDouble();// / Math.Sqrt(2));
                cPunto.dblY = _rnd.NextDouble();// / Math.Sqrt(2));
                lstCiudades.Add(cPunto);
            }
            return (lstCiudades, lstRecorrido);
        }

        /// <summary>
        /// Esta funcion recibe una lista de ciudades y obtiene
        /// la matriz de distancias entre ellas
        /// 
        /// </summary>
        /// <param name="lstCiudades"></param>
        /// <returns></returns>
        private double[][] ObtenerMatrizDistancia(List<clsPunto> lstCiudades, Boolean blnNormalizarMatriz = true)
        {
            double dblValorBase = 100;
            Int32 intNumCities = lstCiudades.Count;
            double dblDistanciaMaxima = -1;
            // Matriz distancia
            double[][] dblDistancia = new double[intNumCities][];
            for (Int32 intI = 0; intI < lstCiudades.Count; intI++)
            {
                dblDistancia[intI] = new double[intNumCities];
                // Calcula la distancia con las ciudades anteriores
                for (Int32 intJ = 0; intJ < intI; intJ++)
                {
                    dblDistancia[intI][intJ] = DistanciaDosCiudades(lstCiudades[intI], lstCiudades[intJ]);
                    dblDistancia[intJ][intI] = dblDistancia[intI][intJ];
                    if (dblDistancia[intI][intJ] > dblDistanciaMaxima)
                        dblDistanciaMaxima = dblDistancia[intI][intJ];
                }
            }
            // Si hay que normalizar la matriz
            if (blnNormalizarMatriz)
            {
                for (Int32 intI = 0; intI < lstCiudades.Count; intI++)
                {
                    for (Int32 intJ = 0; intJ < intI; intJ++)
                    {
                        dblDistancia[intI][intJ] = dblValorBase * dblDistancia[intI][intJ] / dblDistanciaMaxima;
                        dblDistancia[intJ][intI] = dblDistancia[intI][intJ];

                    }
                }
            }
            return dblDistancia;
        }

        private (List<Int32> lstRecorridoMin, string strFichero) GenerarIteracionesCiudad(Int32 intMuestrearCada, Boolean blnFicheroParaRl, List<clsPunto> lstCiudades, double[][] dblDistancias, List<Int32> lstRecorrido, Int32 intNumIteraciones, double dblEpsilon)
        {
            StringBuilder sbFichero = new StringBuilder();
            HashSet<string> hsSolucionesProbadas = new HashSet<string>();
            double dblCosteMin = double.MaxValue;
            List<Int32> lstRecorridoMin = new List<int>();
            // Obtiene los datos para el recorrido actual
            (string strFirma, string strIncremento, Int32 intIndexMin, double dblIncrementoMin) = ObtenerFirmaIncremento(lstCiudades, dblDistancias, lstRecorrido);
            // Realiza las iteraciones
            Int32 intMuestrearCuenta = 0;
            for (Int32 intIteracion = 0; intIteracion < intNumIteraciones; intIteracion++)
            {
                // Si es epsilon toma uno al azar si no toma intIndexMin
                if (_rnd.NextDouble() < dblEpsilon)
                {
                    // Toma uno al azar
                    intIndexMin = _rnd.Next(0, lstRecorrido.Count - 1);
                }
                // Calcula el coste antes
                double dblCosteAntes = ObtenerCoste(dblDistancias, lstRecorrido);
                // Obtiene la firma antes del cambio
                string strFirmaAntes = strFirma;
                // Obtiene todas las firmas de todos los cambios posibles
                string strFirmasTodas = FirmasDeTodasLasAcciones(lstCiudades, dblDistancias, dblCosteAntes, lstRecorrido);
                // Realiza el cambio
                RealizaMovimiento(intIndexMin, lstRecorrido);
                string strRecorrido = string.Join(";", lstRecorrido.ToArray());
                // Si lo contiene deshace el cambio y toma uno al azar
                if (hsSolucionesProbadas.Contains(strRecorrido))
                {
                    RealizaMovimiento(intIndexMin, lstRecorrido);
                    intIndexMin = _rnd.Next(0, lstRecorrido.Count - 1);
                    RealizaMovimiento(intIndexMin, lstRecorrido);
                    strRecorrido = string.Join(";", lstRecorrido.ToArray());
                    Console.WriteLine("******************");
                }
                if (!hsSolucionesProbadas.Contains(strRecorrido))
                    hsSolucionesProbadas.Add(strRecorrido);
                // Calcula el coste tras el cambio
                double dblCosteDespues = ObtenerCoste(dblDistancias, lstRecorrido);
                if (dblCosteDespues < dblCosteMin)
                {
                    dblCosteMin = dblCosteDespues;
                    lstRecorridoMin = new List<Int32>(lstRecorrido);
                }
                // Obtiene los datos tras el cambio
                Int32 intIndexMinOld = intIndexMin;
                (strFirma, strIncremento, intIndexMin, dblIncrementoMin) = ObtenerFirmaIncremento(lstCiudades, dblDistancias, lstRecorrido);
                // Obtener firma despues
                string strFirmaDespues = strFirma;
                Console.WriteLine(dblCosteMin);
                // Guarda para el fichero para RL
                intMuestrearCuenta++;
                if (intMuestrearCuenta >= intMuestrearCada)
                {
                    if (blnFicheroParaRl)
                        sbFichero.Append(strFirmasTodas + Environment.NewLine);// strFirmaAntes + ";" + intIndexMinOld + ";" + (dblCosteDespues - dblCosteAntes).ToString().Replace(',', '.') + strFirmaDespues + Environment.NewLine);
                    else
                        sbFichero.Append(strFirma + ";" + strIncremento + Environment.NewLine);
                    intMuestrearCuenta = 0;
                }
            }
            return (lstRecorridoMin, sbFichero.ToString().Trim());
        }

        /// <summary>
        /// Esta clase parte de un recorrido (St) y obtiene to
        /// </summary>
        /// <param name="lstCiudades"></param>
        /// <param name="dblDistancias"></param>
        /// <param name="dblCosteRecorridoInicial"></param>
        /// <param name="lstRecorridoInicial"></param>
        /// <returns></returns>
        private string FirmasDeTodasLasAcciones(List<clsPunto> lstCiudades, double[][] dblDistancias, double dblCosteRecorridoInicial, List<Int32> lstRecorridoInicial)
        {
            // Del recorrido original solo nos interesan la firma e incremento
            (string strFirmaOriginal, string strIncrementoOriginal, Int32 intIndexMinOriginal, double dblIncrementoMinOriginal) = ObtenerFirmaIncremento(lstCiudades, dblDistancias, lstRecorridoInicial);
            StringBuilder sbFirmas = new StringBuilder();
            for (Int32 intIndex = 0; intIndex < lstRecorridoInicial.Count; intIndex++)
            {
                // Copia el recorrido para actuar siempre subre una copia
                List<Int32> lstRecorridoTmp = new List<Int32>(lstRecorridoInicial);
                RealizaMovimiento(intIndex, lstRecorridoTmp);
                (string strFirma, string strIncremento, Int32 intIndexMin, double dblIncrementoMin) = ObtenerFirmaIncremento(lstCiudades, dblDistancias, lstRecorridoTmp);
                if (sbFirmas.Length != 0)
                    sbFirmas.Append(";");
                sbFirmas.Append(strFirma);
            }
            // Devuelve la firma original (St) + los incremetnos de la firma original + las firmas de los nuevos estado para cada movimiento (St+1, a0...an)
            return strFirmaOriginal + ";" + strIncrementoOriginal + ";" + sbFirmas.ToString().Replace(',', '.');
        }


        private void RealizaMovimiento(Int32 intIndexMin, List<Int32> lstRecorrido)
        {
            Int32 intValueTmp = lstRecorrido[intIndexMin];
            if ((intIndexMin + 1) >= lstRecorrido.Count)
            {
                lstRecorrido[intIndexMin] = lstRecorrido[0];
                lstRecorrido[0] = intValueTmp;
            }
            else
            {
                lstRecorrido[intIndexMin] = lstRecorrido[intIndexMin + 1];
                lstRecorrido[intIndexMin + 1] = intValueTmp;
            }
        }


        private double ObtenerCoste(double[][] dblDistancias, List<Int32> lstRecorrido)
        {
            double dblCoste = 0;
            Int32 intIndexOld = 0;
            for (Int32 intIndex = 1; intIndex < lstRecorrido.Count; intIndex++)
            {
                dblCoste = dblCoste + dblDistancias[lstRecorrido[intIndexOld]][lstRecorrido[intIndex]];
                intIndexOld = intIndex;
            }
            dblCoste = dblCoste + dblDistancias[lstRecorrido[lstRecorrido.Count - 1]][lstRecorrido[0]];
            return dblCoste;
        }

        private (string strFirma, string strIncremento, Int32 intIndexMin, double dblIncrementoMin) ObtenerFirmaIncremento(List<clsPunto> lstCiudades, double[][] dblDistancias, List<Int32> lstRecorrido)
        {
            Int32 intIndexMin = -1;
            double dblIncrementoMin = double.MaxValue;
            // Calcula el coste de cada movimiento
            StringBuilder sbFirma = new StringBuilder();
            StringBuilder sbIncremento = new StringBuilder();
            List<clsTresValores> lstTresValores = new List<clsTresValores>();
            for (Int32 intIndex = 0; intIndex < lstRecorrido.Count; intIndex++)
            {
                Int32 intMas1 = -1;
                Int32 intMenos1 = -1;
                Int32 intMas2 = -1;
                Int32 intI = lstRecorrido[intIndex];
                clsTresValores cTres = new clsTresValores();
                // Es el primero
                if (intIndex == 0)
                    intMenos1 = lstRecorrido[lstRecorrido.Count - 1];
                else
                    intMenos1 = lstRecorrido[intIndex - 1];
                // Es el ultimo
                if (intIndex == (lstRecorrido.Count - 1))
                {
                    intMas1 = lstRecorrido[0];
                    intMas2 = lstRecorrido[2];
                }
                else
                {
                    intMas1 = lstRecorrido[intIndex + 1];
                    if (intIndex == (lstRecorrido.Count - 2))
                        intMas2 = lstRecorrido[0];
                    else
                        intMas2 = lstRecorrido[intIndex + 2];
                }
                // Firma
                string strFirma = dblDistancias[intI][intMenos1] + ";" + dblDistancias[intI][intMas1] + ";" + dblDistancias[intI][intMas2];
                if (sbFirma.Length > 0)
                    sbFirma.Append(";");
                sbFirma.Append(strFirma);

                // Calcula el incremento
                //  Di-1,i+1 + Di,i+2 - Di-1,i -Di+1,i+2  
                double dblIncremento = dblDistancias[intMenos1][intMas1] + dblDistancias[intI][intMas2] - dblDistancias[intMenos1][intI] - dblDistancias[intMas1][intMas2];
                if (sbIncremento.Length > 0)
                    sbIncremento.Append(";");
                sbIncremento.Append(dblIncremento);
                if (dblIncremento < dblIncrementoMin)
                {
                    dblIncrementoMin = dblIncremento;
                    intIndexMin = intIndex;
                }
            }
            return (sbFirma.ToString().Replace(',', '.'), sbIncremento.ToString().Replace(',', '.'), intIndexMin, dblIncrementoMin);
        }


        private double DistanciaDosCiudades(clsPunto tpCity1, clsPunto tpCity2)
        {
            return (Math.Sqrt((tpCity1.dblX - tpCity2.dblX) * (tpCity1.dblX - tpCity2.dblX) + (tpCity1.dblY - tpCity2.dblY) * (tpCity1.dblY - tpCity2.dblY)));
        }
    }
}
