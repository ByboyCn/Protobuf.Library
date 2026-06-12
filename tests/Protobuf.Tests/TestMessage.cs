using Protobuf.Core;

namespace Test;

/// <summary>
/// 手写的测试消息类，用于测试序列化功能
/// </summary>
public class SimpleMessage : IMessage<SimpleMessage>
{
    #region Private Fields

    private int _id = 0;
    private string _name = "";
    private bool _active = false;

    #endregion

    #region Has Methods

    public bool HasId => _id != 0;
    public bool HasName => _name != "";
    public bool HasActive => _active != false;

    #endregion

    #region Properties

    public int Id => _id;
    public string Name => _name;
    public bool Active => _active;

    #endregion

    #region Setters

    public void SetId(int value)
    {
        _id = value;
    }

    public void SetName(string value)
    {
        _name = value ?? "";
    }

    public void SetActive(bool value)
    {
        _active = value;
    }

    #endregion

    #region Clear Methods

    public void ClearId()
    {
        _id = 0;
    }

    public void ClearName()
    {
        _name = "";
    }

    public void ClearActive()
    {
        _active = false;
    }

    #endregion

    #region IMessage Implementation

    public void MergeFrom(SimpleMessage other)
    {
        if (other == null) return;

        if (other.HasId) SetId(other.Id);
        if (other.HasName) SetName(other.Name);
        if (other.HasActive) SetActive(other.Active);
    }

    public void MergeFrom(CodedInputStream input)
    {
        while (true)
        {
            var tag = input.ReadTag();
            if (tag == 0) break;

            var fieldNumber = WireFormat.GetTagFieldNumber(tag);
            var wireType = WireFormat.GetTagWireType(tag);

            if (fieldNumber == 1 && wireType == WireFormat.VarintType)
            {
                SetId(input.ReadInt32());
            }
            else if (fieldNumber == 2 && wireType == WireFormat.LengthDelimitedType)
            {
                SetName(input.ReadString());
            }
            else if (fieldNumber == 3 && wireType == WireFormat.VarintType)
            {
                SetActive(input.ReadBool());
            }
            else
            {
                input.SkipField(tag);
            }
        }
    }

    public void WriteTo(CodedOutputStream output)
    {
        if (HasId)
        {
            output.WriteTag(0x08); // field 1, wire type 0
            output.WriteInt32(_id);
        }

        if (HasName)
        {
            output.WriteTag(0x12); // field 2, wire type 2
            output.WriteString(_name);
        }

        if (HasActive)
        {
            output.WriteTag(0x18); // field 3, wire type 0
            output.WriteBool(_active);
        }
    }

    public int CalculateSize()
    {
        int size = 0;

        if (HasId)
        {
            size += SizeCalculator.ComputeTagSize(1);
            size += SizeCalculator.ComputeInt32Size(_id);
        }

        if (HasName)
        {
            size += SizeCalculator.ComputeTagSize(2);
            size += SizeCalculator.ComputeStringSize(_name);
        }

        if (HasActive)
        {
            size += SizeCalculator.ComputeTagSize(3);
            size += SizeCalculator.ComputeBoolSize(_active);
        }

        return size;
    }

    public bool IsInitialized() => true;

    public SimpleMessage Clone()
    {
        var clone = new SimpleMessage();
        clone.MergeFrom(this);
        return clone;
    }

    #endregion

    #region Parser

    public static readonly MessageParser<SimpleMessage> Parser =
        new MessageParser<SimpleMessage>(() => new SimpleMessage());

    #endregion
}
