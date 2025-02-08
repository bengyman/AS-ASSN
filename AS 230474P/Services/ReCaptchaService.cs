using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ReCaptchaService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ReCaptchaService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
        var response = await _httpClient.GetStringAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}");

        var result = JsonSerializer.Deserialize<ReCaptchaResponse>(response);
        return result != null && result.Success && result.Score >= 0.5;
    }
}

public class ReCaptchaResponse
{
    public bool Success { get; set; }
    public float Score { get; set; }
    public string Action { get; set; }
}
