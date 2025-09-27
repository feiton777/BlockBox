using System;
using System.Collections.Generic;

namespace SymbolEntity.Multisig
{
    [Serializable]
    public class Multisig
    {
        public int version;
        public string accountAddress;
        public int minApproval;
        public int minRemoval;
        public List<string> cosignatoryAddresses;
        public List<string> multisigAddresses;
    }

    [Serializable]
    public class MultisigRoot
    {
        public Multisig multisig;
    }
}