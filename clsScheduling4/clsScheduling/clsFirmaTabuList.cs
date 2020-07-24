using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITelligent.Scheduling
{
    /// <summary>
    /// Esta clase se ocupa de obtener la firma que se utilizara para representar una solucion
    /// en Tabu List, hay diversos tipos implementados y se guardan en un enum
    /// de la clase parametros
    /// </summary>
    class clsFirmaTabuList
    {
        private TiposFirmaTabuList _enuTipoFirma;
        public clsFirmaTabuList(TiposFirmaTabuList enuTipoFirma)
        {
            _enuTipoFirma = enuTipoFirma;
        }

        public string GenerarFirma(double dblMakespan, clsDatosCambio cCambio)
        {
            string strFirma = "";
            if (_enuTipoFirma == TiposFirmaTabuList.CaminoUaV)
                strFirma = cCambio.strPathUaV;
            else if (_enuTipoFirma == TiposFirmaTabuList.CaminoUaVYPosicionMaquinas )
                strFirma = cCambio.strPathUaV + "_"+ cCambio .intPosicionPrimeraMaquina ;
            else
                new Exception("Tipo no implementado");

            return strFirma;
        }
    }
}
