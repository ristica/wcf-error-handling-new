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
using System.ServiceModel.Description;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Demo.Business.Managers
{
    /// <summary>
    /// do set initialization per call and not per session (default)
    /// because it is not scalable
    /// set concurency mode to multiple (default = single) because we have per call situation
    /// set ReleaseServiceInstanceOnTransactionComplete to true if there will be at least 
    /// one operation with attribute TransactionScopeRequired = true
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall, 
        ConcurrencyMode = ConcurrencyMode.Multiple, 
        ReleaseServiceInstanceOnTransactionComplete = false)]
    public class InventoryManager : ManagerBase, IInventoryService, IErrorHandler, IServiceBehavior
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
            var ex = new ArgumentException($"Product with id: {productId} not found!");
            throw new FaultException<ArgumentException>(ex, ex.Message);
        }

        [TransactionFlow(TransactionFlowOption.Allowed)]
        [OperationBehavior(TransactionScopeRequired = true)]
        public void ActivateProduct(int productId)
        {
            var ex = new NotImplementedException($"Product with id: {productId} not found!");
            throw new FaultException<NotImplementedException>(ex, ex.Message);
        }

        #endregion

        #region IErrorHandler implementation

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is ArgumentException)
            {
                var faultException = new FaultException<ArgumentException>(new ArgumentException(error.Message), error.Message);
                fault = Message.CreateMessage(version, faultException.CreateMessageFault(), faultException.Action);
            }
            else if (error is NotImplementedException)
            {
                var faultException = new FaultException<NotImplementedException>(new NotImplementedException(error.Message), error.Message);
                fault = Message.CreateMessage(version, faultException.CreateMessageFault(), faultException.Action);
            }
            else
            {
                fault = null;
            }
        }

        public bool HandleError(Exception error)
        {
            return true;
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
                        if (operationDescription.Faults.FirstOrDefault(item => item.DetailType.Equals(typeof(ArgumentException))) == null)
                        {
                            throw new InvalidOperationException("DeleteProduct operation requires a fault contract for ArgumentException.");
                        }
                    }

                    if (operationDescription.Name.Equals("ActivateProduct"))
                    {
                        if (operationDescription.Faults.FirstOrDefault(item => item.DetailType.Equals(typeof(NotImplementedException))) == null)
                        {
                            throw new InvalidOperationException("ActivateProduct operation requires a fault contract for NotImplementedException.");
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
