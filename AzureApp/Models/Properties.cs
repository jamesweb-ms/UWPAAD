// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using Newtonsoft.Json;

    public class Properties
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ProvisioningState { get; set; }
    }
}
