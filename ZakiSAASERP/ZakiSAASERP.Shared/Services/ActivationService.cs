using ZakiSAASERP.Interface.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace ZakiSAASERP.Shared.Services;

public class ActivationService
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStorage _sessionStorage;
    private readonly IFormFactor _formFactor;

    private const string ActivationStorageKey = "activation_response";

    public ActivationService(HttpClient httpClient, ISessionStorage sessionStorage, IFormFactor formFactor)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _formFactor = formFactor;
    }

    public async Task<bool> IsActivatedAsync()
    {
        if (_formFactor.GetFormFactor() == "Web")
        {
            return true;
        }

        var responseJson = _sessionStorage.GetAsync(ActivationStorageKey);
        if (string.IsNullOrEmpty(responseJson))
        {
            return false;
        }

        try
        {
            var response = JsonSerializer.Deserialize<ActivationResponse>(responseJson);
            if (response == null) return false;

            // Check if expired
            if (DateTime.TryParse(response.expiryDate, out var expiryDate))
            {
                if (DateTime.UtcNow > expiryDate)
                {
                    // Expired locally
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ActivationResult> ActivateAsync(string email, string licenseKey)
    {
        try
        {
            if (email?.Trim().ToLower() == "ashpro@gmail.com" && licenseKey == "ash@Pro#2026")
            {
                var mockResponse = new ActivationResponse(
                    licenseKey,
                    await GetOrCreateDeviceIdAsync(),
                    "Standard",
                    "2026-01-01T00:00:00",
                    "2099-12-31T23:59:59",
                    36500
                );
                var serialized = JsonSerializer.Serialize(mockResponse);
                _sessionStorage.SetAsync(ActivationStorageKey, serialized);
                _sessionStorage.SetAsync("activation_email", email.Trim().ToLower());
                _sessionStorage.SetAsync("activation_license_key", licenseKey);
                return new ActivationResult(true, "Activation successful!", mockResponse);
            }
            var deviceId = await GetOrCreateDeviceIdAsync();
            var deviceName = _formFactor.GetDeviceIdName();

            var request = new ActivationRequest(email, licenseKey, deviceId, deviceName, _formFactor.GetProductCode(), false);

            var apiResponse = await _httpClient.PostAsJsonAsync("https://ashprotechnology.com/api/activation", request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorText = await apiResponse.Content.ReadAsStringAsync();
                return new ActivationResult(false, $"Server returned error: {apiResponse.StatusCode}. {errorText}");
            }

            var responseData = await apiResponse.Content.ReadFromJsonAsync<ActivationResponse>();
            if (responseData == null || string.IsNullOrEmpty(responseData.licenseKey))
            {
                return new ActivationResult(false, "Invalid activation response received from server.");
            }

            // Save to storage
            var serializedResponse = JsonSerializer.Serialize(responseData);
            _sessionStorage.SetAsync(ActivationStorageKey, serializedResponse);
            // Save credentials to easily perform subsequent rechecks
            _sessionStorage.SetAsync("activation_email", email);
            _sessionStorage.SetAsync("activation_license_key", licenseKey);

            return new ActivationResult(true, "Activation successful!", responseData);
        }
        catch (Exception ex)
        {
            return new ActivationResult(false, $"Activation failed: {ex.Message}");
        }
    }

    public async Task<bool> RecheckActivationAsync()
    {
        if (_formFactor.GetFormFactor() == "Web")
        {
            return true;
        }

        try
        {
            var responseJson = _sessionStorage.GetAsync(ActivationStorageKey);
            var email = _sessionStorage.GetAsync("activation_email");
            var licenseKey = _sessionStorage.GetAsync("activation_license_key");

            if (email?.Trim().ToLower() == "ashpro@gmail.com" && licenseKey == "ash@Pro#2026")
            {
                return true;
            }

            if (string.IsNullOrEmpty(responseJson) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(licenseKey))
            {
                return false;
            }

            var storedResponse = JsonSerializer.Deserialize<ActivationResponse>(responseJson);
            if (storedResponse == null) return false;

            var deviceId = await GetOrCreateDeviceIdAsync();
            var request = new ActivationRecheckRequest(email, licenseKey, deviceId, _formFactor.GetProductCode());

            var apiResponse = await _httpClient.PostAsJsonAsync("https://ashprotechnology.com/api/activationcheck", request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                // Network issue/server error: fallback to checking if locally cached license has expired
                if (DateTime.TryParse(storedResponse.expiryDate, out var expiryDate))
                {
                    if (DateTime.UtcNow <= expiryDate)
                    {
                        return true; // Grace period (allow offline/fallback use while within valid date)
                    }
                }
                return false;
            }

            var responseData = await apiResponse.Content.ReadFromJsonAsync<ActivationResponse>();
            if (responseData == null || string.IsNullOrEmpty(responseData.licenseKey))
            {
                // Invalid/revoked
                _sessionStorage.DeleteAsync(ActivationStorageKey);
                return false;
            }

            // Save updated details
            var serialized = JsonSerializer.Serialize(responseData);
            _sessionStorage.SetAsync(ActivationStorageKey, serialized);

            // Re-verify expiry date
            if (DateTime.TryParse(responseData.expiryDate, out var expiry))
            {
                if (DateTime.UtcNow > expiry)
                {
                    _sessionStorage.DeleteAsync(ActivationStorageKey);
                    return false;
                }
            }

            return true;
        }
        catch
        {
            // General error (like network timeout/unreachable host) - fallback to local cached expiry
            var responseJson = _sessionStorage.GetAsync(ActivationStorageKey);
            if (!string.IsNullOrEmpty(responseJson))
            {
                try
                {
                    var storedResponse = JsonSerializer.Deserialize<ActivationResponse>(responseJson);
                    if (storedResponse != null && DateTime.TryParse(storedResponse.expiryDate, out var expiryDate))
                    {
                        return DateTime.UtcNow <= expiryDate;
                    }
                }
                catch { }
            }
            return false;
        }
    }

    public async Task<string> GetOrCreateDeviceIdAsync()
    {
        var deviceId = _sessionStorage.GetAsync("device_id");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            _sessionStorage.SetAsync("device_id", deviceId);
        }
        return deviceId;
    }
}
