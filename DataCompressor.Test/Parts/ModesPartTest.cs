using DataCompressor.Helpers;
using DataCompressor.Parts;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class ModesPartTest
    {
        [Theory]
        [InlineData(0b0000_0011)]
        [InlineData(0b1000_0011)]
        [InlineData(0b0010_0011)]
        public void AllModesSet_ShouldSucceed(byte modesByte)
        {
            ModesPart manager = new(modesByte);
            Assert.True(manager.IsOn(Mode.RepeatMode));
            Assert.True(manager.IsOn(Mode.FileTreeMode));
        }

        [Theory]
        [InlineData(0b0000_0010)]
        [InlineData(0b1000_0010)]
        [InlineData(0b0010_0010)]
        public void OnlyFileTreeModeSet_ShouldSucceed(byte modesByte)
        {
            ModesPart manager = new(modesByte);
            Assert.False(manager.IsOn(Mode.RepeatMode));
            Assert.True(manager.IsOff(Mode.RepeatMode));
            Assert.True(manager.IsOn(Mode.FileTreeMode));

        }

        [Theory]
        [InlineData(0b0000_0001)]
        [InlineData(0b1000_0001)]
        [InlineData(0b0010_0001)]
        public void OnlyRepeatModeSet_ShouldSucceed(byte modesByte)
        {
            ModesPart manager = new(modesByte);
            Assert.True(manager.IsOn(Mode.RepeatMode));
            Assert.False(manager.IsOn(Mode.FileTreeMode));
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1000_0000)]
        [InlineData(0b0010_0000)]
        public void NoModeSet_ShouldSucceed(byte modesByte)
        {
            ModesPart manager = new(modesByte);
            Assert.False(manager.IsOn(Mode.RepeatMode));
            Assert.False(manager.IsOn(Mode.FileTreeMode));
        }

        [Theory]
        [InlineData(0b0000_0000, Mode.RepeatMode, 0b0000_0001)]
        [InlineData(0b0000_0000, Mode.FileTreeMode, 0b0000_0010)]
        [InlineData(0b0010_0001, Mode.RepeatMode, 0b0010_0001)]
        [InlineData(0b1000_0010, Mode.FileTreeMode, 0b1000_0010)]
        [InlineData(0b0000_0010, Mode.RepeatMode, 0b0000_0011)]
        [InlineData(0b0000_0001, Mode.FileTreeMode, 0b0000_0011)]
        [InlineData(0b0110_0010, Mode.RepeatMode, 0b0110_0011)]
        public void SetModeOn_ShouldSucceed(
            byte modesByte, Mode mode, byte expectedModesByte)
        {
            ModesPart manager = new ModesPart(modesByte).SetOn(mode);
            Assert.Equal(expectedModesByte, manager.ModesByte);
            Assert.True(manager.IsOn(mode));
            Assert.False(manager.IsOff(mode));
        }

        [Theory]
        [InlineData(0b0000_0001, Mode.RepeatMode, 0b0000_0000)]
        [InlineData(0b0000_0010, Mode.FileTreeMode, 0b0000_0000)]
        [InlineData(0b1000_0000, Mode.FileTreeMode, 0b1000_0000)]
        [InlineData(0b0000_0011, Mode.RepeatMode, 0b0000_0010)]
        [InlineData(0b0000_0011, Mode.FileTreeMode, 0b0000_0001)]
        [InlineData(0b0110_0011, Mode.RepeatMode, 0b0110_0010)]
        public void SetModeOff_ShouldSucceed(
            byte modesByte, Mode mode, byte expectedModesByte)
        {
            ModesPart manager = new ModesPart(modesByte).SetOff(mode);
            Assert.Equal(expectedModesByte, manager.ModesByte);
            Assert.True(manager.IsOff(mode));
        }

        [Theory]
        [InlineData(false, false, 1)]
        [InlineData(true, false, 2)]
        [InlineData(false, true, 4)]
        [InlineData(true, true, 8)]
        public void GetBytesCount_ShouldSucceed(
            bool shortModeOn, bool intModeOn, int expectedCount)
        {
            Assert.Equal(
                expectedCount,
                ModesPart.GetBytesCount(shortModeOn, intModeOn));
        }

        [Theory]
        [InlineData(false, false, 0b0000_0001)]
        [InlineData(true, false, 0b1010_0001_0001_0100)]
        [InlineData(false, true, 0b1000_0001_0001_0100_1100)]
        [InlineData(true, true, 0b1001_0001_0001_0100_1100_1100_0000_1111_0010)]
        public void ChooseIntegerModes_ShouldSucceed(
            bool expectedShortModeOn, bool expectedIntModeOn,
            ulong setInteger
        )
        {
            ModesPart modes = new();
            Assert.True(modes.ChooseIntegerModes(
                Mode.FragmentsCountShortMode,
                Mode.FragmentsCountIntMode,
                () =>
                {
                    DynamicInteger integer = new(
                        ModesPart.GetBytesCount(
                            modes.IsOn(Mode.FragmentsCountShortMode),
                            modes.IsOn(Mode.FragmentsCountIntMode)));
                    return integer.Set(setInteger);
                }));

            Assert.Equal(
                expectedShortModeOn,
                modes.IsOn(Mode.FragmentsCountShortMode));

            Assert.Equal(
                expectedIntModeOn,
                modes.IsOn(Mode.FragmentsCountIntMode));
        }

        [Fact]
        public void ChooseIntegerModes_ShouldFail()
        {
            ModesPart modes = new();
            Assert.False(modes.ChooseIntegerModes(
                Mode.FragmentsCountShortMode,
                Mode.FragmentsCountIntMode,
                () => false));
        }
    }
}