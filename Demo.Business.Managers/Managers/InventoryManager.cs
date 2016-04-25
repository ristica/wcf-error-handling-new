using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using Core.Common.Contracts;
using Core.Common.Exceptions;
using Demo.Business.Common;
using Demo.Business.Contracts;
using Demo.Business.Entities;
using Demo.Data.Contracts;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Demo.Business.Managers
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall, 
        ConcurrencyMode = ConcurrencyMode.Multiple, 
        ReleaseServiceInstanceOnTransactionComplete = false)]
    //[ArgumentExceptionHandler] => no need for this here because it is handled in the host's config file
    //[NotSupportedExceptionHandler] => no need for this here because it is handled in the host's config file
    public class InventoryManager : ManagerBase, IInventoryService, IServiceBehavior
    {
        #region Fields

        [Import]
        private IDataRepositoryFactory _repositoryFactory;

        [Import]
        private IBusinessEngineFactory _businessFactory;

        #endregion

        #region C-Tor

        /// <summary>
        /// default c-tor for wcf
        /// </summary>
        public InventoryManager()
        {

        }

        /// <summary>
        /// for test purposes
        /// </summary>
        /// <param name="repositoryFactory"></param>
        public InventoryManager(IDataRepositoryFactory repositoryFactory)
        {
            this._repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// for test purposes
        /// </summary>
        /// <param name="businessFactory"></param>
        public InventoryManager(IBusinessEngineFactory businessFactory)
        {
            this._businessFactory = businessFactory;
        }

        /// <summary>
        /// for test purposes
        /// </summary>
        /// <param name="repositoryFactory"></param>
        /// <param name="businessFactory"></param>
        public InventoryManager(IDataRepositoryFactory repositoryFactory, IBusinessEngineFactory businessFactory)
        {
            this._repositoryFactory = repositoryFactory;
            this._businessFactory = businessFactory;
        }

        #endregion

        #region IInventoryManager implementation

        public Product[] GetProducts()
        {
            return ExecuteFaultHandledOperation(() =>
            {
                var productRepository = this._repositoryFactory.GetDataRepository<IProductRepository>();
                var products = productRepository.GetProducts();

                return products.ToArray();
            });
        }

        public Product[] GetActiveProducts()
        {
            return ExecuteFaultHandledOperation(() =>
            {
                var productRepository = this._repositoryFactory.GetDataRepository<IProductRepository>();
                var products = productRepository.GetActiveProducts();

                return products;
            });
        }

        public Product GetProductById(int id, bool acceptNullable = false)
        {
            return ExecuteFaultHandledOperation(() =>
            {
                var productRepository = this._repositoryFactory.GetDataRepository<IProductRepository>();
                var product = productRepository.GetProductById(id);

                if (product == null && acceptNullable)
                {
                    return null;
                }

                if (product == null)
                {
                    var ex = new NotFoundException($"Product with id: {id} not found!");
                    throw new FaultException<NotFoundException>(ex, ex.Message);
                }

                return product;
            });
        }

        [TransactionFlow(TransactionFlowOption.Allowed)]
        [OperationBehavior(TransactionScopeRequired = true)]
        public Product UpdateProduct(Product product)
        {
            return ExecuteFaultHandledOperation(() =>
            {
                Product updatedEntity;
                var productRepository = this._repositoryFactory.GetDataRepository<IProductRepository>();
                if (product.ProductId == 0)
                {
                    product.ArticleNumber =
                        this._businessFactory.GetBusinessEngine<IProductInventoryEngine>().GenerateArticleNumber();
                }

                updatedEntity = productRepository.UpdateProduct(product);
                return updatedEntity;
            });
        }

        [TransactionFlow(TransactionFlowOption.Allowed)]
        [OperationBehavior(TransactionScopeRequired = true)]
        public void DeleteProduct(int productId)
        {
            throw new ArgumentException($"Product with id: {productId} not found!");
        }

        [TransactionFlow(TransactionFlowOption.Allowed)]
        [OperationBehavior(TransactionScopeRequired = true)]
        public void ActivateProduct(int productId)
        {
            throw new NotSupportedException($"Product with id: {productId} not found!");
        }

        #endregion

        #region IServiceBehavior implementation

        /// <summary>
        ///  validate per service if the contract has attribute 
        ///  with faultexception of T
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var enpoint in serviceDescription.Endpoints)
            {
                if (!enpoint.Contract.Name.Equals("IInventoryService")) continue;

                foreach (var operationDescription in enpoint.Contract.Operations)
                {
                    if (operationDescription.Name.Equals("DeleteProduct"))
                    {
                        if (operationDescription.Faults.FirstOrDefault(item => item.DetailType == typeof(ArgumentException)) == null)
                        {
                            throw new InvalidOperationException("DeleteProduct operation requires a fault contract for ArgumentException.");
                        }
                    }

                    if (operationDescription.Name.Equals("ActivateProduct"))
                    {
                        if (operationDescription.Faults.FirstOrDefault(item => item.DetailType == typeof(NotSupportedException)) == null)
                        {
                            throw new InvalidOperationException("ActivateProduct operation requires a fault contract for NotSupportedException.");
                        }
                    }
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}
