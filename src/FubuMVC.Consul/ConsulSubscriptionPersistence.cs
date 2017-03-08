using System;
using System.Linq;
using System.Collections.Generic;
using FubuMVC.Core.ServiceBus.Subscriptions;
using System.Text;
using Consul;

namespace FubuMVC.ConsulIntegration
{
    public class ConsulSubscriptionPersistence : ISubscriptionPersistence
    {
        private IKVEndpoint _kvs;
        private const string GLOBAL_PREFIX = "fubumvc/";
        private const string TRANSPORTNODE_PREFIX = GLOBAL_PREFIX + "node/";
        private const string SUBSCRIPTION_PREFIX = GLOBAL_PREFIX + "subscription/";

        public ConsulSubscriptionPersistence()
        {
            var client = new Consul.ConsulClient();
            _kvs = client.KV;
        }

        public IEnumerable<TransportNode> AllNodes()
        {
            var nodes = _kvs.List(TRANSPORTNODE_PREFIX).Result;
            return nodes.Response?.Select(kv => Deserialize<TransportNode>(kv.Value)) ?? new TransportNode[0];
        }

        public IEnumerable<Subscription> AllSubscriptions()
        {
            var subs = _kvs.List(SUBSCRIPTION_PREFIX).Result;
            return subs.Response?.Select(kv => Deserialize<Subscription>(kv.Value)) ?? new Subscription[0];
        }

        public void Alter(string id, Action<TransportNode> alteration)
        {
            var kv = _kvs.Get(TRANSPORTNODE_PREFIX + id).Result;
            var node = Deserialize<TransportNode>(kv.Response.Value);
            alteration.Invoke(node);
            _kvs.CAS(new KVPair(kv.Response.Key) { Value = Serialize(node), ModifyIndex = kv.LastIndex }).Wait();
        }

        public void DeleteSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            _kvs.Txn(subscriptions
                .Select(s => new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id, KVTxnVerb.Delete))
                .ToList()
            ).Wait();
        }

        public TransportNode LoadNode(string nodeId)
        {
            var kv = _kvs.Get(TRANSPORTNODE_PREFIX + nodeId).Result;
            return Deserialize<TransportNode>(kv.Response.Value);
        }

        public IEnumerable<Subscription> LoadSubscriptions(string name, SubscriptionRole role)
        {
            return AllSubscriptions().Where(s => s.NodeName == name && s.Role == role);
        }

        public IEnumerable<TransportNode> NodesForGroup(string name)
        {
            return AllNodes().Where(n => n.NodeName == name);
        }

        public void Persist(IEnumerable<Subscription> subscriptions)
        {
            _kvs.Txn(
                subscriptions
                    .Select(s =>
                    {
                        if (s.Id == null || s.Id == Guid.Empty) s.Id = Guid.NewGuid();
                        return new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id.ToString(), KVTxnVerb.Set) { Value = Serialize(s) };
                    })
                    .ToList()
                ).Wait();
        }

        public void Persist(Subscription subscription)
        {
            if (subscription.Id == null || subscription.Id == Guid.Empty) subscription.Id = Guid.NewGuid();
            _kvs.Put(new KVPair(SUBSCRIPTION_PREFIX + subscription.Id.ToString()) { Value = Serialize(subscription) }).Wait();
        }

        public void Persist(params TransportNode[] nodes)
        {
            _kvs.Txn(
                nodes
                    .Select(n =>
                    {
                        if (string.IsNullOrWhiteSpace(n.Id)) n.Id = Guid.NewGuid().ToString();
                        return new KVTxnOp(TRANSPORTNODE_PREFIX + n.Id, KVTxnVerb.Set) { Value = Serialize(n) };
                    })
                    .ToList()
                ).Wait();
        }

        private byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }

        private T Deserialize<T>(byte[] data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
    }
}