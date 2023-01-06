using MessagePack;

namespace NatsMnemoschemeWriter
{
    [MessagePackObject]
    public struct TestDataStruct
    {
        [Key(0)]
        public int ParameterId { get; set; }
        [Key(1)]
        public float Value { get; set; }
    }
}
