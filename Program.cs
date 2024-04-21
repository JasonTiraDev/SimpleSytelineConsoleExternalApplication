using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static readonly HttpClient client = new HttpClient();

    static async Task Main()
    {
        // Fill out the following values. The serverName should be the name of the server where Infor Mongoose is running.
        string serverName = "localhost";
        string configName = "jtdemo_dals";
        string idoName = "UserNames";
        string properties = "Username,UserId,Status";

        // The username and password should be the credentials for the Infor Mongoose server
        // This example hard codes these values in the code. This is HORRIBLE practice and should never be done in production code.
        // In production code, you should use a secure method to store and retrieve these values.
        string username = "sa";
        string password = "sa";

        // Double check the urls here, http vs https and the paths
        try
        {
            string token = await GetSecurityToken($"http://{serverName}/IDORequestService/MGRESTService.svc", configName, username, password);
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Failed to obtain security token.");
                return;
            }

            string content = await LoadIDO($"https://{serverName}/IDORequestService", idoName, properties, token, configName);
            Console.WriteLine(content);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }


    // Get the security token for the specified configuration
    static async Task<string> GetSecurityToken(string baseUrl, string configName, string username, string password)
    {
        string requestUrl = $"{baseUrl}/js/token/{configName}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Add("userid", Uri.EscapeDataString(username));
        request.Headers.Add("password", Uri.EscapeDataString(password));

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to obtain security token: " + response.StatusCode);
                Console.ResetColor();
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent.Trim('"');
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception while obtaining token: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }


    // Load the specified IDO
    static async Task<string> LoadIDO(string baseUrl, string idoName, string properties, string token, string configName)
    {
        string requestUrl = $"{baseUrl}/ido/load/{idoName}?properties={Uri.EscapeDataString(properties)}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation("Authorization", token);
        }
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Infor-MongooseConfig", configName);

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to load IDO - StatusCode: {response.StatusCode}");
                Console.ResetColor();
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error while loading IDO: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}
