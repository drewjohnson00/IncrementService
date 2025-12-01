using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Increments.Pages
{
    public class IndexModel(IConfiguration configuration, ILogger<IndexModel> logger) : PageModel
    {
        public string ApiBaseAddress { get; private set; } = string.Empty;

        public void OnGet()
        {
            string? apiBaseAddressConfiguration = configuration["ApiBaseAddress"];
            const string defaultValue = "http://localhost:5000";

            if (string.IsNullOrWhiteSpace(apiBaseAddressConfiguration))
            {
                logger.LogWarning("API base address is not configured. Using default value of {DefaultValue}.", defaultValue);
                ApiBaseAddress = defaultValue;
                return;
            }

            ApiBaseAddress = apiBaseAddressConfiguration;
            logger.LogInformation("APIBaseAddress: {BaseAddress}", apiBaseAddressConfiguration);
        }
    }
}
