namespace Shared.Common.Characters;

/// <summary>
/// Maps CharCreate color SDB ids (WarpaintPalette) to packed RGB565 light/dark pairs
/// for list paperdoll / in-world visuals. Packing matches retail FColor.CombineLightDark
/// (ARGB8888 highlight/shadow → RGB565 pair). Generated from clientdb.sd2
/// dbvisualrecords::WarpaintPalette (type_flags skin/hair/eye).
/// </summary>
public static class CharCreateColors
{
    /// <summary>
    /// Default chassis warpaint for Accord starter frames (palette 77221 fullbody).
    /// Same 7-tuple as SDBUtils.GetChassisWarpaint(startClassId, 0, 0, 0, 0).
    /// </summary>
    public static readonly long[] AccordChassisWarpaint =
    [
        0xFFFF2104L,
        0x9CD30000L,
        0x31860000L,
        0x4A490000L,
        0x94B27BAEL,
        0xCC803141L,
        0xCC803141L,
    ];

    private static readonly System.Collections.Generic.Dictionary<int, uint> Known = new()
    {
        { 76798, 0xEEB76A49u }, // skin
        { 76866, 0x9CD81846u }, // eye
        { 76878, 0x71E3B46Du }, // hair
        { 77176, 0xFFFF0000u }, // skin
        { 77177, 0xF71B0000u }, // skin
        { 77178, 0xDDB40000u }, // skin
        { 77179, 0xA44E0000u }, // skin
        { 77180, 0x9C2E0000u }, // skin
        { 77181, 0x52680000u }, // skin
        { 77182, 0x0061398Au }, // eye
        { 77183, 0x6A2440E0u }, // eye
        { 77184, 0x751D29ECu }, // eye
        { 77185, 0x450A2A23u }, // eye
        { 77187, 0x31A60020u }, // hair
        { 77188, 0x62240020u }, // hair
        { 77189, 0xCD8D1880u }, // hair
        { 77190, 0x79431000u }, // hair
        { 77191, 0xFFFF0841u }, // hair
        { 77192, 0xB75D0082u }, // hair
        { 77193, 0x724F0021u }, // hair
        { 77194, 0x320D0021u }, // hair
        { 77195, 0x54670000u }, // hair
        { 77565, 0x41841841u }, // skin
        { 77570, 0x6A661881u }, // skin
        { 77752, 0xDE9C51C4u }, // skin
        { 77753, 0xF77E51C4u }, // skin
        { 77754, 0xEE7A31C9u }, // skin
        { 77757, 0xFFDF5A2Du }, // skin
        { 77758, 0xDF3A6A28u }, // skin
        { 77759, 0xCF1A3165u }, // skin
        { 86347, 0xD7E33B99u }, // eye
        { 86348, 0xFD635901u }, // eye
        { 86349, 0x3B350021u }, // eye
        { 86350, 0xAB0020A1u }, // eye
        { 86351, 0xA80018C3u }, // eye
        { 86352, 0xFFFFFFFFu }, // eye
        { 86353, 0x881C881Cu }, // eye
        { 86354, 0x6C9F2700u }, // eye
        { 86355, 0x8C718C71u }, // eye
        { 86356, 0xF816F816u }, // eye
        { 118967, 0x9C0D3124u }, // skin
        { 118968, 0x62A80000u }, // skin
        { 118969, 0xFFFF7186u }, // skin
        { 118970, 0xFEB66228u }, // skin
        { 118971, 0xCD3151E6u }, // skin
        { 118980, 0x61601060u }, // eye
        { 118981, 0x063F09A5u }, // eye
        { 119134, 0x3ED53ED5u }, // eye
        { 119430, 0xE60F72A3u }, // hair
        { 119431, 0x29020861u }, // hair
        { 119432, 0xB1601000u }, // hair
        { 123055, 0xE4710841u }, // skin
        { 124570, 0xFFFFAF1Eu }, // hair
        { 124571, 0xBF9E0638u }, // hair
        { 124572, 0x04E01CE6u }, // hair
        { 124573, 0xB160D8C3u }, // hair
        { 124574, 0xFEB0EF19u }, // hair
        { 124575, 0x482EE0D3u }, // hair
        { 124576, 0xCC265A00u }, // hair
        { 124577, 0x61E26A83u }, // hair
        { 124578, 0xBE38BE18u }, // hair
        { 124579, 0xD1A0D800u }, // hair
        { 124580, 0x057705B6u }, // hair
        { 124581, 0xD006FF00u }, // hair
    };

    public static uint Resolve(int colorId, uint fallback)
    {
        return Known.TryGetValue(colorId, out var color) ? color : fallback;
    }

    public static void Resolve(CreatedCharacterRecord record)
    {
        record.SkinColor = Resolve(record.SkinColorId, 0xEEB76A49u);
        record.EyeColor = Resolve(record.EyeColorId, 0x751D29ECu);
        record.HairColor = Resolve(record.HairColorId, 0x724F0021u);
        if (record.LipColor == 0)
        {
            record.LipColor = 0xffff0000u;
        }
    }
}
