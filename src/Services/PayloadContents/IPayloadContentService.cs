using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Models.Results;

namespace OregonNexus.Broker.Web.Services.PayloadContents
{
    public interface IPayloadContentService
    {
        Task<IServiceResult<string>> AddPayloadContentsAsync(
            IFormFileCollection files,
            Guid requestId);

        Task<IEnumerable<PayloadContent>> ToPayloadContentsAsync(
            IFormFileCollection files,
            Guid requestId);
    }
}