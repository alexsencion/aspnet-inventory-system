using AngleSharp;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Helpers
{
    public static class AntiForgeryTokenExtractor
    {
        public static async Task<string> ExtractAntiForgeryToken(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(content));

            var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement;

            return tokenInput?.Value ?? string.Empty;
        }

        public static async Task<(HttpClient client, string token)> GetClientWithToken(
            HttpClient client,
            string getUrl)
        {
            var response = await client.GetAsync(getUrl);
            var token = await ExtractAntiForgeryToken(response);
            return (client, token);
        }
    }
}
