# etcd.Provider.Service
etcd Registration service  
    提供服务注册
	1.注册服务方法  
	   etcd客户端的RegisterAsync方法  
	2.获取服务方法  
	    etcd客户端的GetServicesAsync方法  
	3.使用集群功能  
	   如果你要使用集群刷新，并且有用户，密码等严重信息，则需要调用etcd客户端的SetInfo方法单独设置一下
	    
	   
