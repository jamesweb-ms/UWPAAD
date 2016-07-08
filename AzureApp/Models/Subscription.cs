// ---------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    public sealed class Subscription
    {
        public string Id { get; set; }

        public string SubscriptionId { get; set; }

        public string DisplayName { get; set; }

        public string State { get; set; }

        public SubscriptionPolicies SubscriptionPolices { get; set; }

        public override string ToString()
        {
            return "{0} ({1})".FormatInvariant(this.DisplayName, this.SubscriptionId);
        }
    }
}
