namespace Sigurd.Common.Core;

public interface IHolderOwner<THeld>
{
    bool CanSerializeIn(IHolderOwner<THeld> owner) => owner.Equals(this);
}
