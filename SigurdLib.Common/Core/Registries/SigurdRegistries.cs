using Sigurd.Common.Core.Resources;

namespace Sigurd.Common.Core.Registries;

/// <summary>
/// Exposes static references to all Sigurd registries.
/// It is advised to use <see cref="DeferredRegister"/> and/or <see cref="RegisterEvent"/> to
/// register things, but queries and iterations are safe.
/// </summary>
public class SigurdRegistries
{
    public static readonly ResourceName RootRegistryName = new ResourceName("root");

    #region Game Content

    // public static readonly ISigurdRegistry<Item> ITEMS = ...;
    // public static readonly ISigurdRegistry<Moon> MOONS = ...;
    // public static readonly ISigurdRegistry<DungeonFlow> DUNGEON_FLOWS = ...;
    // public static readonly ISigurdRegistry<MapObject> MAP_OBJECTS = ...;
    // public static readonly ISigurdRegistry<EnemyType> ENEMY_TYPES = ...;
    // public static readonly ISigurdRegistry<WeatherCondition> WEATHER_CONDITIONS = ...;

    // public static readonly ISigurdRegistry<Unlockable> UNLOCKABLES = ...;
    // public static readonly ISigurdRegistry<TerminalCommand> TERMINAL_COMMANDS = ...;

    // public static readonly ISigurdRegistry<ActiveEffect> ACTIVE_EFFECTS = ...;
    // public static readonly ISigurdRegistry<AudioClip> AUDIO_CLIPS = ...;

    #endregion

    #region Model Replacements

    // public static readonly ISigurdRegistry<PlayerModelVariant> PLAYER_MODEL_VARIANTS = ...;
    // public static readonly ISigurdRegistry<EnemyTypeModelVariant> ENEMY_TYPE_MODEL_VARIANTS = ...;

    #endregion

    #region Texture Replacements

    // public static readonly ISigurdRegistry<PlayerTextureVariant> PLAYER_TEXTURE_VARIANTS = ...;
    // public static readonly ISigurdRegistry<PosterVariant> POSTER_VARIANTS = ...;
    // public static readonly ISigurdRegistry<PaintingVariant> PAINTING_VARIANTS = ...;

    #endregion

    public static class Keys
    {

    }
}
