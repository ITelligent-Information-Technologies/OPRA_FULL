using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Drawing;
using System.IO;


namespace conTspRL
{
    class clsImagenes
    {
        Bitmap _bmImagenBase;



        public double PintarRecorrido(List<clsPunto> lstPuntos, List<Int32> lstRecorrido, double[][] dblDistancias, Int32 intWidth, Int32 intHeight, string strFilePathOutput, Boolean blnPintarSoloCiudades)
        {
            double dblCoste = 0;
            Bitmap bm = new Bitmap(intWidth, intHeight);
            Image img = new Bitmap(bm);
            Graphics drawing = Graphics.FromImage(img);
            drawing.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
            Pen BlackPen = new Pen(Color.DarkGreen, 1);
            Pen RedPen = new Pen(Color.DarkGreen, 1);
            
            for (Int32 intI = 1; intI < lstRecorrido.Count; intI++)
            {
                Int32 intIndexAnt = lstRecorrido[intI - 1];
                Int32 intIndex = lstRecorrido[intI];
                float dblOldX = (float)lstPuntos[intIndexAnt].dblX;
                float dblOldY = (float)lstPuntos[intIndexAnt].dblY;
                float dblX = (float)lstPuntos[intIndex].dblX;
                float dblY = (float)lstPuntos[intIndex].dblY;
                dblCoste = dblCoste + dblDistancias [intIndexAnt][intIndex] ;
                // Adapta valores a la caja
                dblOldX = dblOldX * intWidth;
                dblX = dblX * intWidth ;
                dblOldY = dblOldY * (intHeight-20) ;
                dblY = dblY * (intHeight-20) ;

                Rectangle rec = new Rectangle(Convert.ToInt32(dblX - 3), Convert.ToInt32(dblY - 3), 6, 6);
                drawing.FillRectangle(Brushes.DarkGray, rec);
                if (intIndex == 1)
                {
                    rec = new Rectangle(Convert.ToInt32(dblOldX - 3), Convert.ToInt32(dblOldY - 3), 6, 6);
                    drawing.FillRectangle(Brushes.DarkGray, rec);
                }
                if (!blnPintarSoloCiudades)
                {
                    drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                
            }
            // Cierra el loop
            if (lstPuntos.Count > 1)
            {
                Int32 intIndexAnt = lstRecorrido[lstPuntos.Count - 1];
                Int32 intIndex = lstRecorrido[0];

                float dblOldX =(float) lstPuntos[intIndexAnt].dblX;
                float dblOldY = (float)lstPuntos[intIndexAnt].dblY;
                float dblX = (float)lstPuntos[intIndex].dblX;
                float dblY = (float)lstPuntos[intIndex].dblY;
                dblCoste = dblCoste + dblDistancias [intIndexAnt][intIndex];
                dblOldX = dblOldX * intWidth;
                dblX = dblX * intWidth;
                dblOldY = dblOldY * (intHeight - 20);
                dblY = dblY * (intHeight - 20);


                if (!blnPintarSoloCiudades)
                {
                    drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                
            }
            Font font = new Font("Century Gothic", 8);
            Brush brush = new SolidBrush(Color.DarkGreen );
            string strTexto = "@ITelligent, coste:" + Convert.ToInt32(dblCoste).ToString("N"); ;
            drawing.DrawString(strTexto, font, brush, 10,intHeight - 20);
            //font = new Font("Eurostile", 10);
            //string strCoste ="ITelligent, Coste:"+ Convert.ToInt32(dblCoste).ToString("N");//, CultureInfo.InvariantCulture);
            //drawing.DrawString(strCoste, font, brush, 10, 40);
            drawing.Save();
            img.Save(strFilePathOutput);
            //bm.Save(strFilePathOutput);
            drawing.Dispose();
            bm.Dispose();
            return dblCoste;
        }

    }
}
