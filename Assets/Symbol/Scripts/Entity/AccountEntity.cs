using System;
using System.Collections.Generic;

namespace SymbolEntity.Account
{
    [Serializable]
    public class Linked
    {
        public string publicKey;
    }

    [Serializable]
    public class Node
    {
        public string publicKey;
    }

    [Serializable]
    public class Vrf
    {
        public string publicKey;
    }

    [Serializable]
    public class Voting
    {
        public List<string> keys;
    }

    [Serializable]
    public class SupplementalPublicKeys
    {
        public Linked linked;
        public Node node;
        public Vrf vrf;
        public List<Voting> voting;
    }

    [Serializable]
    public class ActivityBucket
    {
        public string startHeight;
        public string totalFeesPaid;
        public int beneficiaryCount;
        public string rawScore;
    }

    [Serializable]
    public class Mosaic
    {
        public string id;
        public string amount;
    }

    [Serializable]
    public class Account
    {
        public int version;
        public string address;
        public string addressHeight;
        public string publicKey;
        public string publicKeyHeight;
        public int accountType;
        public SupplementalPublicKeys supplementalPublicKeys;
        public List<ActivityBucket> activityBuckets;
        public List<Mosaic> mosaics;
        public string importance;
        public string importanceHeight;
    }

    [Serializable]
    public class AccountDatum
    {
        public Account account;
        public string id;
    }

    [Serializable]
    public class AccountPagination
    {
        public int pageNumber;
        public int pageSize;
    }

    [Serializable]
    public class AccountRoot
    {
        public List<AccountDatum> data;
        public AccountPagination pagination;
    }
}