using System;
using System.Collections.Generic;

namespace SymbolEntity.Metadata
{
    [Serializable]
    public class MetadataEntry
    {
        public int version;
        public string compositeHash;
        public string sourceAddress;
        public string targetAddress;
        public string scopedMetadataKey;
        public string targetId;
        public int metadataType;
        public int valueSize;
        public string value;
    }

    [Serializable]
    public class MetadataDatum
    {
        public MetadataEntry metadataEntry;
        public string id;
    }

    [Serializable]
    public class MetadataPagination
    {
        public int pageNumber;
        public int pageSize;
    }

    [Serializable]
    public class MetadataRoot
    {
        public List<MetadataDatum> data;
        public MetadataPagination pagination;
    }
}