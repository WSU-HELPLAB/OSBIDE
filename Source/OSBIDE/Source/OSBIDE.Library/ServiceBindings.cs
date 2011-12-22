using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace OSBIDE.Library
{
    public static class ServiceBindings
    {
        public static EndpointAddress OsbideServiceEndpoint
        {
            get
            {
#if DEBUG
                //go to the debug endpoint address if we're in debug mode
                EndpointAddress endpoint = new EndpointAddress("http://localhost:49263/OsbideWebService.svc");
#else
                //otherwise, hit the real server
                EndpointAddress endpoint = new EndpointAddress("http://osbide.osble.org/OsbideWebService.svc");
#endif
                return endpoint;
            }
        }

        public static Binding OsbideServiceBinding
        {
            get
            {
                CustomBinding serviceBinding = new CustomBinding();
                serviceBinding.Name = "OsbideWebServiceBinding";

                //transport values are pulled from the ones auto generated inside app.config
                HttpTransportBindingElement transportElement = new HttpTransportBindingElement()
                {
                    ManualAddressing = false,
                    MaxBufferPoolSize = 524288,
                    MaxReceivedMessageSize = 65536,
                    AllowCookies = false,
                    AuthenticationScheme = System.Net.AuthenticationSchemes.Anonymous,
                    BypassProxyOnLocal = false,
                    DecompressionEnabled = true,
                    HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard,
                    KeepAliveEnabled = true,
                    MaxBufferSize = 65536,
                    ProxyAuthenticationScheme = System.Net.AuthenticationSchemes.Anonymous,
                    Realm = "",
                    TransferMode = System.ServiceModel.TransferMode.Buffered,
                    UnsafeConnectionNtlmAuthentication = false,
                    UseDefaultWebProxy = true
                };

                BinaryMessageEncodingBindingElement messageElement = new BinaryMessageEncodingBindingElement()
                {
                    MaxReadPoolSize = 64,
                    MaxWritePoolSize = 16,
                    MaxSessionSize = 2048,
                };
                messageElement.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                {
                    MaxDepth = 32,
                    MaxStringContentLength = 8192,
                    MaxArrayLength = 16384,
                    MaxBytesPerRead = 4096,
                    MaxNameTableCharCount = 16384
                };
                serviceBinding.Elements.Add(messageElement);
                serviceBinding.Elements.Add(transportElement);

                return serviceBinding;
            }
        }
    }
}
