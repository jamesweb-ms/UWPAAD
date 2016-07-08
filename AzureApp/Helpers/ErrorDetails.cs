// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "Error")]
    public sealed class ErrorDetails
    {
        [DataMember]
        [JsonProperty("Code")]
        public int Code { get; set; }

        [DataMember]
        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}
