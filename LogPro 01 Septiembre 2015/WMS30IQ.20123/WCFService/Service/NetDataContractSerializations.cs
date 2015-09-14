using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;
using System.ServiceModel.Channels;

namespace WcfService
{
    public class NetDataContractOperationBehavior : DataContractSerializerOperationBehavior
    {
        public NetDataContractOperationBehavior(OperationDescription operation)
            : base(operation)
        {
        }

        public NetDataContractOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormatAttribute)
            : base(operation, dataContractFormatAttribute)
        {
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns,
            IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name,
            XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }
    }

    public class UseNetDataContractSerializerAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription description,
            System.ServiceModel.Dispatcher.ClientOperation proxy)
        {
            ReplaceDataContractSerializerOperationBehavior(description);
        }

        public void ApplyDispatchBehavior(OperationDescription description,
            System.ServiceModel.Dispatcher.DispatchOperation dispatch)
        {
            ReplaceDataContractSerializerOperationBehavior(description);
        }

        public void Validate(OperationDescription description)
        {
        }

        private static void ReplaceDataContractSerializerOperationBehavior(OperationDescription description)
        {
            DataContractSerializerOperationBehavior dcsOperationBehavior =
            description.Behaviors.Find<DataContractSerializerOperationBehavior>();

            if (dcsOperationBehavior != null)
            {
                description.Behaviors.Remove(dcsOperationBehavior);
                description.Behaviors.Add(new NetDataContractOperationBehavior(description));
            }
        }
    }

    //class ReferencePreservingDataContractSerializerOperationBehavior
    //    : DataContractSerializerOperationBehavior
    //{

    //    public ReferencePreservingDataContractSerializerOperationBehavior(OperationDescription operationDescription) : base(operationDescription) { }

    //    public override XmlObjectSerializer CreateSerializer(
    //        Type type, string name, string ns, IList<Type> knownTypes)
    //    { return CreateDataContractSerializer(type, name, ns, knownTypes); }



    //    private static XmlObjectSerializer CreateDataContractSerializer(
    //      Type type, string name, string ns, IList<Type> knownTypes)
    //    { return CreateDataContractSerializer(type, name, ns, knownTypes); }



    //    public override XmlObjectSerializer CreateSerializer(
    //        Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
    //    {
    //        return new DataContractSerializer(type, name, ns, knownTypes,

    //            0x7FFF /*maxItemsInObjectGraph*/,

    //            false/*ignoreExtensionDataObject*/,

    //            true/*preserveObjectReferences*/,

    //            null/*dataContractSurrogate*/);
    //    }

    //}

    //public class NetDataContractOperationBehavior : DataContractSerializerOperationBehavior
    //{
    //    public NetDataContractOperationBehavior(OperationDescription operation)
    //        : base(operation) { }

    //    public NetDataContractOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormatAttribute)
    //        : base(operation, dataContractFormatAttribute) { }

    //    public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns,
    //        IList<Type> knownTypes)
    //    { return new NetDataContractSerializer(name, ns); }

    //    public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name,
    //        XmlDictionaryString ns, IList<Type> knownTypes)
    //    {  return new NetDataContractSerializer(name, ns); }
    //}

    //public class UseNetDataContractSerializerAttribute : Attribute, IOperationBehavior
    //{
    //    public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
    //    { }

    //    public void ApplyClientBehavior(OperationDescription description,
    //        System.ServiceModel.Dispatcher.ClientOperation proxy)
    //    { ReplaceDataContractSerializerOperationBehavior(description); }

    //    public void ApplyDispatchBehavior(OperationDescription description,
    //        System.ServiceModel.Dispatcher.DispatchOperation dispatch)
    //    {  ReplaceDataContractSerializerOperationBehavior(description); }

    //    public void Validate(OperationDescription description)
    //    { }

    //    private static void ReplaceDataContractSerializerOperationBehavior(OperationDescription description)
    //    {
    //        DataContractSerializerOperationBehavior dcsOperationBehavior =
    //        description.Behaviors.Find<DataContractSerializerOperationBehavior>();

    //        if (dcsOperationBehavior != null)
    //        {
    //            description.Behaviors.Remove(dcsOperationBehavior);
    //            description.Behaviors.Add(new ReferencePreservingDataContractSerializerOperationBehavior(description));
    //        }
    //    }
    //}

    //public class ReferencePreservingDataContractFormatAttribute : Attribute, IOperationBehavior
    //{
    //    #region IOperationBehavior Members
    //    public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
    //    {
    //    }

    //    public void ApplyClientBehavior(OperationDescription description, System.ServiceModel.Dispatcher.ClientOperation proxy)
    //    {
    //        IOperationBehavior innerBehavior = new ReferencePreservingDataContractSerializerOperationBehavior(description);
    //        innerBehavior.ApplyClientBehavior(description, proxy);
    //    }

    //    public void ApplyDispatchBehavior(OperationDescription description, System.ServiceModel.Dispatcher.DispatchOperation dispatch)
    //    {
    //        IOperationBehavior innerBehavior = new ReferencePreservingDataContractSerializerOperationBehavior(description);
    //        innerBehavior.ApplyDispatchBehavior(description, dispatch);
    //    }

    //    public void Validate(OperationDescription description)
    //    {
    //    }

    //    #endregion

    //}
}
