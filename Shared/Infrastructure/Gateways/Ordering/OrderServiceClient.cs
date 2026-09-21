using System.Threading.Tasks;
using Grpc.Core;
using SharedKernel.Abstractions;

namespace Ordering.Application.Protos;

public partial class OrderService
{
    public partial class OrderServiceClient : IServiceClient<OrderDto>
    {
        public async Task<OrderDto> Create(OrderDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<OrderDto> IServiceClient<OrderDto>.Read(OrderDto dto) 
            => Read(dto ?? new() { Id = "0" });
        public async Task<OrderDto> Update(OrderDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(OrderDto dto) => (await DeleteAsync(dto)) != null;
    }
}
