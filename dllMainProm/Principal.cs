using dllMainProm.Globals;
using dllMainProm.Modelos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dllMainProm
{
    public class Principal
    {

        public static List<ObjRespuesta> CreateSapDocs()
        {
            List<ObjRespuesta> listRespuestas = new List<ObjRespuesta>();

            try
            {
                List<DetalleDevolucion> dsDevPen = GlobalsSQL.GetDevPendientes();
                List<int> idsDevs = dsDevPen.Select(x => x.NNuIdDevolucion).Distinct().ToList();

                foreach (var item in idsDevs)
                {
                   
                    try
                    {
                        
                        string respCn = Globals.Globals.Connect();
                        if (respCn == "")
                        {
                            if (Globals.Globals.otherCompany.InTransaction == false)
                            {
                                Globals.Globals.otherCompany.StartTransaction();
                            }
                            
                            List<DetalleDevolucion> detDev = dsDevPen.Where(x => x.NNuIdDevolucion == item).ToList();

                            ObjRespuesta objResST = new ObjRespuesta();
                            ObjRespuesta objResEM = CreateEM(detDev);

                            if (objResEM.CodError == 0)
                            {
                                objResST = CreateST(detDev);

                                if (objResST.CodError != 0)
                                {
                                    listRespuestas.Add(new ObjRespuesta { Mensaje = objResST.Mensaje, CodError = objResST.CodError, NNuIdDevolucion = item, DocTypeError = "ST" });
                                }
                            }
                            else
                            {
                                listRespuestas.Add(new ObjRespuesta { Mensaje = objResEM.Mensaje, CodError = objResEM.CodError, NNuIdDevolucion = item, DocTypeError = "EM" });
                            }

                            if (objResEM.CodError == 0 && objResST.CodError == 0)
                            {
                                Globals.Globals.otherCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                                
                                listRespuestas.Add(new ObjRespuesta { Mensaje = objResEM.Mensaje, CodError = objResEM.CodError, DocEntryEM = objResEM.DocEntryEM, DocEntryST = objResST.DocEntryST, NNuIdDevolucion = item });
                            }
                            else
                            {
                                if (Globals.Globals.otherCompany.InTransaction == true)
                                {
                                    Globals.Globals.otherCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (Globals.Globals.otherCompany.InTransaction == true)
                        {
                            Globals.Globals.otherCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        listRespuestas.Add(new ObjRespuesta { Mensaje = ex.Message, CodError = -5, NNuIdDevolucion = item, DocTypeError = "EM" });
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                listRespuestas.Add(new ObjRespuesta { Mensaje = ex.Message, CodError = -6 });
            }

            Globals.Globals.DisconnectOtherCompany();
            return listRespuestas;
        }

        public static ObjRespuesta CreateEM(List<DetalleDevolucion> detDev)
        {
            ObjRespuesta objResEM = Globals.Globals.GeneraEntradaMercancia(detDev);
            return objResEM;
        }

        public static ObjRespuesta CreateST(List<DetalleDevolucion> detDev)
        {
            ObjRespuesta objResST = Globals.Globals.GenerarTransSap(detDev);
            return objResST;
        }

        public static ObjRespuesta UpdateDevSql(ObjRespuesta poObjRes)
        {
            ObjRespuesta objResST = Globals.GlobalsSQL.UpdateDev(poObjRes);
            return objResST;
        }

    }
}
