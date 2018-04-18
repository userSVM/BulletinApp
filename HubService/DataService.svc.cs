﻿using BulletinBridge.Data;
using BulletinEngine.Core;
using BulletinEngine.Entity.Data;
using FessooFramework.Objects.Message;
using FessooFramework.Tools.DCT;
using FessooFramework.Tools.Web;
using FessooFramework.Tools.Web.DataService.Configuration;
using HubService.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace HubService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AutomaticSessionShutdown = true, UseSynchronizationContext = false)]
    [ServiceContract]
    public class DataService : FessooFramework.Tools.Web.DataService.DataServiceAPI
    {
        [WebInvoke(
           Method = "POST",
           UriTemplate = "Ping",
           RequestFormat = WebMessageFormat.Xml,
           ResponseFormat = WebMessageFormat.Xml)]
        [OperationContract]
        public override bool Ping(string p)
        {
            return _Ping(p);
        }
        [WebInvoke(
        Method = "GET",
        UriTemplate = "Stat",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public override string Stat()
        {
            return _Stat(this);
        }
        [WebInvoke(
         Method = "POST",
         UriTemplate = "Execute",
         RequestFormat = WebMessageFormat.Xml,
         ResponseFormat = WebMessageFormat.Xml)]
        [OperationContract]
        public override ServiceMessage Execute(ServiceMessage data)
        {
            var result = default(ServiceMessage);
            BCT.Execute(c => {
                result = _Execute(data);
            });
            return result;
        }
        public override IEnumerable<DataServiceConfigurationBase> Convertors => new DataServiceConfigurationBase[]
        {
            new DataServiceConfiguration<Access, AccessPackage>(),
            new DataServiceConfiguration<Bulletin, BulletinPackage>(),
            new DataServiceConfiguration<Bulletin, BulletinCache>(),
            new DataServiceConfiguration<BulletinHub.Entity.Data.Task, TaskCache>(),
        };

        protected override IEnumerable<ServiceRequestConfigBase> CustomConfigurations => Enumerable.Empty<ServiceRequestConfigBase>();
    }
}
