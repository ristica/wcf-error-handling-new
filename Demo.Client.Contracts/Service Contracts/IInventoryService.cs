using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Core.Common.Exceptions;
using Demo.Client.Entities;
using Core.Common.Contracts;

namespace Demo.Client.Contracts
{
    [ServiceContract]
    public interface IInventoryService : IServiceContract
    {
        [OperationContract]
        Product[] GetProducts();

        [OperationContract]
        Product[] GetActiveProducts();

        [OperationContract]
        [FaultContract(typeof(NotFoundException))]
        Product GetProductById(int id, bool acceptNullable = false);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        Product UpdateProduct(Product product);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void DeleteProduct(int productId);

        [OperationContract]
        [FaultContract(typeof(NotSupportedException))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void ActivateProduct(int productId);
    }
}
