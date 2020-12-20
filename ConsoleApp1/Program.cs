using dotnet_etcd;
using System;
using etcd.Provider.Service;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            EtcdClient etcdClient = new EtcdClient("https://127.0.0.1", 2379);
           // etcdClient.SetInfo();
            AgentServiceRegistration registration = new AgentServiceRegistration()
            {
                Address = "127.0.0.1",
                ID = "Test1",
                Name = "Test1",
                Port = 5555,
                Version = "1.0",
            };
          
           etcdClient.RegisterAsync(registration);
            Console.Read();
        }
    }
}
