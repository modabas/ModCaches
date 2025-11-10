# Orleans Caches (Server)

- **Microsoft Orleans `IDistributedCache` implementations** – both volatile and persisted versions, along with services that can be used from within an Orleans server.
- **Microsoft Orleans cluster abstract cache grain implementations** – both volatile and persisted versions that leverage Orleans’ built-in capabilities for concurrency and serialization. These provide cache stampede protection and simplify architecture by eliminating the need for a separate caching server. They support cache-aside, read-through, write-around, and write-through caching strategies.

