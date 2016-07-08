// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System;

    public sealed class Tenant
    {
        public string Id { get; set; }

        public string IssValue { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public string TenantId { get; set; }

        public string DirectoryName { get; set; }

        public string Domain { get; set; }

        public bool AdminConsented { get; set; }
    }
}
