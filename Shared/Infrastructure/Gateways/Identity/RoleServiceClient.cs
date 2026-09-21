using System.Threading.Tasks;
using Grpc.Core;

namespace Identifying.Application.Protos;

public partial class RoleService
{
    public partial class RoleServiceClient : IServiceClient<RoleDto>
    {
        public async Task<RoleDto> Create(RoleDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<RoleDto> IServiceClient<RoleDto>.Read(RoleDto dto) => Read(dto ?? new());
        public async Task<RoleDto> Update(RoleDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(RoleDto dto) => (await DeleteAsync(dto)) != null;
    }
}
