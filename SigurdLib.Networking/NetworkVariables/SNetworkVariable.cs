using System;
using OdinSerializer;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.Networking.NetworkVariables;

/// <summary>
/// A variable that can be synchronized over the network.
/// </summary>
/// <typeparam name="T">the unmanaged type for <see cref="SNetworkVariable{T}"/> </typeparam>
[Serializable]
public class SNetworkVariable<T> : SNetworkVariableBase
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/NetworkVariable/NetworkVariable.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    /// <summary>
    /// Delegate type for value changed event
    /// </summary>
    /// <param name="previousValue">The value before the change</param>
    /// <param name="newValue">The new value</param>
    public delegate void OnValueChangedDelegate(T previousValue, T newValue);

    // ReSharper disable once UnassignedField.Global
    /// <summary>
    /// The callback to be invoked when the value gets changed
    /// </summary>
    public OnValueChangedDelegate? OnValueChanged;

    /// <summary>
    /// Constructor for <see cref="SNetworkVariable{T}"/>
    /// </summary>
    /// <param name="value">initial value set that is of type T</param>
    /// <param name="readPerm">the <see cref="NetworkVariableReadPermission"/> for this <see cref="SNetworkVariable{T}"/></param>
    /// <param name="writePerm">the <see cref="NetworkVariableWritePermission"/> for this <see cref="SNetworkVariable{T}"/></param>
    public SNetworkVariable(T value = default!,
        NetworkVariableReadPermission readPerm = DefaultReadPerm,
        NetworkVariableWritePermission writePerm = DefaultWritePerm)
        : base(readPerm, writePerm)
    {
        internalValue = value;
        // Since we start with IsDirty = true, this doesn't need to be duplicated
        // right away. It won't get read until after ResetDirty() is called, and
        // the duplicate will be made there. Avoiding calling
        // NetworkVariableSerialization<T>.Duplicate() is important because calling
        // it in the constructor might not give users enough time to set the
        // DuplicateValue callback if they're using UserNetworkVariableSerialization
        PreviousValue = default!;
    }

    /// <summary>
    /// The internal value of the NetworkVariable
    /// </summary>
    [SerializeField]
    private protected T internalValue;

    private protected T PreviousValue;

    private bool _hasPreviousValue;
    private bool _isDisposed;

    /// <summary>
    /// The value of the NetworkVariable container
    /// </summary>
    public virtual T Value
    {
        get => internalValue;
        set
        {
            // Compare bitwise
            if (Equals(internalValue, value))
            {
                return;
            }

            if (SNetworkBehaviour && !CanClientWrite(SNetworkBehaviour.NetworkManager.LocalClientId))
            {
                throw new InvalidOperationException("Client is not allowed to write to this NetworkVariable");
            }

            Set(value);
            _isDisposed = false;
        }
    }

    internal ref T RefValue()
    {
        return ref internalValue;
    }

    /// <summary>
    ///
    /// </summary>
    public override void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (internalValue is IDisposable internalValueDisposable)
        {
            internalValueDisposable.Dispose();
        }

        internalValue = default!;
        if (_hasPreviousValue && PreviousValue is IDisposable previousValueDisposable)
        {
            _hasPreviousValue = false;
            previousValueDisposable.Dispose();
        }

        PreviousValue = default!;
    }

    ~SNetworkVariable()
    {
        Dispose();
    }

    /// <summary>
    /// Gets Whether or not the container is dirty
    /// </summary>
    /// <returns>Whether or not the container is dirty</returns>
    public override bool IsDirty()
    {
        // For most cases we can use the dirty flag.
        // This doesn't work for cases where we're wrapping more complex types
        // like INetworkSerializable, NativeList, NativeArray, etc.
        // Changes to the values in those types don't call the Value.set method,
        // so we can't catch those changes and need to compare the current value
        // against the previous one.
        if (base.IsDirty())
        {
            return true;
        }

        // Cache the dirty value so we don't perform this again if we already know we're dirty
        // Unfortunately we can't cache the NOT dirty state, because that might change
        // in between to checks... but the DIRTY state won't change until ResetDirty()
        // is called.
        var dirty = !Equals(PreviousValue, internalValue);
        SetDirty(dirty);
        return dirty;
    }

    /// <summary>
    /// Resets the dirty state and marks the variable as synced / clean
    /// </summary>
    public override void ResetDirty()
    {
        base.ResetDirty();
        // Resetting the dirty value declares that the current value is not dirty
        // Therefore, we set the PreviousValue field to a duplicate of the current
        // field, so that our next dirty check is made against the current "not dirty"
        // value.
        _hasPreviousValue = true;
        NetworkVariableSerialization<T>.Serializer.Duplicate(internalValue, ref PreviousValue);
    }

    /// <summary>
    /// Sets the <see cref="Value"/>, marks the <see cref="SNetworkVariable{T}"/> dirty, and invokes the <see cref="OnValueChanged"/> callback
    /// if there are subscribers to that event.
    /// </summary>
    /// <param name="value">the new value of type `T` to be set/></param>
    private protected void Set(T value)
    {
        SetDirty(true);
        T previousValue = internalValue;
        internalValue = value;
        OnValueChanged?.Invoke(previousValue, internalValue);
    }

    /// <summary>
    /// Writes the variable to the writer
    /// </summary>
    /// <param name="writer">The stream to write the value to</param>
    public override void WriteDelta(FastBufferWriter writer)
    {
        WriteField(writer);
    }

    /// <summary>
    /// Reads value from the reader and applies it
    /// </summary>
    /// <param name="reader">The stream to read the value from</param>
    /// <param name="keepDirtyDelta">Whether or not the container should keep the dirty delta, or mark the delta as consumed</param>
    public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
    {
        // todo:
        // keepDirtyDelta marks a variable received as dirty and causes the server to send the value to clients
        // In a prefect world, whether a variable was A) modified locally or B) received and needs retransmit
        // would be stored in different fields

        T previousValue = internalValue;
        ReadField(reader);

        if (keepDirtyDelta)
        {
            SetDirty(true);
        }

        OnValueChanged?.Invoke(previousValue, internalValue);
    }

    /// <inheritdoc />
    public override void ReadField(FastBufferReader reader)
    {
        byte[] deserializedValue = [];
        NetworkVariableSerialization<byte[]>.Read(reader, ref deserializedValue);
        internalValue = SerializationUtility.DeserializeValue<T>(deserializedValue, DataFormat.Binary);
    }

    /// <inheritdoc />
    public override void WriteField(FastBufferWriter writer)
    {
        byte[] serializedValue = SerializationUtility.SerializeValue(internalValue, DataFormat.Binary);
        NetworkVariableSerialization<byte[]>.Write(writer, ref serializedValue);
    }
}
