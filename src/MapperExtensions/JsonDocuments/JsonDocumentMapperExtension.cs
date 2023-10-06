using System.Text.Json;
using OregonNexus.Broker.Web.Models.JsonDocuments;
using OregonNexus.Broker.Web.ViewModels.IncomingRequests;
using OregonNexus.Broker.Web.ViewModels.OutgoingRequests;

namespace OregonNexus.Broker.Web.MapperExtensions.JsonDocuments;

public static class JsonDocumentMapperExtension
{
    public static JsonDocument MapToRequestManifestJsonModel(this CreateIncomingRequestViewModel viewModel)
    {
        var requestManifest = new RequestManifestJsonModel
        {
            RequestId = viewModel.RequestId,
            RequestType = "OregonNexus.Broker.Connector.Payload.StudentCumulativeRecord",
            Student = new StudentJsonModel
            {
                Id = viewModel.Id,
                StudentUniqueId = viewModel.StudentUniqueId,
                FirstName = viewModel.FirstName,
                MiddleName = viewModel.MiddleName,
                LastSurname = viewModel.LastSurname
            },
            From = new SchoolJsonModel
            {
                District = viewModel.FromDistrict,
                School = viewModel.FromSchool,
                Email = viewModel.FromEmail
            },
            To = new SchoolJsonModel
            {
                District = viewModel.ToDistrict,
                School = viewModel.ToSchool,
                Email = viewModel.ToEmail
            },
            Note = viewModel.Note,
            Contents = viewModel.Contents
        };
        return requestManifest.ToJsonDocument();
    }

    public static JsonDocument MapToSynergyJsonModel(this CreateIncomingRequestViewModel viewModel)
    {
        var synergyStudentModel = new SynergyJsonModel
        {
            Student = new SynergyStudentJsonModel
            {
                SisNumber = viewModel.SisNumber
            }
        };

        return synergyStudentModel.ToJsonDocument();
    }
    public static JsonDocument MapToEdfiStudentJsonModel(this CreateOutgoingRequestViewModel viewModel) => new EdfiJsonModel
    {
        Student = new EdfiStudentJsonModel
        {
            Id = viewModel.EdfiId,
            StudentUniqueId = viewModel.EdfiStudentUniqueId
        }
    }.ToJsonDocument();

    public static JsonDocument MapToResponseManifestJsonModel(this CreateOutgoingRequestViewModel viewModel)
    {
        var responseManifest = new ResponseManifestJsonModel
        {
            RequestId = Guid.NewGuid(),
            ResponseType = "OregonNexus.Broker.Connector.Payload.StudentCumulativeRecord",
            Student = new StudentJsonModel
            {
                Id = viewModel.Id,
                StudentUniqueId = viewModel.StudentUniqueId,
                FirstName = viewModel.FirstName,
                MiddleName = viewModel.MiddleName,
                LastSurname = viewModel.LastSurname
            },
            From = new SchoolJsonModel
            {
                District = viewModel.FromDistrict,
                School = viewModel.FromSchool,
                Email = viewModel.FromEmail
            },
            To = new SchoolJsonModel
            {
                District = viewModel.ToDistrict,
                School = viewModel.ToSchool,
                Email = viewModel.ToEmail
            },
            Note = viewModel.Note,
            Contents = viewModel.Contents
        };

        return responseManifest.ToJsonDocument();
    }
}
