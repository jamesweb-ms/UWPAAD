// ---------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public sealed class ODataResponse<T>
    {
        [JsonProperty("odata.metadata")]
        public string Metadata { get; set; }

        public IEnumerable<T> Value { get; set; }

        public string NextLink { get; set; }
    }
}
