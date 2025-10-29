using BlockSLAE.Smoothing;

namespace BlockSLAE.Testing.Configs;

public record SmoothingConfig(string Name, ISmoothingStrategy Strategy);