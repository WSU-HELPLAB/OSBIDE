﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17379
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OSBIDE.VSPackage.WebServices {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="", ConfigurationName="WebServices.OsbideWebService")]
    public interface OsbideWebService {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/Echo", ReplyAction="urn:OsbideWebService/EchoResponse")]
        string Echo(string toEcho);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/Echo", ReplyAction="urn:OsbideWebService/EchoResponse")]
        System.IAsyncResult BeginEcho(string toEcho, System.AsyncCallback callback, object asyncState);
        
        string EndEcho(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/GetUserById", ReplyAction="urn:OsbideWebService/GetUserByIdResponse")]
        OSBIDE.Library.Models.OsbideUser GetUserById(int id);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/GetUserById", ReplyAction="urn:OsbideWebService/GetUserByIdResponse")]
        System.IAsyncResult BeginGetUserById(int id, System.AsyncCallback callback, object asyncState);
        
        OSBIDE.Library.Models.OsbideUser EndGetUserById(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/SaveUser", ReplyAction="urn:OsbideWebService/SaveUserResponse")]
        OSBIDE.Library.Models.OsbideUser SaveUser(OSBIDE.Library.Models.OsbideUser userToSave);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/SaveUser", ReplyAction="urn:OsbideWebService/SaveUserResponse")]
        System.IAsyncResult BeginSaveUser(OSBIDE.Library.Models.OsbideUser userToSave, System.AsyncCallback callback, object asyncState);
        
        OSBIDE.Library.Models.OsbideUser EndSaveUser(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/SubmitLog", ReplyAction="urn:OsbideWebService/SubmitLogResponse")]
        int SubmitLog(OSBIDE.Library.Models.EventLog log);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/SubmitLog", ReplyAction="urn:OsbideWebService/SubmitLogResponse")]
        System.IAsyncResult BeginSubmitLog(OSBIDE.Library.Models.EventLog log, System.AsyncCallback callback, object asyncState);
        
        int EndSubmitLog(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/GetPastEvents", ReplyAction="urn:OsbideWebService/GetPastEventsResponse")]
        OSBIDE.Library.Models.EventLog[] GetPastEvents(System.DateTime start, bool waitForContent);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/GetPastEvents", ReplyAction="urn:OsbideWebService/GetPastEventsResponse")]
        System.IAsyncResult BeginGetPastEvents(System.DateTime start, bool waitForContent, System.AsyncCallback callback, object asyncState);
        
        OSBIDE.Library.Models.EventLog[] EndGetPastEvents(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/GetUsers", ReplyAction="urn:OsbideWebService/GetUsersResponse")]
        OSBIDE.Library.Models.OsbideUser[] GetUsers(int[] osbideIds);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/GetUsers", ReplyAction="urn:OsbideWebService/GetUsersResponse")]
        System.IAsyncResult BeginGetUsers(int[] osbideIds, System.AsyncCallback callback, object asyncState);
        
        OSBIDE.Library.Models.OsbideUser[] EndGetUsers(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/LibraryVersionNumber", ReplyAction="urn:OsbideWebService/LibraryVersionNumberResponse")]
        string LibraryVersionNumber();
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/LibraryVersionNumber", ReplyAction="urn:OsbideWebService/LibraryVersionNumberResponse")]
        System.IAsyncResult BeginLibraryVersionNumber(System.AsyncCallback callback, object asyncState);
        
        string EndLibraryVersionNumber(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:OsbideWebService/OsbidePackageUrl", ReplyAction="urn:OsbideWebService/OsbidePackageUrlResponse")]
        string OsbidePackageUrl();
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:OsbideWebService/OsbidePackageUrl", ReplyAction="urn:OsbideWebService/OsbidePackageUrlResponse")]
        System.IAsyncResult BeginOsbidePackageUrl(System.AsyncCallback callback, object asyncState);
        
        string EndOsbidePackageUrl(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface OsbideWebServiceChannel : OSBIDE.VSPackage.WebServices.OsbideWebService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class EchoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public EchoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetUserByIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetUserByIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public OSBIDE.Library.Models.OsbideUser Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((OSBIDE.Library.Models.OsbideUser)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SaveUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public SaveUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public OSBIDE.Library.Models.OsbideUser Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((OSBIDE.Library.Models.OsbideUser)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SubmitLogCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public SubmitLogCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public int Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetPastEventsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetPastEventsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public OSBIDE.Library.Models.EventLog[] Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((OSBIDE.Library.Models.EventLog[])(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public OSBIDE.Library.Models.OsbideUser[] Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((OSBIDE.Library.Models.OsbideUser[])(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LibraryVersionNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public LibraryVersionNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class OsbidePackageUrlCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public OsbidePackageUrlCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class OsbideWebServiceClient : System.ServiceModel.ClientBase<OSBIDE.VSPackage.WebServices.OsbideWebService>, OSBIDE.VSPackage.WebServices.OsbideWebService {
        
        private BeginOperationDelegate onBeginEchoDelegate;
        
        private EndOperationDelegate onEndEchoDelegate;
        
        private System.Threading.SendOrPostCallback onEchoCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetUserByIdDelegate;
        
        private EndOperationDelegate onEndGetUserByIdDelegate;
        
        private System.Threading.SendOrPostCallback onGetUserByIdCompletedDelegate;
        
        private BeginOperationDelegate onBeginSaveUserDelegate;
        
        private EndOperationDelegate onEndSaveUserDelegate;
        
        private System.Threading.SendOrPostCallback onSaveUserCompletedDelegate;
        
        private BeginOperationDelegate onBeginSubmitLogDelegate;
        
        private EndOperationDelegate onEndSubmitLogDelegate;
        
        private System.Threading.SendOrPostCallback onSubmitLogCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetPastEventsDelegate;
        
        private EndOperationDelegate onEndGetPastEventsDelegate;
        
        private System.Threading.SendOrPostCallback onGetPastEventsCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetUsersDelegate;
        
        private EndOperationDelegate onEndGetUsersDelegate;
        
        private System.Threading.SendOrPostCallback onGetUsersCompletedDelegate;
        
        private BeginOperationDelegate onBeginLibraryVersionNumberDelegate;
        
        private EndOperationDelegate onEndLibraryVersionNumberDelegate;
        
        private System.Threading.SendOrPostCallback onLibraryVersionNumberCompletedDelegate;
        
        private BeginOperationDelegate onBeginOsbidePackageUrlDelegate;
        
        private EndOperationDelegate onEndOsbidePackageUrlDelegate;
        
        private System.Threading.SendOrPostCallback onOsbidePackageUrlCompletedDelegate;
        
        public OsbideWebServiceClient() {
        }
        
        public OsbideWebServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public OsbideWebServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public OsbideWebServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public OsbideWebServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public event System.EventHandler<EchoCompletedEventArgs> EchoCompleted;
        
        public event System.EventHandler<GetUserByIdCompletedEventArgs> GetUserByIdCompleted;
        
        public event System.EventHandler<SaveUserCompletedEventArgs> SaveUserCompleted;
        
        public event System.EventHandler<SubmitLogCompletedEventArgs> SubmitLogCompleted;
        
        public event System.EventHandler<GetPastEventsCompletedEventArgs> GetPastEventsCompleted;
        
        public event System.EventHandler<GetUsersCompletedEventArgs> GetUsersCompleted;
        
        public event System.EventHandler<LibraryVersionNumberCompletedEventArgs> LibraryVersionNumberCompleted;
        
        public event System.EventHandler<OsbidePackageUrlCompletedEventArgs> OsbidePackageUrlCompleted;
        
        public string Echo(string toEcho) {
            return base.Channel.Echo(toEcho);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginEcho(string toEcho, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginEcho(toEcho, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public string EndEcho(System.IAsyncResult result) {
            return base.Channel.EndEcho(result);
        }
        
        private System.IAsyncResult OnBeginEcho(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string toEcho = ((string)(inValues[0]));
            return this.BeginEcho(toEcho, callback, asyncState);
        }
        
        private object[] OnEndEcho(System.IAsyncResult result) {
            string retVal = this.EndEcho(result);
            return new object[] {
                    retVal};
        }
        
        private void OnEchoCompleted(object state) {
            if ((this.EchoCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.EchoCompleted(this, new EchoCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void EchoAsync(string toEcho) {
            this.EchoAsync(toEcho, null);
        }
        
        public void EchoAsync(string toEcho, object userState) {
            if ((this.onBeginEchoDelegate == null)) {
                this.onBeginEchoDelegate = new BeginOperationDelegate(this.OnBeginEcho);
            }
            if ((this.onEndEchoDelegate == null)) {
                this.onEndEchoDelegate = new EndOperationDelegate(this.OnEndEcho);
            }
            if ((this.onEchoCompletedDelegate == null)) {
                this.onEchoCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnEchoCompleted);
            }
            base.InvokeAsync(this.onBeginEchoDelegate, new object[] {
                        toEcho}, this.onEndEchoDelegate, this.onEchoCompletedDelegate, userState);
        }
        
        public OSBIDE.Library.Models.OsbideUser GetUserById(int id) {
            return base.Channel.GetUserById(id);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginGetUserById(int id, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetUserById(id, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public OSBIDE.Library.Models.OsbideUser EndGetUserById(System.IAsyncResult result) {
            return base.Channel.EndGetUserById(result);
        }
        
        private System.IAsyncResult OnBeginGetUserById(object[] inValues, System.AsyncCallback callback, object asyncState) {
            int id = ((int)(inValues[0]));
            return this.BeginGetUserById(id, callback, asyncState);
        }
        
        private object[] OnEndGetUserById(System.IAsyncResult result) {
            OSBIDE.Library.Models.OsbideUser retVal = this.EndGetUserById(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetUserByIdCompleted(object state) {
            if ((this.GetUserByIdCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetUserByIdCompleted(this, new GetUserByIdCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetUserByIdAsync(int id) {
            this.GetUserByIdAsync(id, null);
        }
        
        public void GetUserByIdAsync(int id, object userState) {
            if ((this.onBeginGetUserByIdDelegate == null)) {
                this.onBeginGetUserByIdDelegate = new BeginOperationDelegate(this.OnBeginGetUserById);
            }
            if ((this.onEndGetUserByIdDelegate == null)) {
                this.onEndGetUserByIdDelegate = new EndOperationDelegate(this.OnEndGetUserById);
            }
            if ((this.onGetUserByIdCompletedDelegate == null)) {
                this.onGetUserByIdCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetUserByIdCompleted);
            }
            base.InvokeAsync(this.onBeginGetUserByIdDelegate, new object[] {
                        id}, this.onEndGetUserByIdDelegate, this.onGetUserByIdCompletedDelegate, userState);
        }
        
        public OSBIDE.Library.Models.OsbideUser SaveUser(OSBIDE.Library.Models.OsbideUser userToSave) {
            return base.Channel.SaveUser(userToSave);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginSaveUser(OSBIDE.Library.Models.OsbideUser userToSave, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginSaveUser(userToSave, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public OSBIDE.Library.Models.OsbideUser EndSaveUser(System.IAsyncResult result) {
            return base.Channel.EndSaveUser(result);
        }
        
        private System.IAsyncResult OnBeginSaveUser(object[] inValues, System.AsyncCallback callback, object asyncState) {
            OSBIDE.Library.Models.OsbideUser userToSave = ((OSBIDE.Library.Models.OsbideUser)(inValues[0]));
            return this.BeginSaveUser(userToSave, callback, asyncState);
        }
        
        private object[] OnEndSaveUser(System.IAsyncResult result) {
            OSBIDE.Library.Models.OsbideUser retVal = this.EndSaveUser(result);
            return new object[] {
                    retVal};
        }
        
        private void OnSaveUserCompleted(object state) {
            if ((this.SaveUserCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.SaveUserCompleted(this, new SaveUserCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void SaveUserAsync(OSBIDE.Library.Models.OsbideUser userToSave) {
            this.SaveUserAsync(userToSave, null);
        }
        
        public void SaveUserAsync(OSBIDE.Library.Models.OsbideUser userToSave, object userState) {
            if ((this.onBeginSaveUserDelegate == null)) {
                this.onBeginSaveUserDelegate = new BeginOperationDelegate(this.OnBeginSaveUser);
            }
            if ((this.onEndSaveUserDelegate == null)) {
                this.onEndSaveUserDelegate = new EndOperationDelegate(this.OnEndSaveUser);
            }
            if ((this.onSaveUserCompletedDelegate == null)) {
                this.onSaveUserCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnSaveUserCompleted);
            }
            base.InvokeAsync(this.onBeginSaveUserDelegate, new object[] {
                        userToSave}, this.onEndSaveUserDelegate, this.onSaveUserCompletedDelegate, userState);
        }
        
        public int SubmitLog(OSBIDE.Library.Models.EventLog log) {
            return base.Channel.SubmitLog(log);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginSubmitLog(OSBIDE.Library.Models.EventLog log, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginSubmitLog(log, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public int EndSubmitLog(System.IAsyncResult result) {
            return base.Channel.EndSubmitLog(result);
        }
        
        private System.IAsyncResult OnBeginSubmitLog(object[] inValues, System.AsyncCallback callback, object asyncState) {
            OSBIDE.Library.Models.EventLog log = ((OSBIDE.Library.Models.EventLog)(inValues[0]));
            return this.BeginSubmitLog(log, callback, asyncState);
        }
        
        private object[] OnEndSubmitLog(System.IAsyncResult result) {
            int retVal = this.EndSubmitLog(result);
            return new object[] {
                    retVal};
        }
        
        private void OnSubmitLogCompleted(object state) {
            if ((this.SubmitLogCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.SubmitLogCompleted(this, new SubmitLogCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void SubmitLogAsync(OSBIDE.Library.Models.EventLog log) {
            this.SubmitLogAsync(log, null);
        }
        
        public void SubmitLogAsync(OSBIDE.Library.Models.EventLog log, object userState) {
            if ((this.onBeginSubmitLogDelegate == null)) {
                this.onBeginSubmitLogDelegate = new BeginOperationDelegate(this.OnBeginSubmitLog);
            }
            if ((this.onEndSubmitLogDelegate == null)) {
                this.onEndSubmitLogDelegate = new EndOperationDelegate(this.OnEndSubmitLog);
            }
            if ((this.onSubmitLogCompletedDelegate == null)) {
                this.onSubmitLogCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnSubmitLogCompleted);
            }
            base.InvokeAsync(this.onBeginSubmitLogDelegate, new object[] {
                        log}, this.onEndSubmitLogDelegate, this.onSubmitLogCompletedDelegate, userState);
        }
        
        public OSBIDE.Library.Models.EventLog[] GetPastEvents(System.DateTime start, bool waitForContent) {
            return base.Channel.GetPastEvents(start, waitForContent);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginGetPastEvents(System.DateTime start, bool waitForContent, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetPastEvents(start, waitForContent, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public OSBIDE.Library.Models.EventLog[] EndGetPastEvents(System.IAsyncResult result) {
            return base.Channel.EndGetPastEvents(result);
        }
        
        private System.IAsyncResult OnBeginGetPastEvents(object[] inValues, System.AsyncCallback callback, object asyncState) {
            System.DateTime start = ((System.DateTime)(inValues[0]));
            bool waitForContent = ((bool)(inValues[1]));
            return this.BeginGetPastEvents(start, waitForContent, callback, asyncState);
        }
        
        private object[] OnEndGetPastEvents(System.IAsyncResult result) {
            OSBIDE.Library.Models.EventLog[] retVal = this.EndGetPastEvents(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetPastEventsCompleted(object state) {
            if ((this.GetPastEventsCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetPastEventsCompleted(this, new GetPastEventsCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetPastEventsAsync(System.DateTime start, bool waitForContent) {
            this.GetPastEventsAsync(start, waitForContent, null);
        }
        
        public void GetPastEventsAsync(System.DateTime start, bool waitForContent, object userState) {
            if ((this.onBeginGetPastEventsDelegate == null)) {
                this.onBeginGetPastEventsDelegate = new BeginOperationDelegate(this.OnBeginGetPastEvents);
            }
            if ((this.onEndGetPastEventsDelegate == null)) {
                this.onEndGetPastEventsDelegate = new EndOperationDelegate(this.OnEndGetPastEvents);
            }
            if ((this.onGetPastEventsCompletedDelegate == null)) {
                this.onGetPastEventsCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetPastEventsCompleted);
            }
            base.InvokeAsync(this.onBeginGetPastEventsDelegate, new object[] {
                        start,
                        waitForContent}, this.onEndGetPastEventsDelegate, this.onGetPastEventsCompletedDelegate, userState);
        }
        
        public OSBIDE.Library.Models.OsbideUser[] GetUsers(int[] osbideIds) {
            return base.Channel.GetUsers(osbideIds);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginGetUsers(int[] osbideIds, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetUsers(osbideIds, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public OSBIDE.Library.Models.OsbideUser[] EndGetUsers(System.IAsyncResult result) {
            return base.Channel.EndGetUsers(result);
        }
        
        private System.IAsyncResult OnBeginGetUsers(object[] inValues, System.AsyncCallback callback, object asyncState) {
            int[] osbideIds = ((int[])(inValues[0]));
            return this.BeginGetUsers(osbideIds, callback, asyncState);
        }
        
        private object[] OnEndGetUsers(System.IAsyncResult result) {
            OSBIDE.Library.Models.OsbideUser[] retVal = this.EndGetUsers(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetUsersCompleted(object state) {
            if ((this.GetUsersCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetUsersCompleted(this, new GetUsersCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetUsersAsync(int[] osbideIds) {
            this.GetUsersAsync(osbideIds, null);
        }
        
        public void GetUsersAsync(int[] osbideIds, object userState) {
            if ((this.onBeginGetUsersDelegate == null)) {
                this.onBeginGetUsersDelegate = new BeginOperationDelegate(this.OnBeginGetUsers);
            }
            if ((this.onEndGetUsersDelegate == null)) {
                this.onEndGetUsersDelegate = new EndOperationDelegate(this.OnEndGetUsers);
            }
            if ((this.onGetUsersCompletedDelegate == null)) {
                this.onGetUsersCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetUsersCompleted);
            }
            base.InvokeAsync(this.onBeginGetUsersDelegate, new object[] {
                        osbideIds}, this.onEndGetUsersDelegate, this.onGetUsersCompletedDelegate, userState);
        }
        
        public string LibraryVersionNumber() {
            return base.Channel.LibraryVersionNumber();
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginLibraryVersionNumber(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginLibraryVersionNumber(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public string EndLibraryVersionNumber(System.IAsyncResult result) {
            return base.Channel.EndLibraryVersionNumber(result);
        }
        
        private System.IAsyncResult OnBeginLibraryVersionNumber(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return this.BeginLibraryVersionNumber(callback, asyncState);
        }
        
        private object[] OnEndLibraryVersionNumber(System.IAsyncResult result) {
            string retVal = this.EndLibraryVersionNumber(result);
            return new object[] {
                    retVal};
        }
        
        private void OnLibraryVersionNumberCompleted(object state) {
            if ((this.LibraryVersionNumberCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.LibraryVersionNumberCompleted(this, new LibraryVersionNumberCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void LibraryVersionNumberAsync() {
            this.LibraryVersionNumberAsync(null);
        }
        
        public void LibraryVersionNumberAsync(object userState) {
            if ((this.onBeginLibraryVersionNumberDelegate == null)) {
                this.onBeginLibraryVersionNumberDelegate = new BeginOperationDelegate(this.OnBeginLibraryVersionNumber);
            }
            if ((this.onEndLibraryVersionNumberDelegate == null)) {
                this.onEndLibraryVersionNumberDelegate = new EndOperationDelegate(this.OnEndLibraryVersionNumber);
            }
            if ((this.onLibraryVersionNumberCompletedDelegate == null)) {
                this.onLibraryVersionNumberCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnLibraryVersionNumberCompleted);
            }
            base.InvokeAsync(this.onBeginLibraryVersionNumberDelegate, null, this.onEndLibraryVersionNumberDelegate, this.onLibraryVersionNumberCompletedDelegate, userState);
        }
        
        public string OsbidePackageUrl() {
            return base.Channel.OsbidePackageUrl();
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.IAsyncResult BeginOsbidePackageUrl(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginOsbidePackageUrl(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public string EndOsbidePackageUrl(System.IAsyncResult result) {
            return base.Channel.EndOsbidePackageUrl(result);
        }
        
        private System.IAsyncResult OnBeginOsbidePackageUrl(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return this.BeginOsbidePackageUrl(callback, asyncState);
        }
        
        private object[] OnEndOsbidePackageUrl(System.IAsyncResult result) {
            string retVal = this.EndOsbidePackageUrl(result);
            return new object[] {
                    retVal};
        }
        
        private void OnOsbidePackageUrlCompleted(object state) {
            if ((this.OsbidePackageUrlCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OsbidePackageUrlCompleted(this, new OsbidePackageUrlCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OsbidePackageUrlAsync() {
            this.OsbidePackageUrlAsync(null);
        }
        
        public void OsbidePackageUrlAsync(object userState) {
            if ((this.onBeginOsbidePackageUrlDelegate == null)) {
                this.onBeginOsbidePackageUrlDelegate = new BeginOperationDelegate(this.OnBeginOsbidePackageUrl);
            }
            if ((this.onEndOsbidePackageUrlDelegate == null)) {
                this.onEndOsbidePackageUrlDelegate = new EndOperationDelegate(this.OnEndOsbidePackageUrl);
            }
            if ((this.onOsbidePackageUrlCompletedDelegate == null)) {
                this.onOsbidePackageUrlCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOsbidePackageUrlCompleted);
            }
            base.InvokeAsync(this.onBeginOsbidePackageUrlDelegate, null, this.onEndOsbidePackageUrlDelegate, this.onOsbidePackageUrlCompletedDelegate, userState);
        }
    }
}
