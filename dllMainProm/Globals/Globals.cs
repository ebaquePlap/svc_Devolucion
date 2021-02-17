using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometeo;
using Sap.Data.Hana;
using SAPbobsCOM;
using System.Configuration;
using dllMainProm.Modelos;
//using dllConexionProm;

namespace dllMainProm.Globals
{
    public class Globals
    {

        public static SAPbobsCOM.Company otherCompany = new SAPbobsCOM.Company();

        public static string Server = string.Empty;
        public static string CompanyDB = string.Empty;
        public static string DbServerType = string.Empty;
        public static string DbUserName = string.Empty;
        public static string DbPassword = string.Empty;
        public static string UserName = string.Empty;
        //public static string Password = string.Empty;

        public static string LicenseServer = string.Empty;
        //private static Conexion objConxion = new Conexion();

        public static void DisconnectOtherCompany()
        {

            if (otherCompany != null)
            {
                if (otherCompany.Connected)
                {
                    otherCompany.Disconnect();
                    otherCompany = null;
                }
            }

        }

        public static string Changedb(string username, string password)
        {
            string error = string.Empty;
            GetVariables();

            try
            {
                if (otherCompany == null)
                {
                    otherCompany = new SAPbobsCOM.Company();
                }

                otherCompany.Server = Server;
                otherCompany.CompanyDB = CompanyDB;
                otherCompany.UserName = username;
                otherCompany.Password = password;
                otherCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
                otherCompany.DbUserName = DbUserName;
                otherCompany.DbPassword = DbPassword;
                otherCompany.UseTrusted = false;
                otherCompany.language = SAPbobsCOM.BoSuppLangs.ln_English;

                int lRetCode2 = otherCompany.Connect();

                if (lRetCode2 != 0)
                {
                    otherCompany.GetLastError(out lRetCode2, out error);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }

        public static string Connect(/*string username*/)
        {
            string errorcdb = string.Empty;
            string passwnc = string.Empty;
            GetVariables();
            //string username = objConxion.UserName.ToString();            

            using (PrometeoAdminEntities dbAdmin = new PrometeoAdminEntities())
            {
                //passwnc = dbAdmin.tblAdUsuario.Where(x => x.CCiUsuario == username).FirstOrDefault().CTxClaveSAP;
                passwnc = dbAdmin.tblAdUsuario.Where(x => x.CCiUsuario == UserName).FirstOrDefault().CTxClaveSAP;
            }

            if (Globals.otherCompany != null)
            {
                if (!Globals.otherCompany.Connected)
                {
                    //errorcdb = Globals.Changedb(username, passwnc);
                    errorcdb = Globals.Changedb(UserName, passwnc);
                }
            }
            else
            {
                //errorcdb = Globals.Changedb(username, passwnc);
                errorcdb = Globals.Changedb(UserName, passwnc);
            }

            return errorcdb;
        }

        private static void GetVariables()
        {
            Server = ConfigurationManager.AppSettings["Server"];
            CompanyDB = ConfigurationManager.AppSettings["CompanyDB"];
            DbUserName = ConfigurationManager.AppSettings["DbUserName"];
            DbPassword = ConfigurationManager.AppSettings["DbPassword"];
            UserName = ConfigurationManager.AppSettings["UserName"];
            LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
        }

        //public ObjRespuesta GeneraEntradaMercancia(TblPrCabDevolucion objCabDev, List<TblPrDetDevolucion> listDet, string username)
        public static ObjRespuesta GeneraEntradaMercancia(List<DetalleDevolucion> listDev)
        {
            string respuesta = string.Empty;
            string passwnc = string.Empty;
            ObjRespuesta objRespuesta = new ObjRespuesta();

            try
            {
                
                using (PrometeoEntities db = new PrometeoEntities())
                    {

                    int NNuIdOF = listDev.FirstOrDefault().NNuIdOF;
                    int? NNuIdReciboCorte = listDev.FirstOrDefault().NNuIdReciboCorte;
                    //string WhsCodeDestino = listDev.FirstOrDefault().WhsCodeDestino;
                    string WhsCodeOrigen = listDev.FirstOrDefault().WhsCodeOrigen;

                    TblProDetOrdenFabricacion objOF = db.TblProDetOrdenFabricacion.AsNoTracking().Where(x => x.NNuIdOF == NNuIdOF).FirstOrDefault();
                       
                        int error = 0;

                        //Asigno Objeto para las entradas
                        Documents Entradas = Globals.otherCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);
                    
                        var detaOrdenCorte = (from deta in db.TblPrDetCorteRequisicion
                                              where deta.NNuIdReciboCorte == NNuIdReciboCorte
                                              select deta).ToList().FirstOrDefault();

                        var detEntrada = (from deta in db.ZPLA_DETA_DEFINICIONES
                                              where deta.CCiDefinicion.Contains("TPIG")
                                              && deta.CCiDetDefinicion == detaOrdenCorte.CTpPigmento
                                              select deta).FirstOrDefault();

                        //recupero el Costo del articulo desde sap
                        DataSet dtsCosto = Globals.GetCostoArticulo(detEntrada.CTxValor);
                        DataSet dtsCuentaCosto = Globals.GetCuentaContable(detEntrada.CTxCuenta);

                        string sItemCode = string.Empty;
                        string sItemName = string.Empty;
                        double AvgPrice = 0;
                        string AcctCode = string.Empty;
                        double NVtDevolucion = 0;

                        foreach (DataRow dataRow in dtsCosto.Tables[0].Rows)
                        {
                            AvgPrice = double.Parse(dataRow["AvgPrice"].ToString());
                            sItemCode = dataRow["ItemCode"].ToString();
                            sItemName = dataRow["ItemName"].ToString();
                        }

                        foreach (DataRow dataRow in dtsCuentaCosto.Tables[0].Rows)
                        {
                            AcctCode = dataRow["AcctCode"].ToString();
                        }

                        //foreach (var item in listDet)
                        foreach (var item in listDev)
                        {
                            NVtDevolucion += Convert.ToDouble(item.NVtDetDevolucion);
                        }

                        Entradas.Series = 105;
                        Entradas.DocDueDate = DateTime.Now;
                        Entradas.UserFields.Fields.Item("U_NUM_ORDEN_FABRICACION").Value = objOF.DocNum.ToString();
                        Entradas.UserFields.Fields.Item("U_TRANS_SAP").Value = "NO";
                        //Entradas.Comments = "Entrada generada por Prometeo. SM: " + listDev.FirstOrDefault().DocNumSM;
                        Entradas.Comments = listDev.FirstOrDefault().Remarks;

                        Entradas.Lines.ItemCode = sItemCode;
                        Entradas.Lines.ItemDescription = sItemName;
                        //Entradas.Lines.WarehouseCode = WhsCodeDestino;
                        Entradas.Lines.WarehouseCode = WhsCodeOrigen;
                        Entradas.Lines.Quantity = NVtDevolucion;
                        Entradas.Lines.Price = AvgPrice;
                        Entradas.Lines.AccountCode = AcctCode;

                        error = Entradas.Add();

                        if (error != 0)
                        {
                            Globals.otherCompany.GetLastError(out error, out respuesta);
                            objRespuesta.Mensaje = "Error al grabar la entrada de mercancía en SAP: " + respuesta;
                            objRespuesta.CodError = -1;
                        }
                        else
                        {
                            objRespuesta.DocEntryEM = int.Parse(Globals.otherCompany.GetNewObjectKey());
                            objRespuesta.CodError = 0;
                        }

                    }
                
            }
            catch (Exception ex)
            {
                objRespuesta.CodError = -2;
                objRespuesta.Mensaje = "Ocurrió un error al grabar la entrada de mercancía en SAP: " + ex.Message;
            }

            return objRespuesta;
        }

        public static ObjRespuesta GenerarTransSap(List<DetalleDevolucion> listDev)
        {

            ObjRespuesta objRespuesta = new ObjRespuesta();

            try
            {

                string respuesta = "";
                
                TblProDetOrdenFabricacion objOF = new TblProDetOrdenFabricacion();
                int NNuIdOF = listDev.FirstOrDefault().NNuIdOF;
                string WhsCodeDestino = listDev.FirstOrDefault().WhsCodeDestino;
                string WhsCodeOrigen = listDev.FirstOrDefault().WhsCodeOrigen;
                string CTxComentarios = listDev.FirstOrDefault().CTxComentarios;
                string Remarks = listDev.FirstOrDefault().Remarks;
                DateTime DFxContabilizacion = Convert.ToDateTime(listDev.FirstOrDefault().DFxContabilizacion);
                DateTime DFxDevolucion = Convert.ToDateTime(listDev.FirstOrDefault().DFxDevolucion);

                //Recupero doc entry y docnum de of
                using (PrometeoEntities db = new PrometeoEntities())
                {
                    objOF = db.TblProDetOrdenFabricacion.Where(x => x.NNuIdOF == NNuIdOF).FirstOrDefault();
                }

                int error = 0;
                StockTransfer poTrans = Globals.otherCompany.GetBusinessObject(BoObjectTypes.oStockTransfer);

                poTrans.FromWarehouse = WhsCodeOrigen;
                poTrans.ToWarehouse = WhsCodeDestino;

                poTrans.PriceList = -2;
                poTrans.UserFields.Fields.Item("U_NUM_ORDEN_FABRICACION").Value = objOF.DocNum.ToString();
                poTrans.UserFields.Fields.Item("U_TRANS_SAP").Value = "NO";
                poTrans.JournalMemo = CTxComentarios;
                poTrans.Comments = Remarks;
                poTrans.DocDate = DateTime.Parse(DFxContabilizacion.ToString());
                poTrans.TaxDate = DateTime.Parse(DFxDevolucion.ToString());

                DataRow dtsNR = Globals.GetNROF(objOF.DocEntry.ToString()).Tables[0].Rows[0];
                 int i = 0;

                    //foreach (var item in listDet)
                foreach (var item in listDev)
                {
                    if (i > 0) { poTrans.Lines.Add(); }
                    poTrans.Lines.ItemCode = item.ItemCode;
                    poTrans.Lines.FromWarehouseCode = WhsCodeOrigen;
                    poTrans.Lines.WarehouseCode = WhsCodeDestino;
                    poTrans.Lines.Quantity = Convert.ToDouble(item.NVtDetDevolucion);

                    poTrans.Lines.DistributionRule = dtsNR["OcrCode"].ToString() == null ? "" : dtsNR["OcrCode"].ToString();
                    poTrans.Lines.DistributionRule2 = dtsNR["OcrCode2"].ToString() == null ? "" : dtsNR["OcrCode2"].ToString();
                    i++;
                }

                //Comentario que llega a Jorge al ver la notificacion para aprobar documento
                poTrans.GetApprovalTemplates();
                poTrans.StockTransfer_ApprovalRequests.SetCurrentLine(0);

                //string RemarkApproval = string.Concat(objOF.DocNum.ToString(), " - ", objOF.ItemCode, " - ", objOF.ItemName);
                //RemarkApproval = string.Concat(RemarkApproval, "; ", Remarks);

                string RemarkApproval = Remarks;

                poTrans.StockTransfer_ApprovalRequests.Remarks = RemarkApproval;

                error = poTrans.Add();

                if (error != 0)
                {
                    Globals.otherCompany.GetLastError(out error, out respuesta);
                    objRespuesta.CodError = -3;
                    objRespuesta.Mensaje = "Error al grabar la transferencia en SAP: " + respuesta;
                }
                else
                {
                    objRespuesta.DocEntryST = int.Parse(Globals.otherCompany.GetNewObjectKey());
                    objRespuesta.CodError = 0;                        
                }

            }
            catch (Exception ex)
            {
                objRespuesta.CodError = -4;
                objRespuesta.Mensaje = "Ocurrió un error al grabar la transferencia en SAP: " + ex.Message;
            }

            return objRespuesta;

        }

        private static DataSet GetCostoArticulo(string ItemCode)
        {
            DataSet dtsConsulta = new DataSet();
            string ConnectionStrinHANA = ConfigurationManager.ConnectionStrings["Hana"].ConnectionString;
            HanaConnection conn = new HanaConnection(ConnectionStrinHANA);
            conn.Open();
            GetVariables();

            string sQuery = string.Empty;

            sQuery = string.Concat(sQuery, "Select T0.");
            sQuery = string.Concat(sQuery, '\u0022', "ItemCode", '\u0022', ", T0.");
            sQuery = string.Concat(sQuery, '\u0022', "ItemName", '\u0022', ", T0.");
            sQuery = string.Concat(sQuery, '\u0022', "AvgPrice", '\u0022', ", T0.");
            sQuery = string.Concat(sQuery, '\u0022', "PrdStdCst", '\u0022');

            sQuery = string.Concat(sQuery, "From ", '\u0022', CompanyDB, '\u0022', ".");
            sQuery = string.Concat(sQuery, '\u0022', "OITM", '\u0022', " T0 ");
            sQuery = string.Concat(sQuery, "  where 1=1 ");

            if (ItemCode != "")
            {
                sQuery = string.Concat(sQuery, " and T0.", '\u0022', "ItemCode", '\u0022', " = '", ItemCode, "'");
            }

            HanaCommand cmd = new HanaCommand(sQuery, conn);
            HanaDataAdapter da = new HanaDataAdapter(cmd);
            da.Fill(dtsConsulta);

            conn.Close();

            return dtsConsulta;
        }

        private static DataSet GetCuentaContable(string AcctCode)
        {
            DataSet dtsConsulta = new DataSet();
            string ConnectionStrinHANA = ConfigurationManager.ConnectionStrings["Hana"].ConnectionString;
            HanaConnection conn = new HanaConnection(ConnectionStrinHANA);
            conn.Open();
            GetVariables();

            string sQuery = string.Empty;
            sQuery = string.Concat(sQuery, "Select ");
            sQuery = string.Concat(sQuery, '\u0022', "AcctCode", '\u0022');
            sQuery = string.Concat(sQuery, " From ", '\u0022', CompanyDB, '\u0022', ".");
            sQuery = string.Concat(sQuery, '\u0022', "OACT", '\u0022');
            sQuery = string.Concat(sQuery, " where ", '\u0022', "Segment_0", '\u0022');
            sQuery = string.Concat(sQuery, " like '%", AcctCode, "%'");
            HanaCommand cmd = new HanaCommand(sQuery, conn);
            HanaDataAdapter da = new HanaDataAdapter(cmd);
            da.Fill(dtsConsulta);

            conn.Close();

            return dtsConsulta;
        }

        private static DataSet GetNROF(string docsEntry)
        {
            DataSet dtsConsulta = new DataSet();
            string ConnectionStrinHANA = ConfigurationManager.ConnectionStrings["Hana"].ConnectionString;
            HanaConnection conn = new HanaConnection(ConnectionStrinHANA);
            conn.Open();
            GetVariables();

            string sQuery = string.Empty;
            sQuery = string.Concat(sQuery, "Select ");
            sQuery = string.Concat(sQuery, '\u0022', "DocEntry", '\u0022', ',');
            sQuery = string.Concat(sQuery, '\u0022', "DocNum", '\u0022', ',');
            sQuery = string.Concat(sQuery, '\u0022', "OcrCode", '\u0022', ',');
            sQuery = string.Concat(sQuery, '\u0022', "OcrCode2", '\u0022');
            sQuery = string.Concat(sQuery, " From ", '\u0022', CompanyDB, '\u0022', ".");
            sQuery = string.Concat(sQuery, '\u0022', "OWOR", '\u0022');
            sQuery = string.Concat(sQuery, " where ", '\u0022', "DocEntry", '\u0022', " in(", docsEntry, ")");

            HanaCommand cmd = new HanaCommand(sQuery, conn);
            HanaDataAdapter da = new HanaDataAdapter(cmd);
            da.Fill(dtsConsulta);

            conn.Close();

            return dtsConsulta;
        }

    }
}
