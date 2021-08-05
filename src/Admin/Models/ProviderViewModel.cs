﻿using System.Collections.Generic;
using System.Linq;
using Bit.Core.Enums.Provider;
using Bit.Core.Models.Data;
using Bit.Core.Models.Table.Provider;

namespace Bit.Admin.Models
{
    public class ProviderViewModel
    {
        public ProviderViewModel() { }

        public ProviderViewModel(Provider provider, IEnumerable<ProviderUserUserDetails> providerUsers)
        {
            Provider = provider;
            UserCount = providerUsers.Count();
            ProviderAdmins = providerUsers.Where(u => u.Type == ProviderUserType.ProviderAdmin);
        }

        public int UserCount { get; set; }
        public Provider Provider { get; set; }
        public IEnumerable<ProviderUserUserDetails> ProviderAdmins { get; set; }
    }
}
