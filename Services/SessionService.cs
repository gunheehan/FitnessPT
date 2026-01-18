using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FitnessPT.Services;

public interface ISession
{
    public Task<T> Get<T>(string sessionKey);
    public Task<bool> Set<T>(string sessionKey, T data);
    public Task<bool> Delete(string sessionKey);
}

public class SessionService : ISession
{
    private readonly ProtectedSessionStorage sessionStorage;

    public SessionService(ProtectedSessionStorage session)
    {
        sessionStorage = session;
    }


    public async Task<T> Get<T>(string sessionKey)
    {
        try
        {
            var resultValue = sessionStorage.GetAsync<T>(sessionKey).Result.Value;
            if (resultValue != null)
            {
                var result = resultValue;
                return result;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return default;
    }

    public async Task<bool> Set<T>(string sessionKey, T data)
    {
        try
        {
            await sessionStorage.SetAsync(sessionKey, data);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        return false;
    }

    public async Task<bool> Delete(string sessionKey)
    {
        try
        {
            await sessionStorage.SetAsync(sessionKey, null);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        return false;
    }
}