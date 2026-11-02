namespace AshproSAASERP.Shared.Services
{
    public class JSService(IJSRuntime js)
    {
        public async Task<string> GetLocal(string value)
        {
            try
            {
                string data = await js.InvokeAsync<string>("getLocalData", value);
                return data;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }
        public async Task SetLocal(string value, string key)
        {
            try
            {
                await js.InvokeVoidAsync("setLocalData", value, key);
            }
            catch (Exception ex) { string s = ex.Message; }
        }
        public async Task RemoveLocal(string key)
        {
            try
            {
                await js.InvokeVoidAsync("removeLocalData", key);
            }
            catch (Exception ex) { string s = ex.Message; }
        }

    }
}
