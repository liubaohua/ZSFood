﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.33440
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyWebApp.ZSNS {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://ws.zhengshan.com/", ConfigurationName="ZSNS.ZhengShanWsI")]
    public interface ZhengShanWsI {
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.DataContractFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        string ST_AppTransVouchAdd(string arg0);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.DataContractFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        string inventory(string content);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.DataContractFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        string addWarehouse(string arg0);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.DataContractFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        string InventoryClass(string arg0);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.DataContractFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        string queryInv(int arg0);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ZhengShanWsIChannel : MyWebApp.ZSNS.ZhengShanWsI, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ZhengShanWsIClient : System.ServiceModel.ClientBase<MyWebApp.ZSNS.ZhengShanWsI>, MyWebApp.ZSNS.ZhengShanWsI {
        
        public ZhengShanWsIClient() {
        }
        
        public ZhengShanWsIClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ZhengShanWsIClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ZhengShanWsIClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ZhengShanWsIClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string ST_AppTransVouchAdd(string arg0) {
            return base.Channel.ST_AppTransVouchAdd(arg0);
        }
        
        public string inventory(string content) {
            return base.Channel.inventory(content);
        }
        
        public string addWarehouse(string arg0) {
            return base.Channel.addWarehouse(arg0);
        }
        
        public string InventoryClass(string arg0) {
            return base.Channel.InventoryClass(arg0);
        }
        
        public string queryInv(int arg0) {
            return base.Channel.queryInv(arg0);
        }
    }
}
