using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dllMainProm.Modelos
{
    public class ObjRespuesta
    {
        public string Mensaje { get; set; }
        public string MensajeAdic { get; set; }
        public string DocTypeError { get; set; }
        public int CodError { get; set; }
        public int DocEntryEM { get; set; }
        public int DocEntryST { get; set; }
        public int NNuIdDevolucion { get; set; }
    }
}
