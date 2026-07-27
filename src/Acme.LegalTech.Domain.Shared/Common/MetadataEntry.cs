namespace Acme.LegalTech.Common;

public enum MetadataValueType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3
}

public class MetadataEntry
{
    public string Key { get; }

    public string Value { get; }

    public MetadataValueType ValueType { get; }

    protected MetadataEntry()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public MetadataEntry(string key, string value, MetadataValueType valueType = MetadataValueType.Text)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }
}
