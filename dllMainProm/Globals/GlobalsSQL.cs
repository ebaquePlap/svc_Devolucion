using dllMainProm.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometeo;
using System.Data.Entity;

namespace dllMainProm.Globals
{
    public class GlobalsSQL
    {

        public static List<DetalleDevolucion> GetDevPendientes()
        {

            using (PrometeoEntities db = new PrometeoEntities())
            {
                List<DetalleDevolucion> devPen = (from cabDev in db.TblPrCabDevolucion.AsNoTracking()
                                                  join detDev in db.TblPrDetDevolucion.AsNoTracking() on cabDev.NNuIdDevolucion equals detDev.NNuIdDevolucion
                                                  join salSap in db.ZPLA_SALIDAS_BORR.AsNoTracking() on cabDev.DocEntrySM equals salSap.DocEntry
                                                  where salSap.EstadoSal == "Generado" && (cabDev.DocEntryEM == 0 || cabDev.DocEntryTrans == 0) && cabDev.DocEntrySM != 0
                                                  && (cabDev.CTpTransDev == "DEV_RC1" || cabDev.CTpTransDev == "DEV_RC3")
                                                  && cabDev.CCeDevolucion == "A"
                                                  select new DetalleDevolucion
                                                  {
                                                      NNuIdDevolucion = cabDev.NNuIdDevolucion,
                                                      //EM
                                                      NNuIdOF = cabDev.NNuIdOF,
                                                      NNuIdReciboCorte = detDev.NNuIdReciboCorteRef,
                                                      NVtDetDevolucion = detDev.NVtTotalLine,
                                                      DocNumSM = salSap.DocNumProd,

                                                      //TRANS
                                                      WhsCodeOrigen = cabDev.WhsCodeOrigen,
                                                      WhsCodeDestino = cabDev.WhsCodeDestino,
                                                      CTxComentarios = cabDev.CTxComentarios,
                                                      
                                                      DFxContabilizacion = cabDev.DFxContabilizacion,
                                                      DFxDevolucion = cabDev.DFxDevolucion,
                                                      ItemCode = detDev.ItemCode,

                                                      //AMBOS
                                                      Remarks = cabDev.Remarks
                                                  }
                                                 ).ToList();

                return devPen;
            }
            
        }

        public static ObjRespuesta UpdateDev(ObjRespuesta poObjRes)
        {
            ObjRespuesta objResUpSql = new ObjRespuesta();

            try
            {
                objResUpSql.NNuIdDevolucion = poObjRes.NNuIdDevolucion;

                using (PrometeoEntities db = new PrometeoEntities())
                {

                    TblPrCabDevolucion objDev = db.TblPrCabDevolucion.Where(x => x.NNuIdDevolucion == poObjRes.NNuIdDevolucion).FirstOrDefault();

                    if (poObjRes.CodError == 0)
                    {
                        objDev.DocEntryEM = poObjRes.DocEntryEM;
                        objDev.DocEntryTrans = poObjRes.DocEntryST;
                        objDev.CTxObservacionesSM = "";
                        objDev.CTxObservacionesEM = "";
                        objDev.CTxObservacionesTrans = "";
                    }
                    else
                    {
                        objDev.CTxObservacionesSM = "";
                        if (poObjRes.DocTypeError == "EM")
                        {
                            objDev.CTxObservacionesEM = "Error al grabar la EM de la devolución #" + poObjRes.NNuIdDevolucion + ". Código: " + poObjRes.CodError + ":" + poObjRes.Mensaje;
                        }
                        else if(poObjRes.DocTypeError == "ST")
                        {
                            objDev.CTxObservacionesTrans = "Error al grabar la TS de la devolución #" + poObjRes.NNuIdDevolucion + ". Código: " + poObjRes.CodError + ":" + poObjRes.Mensaje;
                        }
                    }

                    db.Entry(objDev).State = EntityState.Modified;
                    db.SaveChanges();

                    objResUpSql.CodError = 0;
                }
                
            }
            catch (Exception ex)
            {
                objResUpSql.CodError = -7;
                objResUpSql.Mensaje = ex.Message;
            }

            return objResUpSql;
        }
        
    }
}
