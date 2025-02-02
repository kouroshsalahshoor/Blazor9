using Blazor9.Auto.Client.Models;

namespace Blazor9.Auto.Client.Services;

public interface IApiService
{
    Task<IEnumerable<Band>> CallLocalApiAsync();
    Task<IEnumerable<Band>> CallRemoteApiAsync();

}
