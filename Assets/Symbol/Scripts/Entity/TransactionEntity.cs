using System;
using System.Collections.Generic;

namespace SymbolEntity.Transaction
{
    [Serializable]
    public class Meta
    {
        public string height;
        public string hash;
        public string merkleComponentHash;
        public int index;
        public string timestamp;
        public int feeMultiplier;
        public string aggregateHash;
        public string aggregateId;
    }

    [Serializable]
    public class InnerTransactionDatum
    {
        public Meta meta;
        public string id;
        public InnerTransaction transaction;
    }

    [Serializable]
    public class Transaction
    {
        public int size;
        public string signature;
        public string signerPublicKey;
        public int version;
        public int network;
        public int type;
        public string maxFee;
        public string deadline;
        public string recipientAddress;
        public string message;
        public List<Cosignatures> cosignatures;
        public List<Mosaic> mosaics;
        public int nonce;
        public string id;
        public int flags;
        public byte divisibility;
        public string duration;
        public string mosaicId;
        public int action;
        public string delta;
        public string amount;
        public int hashAlgorithm;
        public string secret;
        public string proof;
        public string targetAddress;
        public string scopedMetadataKey;
        public int valueSizeDelta;
        public int valueSize;
        public string value;
        public string targetMosaicId;
        public int minRemovalDelta;
        public int minApprovalDelta;
        public List<string> addressAdditions;
        public List<string> addressDeletions;
        public List<InnerTransactionDatum> transactions;
    }

    [Serializable]
    public class InnerTransaction
    {
        public int size;
        public string signature;
        public string signerPublicKey;
        public int version;
        public int network;
        public int type;
        public string maxFee;
        public string deadline;
        public string recipientAddress;
        public string message;
        public List<Cosignatures> cosignatures;
        public List<Mosaic> mosaics;
        public int nonce;
        public string id;
        public int flags;
        public byte divisibility;
        public string duration;
        public string mosaicId;
        public int action;
        public string delta;
        public string amount;
        public int hashAlgorithm;
        public string secret;
        public string proof;
        public string targetAddress;
        public string scopedMetadataKey;
        public int valueSizeDelta;
        public int valueSize;
        public string value;
        public string targetMosaicId;
        public int minRemovalDelta;
        public int minApprovalDelta;
        public List<string> addressAdditions;
        public List<string> addressDeletions;
    }

    [Serializable]
    public class Cosignatures
    {
        public string version;
        public string signerPublicKey;
        public string signature;
    }

    [Serializable]
    public class Mosaic
    {
        public string id;
        public int amount;
    }

    [Serializable]
    public class Datum
    {
        public Meta meta;
        public string id;
        public Transaction transaction;
    }

    [Serializable]
    public class Pagination
    {
        public int pageNumber;
        public int pageSize;
    }

    [Serializable]
    public class Root
    {
        public List<Datum> data;
        public Pagination pagination;
    }
}

