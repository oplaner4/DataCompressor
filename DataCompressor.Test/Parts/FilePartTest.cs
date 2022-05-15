using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using DataCompressor.Parts;
using System.Collections.Generic;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class FilePartTest
    {
        [Theory]
        [InlineData(
            new byte[] { },
            false,
            new byte[] {
                0, 0, 0, 0
            }
        )]
        [InlineData(
            new byte[] { },
            true,
            new byte[] {
                0, 0, 0, 0
            }
        )]
        [InlineData(
            new byte[] { (byte)'b' },
            false,
            new byte[] {
                0, 0, 0, 1,
                (byte)'b',
            }
        )]
        [InlineData(
            new byte[] { (byte)'d' },
            true,
            new byte[] {
                0, 0, 0, 1,
                (byte)'d',
                0, 0, 0, 1,
            }
        )]
        [InlineData(
            new byte[] {
                (byte)'s', (byte)'t', (byte)'r',
                (byte)'i', (byte)'n', (byte)'g'
            },
            false,
            new byte[] {
                0, 0, 0, 6,
                (byte)'s', (byte)'t', (byte)'r',
                (byte)'i', (byte)'n', (byte)'g'
            }
        )]
        [InlineData(
            new byte[] {
                (byte)'l', (byte)'o', (byte)'o',
                (byte)'k', (byte)'i', (byte)'n', (byte)'g'
            },
            true,
            new byte[] {
                0, 0, 0, 6,
                (byte)'l',
                0, 0, 0, 1,
                (byte)'o',
                0, 0, 0, 2,
                (byte)'k',
                0, 0, 0, 1,
                (byte)'i',
                0, 0, 0, 1,
                (byte)'n',
                0, 0, 0, 1,
                (byte)'g',
                0, 0, 0, 1
            }
        )]
        [InlineData(
            new byte[] {
                (byte)'s', (byte)'s', (byte)'h',
                (byte)'h', (byte)'s', (byte)'s'
            },
            true,
            new byte[] {
                0, 0, 0, 3,
                (byte)'s',
                0, 0, 0, 2,
                (byte)'h',
                0, 0, 0, 2,
                (byte)'s',
                0, 0, 0, 2,
            }
        )]
        public void CreateFile_ShouldSucceed(
            IEnumerable<byte> data, bool repeatModeOn, byte[] expectedBytes)
        {
            ModesPart modes = new();

            if (!repeatModeOn)
            {
                modes.SetOff(Mode.RepeatMode);
            }

            FilePart entryManager = new(data, modes);
            Assert.True(entryManager.PrepareBytes());
            Assert.Equal(expectedBytes, entryManager.GetBytes());
        }

        [Fact]
        public void CreateFile_ShouldFail()
        {
            List<byte> data = new();

            int i = 0;
            while (i < 256)
            {
                data.Add(55);
                i++;
            }

            ModesPart modes = new ModesPart()
                .SetOn(Mode.RepeatMode)
                .SetOff(Mode.RepeatCountIntMode)
                .SetOff(Mode.RepeatCountShortMode);

            FilePart entryManager = new(data, modes);
            Assert.False(entryManager.PrepareBytes());
        }

        [Theory]
        [InlineData(
            new byte[] {
                0, 0, 0, 0
            },
            true,
            new byte[] { }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 0
            },
            false,
            new byte[] { }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 2,
                1,
                1,
            },
            false,
            new byte[] { 1, 1 }
        )]
        [InlineData(
            new byte[] {
                 0, 0, 0, 1,
                (byte)'y'
            },
            false,
            new byte[] { (byte)'y' }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 1,
                (byte)'y',
                0, 0, 0, 1,
            },
            true,
            new byte[] { (byte)'y' }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 6,
                (byte)'c', (byte)'s', (byte)'h',
                (byte)'a', (byte)'r', (byte)'p',
            },
            false,
            new byte[] {
                (byte)'c', (byte)'s', (byte)'h',
                (byte)'a', (byte)'r', (byte)'p'
            }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 3,
                (byte)'p',
                0, 0, 0, 1,
                (byte)'o',
                0, 0, 0, 2,
                (byte)'r',
                0, 0, 0, 1,
            },
            true,
            new byte[] {
                (byte)'p', (byte)'o', (byte)'o', (byte)'r',
            }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 6,
                (byte)'g',
                0, 0, 0, 1,
                (byte)'o',
                0, 0, 0, 2,
                (byte)'d',
                0, 0, 0, 1,
                (byte)'b',
                0, 0, 0, 1,
                (byte)'o',
                0, 0, 0, 1,
                (byte)'s',
                0, 0, 0, 2,
            },
            true,
            new byte[] {
                (byte)'g', (byte)'o', (byte)'o', (byte)'d',
                (byte)'b', (byte)'o', (byte)'s', (byte)'s',
            }
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 2,
                (byte)'s',
                0, 0, 0, 3,
                (byte)'r',
                0, 0, 0, 1,
            },
            true,
            new byte[] {
                (byte)'s', (byte)'s', (byte)'s', (byte)'r',
            }
        )]
        public void ReconstructFile_ShouldSucceed(
            IEnumerable<byte> bytes, bool repeatModeOn, IEnumerable<byte> expectedData)
        {
            ModesPart modes = new();

            if (!repeatModeOn)
            {
                modes.SetOff(Mode.RepeatMode);
            }

            FilePart entryManager = new(new ConstructEnumerator(bytes), modes);
            Assert.Equal(expectedData, entryManager.Fragments);
        }

        [Theory]
        [InlineData(
            new byte[] {
                0, 0, 0, 2,
                (byte)'k'
            },
            false
        )]
        [InlineData(
            new byte[] {
                0, 0, 0,
                (byte)'r'
            },
            false
        )]
        public void ReconstructFile_ShouldFail(
            IEnumerable<byte> bytes, bool repeatModeOn)
        {
            ModesPart modes = new();

            if (!repeatModeOn)
            {
                modes.SetOff(Mode.RepeatMode);
            }

            Assert.Throws<UnableToConstructException>(() =>
                new FilePart(new ConstructEnumerator(bytes), modes));
        }
    }
}