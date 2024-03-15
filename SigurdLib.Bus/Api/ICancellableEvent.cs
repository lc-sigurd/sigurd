namespace Sigurd.Bus.Api;

public interface ICancellableEvent
{
    void SetCanceled(bool canceled) => ((Event)this)._isCancelled = canceled;

    bool IsCanceled => ((Event)this)._isCancelled;
}
