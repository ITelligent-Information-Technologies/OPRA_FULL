using System;
using System.Collections.Generic;

namespace conTspRL
{
    /// <summary>
    /// Clase para modelo simple de DRL para pruebas OPRA
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string strPathFileNoRlJson = @"D:\vbDll\clsTspRL\TrainCitiesNoRl.csv";
            string strPathFileTest = @"D:\vbDll\clsTspRL\TestCities.csv";
            string strPathFileModelH5 = @"D:\vbDll\clsTspRL\Modelos\modelNoRl.h5";
            string strPathFileModelJson = @"D:\vbDll\clsTspRL\Modelos\modelNoRl.json";
            //TestearPintar();
            testearAplicarRl(strPathFileModelH5, strPathFileModelJson);
            return;
            //     PrepararFicherosDinamicos(strPathFileNoRlJson, strPathFileTest);
            //clsKerasModel.Run(strPathFileTrain, strPathFileTest);
            clsKerasModel.RunRl(strPathFileNoRlJson, strPathFileModelH5, strPathFileModelJson);
        }

        static void testearAplicarRl(string strPathFileModelH5, string strPathFileModelJson)
            {
            clsGenerarTsp cGenerar = new clsGenerarTsp();
            cGenerar.ResolverRL(strPathFileModelH5, strPathFileModelJson, 25,5);
            }

        static void TestearPintar()
        {
            clsGenerarTsp cGenerar = new clsGenerarTsp();
            (List<clsPunto> lstPuntos, List<Int32> lstRecorrido) = cGenerar.ObtenerCiudades(50);
            clsImagenes cImagen = new clsImagenes();
           // cImagen.PintarRecorrido(lstPuntos, lstRecorrido, 600, 600, @"D:\vbDll\clsTspRL\TestCities.jpg", false);
        }
        static void PrepararFicherosDinamicos(string strPathFileTrain, string strPathFileTest)
        {
            clsGenerarTsp cGenerar = new clsGenerarTsp();
            //Train
            Int32 intNumCities = 25;
            Int32 intNumEjemplos =15 ;
            Int32 intNumIteraciones = 1000;
            double dblEpsilon = 0.2;
            Int32 intMuestrearCada = 1;
            Boolean blnFicheroParaRl = true;
            if (System.IO.File.Exists(strPathFileTrain))
                System.IO.File.Delete(strPathFileTrain);
            cGenerar.GenerarTspEpsilonGreedy(intMuestrearCada , blnFicheroParaRl, strPathFileTrain,intNumCities, intNumEjemplos, intNumIteraciones, dblEpsilon);
            //System.IO.File.WriteAllText(strPathFileTrain, strFile);
            //// Test
            //intNumCities = 25;
            //intNumEjemplos = 10000;
            //strFile = cGenerar.GenerarTsp(intNumCities, intNumEjemplos);
            //System.IO.File.WriteAllText(strPathFileTest, strFile);

        }


        static void PrepararFicherosEstaticos(string strPathFileTrain, string strPathFileTest)
        {
            //clsGenerarTsp cGenerar = new clsGenerarTsp();
            ////Train
            //Int32 intNumCities = 25;
            //Int32 intNumEjemplos = 50000;
            //string strFile = cGenerar.GenerarTsp(intNumCities, intNumEjemplos);
            //            System.IO.File.WriteAllText(strPathFileTrain, strFile);
            //// Test
            //intNumCities = 25;
            //intNumEjemplos = 10000;
            //strFile = cGenerar.GenerarTsp(intNumCities, intNumEjemplos);
            //System.IO.File.WriteAllText(strPathFileTest, strFile);

        }
    }
}
