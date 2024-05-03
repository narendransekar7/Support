using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SupportSystem.Backend.SQLServer.Deployment
{
    public class MetaData
    {
        private IParameterModel _parameter;
        public MetaData(IParameterModel parameter)
        {
            _parameter = parameter;
        }

        public bool Create()
        {

            //create connection first
            SqlConnection sqlConnection = SQLSeverBackendFactory.CreateSQLServerConnection(_parameter);


            //create database  if not exist
            SQLSeverBackendFactory.CreateMetaDatabase(sqlConnection, _parameter.Database);

            sqlConnection = SQLSeverBackendFactory.UpdateDatabaseNameInConnection(sqlConnection, _parameter.Database);

            //then create table with in it.
            SQLSeverBackendFactory.CreateMetaUserTable(sqlConnection);

            return true;

        }
    }
}
