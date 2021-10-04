using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using opra.itelligent.es.Models;
using opra.itelligent.es.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Services
{
    public class VehicleRoutingService
    {
        private readonly IConfiguration _conf;
        private readonly BdOpraContext _context;
        private readonly IAmazonS3 _s3Client;

        public VehicleRoutingService(IConfiguration conf, BdOpraContext context, IAmazonS3 s3Client)
        {
            _conf = conf;
            _context = context;
            _s3Client = s3Client;
        }

        public async Task<List<PointData>> ObtenerCoordenadas(int idProblema)
        {
            TblMaestraProblemaTsp datosProblema = _context.TblMaestraProblemaTsp.FirstOrDefault(x => x.IntId == idProblema);

            return await ObtenerCoordenadas(datosProblema);
        }

        public async Task<List<PointData>> ObtenerCoordenadas(TblMaestraProblemaTsp datosProblema)
        {
            double bottomLeftLng = _conf.GetValue<double>("Coordenadas:bottomLeftLng");
            double bottomLeftLat = _conf.GetValue<double>("Coordenadas:bottomLeftLat");
            double upperRightLng = _conf.GetValue<double>("Coordenadas:upperRightLng");
            double upperRightLat = _conf.GetValue<double>("Coordenadas:upperRightLat");

            double distanceX = Math.Abs(upperRightLng - bottomLeftLng);
            double distanceY = Math.Abs(upperRightLat - bottomLeftLat);

            using (GetObjectResponse responseS3 = await _s3Client.GetObjectAsync(datosProblema.StrBucketS3, datosProblema.StrKeyS3))
            using (Stream stream = responseS3.ResponseStream)
            using (StreamReader reader = new StreamReader(stream))
            {
                string[] data = reader.ReadToEnd().Split("output");
                string[] rawPoints = data[0].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                List<PointData> puntos = new List<PointData>();

                for (int i = 0; i < rawPoints.Length; i += 2)
                {
                    int index = i / 2;
                    double x = double.Parse(rawPoints[i], CultureInfo.InvariantCulture);
                    double y = double.Parse(rawPoints[i + 1], CultureInfo.InvariantCulture);

                    PointData pd = new PointData
                    {
                        nombre = $"Punto {index + 1}",
                        lng = bottomLeftLng + distanceX * x,
                        lat = bottomLeftLat + distanceY * y,
                        x = x,
                        y = y
                    };

                    puntos.Add(pd);
                }

                return puntos;
            }
        }

        public async Task<List<PointData>> ObtenerCoordenadasOptimas(int idProblema)
        {
            TblMaestraProblemaTsp datosProblema = _context.TblMaestraProblemaTsp.FirstOrDefault(x => x.IntId == idProblema);
            List<int> ordenOptimo = JsonConvert.DeserializeObject<List<int>>(datosProblema.StrSolucionOptima);
            ordenOptimo.Add(ordenOptimo[0]);

            List<PointData> puntos = await ObtenerCoordenadas(datosProblema);

            List<PointData> puntosOrdenados = new List<PointData>();

            for (int i = 0; i < ordenOptimo.Count; i++)
            {
                puntosOrdenados.Add(puntos[ordenOptimo[i]]);
            }

            return puntosOrdenados;
        }
    }
}
