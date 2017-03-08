using FubuMVC.Core.ServiceBus.Subscriptions;
using System;
using System.Linq;
using Xunit;

namespace FubuMVC.Consul.Tests
{
    public class ConsulSubscriptionTests
    {
        [Fact]
        public void CanSaveSubscription()
        {
            //Arrange
            var subject = new ConsulIntegration.ConsulSubscriptionPersistence();
            var subscription = new Subscription() { MessageType = "this is my message", NodeName = "new node name", Role = SubscriptionRole.Subscribes };

            Assert.Empty(subject.AllSubscriptions());

            //Act
            subject.Persist(subscription);

            //Assert
            Assert.NotEqual(Guid.Empty, subscription.Id);
            var savedSub = subject.AllSubscriptions().FirstOrDefault();
            Assert.Equal(subscription, savedSub);
            Assert.NotSame(subscription, savedSub);
            Assert.Equal(subscription, subject.LoadSubscriptions(subscription.NodeName, subscription.Role).FirstOrDefault(s => s.Id == subscription.Id));

            //Cleanup
            subject.DeleteSubscriptions(new[] { subscription });
            Assert.Empty(subject.AllSubscriptions());
        }
        [Fact]
        public void CanSaveMultipleSubscriptions()
        {
            //Arrange
            var subject = new ConsulIntegration.ConsulSubscriptionPersistence();
            Assert.Empty(subject.AllSubscriptions());

            var subscriptions = new Subscription[]
            {
               new Subscription { MessageType = "this is my message", NodeName = "new node name", Role = SubscriptionRole.Subscribes },
               new Subscription { MessageType = "this is my other message", NodeName = "new node name2", Role = SubscriptionRole.Subscribes }
            };


            //Act
            subject.Persist(subscriptions);

            //Assert
            Assert.Empty(subscriptions.Where(s => s.Id == Guid.Empty));
            foreach (var sub in subscriptions)
            {
                var savedSub = subject.AllSubscriptions().FirstOrDefault(s => s.Id == sub.Id);
                Assert.Equal(sub, savedSub);
                Assert.NotSame(sub, savedSub);
            }


            //Cleanup
            subject.DeleteSubscriptions(subscriptions);
            Assert.Empty(subject.AllSubscriptions());
        }

        [Fact]
        public void CanSaveNodes()
        {
            //Arrange
            var subject = new ConsulIntegration.ConsulSubscriptionPersistence();
            var node = new TransportNode() { NodeName = "new node name", Addresses = new Uri[] { new Uri("http://google.com") }, MachineName = "Terminator" };

            //Act
            subject.Persist(node);

            //Assert
            var savedNode = subject.AllNodes().FirstOrDefault(n => n.Id == node.Id);
            Assert.Equal(node, savedNode);
            Assert.NotSame(node, savedNode);
        }

        [Fact]
        public void CanAlterTransportationNode()
        {
            //Arrange
            var subject = new ConsulIntegration.ConsulSubscriptionPersistence();
            var tn = new TransportNode() { NodeName = "new node name", Addresses = new Uri[] { new Uri("http://google.com") }, MachineName = "Terminator", Id = Guid.NewGuid().ToString() };
            subject.Persist(tn);
            var updateNodeName = "T2";
            var updatedMachineName = "Updated Machine Name";

            //Act
            subject.Alter(tn.Id, (node =>
            {
                node.MachineName = updatedMachineName;
                node.NodeName = updateNodeName;
            }));

            //Assert
            var updatedNode = subject.LoadNode(tn.Id);
            Assert.Equal(updatedMachineName, updatedNode.MachineName);
            Assert.Equal(updateNodeName, updatedNode.NodeName);
        }
    }
}
