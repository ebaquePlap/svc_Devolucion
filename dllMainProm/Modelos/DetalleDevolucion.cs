using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dllMainProm.Modelos
//namespace Modelos
{
    public class DetalleDevolucion
    {     
        public string Remarks { get; set; }
        public string CTxComentarios { get; set; }
        public int DocNumSM { get; set; }
        public DateTime? DFxContabilizacion { get; set; }      
        public DateTime? DFxDevolucion { get; set; }
        public string WhsCodeDestino { get; set; }
        public string WhsCodeOrigen { get; set; }
        public int NNuIdOF { get; set; }
        public int? NNuIdReciboCorte { get; set; }
        public int NNuIdDevolucion { get; set; }
        public string ItemCode { get; set; }
        public decimal NVtDetDevolucion { get; set; }

    }
}
