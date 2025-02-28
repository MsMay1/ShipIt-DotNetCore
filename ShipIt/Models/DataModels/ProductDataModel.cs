﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;

namespace ShipIt.Models.DataModels
{
    public class ProductDataModel : DataModel
    {
        [DatabaseColumnName("p_id")]
        public int Id { get; set; }

        [DatabaseColumnName("gtin_cd")]
        public string Gtin { get; set; }

        [DatabaseColumnName("gcp_cd")]
        public string Gcp { get; set; }

        [DatabaseColumnName("gtin_nm")]
        public string Name { get; set; }

        [DatabaseColumnName("m_g")]
        public double Weight { get; set; }

        [DatabaseColumnName("l_th")]
        public int LowerThreshold { get; set; }

        [DatabaseColumnName("ds")]
        public int Discontinued { get; set; }

        [DatabaseColumnName("min_qt")]
        public int MinimumOrderQuantity { get; set; }

        public ProductDataModel(IDataReader dataReader) : base(dataReader)
        { }

        public ProductDataModel()
        { }

        public ProductDataModel(Product apiModel)
        {
            Id = apiModel.Id;
            Gtin = apiModel.Gtin;
            Gcp = apiModel.Gcp;
            Name = apiModel.Name;
            Weight = apiModel.Weight;
            LowerThreshold = apiModel.LowerThreshold;
            Discontinued = apiModel.Discontinued ? 1 : 0;
            MinimumOrderQuantity = apiModel.MinimumOrderQuantity;
        }
    }

}