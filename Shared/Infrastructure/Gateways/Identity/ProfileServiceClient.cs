using System.Threading.Tasks;
using Grpc.Core;

namespace Identifying.Application.Protos;

public partial class ProfileService
{
    public partial class ProfileServiceClient : IServiceClient<ProfileDto>
    {
        public async Task<ProfileDto> Create(ProfileDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<ProfileDto> IServiceClient<ProfileDto>.Read(ProfileDto dto) => Read(dto ?? new());
        public async Task<ProfileDto> Update(ProfileDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(ProfileDto dto) => (await DeleteAsync(dto)) != null;
    }
}
