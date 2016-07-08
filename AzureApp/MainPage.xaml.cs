// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AzureApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    using System.Net.Http;
     using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string userEmail = null;
        string tenant = null;
        const string AzureManagementVersion = "2015-01-01";
        const string GraphManagementVersion = "1.6";

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Note:  this can be changed for different national clouds
        const string authUri = "https://login.microsoftonline.com/";
        const string azureAuthUri = "https://management.core.windows.net/";
        const string azureUri = "https://management.azure.com/";
        const string graphUri = "https://graph.windows.net/";
        static SolidColorBrush errorColor = new SolidColorBrush(Colors.Red);
        static SolidColorBrush okColor = new SolidColorBrush(Colors.White);

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] user = userName.Text.Split('@');
            if (user.Length != 2)
            {
                userName.Foreground = errorColor;
                return;
            }
            this.userEmail = userName.Text;
            this.tenant = user[1];

            var authResult = await AuthHelper.GetAuthenticationResult(user[1], authUri, azureAuthUri, userName.Text);
            if (authResult == null)
            {
                userName.Background = errorColor;
            }

            userName.Background = okColor;

            // Get tenants from Azure (original tenant is fine)
            var tenants = await this.GetTenants();
            foreach (var tenant in tenants)
            {
                detailList.Items.Add("Tenant: {0}".FormatInvariant(tenant.TenantId));
                this.tenant = tenant.TenantId;

                // Try accessing Graph
                var aadUser = await this.GetUser();
                if (aadUser != null)
                {
                    detailList.Items.Add("  Display Name: {0}".FormatInvariant(aadUser.DisplayName));
                    detailList.Items.Add("  Email: {0}".FormatInvariant(string.Join(",", aadUser.EmailAddresses)));
                }

                // Try accessing Azure
                var subscriptions = await this.GetSubscriptions();
                foreach (var subscription in subscriptions)
                {
                    detailList.Items.Add("    Subscription Name: {0}".FormatInvariant(subscription.DisplayName));
                    detailList.Items.Add("    Subscription Id: {0}".FormatInvariant(subscription.SubscriptionId));
                    var resourceGroups = await this.GetResourceGroups(subscription.SubscriptionId);
                    foreach (var resourceGroup in resourceGroups)
                    {
                        detailList.Items.Add("      Resource Group Name: {0}".FormatInvariant(resourceGroup.Name));
                        detailList.Items.Add("      Location: {0}".FormatInvariant(resourceGroup.Location));
                    }
                }
            }
        }

        async Task<IEnumerable<Tenant>> GetTenants()
        {
            return (await this.GetAzureResource<ODataResponse<Tenant>>("tenants")).Value;
        }

        async Task<IEnumerable<Subscription>> GetSubscriptions()
        {
            return (await this.GetAzureResource<ODataResponse<Subscription>>("subscriptions")).Value.Where(sub => sub.State.Equals("Enabled", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<ResourceGroup>> GetResourceGroups(string subscriptionId)
        {
            return (await this.GetAzureResource<ODataResponse<ResourceGroup>>("subscriptions/{0}/resourcegroups".FormatInvariant(subscriptionId))).Value;
        }

        public Task<ResourceGroup> GetResourceGroup(string subscriptionId, string resourceGroupName)
        {
            return this.GetAzureResource<ResourceGroup>("subscriptions/{0}/resourcegroups/{1}".FormatInvariant(subscriptionId, resourceGroupName), new List<HttpStatusCode>() { HttpStatusCode.NotFound }, AzureManagementVersion);
        }

        public Task<AADUser> GetUser()
        {
            return GetGraphResource<AADUser>("me");
        }

        Task<T> GetAzureResource<T>(string resource, string apiVersion = AzureManagementVersion)
        {
            return this.GetAzureResource<T>(resource, null, apiVersion);
        }

        async Task<T> GetAzureResource<T>(string resource, List<HttpStatusCode> ignoreStatusCodes, string apiVersion)
        {
            var token = (await AuthHelper.GetAuthenticationResult(tenant, authUri, azureAuthUri, userEmail)).CreateAuthorizationHeader();
            var apiString = string.Empty;
            if (!string.IsNullOrEmpty(apiVersion))
            {
                apiString = "?api-version={0}".FormatInvariant(apiVersion);
            }
            return await RequestHelper.SendRequest<T>(
                HttpMethod.Get,
                "{0}{1}{2}".FormatInvariant(azureUri, resource, apiString),
                token,
                apiVersion,
                null,
                ignoreStatusCodes);
        }

        async Task<T> GetGraphResource<T>(string resource, string query = "")
        {
            var token = (await AuthHelper.GetAuthenticationResult(tenant, authUri, graphUri, userEmail)).AccessToken;
            return await RequestHelper.SendRequest<T>(
                HttpMethod.Get,
                "{0}{1}/{2}?api-version={3}{4}".FormatInvariant(graphUri, tenant, resource, GraphManagementVersion, query),
                token,
                GraphManagementVersion,
                null,
                new List<HttpStatusCode>() { HttpStatusCode.Forbidden, HttpStatusCode.NotFound });
        }
    }
}
