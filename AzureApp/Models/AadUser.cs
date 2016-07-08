// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AADUser
    {
        public string ObjectId { get; set; }

        public string UserPrincipalName { get; set; }

        public string DisplayName { get; set; }

        [JsonProperty("givenName")]
        public string FirstName { get; set; }

        [JsonProperty("surname")]
        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string Department { get; set; }

        [JsonProperty("jobTitle")]
        public string Title { get; set; }

        [JsonProperty("mailNickname")]
        public string Alias { get; set; }

        [JsonProperty("otherMails")]
        public List<string> EmailAddresses { get; set; }

        [JsonProperty("telephoneNumber")]
        public string Telephone { get; set; }

        public string Mobile { get; set; }

        [JsonProperty("facsimileTelephoneNumber")]
        public string Fax { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string PostalCode { get; set; }

        public string PreferredLanguage { get; set; }

        public bool HasGraphAccess { get; set; }
    }
}
