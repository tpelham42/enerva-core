using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ECInstanceType : Attribute {
    public Type InstanceType { get; }
    public ECInstanceType(Type instanceType) => InstanceType = instanceType;
}