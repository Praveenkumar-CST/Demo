using WiseHRServer.Models;

namespace WiseHR.Interfaces
{
    public interface ISearchService
    {
        Task<List<EmployeeSearchResultDto>> SpotlightSearch(string query);
    }
}