using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Smort_api.Object.Database.Interfaces
{
    public class QueryBuilderORM<Model> where Model : class, IDatabaseModel, new()
    {
        //string prop name / dynamic value of 
        public Dictionary<string, object> Values = new Dictionary<string, object>();
        private string TableName = "";


        public List<string> QueryParts = new List<string>();

        public QueryBuilderORM(string tableName)
        {
            TableName = tableName;
        }


        public QueryBuilderORM<Model> Select(Model data) 
        {
            var modelType = typeof(Model);

            var modelProperties = modelType.GetProperties();

            var PropertieNamesToGet = modelProperties.Select(p => $"{TableName}.{p.Name}").ToList();

            var ColumDataToGet = string.Join(",", PropertieNamesToGet);

            QueryParts.Add($"SELECT {ColumDataToGet} FROM {TableName}");

            Console.WriteLine(string.Join(" ", QueryParts) + ";");

            return this;
        }


        public QueryBuilderORM<Model> Update(Model data) 
        {
            var modelType = typeof(Model);

            var modelProperties = modelType.GetProperties();

            var PropertieNamesFilledIn = modelProperties.Select(p => $"{p.Name}=@{p.Name}").ToList();

            var ColumDataToBefilled = string.Join(",", PropertieNamesFilledIn);

            QueryParts.Add($"UPDATE {TableName} SET {ColumDataToBefilled}");

            foreach (var Col in modelProperties)
            {
                var value = Col.GetValue(data);

                if (value != null)
                {
                    Values.Add($"@{Col.Name}", value);
                }
            }
            Console.WriteLine(string.Join(" ", QueryParts) + ";");

            return this;
        }

        /// <summary>
        /// Adds a where statements to your QUERY
        /// </summary>
        /// <param name="Parameter"></param>
        /// <param name="Compare"> != 1, == "", ect</param>
        /// <returns></returns>
        public QueryBuilderORM<Model> Where(string Parameter, string Compare) {

            QueryParts.Add($"WHERE {Parameter} {Compare}");
            return this;
        }

        public QueryBuilderORM<Model> And(string Parameter, string Compare)
        {
            QueryParts.Add($"AND {Parameter} {Compare}");
            return this;
        }

        public QueryBuilderORM<Model> Insert(Model data)
        {
            var modelType = typeof(Model);

            var modelProperties = modelType.GetProperties();

            var PropertieNames = modelProperties.Select(p => p.Name).ToList();
            var PropertieNamesFilledIn = modelProperties.Select(p => $"@{p.Name}").ToList();

            var ColumnData = string.Join(",", PropertieNames);
            var ColumDataToBefilled = string.Join(",", PropertieNamesFilledIn);

            QueryParts.Add($"INSERT INTO {modelType.Name} ({ColumnData}) VALUES ({ColumDataToBefilled})");

            foreach (var Col in modelProperties)
            {
                var value = Col.GetValue(data);

                if (value != null)
                {
                    Values.Add($"@{Col.Name}", value);
                }
            }

            Console.WriteLine(string.Join(" ", QueryParts) + ";");
            return this;

        }


        public QueryBuilderORM<Model> Delete()
        {
            QueryParts.Add($"DELETE FROM {TableName}");

            return this;
        }
    }
}
