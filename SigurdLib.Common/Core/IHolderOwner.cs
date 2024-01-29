namespace Sigurd.Common.Core;

public interface IHolderOwner<THeld>
{
    bool canSerializeIn(IHolderOwner<THeld> owner) => owner.Equals(this);
}
