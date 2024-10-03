using Perigee.Serializer;

namespace Samples.Utils;

public class BitPerson
{
    public string Name { get; set; }
    public DateTime DOB { get; set; }
    public byte Age { get; set; }
    public bool Active { get; set; }
    public BitPersonType PersonType { get; set; }
    public List<BitPerson> RelatedPeople { get; set; } = new List<BitPerson> { };

    public string NewField { get; set; } = "";

    [Bit2Ignore]
    public string ignoredExample { get; set; } = "";

    [Bit2PreSerialize]
    public void PreInvoke()
    {
        //Called before serialization.
        //Null out values, or store additional information needed after deserialization
    }

    [Bit2PostDeserialize]
    public void PostDes()
    {
        //Called after deserialization. Do whatever you need here!
    }
}

public class PartialBitPerson
{
    public string Name { get; set; }

    public List<PartialBitPerson> RelatedPeople { get; set; } = new List<PartialBitPerson> { };

}

public enum BitPersonType
{
    adult,
    child
}