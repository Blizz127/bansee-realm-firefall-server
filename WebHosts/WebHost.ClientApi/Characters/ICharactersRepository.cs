using Shared.Common.Characters;
using WebHost.ClientApi.Characters.Models;

namespace WebHost.ClientApi.Characters;

public interface ICharactersRepository
{
    CharactersList GetCharacters();

    CreatedCharacterRecord CreateCharacter(CharacterCreate data);

    CreatedCharacterRecord SoftDelete(ulong characterGuid);

    CreatedCharacterRecord Undelete(ulong characterGuid);
}
