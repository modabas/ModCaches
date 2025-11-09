# ModCaches

[![Nuget](https://img.shields.io/nuget/v/ModCaches.ExtendedDistributedCache.svg)](https://www.nuget.org/packages/ModCaches.ExtendedDistributedCache/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/modabas/ModCaches/blob/main/LICENSE.txt)

**ModCaches** provides cache and cache helper implementations for various use cases.

- Extended Distributed Cache, a wrapper around IDistributedCache to simplify usage of distributed cache implementations and provide in-process cache stampede protection,
- Microsoft Orleans IDistributedCache implementations, both volatile and persisted, along with services to use from an Orleans client application or within Orleans server,
- Microsoft Orleans in-cluster abstract cache grain implementations, both volatile and persisted, that leverages Orleans' built-in capabilities for concurrency and serialization. Provides cache stampede protection and simplifies architecture by eliminating the need for a separate caching server. Supports cache-aside, read-through, write-around and write-through caching strategies.

## Documentation

Please refer to individual documents for detailed information on features, getting started and how to use each component:

- [Extended Distributed Cache](./docs/ExtendedDistributedCache.md)
- [Microsoft Orleans IDistributedCache implementations](./docs/OrleansDistributedCache.md)
- [Microsoft Orleans in-cluster cache grain implementations](./docs/OrleansInClusterCache.md)

