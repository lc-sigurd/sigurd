using System;
using System.Collections.Concurrent;
using System.Configuration.Assemblies;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Sigurd.Bus.Api;
using Sigurd.Bus.Listener;

namespace Sigurd.Bus;

internal class EventListenerFactoryManager
{
    private static EventListenerFactoryManager? _instance;
    public static EventListenerFactoryManager Instance => _instance ??= new();

    private const string RuntimeEmittedAssemblyName = "com.sigurd.sigurd.emitted-event-listeners";
    protected AssemblyBuilder? RuntimeEmittedAssembly;
    protected ModuleBuilder? RuntimeEmittedModule;

    private const string RuntimeEmitToNamespace = "Sigurd.EventBus.RuntimeEmitted";

    delegate IEventListener EventListenerFactory(object? target);

    private readonly ConcurrentDictionary<MethodInfo, EventListenerFactory> _eventListenerFactories = new();

    private EventListenerFactory GetEventListenerFactory(MethodInfo method)
    {
        return _eventListenerFactories.GetOrAdd(method, ComputeListenerFactory);
    }

    private EventListenerFactory ComputeListenerFactory(MethodInfo method)
    {
        try {
            EnsureRuntimeAssembly();
            if (!method.IsAccessible(RuntimeEmittedAssembly))
                throw new MethodAccessException(
                    $"{method} is not sufficiently accessible to be invoked as an event listener.\n" +
                    $"Ensure {method}, its declaring type, and enclosing types are `public`."
                );

            var listenerImplementationType = MakeWrapperType(method);
            return method.IsStatic switch {
                true => StaticFactory,
                false => InstanceFactory,
            };

            IEventListener StaticFactory(object? _) => (IEventListener)Activator.CreateInstance(listenerImplementationType);
            IEventListener InstanceFactory(object? target) => (IEventListener)Activator.CreateInstance(listenerImplementationType, [target]);
        }
        catch (Exception exc) {
            throw new Exception($"Failed to emit {nameof(IEventListener)} implementation for {method}", exc);
        }
    }

    [MemberNotNull(nameof(RuntimeEmittedAssembly))]
    protected void EnsureRuntimeAssembly()
    {
        if (RuntimeEmittedAssembly is not null) return;

        var assemblyName = new AssemblyName(RuntimeEmittedAssemblyName) {
            CultureInfo = CultureInfo.InvariantCulture,
            Flags = AssemblyNameFlags.None,
            ProcessorArchitecture = ProcessorArchitecture.MSIL,
            VersionCompatibility = AssemblyVersionCompatibility.SameDomain,
        };
        RuntimeEmittedAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
    }

    [MemberNotNull(nameof(RuntimeEmittedModule))]
    protected void EnsureRuntimeModule()
    {
        if (RuntimeEmittedModule is not null) return;

        EnsureRuntimeAssembly();
        RuntimeEmittedModule = RuntimeEmittedAssembly.DefineDynamicModule(RuntimeEmittedAssemblyName);
    }

    private Type MakeWrapperType(MethodInfo method)
    {
        EnsureRuntimeModule();
        var builder = RuntimeEmittedModule.DefineType(ComputeEmittedTypeName(method), TypeAttributes.Sealed, typeof(EmittedEventListener));
        PopulateWrapperType(method, builder);
        return builder.CreateTypeInfo();
    }

    private static string ComputeEmittedTypeName(MethodInfo method)
        => $"{RuntimeEmitToNamespace}.{method.DeclaringType!.FullName}.{method.Name}_{method.GetParameters()[0].ParameterType.Name}";

    protected void PopulateWrapperType(MethodInfo method, TypeBuilder builder)
    {
        const string targetFieldName = "Target";

        builder.AddInterfaceImplementation(typeof(IEventListener));

        FieldBuilder? targetField = null;

        DefineTargetField();
        DefineConstructor();
        DefineInvokeImplementation();
        return;

        void DefineTargetField()
        {
            if (method.IsStatic) return;
            targetField = builder.DefineField(targetFieldName, typeof(object), FieldAttributes.Public);
        }

        void DefineConstructor()
        {
            var ctor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, method.IsStatic ? Array.Empty<Type>() : [typeof(object)]);
            var ctorBodyGenerator = ctor.GetILGenerator();

            #region Call base type constructor
            ctorBodyGenerator.Emit(OpCodes.Ldarg_0); // push 'this' onto stack
            ctorBodyGenerator.Emit(OpCodes.Call, typeof(object).GetConstructor(Array.Empty<Type>()));
            #endregion

            #region Initialize 'target' field from constructor argument
            if (!method.IsStatic) {
                ctorBodyGenerator.Emit(OpCodes.Ldarg_0); // push 'this' onto stack
                ctorBodyGenerator.Emit(OpCodes.Ldarg_1); // push first ctor argument onto stack
                ctorBodyGenerator.Emit(OpCodes.Stfld, targetField); // store [pop stack] first ctor argument in 'targetField' of [pop stack] 'this'
            }
            #endregion

            ctorBodyGenerator.Emit(OpCodes.Ret);
        }

        void DefineInvokeImplementation()
        {
            var invokeMethod = builder.DefineMethod(nameof(IEventListener.Invoke), MethodAttributes.Public | MethodAttributes.Virtual, null, [typeof(Event)]);
            var invokeMethodBodyGenerator = invokeMethod.GetILGenerator();

            #region Load 'target' field onto the stack
            if (!method.IsStatic) {
                invokeMethodBodyGenerator.Emit(OpCodes.Ldarg_0); // push 'this' onto stack
                invokeMethodBodyGenerator.Emit(OpCodes.Ldfld, targetField); // push value in 'targetField' of [pop stack] 'this' onto stack
                invokeMethodBodyGenerator.Emit(OpCodes.Castclass, method.DeclaringType); // try to cast the top stack element to the declaring type of 'method'
            }
            #endregion

            #region Load event argument onto the stack
            invokeMethodBodyGenerator.Emit(OpCodes.Ldarg_1); // push the event argument onto the stack
            invokeMethodBodyGenerator.Emit(OpCodes.Castclass, method.GetParameters()[0].ParameterType); // try to cast the top stack element to the type of the first parameter of 'method' (an Event subclass)
            #endregion

            #region Invoke 'method'
            invokeMethodBodyGenerator.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);
            #endregion

            invokeMethodBodyGenerator.Emit(OpCodes.Ret);
        }
    }

    public IEventListener Create(MethodInfo method, object? target)
    {
        try {
            var factory = GetEventListenerFactory(method);

            return method.IsStatic switch {
                true => factory.Invoke(null),
                false => factory.Invoke(target),
            };
        }
        catch (Exception exc) {
            throw new Exception($"Failed to instantiate {nameof(IEventListener)} for {method}", exc);
        }
    }
}

