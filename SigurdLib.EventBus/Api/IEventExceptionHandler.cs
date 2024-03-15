using System;

namespace Sigurd.EventBus.Api;

public interface IEventExceptionHandler
{
    void HandleException(IEventBus bus, Event message, Exception exception);
}
