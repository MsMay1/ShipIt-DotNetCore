﻿using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface IInboundOrderRepository
    {
        List<InboundOrderModel> GetProductStockByWarehouseId (int warehouseId);
    }

    public class InboundOrderRepository : RepositoryBase, IInboundOrderRepository
    {
        public List<InboundOrderModel> GetProductStockByWarehouseId (int warehouseId)
        {
            string sql = 
                "SELECT stock.p_id, w_id, hld, gtin_cd, gcp_cd, gtin_nm, l_th, ds, min_qt " + 
                "FROM stock " +
                "JOIN gtin on stock.p_id = gtin.p_id " +
                "WHERE stock.w_id = @warehouseId " + 
                "AND hld < l_th " +
                "AND ds = 0";
            var parameter = new NpgsqlParameter("@warehouseId", warehouseId);
            string noProductWithIdErrorMessage = string.Format("No stock found with w_id: {0}", warehouseId);
            try
            {
                return base.RunGetQuery(sql, reader => new InboundOrderModel(reader), noProductWithIdErrorMessage, parameter).ToList();
            }
            catch (NoSuchEntityException)
            {
                return new List<InboundOrderModel>();
            }
        }
    }
}