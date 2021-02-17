using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dllConexionProm
{
    public class Conexion
    {
        private static string _ConectionStringHana;
        //private static string _ConectionStringWebFaction;
        private static string _Server;
        private static string _CompanyDB;
        private static string _DbUserName;
        private static string _DbPassword;
        private static string _UserName;
        private static string _Password;
        private static string _LicenseServer;

        public string ConectionStringHana
        {
            get => _ConectionStringHana = "Server=10.72.20.211:30015;UserName=SYSTEM;Password=P1ApasA181409";
            set => _ConectionStringHana = value;
        }

        //public string ConectionStringWebFaction
        //{
        //    get => _ConectionStringWebFaction = "server=web500.webfaction.com;port=3306;uid=plapasa;pwd=Plapasa2020;database=laravel;";
        //    set => _ConectionStringWebFaction = value;
        //}

        public string Server
        {
            get => _Server = "10.72.20.211:30015";
            set => _Server = value;
        }

        public string CompanyDB
        {
            get => _CompanyDB = "SBO_PRUEBAS_PLAPASA";
            set => _CompanyDB = value;
        }

        public string DbUserName
        {
            get => _DbUserName = "SYSTEM";
            set => _DbUserName = value;
        }

        public string DbPassword
        {
            get => _DbPassword = "P1ApasA181409";
            set => _DbPassword = value;
        }

        public string UserName
        {
            //get => _UserName = "manager";
            get => _UserName = "SIST02";
            set => _UserName = value;
        }

        public string Password
        {
            //get => _Password = "pl4P@mng3$";
            get => _Password = "2910";
            set => _Password = value;
        }

        public string LicenseServer
        {
            get => _LicenseServer = "10.72.20.211:40000";
            set => _LicenseServer = value;
        }
    }
}
