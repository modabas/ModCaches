using Orleans.Metadata;

namespace ModCaches.Orleans.Server.Tests;
public class GrainIdFactory
{
  private readonly GrainInterfaceTypeResolver _grainInterfaceTypeResolver;
  private readonly GrainInterfaceTypeToGrainTypeResolver _grainInterfaceTypeToGrainTypeResolver;

  public GrainIdFactory(
      GrainInterfaceTypeResolver interfaceTypeResolver,
      GrainInterfaceTypeToGrainTypeResolver interfaceToTypeResolver)
  {
    _grainInterfaceTypeResolver = interfaceTypeResolver;
    _grainInterfaceTypeToGrainTypeResolver = interfaceToTypeResolver;
  }

  public GrainId CreateGrainId<TInterface>(Guid grainKey) where TInterface : IAddressable
  {
    GrainType grainType = ResolveGrainType(typeof(TInterface));
    GrainId grainId = GrainId.Create(grainType, GrainIdKeyExtensions.CreateGuidKey(grainKey));
    return grainId;
  }

  public GrainId CreateGrainId<TInterface>(long grainKey) where TInterface : IAddressable
  {
    GrainType grainType = ResolveGrainType(typeof(TInterface));
    GrainId grainId = GrainId.Create(grainType, GrainIdKeyExtensions.CreateIntegerKey(grainKey));
    return grainId;
  }

  public GrainId CreateGrainId<TInterface>(string grainKey) where TInterface : IAddressable
  {
    GrainType grainType = ResolveGrainType(typeof(TInterface));
    GrainId grainId = GrainId.Create(grainType, grainKey);
    return grainId;
  }

  private GrainType ResolveGrainType(Type interfaceType)
  {
    GrainInterfaceType grainInterfaceType = _grainInterfaceTypeResolver.GetGrainInterfaceType(interfaceType);
    GrainType grainType = _grainInterfaceTypeToGrainTypeResolver.GetGrainType(grainInterfaceType);
    return grainType;
  }
}
