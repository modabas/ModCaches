# ModCaches

[![Nuget](https://img.shields.io/nuget/v/ModCaches.ExtendedDistributedCache.svg)](https://www.nuget.org/packages/ModCaches.ExtendedDistributedCache/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/modabas/ModCaches/blob/main/LICENSE.txt)

**ModCaches** provides cache and cache helper implementations for various use cases:

- **Extended Distributed Cache** – a wrapper around `IDistributedCache` that simplifies the use of distributed cache implementations and provides in-process cache stampede protection.
- **Microsoft Orleans `IDistributedCache` implementations** – both volatile and persisted versions, along with services that can be used from an Orleans client application or within an Orleans server.
- **Microsoft Orleans cluster abstract cache grain implementations** – both volatile and persisted versions that leverage Orleans’ built-in capabilities for concurrency and serialization. These provide cache stampede protection and simplify architecture by eliminating the need for a separate caching server. They support cache-aside, read-through, write-around, and write-through caching strategies.

## Documentation

Please refer to the individual documents for detailed information on features, getting started, and usage for each component:

- [Extended Distributed Cache](./docs/ExtendedDistributedCache.md)
- [Microsoft Orleans `IDistributedCache` implementations](./docs/OrleansDistributedCache.md)
- [Microsoft Orleans cluster cache grain implementations](./docs/OrleansClusterCache.md)

