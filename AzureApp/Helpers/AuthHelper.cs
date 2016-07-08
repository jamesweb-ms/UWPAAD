// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public static class AuthHelper
    {
        const string publicClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        static Uri publicRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        public static async Task<AuthenticationResult> GetAuthenticationResult(string tenant, string authUri, string resourceUri, string user)
        {
            var authority = "{0}{1}".FormatInvariant(authUri, tenant);
            var authContext = new AuthenticationContext(authority, true);
            var userId = new UserIdentifier(user, UserIdentifierType.OptionalDisplayableId);
            try
            {
                return await authContext.AcquireTokenAsync(resourceUri, publicClientId, publicRedirectUri, PromptBehavior.Auto, userId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
