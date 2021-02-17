using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using dllMainProm;
using dllMainProm.Modelos;

namespace svcPrometeoEM_TS
{
    partial class svcPrometeoEM_TS : ServiceBase
    {

        //Esta variable se pone en true cuando se inicia la ejecucion del servicio, si es que en el intervalo 
        //de tiempo no se termina de ejecutar el proceso, no ejecuta otra vez el proceso
        bool blBandera = false;

        public svcPrometeoEM_TS()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            stLapso.Start();
        }

        protected override void OnStop()
        {
            stLapso.Stop();
        }

        private void stLapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (blBandera) return;

            try
            {
                blBandera = true;
                //EventLog.WriteEntry("Se inicio servicio Prometeo", EventLogEntryType.Information);

                //Creo documentos
                List<ObjRespuesta> listResp = Principal.CreateSapDocs();

                //Si hay error con codiglo -6 quiere decir que el servicio se cayo al principio por lo tanto no es necesario recorrer las respuestas para actualizar
                //la tabla de devolucion
                if (listResp.Where(x=> x.CodError == -6).Count()==0)
                {
                    foreach (var item in listResp)
                    {
                        ObjRespuesta respDev =  Principal.UpdateDevSql(item);
                        if (respDev.CodError != 0)
                        {
                            EventLog.WriteEntry("Ocurrió un error al actualizar la devolución #" + respDev.NNuIdDevolucion + ". " + respDev.Mensaje, EventLogEntryType.Error);
                        }                     
                    }
                }
                else
                {
                    EventLog.WriteEntry("Ocurrió un error al ejecutar el proceso: " + listResp.Where(x => x.CodError == -6).FirstOrDefault().Mensaje, EventLogEntryType.Error);
                }

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

            blBandera = false;
        }
    }
}
