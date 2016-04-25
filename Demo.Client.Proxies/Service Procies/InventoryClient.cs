using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Core.Common.ServiceModel;
using Demo.Client.Contracts;
using Demo.Client.Entities;

namespace Demo.Client.Proxies.Service_Procies
{
    [Export(typeof(IInventoryService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class InventoryClient : UserClientBase<IInventoryService>, IInventoryService
    {
        /// <summary>
        /// use this as importing c-tor when
        /// no dynamically endpoint discovery used and for unit tests!
        /// use [ImportingConstructor] if using hardcoded endpoint
        /// </summary>
        [ImportingConstructor]
        public InventoryClient()
        {

        }

        /// <summary>
        /// if using dynamic endpoint discovery
        /// use [ImportingConstructor] if using discovering the service
        /// </summary>
        /// <param name="endpointName"></param>
        //[ImportingConstructor]
        public InventoryClient([Import("Dynamic.Endpoint")] string endpointName) 
            : base(endpointName)
        {

        }

        #region IInventoryService implementation

        public Product[] GetProducts()
        {
            return Channel.GetProducts();
        }

        public Product[] GetActiveProducts()
        {
            return Channel.GetActiveProducts();
        }

        public Product GetProductById(int id, bool acceptNullable = false)
        {
            return Channel.GetProductById(id, acceptNullable);
        }

        public Product UpdateProduct(Product product)
        {
            return Channel.UpdateProduct(product);
        }

        public void DeleteProduct(int productId)
        {
            Channel.DeleteProduct(productId);
        }

        public void ActivateProduct(int productId)
        {
            Channel.ActivateProduct(productId);
        }

        #endregion
    }
}
