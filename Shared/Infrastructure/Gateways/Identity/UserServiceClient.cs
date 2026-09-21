using System.Threading.Tasks;
using Grpc.Core;

namespace Identifying.Application.Protos;

public partial class UserService
{
    public partial class UserServiceClient : IServiceClient<UserDto>
    {
        public async Task<UserDto> Create(UserDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<UserDto> IServiceClient<UserDto>.Read(UserDto dto) => Read(dto ?? new());
        public async Task<UserDto> Update(UserDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(UserDto dto) => (await DeleteAsync(dto)) != null;
    }
}
