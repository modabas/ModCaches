# ModCaches

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/modabas/ModCaches/blob/main/LICENSE.txt)

**ModCaches** provides cache and cache helper implementations for various use cases.

- Extended Distributed Cache, a wrapper around IDistributedCache to simplify usage of distributed cache implementations and provide some cache stampede protection,
- Microsoft Orleans IDistributedCache implementations, both volatile and persisted, along with services to use from an Orleans client or within Orleans server,
- Microsoft Orleans in-cluster cache grain implementations, both volatile and persisted, that encapsulates cache value generation and caching in one unit and provides easy to use methods to interact with.

## Documentation

Please refer to individual documents for detailed information on features, getting started and how to use each component:

- [Extended Distributed Cache](./docs/ExtendedDistributedCache.md)
- [Microsoft Orleans IDistributedCache implementations](./docs/OrleansDistributedCache.md)
- [Microsoft Orleans in-cluster cache grain implementations](./docs/OrleansInClusterCache.md)

