using Sigurd.Common.Core.Resources;
using Sigurd.Util.Resources;

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

    // public static readonly IRegistry<Item> ITEMS = ...;
    // public static readonly IRegistry<Moon> MOONS = ...;
    // public static readonly IRegistry<DungeonFlow> DUNGEON_FLOWS = ...;
    // public static readonly IRegistry<MapObject> MAP_OBJECTS = ...;
    // public static readonly IRegistry<EnemyType> ENEMY_TYPES = ...;
    // public static readonly IRegistry<WeatherCondition> WEATHER_CONDITIONS = ...;

    // public static readonly IRegistry<Unlockable> UNLOCKABLES = ...;
    // public static readonly IRegistry<TerminalCommand> TERMINAL_COMMANDS = ...;

    // public static readonly IRegistry<ActiveEffect> ACTIVE_EFFECTS = ...;
    // public static readonly IRegistry<AudioClip> AUDIO_CLIPS = ...;

    #endregion

    #region Model Replacements

    // public static readonly IRegistry<PlayerModelVariant> PLAYER_MODEL_VARIANTS = ...;
    // public static readonly IRegistry<EnemyTypeModelVariant> ENEMY_TYPE_MODEL_VARIANTS = ...;

    #endregion

    #region Texture Replacements

    // public static readonly IRegistry<PlayerTextureVariant> PLAYER_TEXTURE_VARIANTS = ...;
    // public static readonly IRegistry<PosterVariant> POSTER_VARIANTS = ...;
    // public static readonly IRegistry<PaintingVariant> PAINTING_VARIANTS = ...;

    #endregion

    public static class Keys
    {

    }
}
