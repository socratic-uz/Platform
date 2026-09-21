using System.Threading.Tasks;
using Grpc.Core;
using SharedKernel.Abstractions;

namespace Ordering.Application.Protos;

public partial class OrderItemService
{
    public partial class OrderItemServiceClient : IServiceClient<OrderItemDto>
    {
        public async Task<OrderItemDto> Create(OrderItemDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<OrderItemDto> IServiceClient<OrderItemDto>.Read(OrderItemDto dto) => Read(dto ?? new());
        public async Task<OrderItemDto> Update(OrderItemDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(OrderItemDto dto) => (await DeleteAsync(dto)) != null;
    }
}
