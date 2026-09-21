using System.Threading.Tasks;
using Grpc.Core;

namespace Shopping.Application.Protos;

public partial class ProductService
{
    public partial class ProductServiceClient : IServiceClient<ProductDto>
    {
        public async Task<ProductDto> Create(ProductDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<ProductDto> IServiceClient<ProductDto>.Read(ProductDto dto) 
            => Read(dto ?? new() { Id = "0" });
        public async Task<ProductDto> Update(ProductDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(ProductDto dto) => (await DeleteAsync(dto)) != null;
    }
}
