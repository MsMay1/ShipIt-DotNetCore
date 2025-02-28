﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    [Route("orders/inbound")]
    public class InboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IInboundOrderRepository _inboundOrderRepository;

        public InboundOrderController(IEmployeeRepository employeeRepository, IInboundOrderRepository inboundOrderRepository, ICompanyRepository companyRepository, IProductRepository productRepository, IStockRepository stockRepository)
        {
            _employeeRepository = employeeRepository;
            _stockRepository = stockRepository;
            _inboundOrderRepository = inboundOrderRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
        }

        private int GetOrderQuantity(InboundOrder productStock) => Math.Max(productStock.LowerThreshold * 3 - productStock.Held, productStock.MinimumOrderQuantity);

        [HttpGet("{warehouseId}")]
        public InboundOrderResponse Get([FromRoute] int warehouseId)
        {
            Log.Info("orderIn for warehouseId: " + warehouseId);

            var operationsManager = new Employee(_employeeRepository.GetOperationsManager(warehouseId));
            Log.Debug(String.Format("Found operations manager: {0}", operationsManager));

            IEnumerable<InboundOrder> allProductStock = _inboundOrderRepository.GetProductStockByWarehouseId(warehouseId).Select(product => new InboundOrder(product));

            Dictionary<Company, List<InboundOrderLine>> orderlinesByCompany =
            _companyRepository
            .GetCompaniesByIds(allProductStock.Select(productStock => productStock.Gcp)) // Enumerable<CompanyDataModel>
            .Select(companyDataModel => new Company(companyDataModel)) // Enumerable<Company>
            .Distinct()
            .ToDictionary(company => company,
            company => allProductStock
                .Where(productStock => productStock.Gcp == company.Gcp)
                .Select(productStock => new InboundOrderLine()
                {
                    gtin = productStock.Gtin,
                    name = productStock.Name,
                    quantity = GetOrderQuantity(productStock)
                })
                        .ToList()
            );

            Log.Debug(String.Format("Constructed order lines: {0}", orderlinesByCompany));

            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment()
            {
                OrderLines = ol.Value,
                Company = ol.Key
            });

            Log.Info("Constructed inbound order");

            return new InboundOrderResponse()
            {
                OperationsManager = operationsManager,
                WarehouseId = warehouseId,
                OrderSegments = orderSegments
            };
        }

        [HttpPost("")]
        public void Post([FromBody] InboundManifestRequestModel requestModel)
        {
            Log.Info("Processing manifest: " + requestModel);

            var gtins = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (gtins.Contains(orderLine.gtin))
                {
                    throw new ValidationException(String.Format("Manifest contains duplicate product gtin: {0}", orderLine.gtin));
                }
                gtins.Add(orderLine.gtin);
            }

            IEnumerable<ProductDataModel> productDataModels = _productRepository.GetProductsByGtin(gtins); // call to backend
            Dictionary<string, Product> products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            Log.Debug(String.Format("Retrieved products to verify manifest: {0}", products));

            var lineItems = new List<StockAlteration>();
            var errors = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(String.Format("Unknown product gtin: {0}", orderLine.gtin));
                    continue;
                }

                Product product = products[orderLine.gtin];
                if (!product.Gcp.Equals(requestModel.Gcp))
                {
                    errors.Add(String.Format("Manifest GCP ({0}) doesn't match Product GCP ({1})",
                        requestModel.Gcp, product.Gcp));
                }
                else
                {
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
                }
            }

            if (errors.Count() > 0)
            {
                Log.Debug(String.Format("Found errors with inbound manifest: {0}", errors));
                throw new ValidationException(String.Format("Found inconsistencies in the inbound manifest: {0}", String.Join("; ", errors)));
            }

            Log.Debug(String.Format("Increasing stock levels with manifest: {0}", requestModel));
            _stockRepository.AddStock(requestModel.WarehouseId, lineItems);
            Log.Info("Stock levels increased");
        }
    }
}
