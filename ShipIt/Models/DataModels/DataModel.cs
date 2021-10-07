using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;

namespace ShipIt.Models.DataModels
{
    public class DatabaseColumnName : Attribute
    {
        public string Name { get; set; }

        public DatabaseColumnName(string name)
        {
            Name = name;
        }
    }


    public abstract class DataModel
    {
        protected DataModel()
        {
        }

        public DataModel(IDataReader dataReader)
        {
            var type = GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
                property.SetValue(this, dataReader[attribute.Name], null);
            }
        }

        public IEnumerable<NpgsqlParameter> GetNpgsqlParameters()
        {
            var type = GetType();
            var properties = type.GetProperties();
            var parameters = new List<NpgsqlParameter>();

            foreach (var property in properties)
            {
                var attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
                parameters.Add(new NpgsqlParameter("@" + attribute.Name, property.GetValue(this, null)));
            }

            return parameters;
        }
    }
}