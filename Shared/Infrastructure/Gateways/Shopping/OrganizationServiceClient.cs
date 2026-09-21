using System.Threading.Tasks;
using Grpc.Core;
using SharedKernel.Abstractions;

namespace Shopping.Application.Protos;

public partial class OrganizationService
{
    public partial class OrganizationServiceClient : IServiceClient<OrganizationDto>
    {
        public async Task<OrganizationDto> Create(OrganizationDto dto) => await CreateAsync(dto);
        AsyncServerStreamingCall<OrganizationDto> IServiceClient<OrganizationDto>.Read(OrganizationDto dto) 
            => Read(dto ?? new() { Id = "0" });
        public async Task<OrganizationDto> Update(OrganizationDto dto) => await UpdateAsync(dto);
        public async Task<bool> Delete(OrganizationDto dto) => (await DeleteAsync(dto)) != null;
    }
}
